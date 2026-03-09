using System.Data;
using Dapper;

namespace NIK.CORE.SERVICES.SHARED.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IDbConnection"/> to execute SQL scripts.
/// </summary>
public static class DataAccessExtensions
{
    extension(IDbConnection connection)
    {
        /// <summary>
        /// Executes a SQL script from a specified file asynchronously within a database transaction.
        /// </summary>
        /// <param name="fileName">
        /// The full path to the SQL script file that will be executed.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the specified SQL script file does not exist.
        /// </exception>
        /// <exception cref="Exception">
        /// Rethrows any exception that occurs during script execution after rolling back the transaction.
        /// </exception>
        public async Task ExecuteScriptFromFileAsync(string fileName)
        {
            var rootPath = Directory.GetCurrentDirectory();
            fileName =  Path.Combine(rootPath, fileName);
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"SQL script file not found: {fileName}");
            }
            var text = await File.ReadAllTextAsync(fileName);
            if (connection.State != ConnectionState.Open)
                connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(text, transaction: transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}