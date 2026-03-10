using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using NIK.CORE.SERVICES.FILE.Dtos;

namespace NIK.CORE.SERVICES.FILE.Services;

/// <summary>
/// Manages physical file storage metadata in the database.
/// 
/// This service provides operations for:
/// - Retrieving physical storage records
/// - Creating new physical storage entries
/// - Managing reference counting for deduplicated files
/// - Deleting storage records when no references remain
/// - Listing stored files with pagination
/// - Calculating total storage usage
/// 
/// The class uses Dapper for database access and supports
/// file deduplication through the <c>fileHash</c> field.
/// </summary>
public class PhysicalStorageManager
{
    private readonly IDbConnection _connection;
    public static readonly string PhysicalStorageTable = "PhysicalStorage";
    public static readonly string PhysicalStorageTableAlias = "ps";
    private readonly ILogger<PhysicalStorageManager> _logger;
    /// <summary>
    /// Initializes a new instance of <see cref="PhysicalStorageManager"/>.
    /// </summary>
    /// <param name="connection">Database connection used for executing queries.</param>
    /// <param name="logger">Logger instance for diagnostics and tracing.</param>
    public PhysicalStorageManager(IDbConnection connection, ILogger<PhysicalStorageManager> logger)
    {
        _connection = connection;
        _logger = logger;
    }
    /// <summary>
    /// Retrieves a physical storage record by its file hash.
    /// </summary>
    /// <param name="fileHash">The hash value of the file.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// A <see cref="PhysicalStorageDto"/> if found; otherwise <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method is typically used for file deduplication to determine
    /// whether a physical file already exists in storage.
    /// </remarks>
    public async Task<PhysicalStorageDto?> GetByHashFileAsync(string fileHash, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(fileHash);

        string sql = $"""
                       SELECT {PhysicalStorageSqlHelper.MappingColumnToDto},
                       FROM {PhysicalStorageTable} {PhysicalStorageTableAlias}     
                       WHERE {PhysicalStorageTableAlias}."FileHash = @FileHash"
                       LIMIT 1;
                       """;
        _logger.LogDebug("Locking up physical storage by hash {hash}", fileHash);
        PhysicalStorageDto? result = await _connection.QueryFirstOrDefaultAsync<PhysicalStorageDto>(
                new CommandDefinition(sql, new {FileHash =  fileHash}, cancellationToken: cancellation)
                );
        return result;
    }
    /// <summary>
    /// Retrieves a physical storage record by its unique identifier.
    /// </summary>
    /// <param name="id">The identifier of the physical storage record.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// A <see cref="PhysicalStorageDto"/> if found; otherwise <c>null</c>.
    /// </returns>
    public async Task<PhysicalStorageDto?> GetByIdAsync(string id, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        string sql = $"""
                     SELECT {PhysicalStorageSqlHelper.MappingColumnToDto},
                     FROM {PhysicalStorageTable} {PhysicalStorageTableAlias}
                     WHERE {PhysicalStorageTableAlias}."Id = @Id"
                     LIMIT 1;
                     """;
        _logger.LogDebug("Locking up physical storage by id {id}", id);
        PhysicalStorageDto? result = await _connection.QueryFirstOrDefaultAsync<PhysicalStorageDto>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellation));
        return result;
    }
    /// <summary>
    /// Retrieves a paginated list of physical storage records.
    /// </summary>
    /// <param name="mimeTypePrefix">
    /// Optional MIME type prefix used for filtering results.
    /// For example: <c>image</c> will return image files.
    /// </param>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Maximum number of records to return.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// A read-only list of <see cref="PhysicalStorageDto"/> objects.
    /// </returns>
    /// <remarks>
    /// Results are ordered by creation date in descending order.
    /// </remarks>
    public async Task<IReadOnlyList<PhysicalStorageDto>> GetAllAsync(string? mimeTypePrefix = null,
        int skip = 0,int take = 50, CancellationToken cancellation = default
        )
    {
        string sql = $"""
                  SELECT {PhysicalStorageSqlHelper.MappingColumnToDto},
                  FROM {PhysicalStorageTable} {PhysicalStorageTableAlias}
                  WHERE (@MimeTypePrefix IS NULL OR {PhysicalStorageTableAlias}."mimeType" LIKE @MimeTypePrefix || '%')
                  ORDER BY {PhysicalStorageTableAlias}."createdAt" DESC
                  OFFSET @Skip
                  LIMIT  @Take;
                  """;
        _logger.LogDebug("Get list of all file with pagination skip {Skip}, and take count {Take}", skip, take);
        IEnumerable<PhysicalStorageDto> result = await _connection.QueryAsync<PhysicalStorageDto>(
            new CommandDefinition(sql,
                new
                {
                    MimeTypePrefix = mimeTypePrefix,
                    Skip = skip,
                    Take = take
                }, cancellationToken: cancellation));
        return result.AsList();
    }
    /// <summary>
    /// Calculates the total size of all stored physical files.
    /// </summary>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>
    /// The total storage size in bytes.
    /// </returns>
    public async Task<long> GetTotalStorageSizeAsync(CancellationToken cancellation = default)
    {
        var sql = $"""
                  SELECT COALESCE(SUM(COALESCE("fileSize", 0)), 0)
                  FROM {PhysicalStorageTable};
                  """;
        _logger.LogDebug("Get total storage size");
        var result = await _connection.QueryFirstOrDefaultAsync<long>(
            new CommandDefinition(sql, cancellationToken: cancellation));
        return result;
    }
    /// <summary>
    /// Increments the reference count of a physical storage record.
    /// </summary>
    /// <param name="id">The identifier of the physical storage record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the storage record does not exist.
    /// </exception>
    /// <remarks>
    /// This method is used when a new logical file references the same
    /// physical storage file.
    /// </remarks>
    public async Task IncrementRefCountAsync(string id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        string sql = $"""
                       UPDATE {PhysicalStorageTable}
                       SET    "refCount" = "refCount" + 1
                       WHERE  "id" = @Id;
                       """;
        var affected = await _connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));

        if (affected == 0)
            throw new KeyNotFoundException($"PhysicalStorage record '{id}' not found.");

        _logger.LogDebug("Incremented refCount for physical storage {Id}", id);
    }
    /// <summary>
    /// Decrements the reference count of a physical storage record.
    /// </summary>
    /// <param name="id">The identifier of the physical storage record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>true</c> if the record was deleted because the reference count reached zero;
    /// otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// When the reference count becomes zero, the storage record is automatically deleted.
    /// </remarks>
    public async Task<bool> DecrementRefCountAsync(string id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        // Decrement; if it reaches 0 delete in same round-trip
        string decrementSql = $"""
                                UPDATE {PhysicalStorageTable}
                                SET    "refCount" = "refCount" - 1
                                WHERE  "id" = @Id
                                  AND  "refCount" > 0
                                RETURNING "refCount" AS RefCount
                                """;
        
        var newRefCount = await _connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(decrementSql, new { Id = id }, cancellationToken: ct));

        if (newRefCount is null)
        {
            _logger.LogWarning(
                "DecrementRefCount: physical storage {Id} not found or refCount already 0", id);
            return false;
        }

        _logger.LogDebug("Physical storage {Id} refCount is now {RefCount}", id, newRefCount);

        if (newRefCount == 0)
        {
            _logger.LogInformation(
                "Physical storage {Id} has no more references – deleting record", id);
            await DeleteAsync(id, ct);
            return true;
        }

        return false;
    }
    /// <summary>
    /// Deletes a physical storage record.
    /// </summary>
    /// <param name="id">The identifier of the storage record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the storage record does not exist.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the storage record still has references.
    /// </exception>
    /// <remarks>
    /// A record can only be deleted when <c>refCount</c> equals zero.
    /// </remarks>
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        string checkSql = $"""
                        SELECT "refCount" FROM {PhysicalStorageTable} WHERE "id" = @Id
                        """;
        var refCount = await _connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(checkSql, new { Id = id }, cancellationToken: ct));

        if (refCount is null)
            throw new KeyNotFoundException($"PhysicalStorage record '{id}' not found.");

        if (refCount > 0)
            throw new InvalidOperationException(
                $"Cannot delete PhysicalStorage '{id}': it still has {refCount} reference(s).");

        string deleteSql = $"""
                          DELETE FROM {PhysicalStorageTable} WHERE "id" = @Id
                          """;
        await _connection.ExecuteAsync(
            new CommandDefinition(deleteSql, new { Id = id }, cancellationToken: ct));

        _logger.LogInformation("Deleted physical storage record {Id}", id);
    }
    /// <summary>
    /// Retrieves an existing physical storage record by file hash
    /// or creates a new one if it does not exist.
    /// </summary>
    /// <param name="request">Request containing file metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item>
    /// <description>The physical storage DTO.</description>
    /// </item>
    /// <item>
    /// <description>A boolean indicating whether the record was newly created.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method implements file deduplication by checking the file hash
    /// before inserting a new record.
    /// </remarks>
    public async Task<(PhysicalStorageDto, bool isNew)> GetOrCreateAsync(CreatePhysicalStorageRequest request 
        , CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await GetByHashFileAsync(request.FileHash, cancellationToken);
        if (existing is not null)
        {
            _logger.LogDebug(
                "Deduplication hit for hash {Hash} → existing physical id {Id}",
                request.FileHash, existing.Id);
            return (existing, false);
        }
        string sql = $"""
                     INSERT INTO {PhysicalStorageTable} AS {PhysicalStorageTableAlias} ("fileHash", "fileSize", "relativePath", "mimeType")
                     VALUES (@FileHash, @FileSize, @relativePath, @mimeType)
                     RETURNING
                        {PhysicalStorageSqlHelper.MappingColumnToDto};
                     """;
        _logger.LogInformation(
            "Creating new physical storage record for hash {Hash} ({Size} bytes)",
            request.FileHash, request.FileSize);
        
        var created = await _connection.QuerySingleAsync<PhysicalStorageDto>(
            new CommandDefinition(sql,
                new
                {
                    request.FileHash,
                    request.FileSize,
                    request.RelativePath,
                    request.MimeType
                },
                cancellationToken: cancellationToken));
        return (created, true);
    }
}

