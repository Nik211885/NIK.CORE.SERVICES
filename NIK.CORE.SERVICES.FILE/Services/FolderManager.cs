using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using NIK.CORE.SERVICES.FILE.Dtos;

namespace NIK.CORE.SERVICES.FILE.Services;
/// <summary>
/// 
/// </summary>
public class FolderManager
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<FolderManager> _logger;
    public static readonly string FolderTable = "Folders";
    public static readonly string FolderTableAlias = "f";
    
    public FolderManager(IDbConnection dbConnection, ILogger<FolderManager> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<FolderDto?> GetFolderByIdAsync(string folderId, CancellationToken cancellationToken = default)
    {
        string sql = $"""
                     SELECT {FolderManagerSqlHelper.MappingColumnToDto}
                     FROM {FolderTable}{FolderTableAlias}
                     WHERE {FolderTableAlias}."id" = @FolderId;
                     """;
        FolderDto? result = await _dbConnection.QueryFirstOrDefaultAsync<FolderDto>(new CommandDefinition(sql, new
        {
            FolderId = folderId
        }, cancellationToken: cancellationToken));
        return result;
    }

    public async Task<string?> GetPathFromFolderIdAsync(string folderId, CancellationToken cancellationToken = default)
    {
        string sql = $"""
                     WITH RECURSIVE folder_path AS (
                        SELECT id, name, parentId,name::text as path
                        FROM {FolderTable}
                        WHERE id = @FolderId
                        
                        UNION ALL
                        
                        SELECT {FolderTableAlias}.id, {FolderTableAlias}.name,
                                {FolderTableAlias}.parentId, {FolderTableAlias}.name || '/' || fp.path
                        FROM  {FolderTable} {FolderTableAlias}
                        JOIN folder_path fp ON fp.parentId = f.id 
                     )
                     SELECT path FROM folder_path
                     WHERE parentId IS NULL
                     LIMIT 1;
                     """;
        string? result = await _dbConnection.QueryFirstOrDefaultAsync<string>(new CommandDefinition(sql, new
        {
            FolderId = folderId
        }, cancellationToken: cancellationToken));
        return result;
    }
}

public static class FolderManagerSqlHelper
{
    public static string MappingColumnToDto => $"""
                                              {FolderManager.FolderTableAlias}."id" = {nameof(FolderDto.Id)},
                                              {FolderManager.FolderTableAlias}."name" = {nameof(FolderDto.Name)},
                                              {FolderManager.FolderTableAlias}."parentId" = {nameof(FolderDto.ParentId)},
                                              {FolderManager.FolderTableAlias}."createdAt" = {nameof(FolderDto.CreatedAt)},
                                              {FolderManager.FolderTableAlias}."updatedAt" = {nameof(FolderDto.UpdatedAt)}
                                              """;
}
