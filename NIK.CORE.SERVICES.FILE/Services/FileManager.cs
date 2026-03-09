using System.Data;
using System.Security.Cryptography;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NIK.CORE.SERVICES.FILE.Dtos;

namespace NIK.CORE.SERVICES.FILE.Services;
/// <summary>
/// Provides high-level file management operations including:
/// 
/// - Uploading files
/// - Deduplicating physical storage
/// - Retrieving file metadata
/// - Streaming file content
/// - Converting files to Base64
/// - Soft deleting files
/// 
/// The system separates logical files (<c>FileEntry</c>) from physical storage
/// (<c>PhysicalStorage</c>) to support file deduplication. Multiple file entries
/// can reference the same physical file via the <c>physicalId</c>.
/// 
/// This class coordinates with:
/// - <see cref="PhysicalStorageManager"/> for physical file storage metadata
/// - <see cref="FolderManager"/> for folder path resolution
/// </summary>
public class FileManager
{
    /// <summary>
    /// Database connection used for executing SQL queries via Dapper.
    /// </summary>
    private readonly IDbConnection _dbConnection;
    /// <summary>
    /// Logger used for tracing file operations such as uploads,
    /// downloads, deduplication events, and error diagnostics.
    /// </summary>
    private readonly ILogger<FolderManager> _logger;
    /// <summary>
    /// Name of the database table storing logical file entries.
    /// </summary>
    public static readonly string FileEntryTable = "FileEntry";
    /// <summary>
    /// SQL alias used for the <see cref="FileEntryTable"/> in queries.
    /// </summary>
    public static readonly string FileEntryTableAlias = "fe";
    /// <summary>
    /// Configuration settings for the file manager service,
    /// including storage root path, maximum file size, and allowed MIME types.
    /// </summary>
    private readonly FileManagerServiceConfig _fileManagerServiceConfig;
    /// <summary>
    /// Service responsible for managing physical file storage metadata
    /// and reference counting for deduplicated files.
    /// </summary>
    private readonly PhysicalStorageManager _physicalStorageManager;
    /// <summary>
    /// Service responsible for folder management and path resolution.
    /// </summary>
    private readonly FolderManager _forderManager;
    /// <summary>
    /// Initializes a new instance of the <see cref="FileManager"/> class.
    /// </summary>
    /// <param name="dbConnection">
    /// Database connection used to perform queries and commands.
    /// </param>
    /// <param name="logger">
    /// Logger used for monitoring file operations and debugging.
    /// </param>
    /// <param name="fileManagerServiceConfig">
    /// Configuration for file storage behavior such as maximum file size
    /// and allowed MIME types.
    /// </param>
    /// <param name="physicalStorageManager">
    /// Service used to manage physical file storage records and
    /// deduplication logic.
    /// </param>
    /// <param name="forderManager">
    /// Service used to retrieve folder paths and manage folder structure.
    /// </param>
    public FileManager(IDbConnection dbConnection, ILogger<FolderManager> logger,
        FileManagerServiceConfig fileManagerServiceConfig, 
        PhysicalStorageManager physicalStorageManager,
        FolderManager forderManager)
    {
        _dbConnection = dbConnection;
        _logger = logger;
        _fileManagerServiceConfig = fileManagerServiceConfig;
        _physicalStorageManager = physicalStorageManager;
        _forderManager = forderManager;
    }
    /// <summary>
    /// Uploads a file to the storage system.
    /// </summary>
    /// <param name="request">
    /// Upload request containing the file, folderId, and optional display name.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="FileDto"/> representing the created file entry.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when:
    /// - File is empty
    /// - File size exceeds the configured limit
    /// - MIME type is not allowed
    /// </exception>
    /// <remarks>
    /// The upload process performs the following steps:
    /// 
    /// 1. Validate file size and MIME type.
    /// 2. Compute SHA256 hash of the file.
    /// 3. Check if a physical file with the same hash already exists.
    /// 4. If not, create a new physical storage record and save the file to disk.
    /// 5. Create a logical file entry in the <c>FileEntry</c> table.
    /// 6. Increment the reference count for the physical storage.
    /// 
    /// Deduplication ensures identical files are stored only once.
    /// </remarks>
    public async Task<FileDto> UploadFileAsync(UploadFileRequest request, CancellationToken cancellationToken = default)
    {
        IFormFile file = request.File;
        if (file.Length == 0)
            throw new InvalidOperationException("Uploaded file is empty.");
        
        if (file.Length > _fileManagerServiceConfig.MaxFileSizeBytes)
            throw new InvalidOperationException(
                $"File size {file.Length} exceeds the limit of {_fileManagerServiceConfig.MaxFileSizeBytes} bytes.");
        
        string mimeType = file.ContentType;
        if (_fileManagerServiceConfig.AllowedMimeTypes.Count > 0 && !_fileManagerServiceConfig.AllowedMimeTypes.Contains(mimeType))
            throw new InvalidOperationException($"MIME type '{mimeType}' is not allowed.");

        await using Stream stream = file.OpenReadStream();
        byte[] fileBytes = new byte[file.Length];
        _ = await stream.ReadAsync(fileBytes, cancellationToken);
        string fileHash = ComputeSha256(fileBytes);
        _logger.LogDebug("Upload: file={Name} size={Size} hash={Hash}",
            file.FileName, file.Length, fileHash);
        string extension  = Path.GetExtension(file.FileName).TrimStart('.');
        string relativePath = BuildRelativePath(fileHash, extension);
        
        var (physicalRecord, isNew) = await _physicalStorageManager.GetOrCreateAsync(new CreatePhysicalStorageRequest()
        {
            FileHash = fileHash,
            FileSize = file.Length,
            RelativePath = relativePath,
            MimeType = mimeType,
        }, cancellationToken);

        if (isNew)
        {
            // find folder id 
            var pathFromFolderId = await _forderManager.GetPathFromFolderIdAsync(request.FolderId, cancellationToken);
            if (pathFromFolderId is null)
                throw new ArgumentNullException(pathFromFolderId, $"Can't find folder for folderId: {request.FolderId}");   
            var absolutePath = Path.Combine(_fileManagerServiceConfig.StorageRoot, pathFromFolderId, physicalRecord.RelativePath);
            var directory = Path.GetDirectoryName(absolutePath)
                ?? throw new Exception($"Can't get directory from path {absolutePath}");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            await File.WriteAllBytesAsync(absolutePath, fileBytes, cancellationToken);
            
            _logger.LogInformation(
                "Saved new physical file to {Path} ({Size} bytes)", absolutePath, file.Length);
        }
        else
        {
            _logger.LogInformation(
                "Deduplication: reusing physical record {Id} for hash {Hash}",
                physicalRecord.Id, fileHash);
        }
        var displayName = request.DisplayName
                          ?? Path.GetFileNameWithoutExtension(file.FileName)
                          ?? file.FileName;
        string insertSql = $"""
                            INSERT INTO {FileEntryTable}
                            ("displayName", "extension", "folderId", "physicalId")
                            VALUES (@DisplayName, @Extension, @FolderId, @PhysicalId)
                            RETURNING 
                                "id"            AS {nameof(FileDto.Id)},
                                "displayName"   AS {nameof(FileDto.DisplayName)},
                                "extension"     AS {nameof(FileDto.Extension)},
                                "folderId"      AS {nameof(FileDto.FolderId)},
                                "physicalId"    AS {nameof(FileDto.PhysicalId)}
                                "isDeleted"     AS {nameof(FileDto.IsDeleted)}
                                "createdAt"     AS {nameof(FileDto.CreatedAt)}
                                "updatedAt"     AS {nameof(FileDto.UploadAt)};
                           """;
        var entryFile = await _dbConnection.QuerySingleAsync<FileDto>(new CommandDefinition(insertSql,
            new
            {
                DisplayName = displayName,
                Extension = string.IsNullOrEmpty(extension) ? null : extension,
                FolderId = request.FolderId,
                PhysicalId = physicalRecord.Id
            }, cancellationToken: cancellationToken));
        
        await _physicalStorageManager.IncrementRefCountAsync(physicalRecord.Id, cancellationToken);
        
        _logger.LogInformation(
            "FileEntry {EntryId} created → physical {PhysicalId}",
            entryFile.Id, physicalRecord.Id);

        entryFile.FileSize = physicalRecord.FileSize;
        entryFile.MimeType = physicalRecord.MimeType;
        entryFile.RelativePath = physicalRecord.RelativePath;
        return entryFile;
    }
    /// <summary>
    /// Retrieves a file entry and its physical storage metadata by file ID.
    /// </summary>
    /// <param name="id">The unique identifier of the file entry.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// A <see cref="FileDto"/> if found; otherwise <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method joins the <c>FileEntry</c> table with the
    /// <c>PhysicalStorage</c> table to return both logical and
    /// physical file information.
    /// </remarks>
    public async Task<FileDto?> GetFileByIdAsync(string id, CancellationToken cancellation = default)
    {
        string sql = $"""
                     SELECT {FileManagerSqlHelper.MappingColumnToDto}
                     FROM {FileEntryTable} {FileEntryTableAlias}
                     JOIN {PhysicalStorageManager.PhysicalStorageTable} {PhysicalStorageManager.PhysicalStorageTableAlias}
                        ON {PhysicalStorageManager.PhysicalStorageTableAlias}."id" = {FileEntryTableAlias}."physicalId"
                     WHERE {PhysicalStorageManager.PhysicalStorageTableAlias}."id" = @Id
                        AND {FileManagerSqlHelper.FilterForSafeDelete};
                     """;
        FileDto? result = await _dbConnection.QueryFirstOrDefaultAsync<FileDto>(new CommandDefinition(sql, new
        {
            Id = id
        }, cancellationToken: cancellation));
        return result;
    }
    /// <summary>
    /// Retrieves all files within a specific folder.
    /// </summary>
    /// <param name="folderId">The identifier of the folder.</param>
    /// <param name="skip">Number of records to skip (pagination).</param>
    /// <param name="take">Maximum number of records to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A collection of <see cref="FileDto"/> objects.
    /// </returns>
    /// <remarks>
    /// Results are sorted by creation date in descending order.
    /// Only files that are not soft-deleted will be returned.
    /// </remarks>
    public async Task<IReadOnlyCollection<FileDto>> GetAllFileByFolderIdAsync(string folderId,
        int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        string sql = $"""
                      SELECT {FileManagerSqlHelper.MappingColumnToDto}
                      FROM {FileEntryTable} {FileEntryTableAlias}
                      JOIN {PhysicalStorageManager.PhysicalStorageTable} {PhysicalStorageManager.PhysicalStorageTableAlias}
                        ON {PhysicalStorageManager.PhysicalStorageTableAlias}."Id" = {FileEntryTableAlias}."physicalId"
                      WHERE {FileEntryTableAlias}."folderId" = @FolderId
                        AND {FileManagerSqlHelper.FilterForSafeDelete}
                      ORDER BY {FileEntryTableAlias}."createdAt" DESC
                      OFFSET @Skip
                      LIMIT @Take;
                      """;
        IEnumerable<FileDto> result = await _dbConnection.QueryAsync<FileDto>(new CommandDefinition(sql, new
        {
            FolderId = folderId,
            Skip = skip,
            Take = take
        }, cancellationToken: cancellationToken));
        return result.AsList();
    }
    /// <summary>
    /// Opens a read-only stream for the specified file.
    /// </summary>
    /// <param name="fileId">The identifier of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A readable <see cref="Stream"/> for the file content.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the file entry does not exist.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the physical file is missing from disk.
    /// </exception>
    /// <remarks>
    /// This method retrieves the file metadata and then opens
    /// the physical file stored on disk.
    /// </remarks>
    public async Task<Stream> OpenReadStreamAsync(string fileId, CancellationToken cancellationToken = default)
    {
        FileDto? entryFile = await GetFileByIdAsync(fileId, cancellationToken);
        if (entryFile is null)
        {
            _logger.LogDebug("Not found file has id {id}", fileId);
            throw new KeyNotFoundException($"File '{fileId}' not found.");
        }

        var absolutePath = Path.Combine(_fileManagerServiceConfig.StorageRoot, entryFile.RelativePath);
        if (!System.IO.File.Exists(absolutePath))
        {
            _logger.LogDebug("Physical file missing on disk: {absolutePath}", absolutePath);
            throw new FileNotFoundException(
                $"Physical file missing on disk: {absolutePath}", absolutePath);
        }
        return new FileStream(absolutePath, FileMode.Open, FileAccess.Read, 
            FileShare.ReadWrite,bufferSize: 81920, useAsync: true);
    }
    /// <summary>
    /// Retrieves a file and returns its content encoded as Base64.
    /// </summary>
    /// <param name="fileId">The identifier of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Base64 string representation of the file content.
    /// </returns>
    /// <remarks>
    /// This method loads the file stream into memory and converts
    /// the bytes to a Base64 encoded string.
    /// 
    /// Warning: For large files this may consume significant memory.
    /// </remarks>
    public async Task<string> GetFileBase64ASync(string fileId, CancellationToken cancellationToken = default)
    {
        await using var stream = await OpenReadStreamAsync(fileId, cancellationToken);
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var bytes = memory.ToArray();
        return Convert.ToBase64String(bytes);
    }
    /// <summary>
    /// Soft deletes a file entry.
    /// </summary>
    /// <param name="fileId">The identifier of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the file does not exist or has already been deleted.
    /// </exception>
    /// <remarks>
    /// This operation performs a soft delete by setting
    /// <c>isDeleted = true</c>.
    /// 
    /// The physical file will not be deleted immediately.
    /// Physical deletion is controlled by reference counting
    /// in <see cref="PhysicalStorageManager"/>.
    /// </remarks>
    public async Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        string selectSql = $"""
                            SELECT "physicalId" 
                            FROM {FileEntryTable} {FileEntryTableAlias}
                            WHERE {FileEntryTable}."id" = @Id 
                              AND {FileManagerSqlHelper.FilterForSafeDelete};
                            """;
        var physicalId = await _dbConnection.ExecuteScalarAsync<string?>(
            new CommandDefinition(selectSql, new { Id = fileId }, cancellationToken: cancellationToken));
        if (physicalId is null)
        {
            _logger.LogDebug($"File {fileId} not found or already deleted.", fileId);
            throw new KeyNotFoundException($"File '{fileId}' not found or already deleted.");
        }
        string softDeleteSql = $"""
                                 UPDATE {FileEntryTable}
                                 SET    "isDeleted" = TRUE,
                                        "updatedAt" = NOW()
                                 WHERE  "id" = @Id;
                                """;
        await _dbConnection.ExecuteAsync(
            new CommandDefinition(softDeleteSql, new { Id = fileId }, cancellationToken: cancellationToken));
    }
    /// <summary>
    /// Computes the SHA256 hash of a byte array.
    /// </summary>
    /// <param name="data">File data.</param>
    /// <returns>
    /// Hexadecimal string representation of the SHA256 hash.
    /// </returns>
    /// <remarks>
    /// Used to detect duplicate files and enable storage deduplication.
    /// </remarks>
    private static string ComputeSha256(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
    /// <summary>
    /// Builds a relative storage path for a file based on its hash.
    /// </summary>
    /// <param name="hash">SHA256 hash of the file.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>
    /// Relative file path used for storage.
    /// </returns>
    /// <remarks>
    /// The method shards the file path to avoid storing too many files
    /// in a single directory.
    /// 
    /// Example:
    /// hash = "abcd1234..." → "ab/cd/abcd1234.png"
    /// </remarks>
    private static string BuildRelativePath(string hash, string extension)
    {
        var shard1 = hash[..2];
        var shard2 = hash[2..4];
        var fileName = string.IsNullOrEmpty(extension) ? hash : $"{hash}.{extension}";
        return Path.Combine(fileName);
    }
}

