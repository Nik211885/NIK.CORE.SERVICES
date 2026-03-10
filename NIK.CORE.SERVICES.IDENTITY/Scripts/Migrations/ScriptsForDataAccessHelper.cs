using System.Data;
using NIK.CORE.SERVICES.SHARED.Extensions;

namespace NIK.CORE.SERVICES.IDENTITY.Scripts.Migrations;

/// <summary>
/// 
/// </summary>
/// <summary>
/// Provides helper methods for executing database initialization
/// and migration scripts used by the data access layer.
/// </summary>
/// <remarks>
/// This helper is typically used during application startup to ensure
/// that the required database structure is created or updated.
/// </remarks>
internal static class ScriptsForDataAccessHelper
{
    /// <summary>
    /// Initializes the database structure by executing the initial
    /// migration SQL script.
    /// </summary>
    /// <param name="dbConnection">
    /// The database connection used to execute the SQL script.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous initialization operation.
    /// </returns>
    /// <remarks>
    /// This method runs the <c>initial_structure.sql</c> script which
    /// creates the required database tables, indexes, and constraints
    /// if they do not already exist.
    ///
    /// The script is expected to be located at:
    /// <c>./Scripts/Migrations/initial_structure.sql</c>.
    /// 
    /// This method is commonly invoked during application startup
    /// to ensure the database schema is properly initialized.
    /// </remarks>
    internal static async Task InitializeDatabase(this IDbConnection dbConnection)
    {
        await dbConnection.ExecuteScriptFromFileAsync("./Scripts/Migrations/initial_structure.sql");
    }
}
