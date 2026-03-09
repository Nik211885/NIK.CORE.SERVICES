using System.Data;
using Microsoft.Extensions.Logging;

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
}

public static class FolderManagerSqlHelper
{
    public static string MappingColumnToDto = string.Empty;
}