/// <summary>
/// Helper class containing reusable SQL fragments used by
/// <see cref="FileManager"/> queries.
/// </summary>
public static class FileManagerSqlHelper
{
    /// <summary>
    /// SQL mapping expression used by Dapper to map database columns
    /// to <see cref="FileDto"/> properties.
    /// </summary>
    public static string MappingColumnToDto =>
        $"""
         {FileManager.FileEntryTableAlias}."id"                             AS ${nameof(FileDto.Id)},
         {FileManager.FileEntryTableAlias}."displayName"                    AS ${nameof(FileDto.DisplayName)},
         {FileManager.FileEntryTableAlias}."extension"                      AS ${nameof(FileDto.Extension)},
         {FileManager.FileEntryTableAlias}."folderId"                       AS ${nameof(FileDto.FolderId)},
         {FileManager.FileEntryTableAlias}."physicalId"                     AS ${nameof(FileDto.PhysicalId)},
         {FileManager.FileEntryTableAlias}."isDeleted"                      AS ${nameof(FileDto.IsDeleted)},
         {FileManager.FileEntryTableAlias}."createdAt"                      AS ${nameof(FileDto.CreatedAt)},
         {FileManager.FileEntryTableAlias}."updatedAt"                      AS ${nameof(FileDto.UploadAt)},
         {PhysicalStorageManager.PhysicalStorageTableAlias}."fileSize"     AS ${nameof(FileDto.FileSize)},
         {PhysicalStorageManager.PhysicalStorageTableAlias}."mimeType"     AS ${nameof(FileDto.MimeType)},
         {PhysicalStorageManager.PhysicalStorageTableAlias}."relativePath" AS ${nameof(FileDto.RelativePath)}
         """;
    /// <summary>
    /// SQL filter ensuring only non-deleted files are returned.
    /// </summary>
    /// <remarks>
    /// Used to prevent soft-deleted files from appearing in queries.
    /// </remarks>
    public static string FilterForSafeDelete => 
        $"""
         {FileManager.FileEntryTableAlias}."isDeleted" = FALSE
         """;
}