/// <summary>
/// Helper class containing reusable SQL fragments for
/// mapping database columns to <see cref="PhysicalStorageDto"/>.
/// </summary>
public static class PhysicalStorageSqlHelper
{
    /// <summary>
    /// SQL column mapping used by Dapper to map database fields
    /// to the <see cref="PhysicalStorageDto"/> properties.
    /// </summary>
    public static string MappingColumnToDto 
        => $"""
            {PhysicalStorageManager.PhysicalStorageTableAlias}."id"             AS ${nameof(PhysicalStorageDto.Id)},
            {PhysicalStorageManager.PhysicalStorageTableAlias}."fileHash"       AS ${nameof(PhysicalStorageDto.FileHash)},
            {PhysicalStorageManager.PhysicalStorageTableAlias}."fileSize"       AS ${nameof(PhysicalStorageDto.FileSize)},
            {PhysicalStorageManager.PhysicalStorageTableAlias}."relativePath"   AS ${nameof(PhysicalStorageDto.RelativePath)},
            {PhysicalStorageManager.PhysicalStorageTableAlias}."mimeType"       AS ${nameof(PhysicalStorageDto.MimeType)},
            {PhysicalStorageManager.PhysicalStorageTableAlias}."refCount"       AS ${nameof(PhysicalStorageDto.RefCount)},
            {PhysicalStorageManager.PhysicalStorageTableAlias}."createdAt"      AS ${nameof(PhysicalStorageDto.CreatedAt)}
            """;
}
