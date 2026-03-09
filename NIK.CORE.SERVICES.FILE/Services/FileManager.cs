using System.Data;
using System.Security.Cryptography;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NIK.CORE.SERVICES.FILE.Dtos;

namespace NIK.CORE.SERVICES.FILE.Services;
/// <summary>
/// 
/// </summary>
public class FileManager
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<FolderManager> _logger;
    public static readonly string FileEntryTable = "FileEntry";
    public static readonly string FileEntryTableAlias = "fe";
    private readonly FileManagerServiceConfig _fileManagerServiceConfig;
    private readonly PhysicalStorageManager _physicalStorageManager;

    public FileManager(IDbConnection dbConnection, ILogger<FolderManager> logger, FileManagerServiceConfig fileManagerServiceConfig, PhysicalStorageManager physicalStorageManager)
    {
        _dbConnection = dbConnection;
        _logger = logger;
        _fileManagerServiceConfig = fileManagerServiceConfig;
        _physicalStorageManager = physicalStorageManager;
    }

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
        throw new Exception();

    }
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

    public async Task<string> GetFileBase64ASync(string fileId, CancellationToken cancellationToken = default)
    {
        await using var stream = await OpenReadStreamAsync(fileId, cancellationToken);
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var bytes = memory.ToArray();
        return Convert.ToBase64String(bytes);
    }
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
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static string ComputeSha256(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
    /// <summary>
    /// Builds a sharded relative path to avoid too many files in one directory.
    /// e.g. hash = "abcd1234..." → "ab/cd/abcd1234....png"
    /// </summary>
    private static string BuildRelativePath(string hash, string extension)
    {
        var shard1 = hash[..2];
        var shard2 = hash[2..4];
        var fileName = string.IsNullOrEmpty(extension) ? hash : $"{hash}.{extension}";
        return Path.Combine(shard1, shard2, fileName);
    }
}


public static class FileManagerSqlHelper
{
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
         {PhysicalStorageManager.PhysicalStorageTableAlias}."relativePath" AS ${nameof(FileDto.RelativePath)},
         """;

    public static string FilterForSafeDelete => 
        $"""
         {FileManager.FileEntryTableAlias}."isDeleted" = FALSE
         """;
}