using System.Data;
using NIK.CORE.SERVICES.SHARED.Extensions;

namespace NIK.CORE.SERVICES.FILE.Scripts.Migrations;
/// <summary>
/// 
/// </summary>
internal static class ScriptsForDataAccessHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbConnection"></param>
    internal static async Task InitializeDatabase(this IDbConnection dbConnection)
    {
        await dbConnection.ExecuteScriptFromFileAsync("./Scripts/Migrations/initial_structure.sql");
    }
}