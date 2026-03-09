using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using NIK.CORE.SERVICES.FILE.Dtos;
using NIK.CORE.SERVICES.FILE.Models;

namespace NIK.CORE.SERVICES.FILE.Services;

/// <summary>
/// 
/// </summary>
public class PhysicalStorageManager
{
    private readonly IDbConnection _connection;
    public static readonly string PhysicalStorageTable = "PhysicalStorage";
    public static readonly string PhysicalStorageTableAlias = "ps";
    private readonly ILogger<PhysicalStorageManager> _logger;

    public PhysicalStorageManager(IDbConnection connection, ILogger<PhysicalStorageManager> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<PhysicalStorageDto?> GetByHashFileAsync(string fileHash, CancellationToken cancellation = default)
    {
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

    public async Task<PhysicalStorageDto?> GetByIdAsync(string id, CancellationToken cancellation = default)
    {
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

    public async Task<long> GetTotalStorageSizeAsync(CancellationToken cancellation = default)
    {
        var sql = $"""
                  SELECT COALESCE(SUM(COALESCE("fileSize", 0)), 0)
                  FROM ${PhysicalStorageTable};
                  """;
        _logger.LogDebug("Get total storage size");
        var result = await _connection.QueryFirstOrDefaultAsync<long>(
            new CommandDefinition(sql, cancellationToken: cancellation));
        return result;
    }

    public async Task<(PhysicalStorageDto, bool isNew)> GetOrCreateAsync(CreatePhysicalStorageRequest request 
        , CancellationToken cancellationToken = default)
    {
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
                        {PhysicalStorageSqlHelper.MappingColumnToDto}
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


public static class PhysicalStorageSqlHelper
{
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