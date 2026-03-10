using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using NIK.CORE.SERVICES.FILE.Dtos;

namespace NIK.CORE.SERVICES.FILE.Services;

/// <summary>
/// Service responsible for managing folder metadata in the database
/// and the corresponding physical folders in the storage system.
/// </summary>
/// <remarks>
/// This service performs operations such as:
/// - Creating folders
/// - Retrieving folder information
/// - Building folder paths
/// - Listing child folders
/// - Renaming folders
///
/// Folder metadata is stored in the database while the actual
/// directory structure is created in the filesystem.
/// </remarks>
public class FolderManager
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<FolderManager> _logger;
    private readonly FileManagerServiceConfig _fileManagerServiceConfig;

    /// <summary>
    /// Name of the folder table in the database.
    /// </summary>
    public static readonly string FolderTable = "Folders";

    /// <summary>
    /// Default SQL alias used for the folder table.
    /// </summary>
    public static readonly string FolderTableAlias = "f";

    /// <summary>
    /// Initializes a new instance of <see cref="FolderManager"/>.
    /// </summary>
    /// <param name="dbConnection">Database connection used for executing queries.</param>
    /// <param name="logger">Logger instance for logging folder operations.</param>
    /// <param name="fileManagerServiceConfig">Configuration containing storage settings.</param>
    public FolderManager(
        IDbConnection dbConnection,
        ILogger<FolderManager> logger,
        FileManagerServiceConfig fileManagerServiceConfig)
    {
        _dbConnection = dbConnection;
        _logger = logger;
        _fileManagerServiceConfig = fileManagerServiceConfig;
    }

    /// <summary>
    /// Creates a new folder in both the filesystem and the database.
    /// </summary>
    /// <param name="parentId">The parent folder identifier. Null if creating a root folder.</param>
    /// <param name="name">The name of the new folder.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="FolderDto"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when folder name is empty.</exception>
    public async Task<FolderDto> CreateFolderAsync(
        string? parentId,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Folder name cannot be empty.");

        string path = string.Empty;

        if (parentId is not null)
        {
            path = await GetPathFromFolderIdAsync(parentId, cancellationToken) ?? string.Empty;
        }

        string folderPath = Path.Combine(_fileManagerServiceConfig.StorageRoot, path, name);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string insertSql = $"""
                           INSERT INTO {FolderTable} AS {FolderTableAlias} ("name", "parentId")
                           VALUES (@name, @parentId)
                           RETURNING {FolderManagerSqlHelper.MappingColumnToDto};
                           """;

        var folder = await _dbConnection.QuerySingleAsync<FolderDto>(
            new CommandDefinition(
                insertSql,
                new { name = name, parentId = parentId },
                cancellationToken: cancellationToken));

        return folder;
    }

    /// <summary>
    /// Retrieves folder information by its identifier.
    /// </summary>
    /// <param name="folderId">The folder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The <see cref="FolderDto"/> if found; otherwise <c>null</c>.
    /// </returns>
    public async Task<FolderDto?> GetFolderByIdAsync(
        string folderId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(folderId);

        string sql = $"""
                     SELECT {FolderManagerSqlHelper.MappingColumnToDto}
                     FROM {FolderTable} {FolderTableAlias}
                     WHERE {FolderTableAlias}."id" = @FolderId;
                     """;

        FolderDto? result = await _dbConnection.QueryFirstOrDefaultAsync<FolderDto>(
            new CommandDefinition(
                sql,
                new { FolderId = folderId },
                cancellationToken: cancellationToken));

        return result;
    }

    /// <summary>
    /// Builds the relative filesystem path for a folder using its ID.
    /// </summary>
    /// <remarks>
    /// This method recursively traverses parent folders in the database
    /// to construct the full folder hierarchy path.
    /// </remarks>
    /// <param name="folderId">The folder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The folder path (e.g. <c>root/child/subchild</c>) or null if not found.
    /// </returns>
    public async Task<string?> GetPathFromFolderIdAsync(
        string folderId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(folderId);

        string sql = $"""
                     WITH RECURSIVE folder_path AS (
                        SELECT id, name, parentId, name::text as path
                        FROM {FolderTable}
                        WHERE id = @FolderId
                        
                        UNION ALL
                        
                        SELECT {FolderTableAlias}.id, {FolderTableAlias}.name,
                               {FolderTableAlias}.parentId,
                               {FolderTableAlias}.name || '/' || fp.path
                        FROM {FolderTable} {FolderTableAlias}
                        JOIN folder_path fp ON fp.parentId = f.id
                     )
                     SELECT path
                     FROM folder_path
                     WHERE parentId IS NULL
                     LIMIT 1;
                     """;

        string? result = await _dbConnection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(
                sql,
                new { FolderId = folderId },
                cancellationToken: cancellationToken));

        return result;
    }

    /// <summary>
    /// Retrieves all child folders of a specified parent folder.
    /// </summary>
    /// <param name="parentId">The parent folder identifier.</param>
    /// <param name="cancellation">Cancellation token.</param>
    /// <returns>A read-only collection of child folders.</returns>
    public async Task<IReadOnlyCollection<FolderDto>> GetChildrenAsync(
        string parentId,
        CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(parentId);

        string sql = $"""
                     SELECT {FolderManagerSqlHelper.MappingColumnToDto}
                     FROM {FolderTable} {FolderTableAlias}
                     WHERE {FolderTableAlias}.parentId = @ParentId
                     ORDER BY {FolderTableAlias}."name";
                     """;

        IEnumerable<FolderDto> results =
            await _dbConnection.QueryAsync<FolderDto>(
                new CommandDefinition(
                    sql,
                    new { ParentId = parentId },
                    cancellationToken: cancellation));

        return results.AsList();
    }

    /// <summary>
    /// Renames an existing folder.
    /// </summary>
    /// <param name="folderId">The folder identifier.</param>
    /// <param name="newName">The new folder name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated folder information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when folder is not found.</exception>
    public async Task<FolderDto> RenameFolderAsync(
        string folderId,
        string newName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(folderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        FolderDto? folder = await GetFolderByIdAsync(folderId, cancellationToken);

        if (folder is null)
        {
            throw new ArgumentNullException(
                nameof(folder),
                $"Not found folder to rename function with folder id: {folderId}");
        }

        folder.Name = newName;

        string sqlRename = $"""
                           UPDATE {FolderTable}
                           SET "name" = @Name
                           WHERE "id" = @Id;
                           """;

        await _dbConnection.ExecuteAsync(
            new CommandDefinition(
                sqlRename,
                new { Name = newName, Id = folderId },
                cancellationToken: cancellationToken));

        return folder;
    }
}

/// <summary>
/// Helper class containing SQL mapping utilities for <see cref="FolderManager"/>.
/// </summary>
public static class FolderManagerSqlHelper
{
    /// <summary>
    /// Column mapping used to convert database fields into <see cref="FolderDto"/>.
    /// </summary>
    public static string MappingColumnToDto => $"""
        {FolderManager.FolderTableAlias}."id" = {nameof(FolderDto.Id)},
        {FolderManager.FolderTableAlias}."name" = {nameof(FolderDto.Name)},
        {FolderManager.FolderTableAlias}."parentId" = {nameof(FolderDto.ParentId)},
        {FolderManager.FolderTableAlias}."createdAt" = {nameof(FolderDto.CreatedAt)},
        {FolderManager.FolderTableAlias}."updatedAt" = {nameof(FolderDto.UpdatedAt)}
        """;
}
