using System.Data;
using Microsoft.Extensions.Logging;

namespace NIK.CORE.SERVICES.FILE.Services;
/// <summary>
/// 
/// </summary>
public class FileShareManager
{
    private readonly ILogger<FileShareManager> _logger;
    private readonly IDbConnection _dbConnection;
    public static readonly string FileShareTable = "FileShares";
    public static readonly string FileShareTableAlias = "fs";


    public FileShareManager(ILogger<FileShareManager> logger, IDbConnection dbConnection)
    {
        _logger = logger;
        _dbConnection = dbConnection;
    }
}

public static class FileShareSqlHelper
{
    public static string MappingColumnToDto = string.Empty;
}