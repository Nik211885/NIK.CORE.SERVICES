using System.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NIK.CORE.SERVICES.FILE.Apis;
using NIK.CORE.SERVICES.FILE.Scripts.Migrations;
using NIK.CORE.SERVICES.FILE.Services;
using Npgsql;

namespace NIK.CORE.SERVICES.FILE;
/// <summary>
/// Provides extension methods for registering File Manager services
/// and API endpoints into the ASP.NET dependency injection container
/// and routing pipeline.
/// </summary>
/// <remarks>
/// This class centralizes the configuration required to enable the
/// file management module, including:
/// 
/// - Dependency injection registrations
/// - Database connection setup
/// - Database initialization
/// - API endpoint mappings
/// </remarks>
public static class DependencyInjectionExtension
{
    extension(IServiceCollection collection)
    {
        /// <summary>
        /// Registers all services required for the File Manager module.
        /// </summary>
        /// <param name="config">
        /// Configuration delegate used to configure <see cref="FileManagerServiceConfig"/>.
        /// </param>
        /// <returns>
        /// The updated <see cref="IServiceCollection"/> instance.
        /// </returns>
        /// <remarks>
        /// This method performs the following actions:
        /// 
        /// 1. Configures <see cref="FileManagerServiceConfig"/>.
        /// 2. Registers the PostgreSQL database connection.
        /// 3. Registers all file management related services:
        ///    - <see cref="FileManager"/>
        ///    - <see cref="FolderManager"/>
        ///    - <see cref="FileShareManager"/>
        ///    - <see cref="PhysicalStorageManager"/>
        /// 4. Initializes the database by executing migration scripts.
        /// 
        /// The database initialization is executed in a background task.
        /// </remarks>
        public IServiceCollection AddFileManagerServices(Action<FileManagerServiceConfig> config)
        {
            ArgumentNullException.ThrowIfNull(config);
            
            var dataConfig = new FileManagerServiceConfig();
            config.Invoke(dataConfig);
            ArgumentNullException.ThrowIfNull(dataConfig.ConnectionString);
            collection.AddSingleton(dataConfig);
            var npgsqlConnection = new NpgsqlConnection(dataConfig.ConnectionString);
            collection.AddScoped<IDbConnection>(x=>npgsqlConnection);
            collection.AddScoped<FileManager>();
            collection.AddScoped<FolderManager>();
            collection.AddScoped<FileShareManager>();
            collection.AddScoped<PhysicalStorageManager>();
            // if just send task for thread pool ot will don't throw exception when catch error, 
            Task.Run(async () =>
            {
                await npgsqlConnection.InitializeDatabase();
            });
            return collection;
        }
    }
    /// <summary>
    /// Registers all File Manager related API endpoints.
    /// </summary>
    /// <returns>
    /// The updated <see cref="IEndpointRouteBuilder"/> instance.
    /// </returns>
    /// <remarks>
    /// This method maps the API endpoints for:
    /// 
    /// - File management
    /// - Folder management
    /// - File sharing
    /// - Physical storage management
    /// 
    /// Each API group is registered through its corresponding endpoint extension.
    /// </remarks>
    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEndpointRouteBuilder AddFileManagerApi()
        {
            endpoints.AddFileManagerEndpoint();
            endpoints.AddFolderManagerEndpoint();
            endpoints.AddFileShareEndpoint();
            endpoints.AddPhysicalStorageManagerEndpoint();
            return endpoints;
        }
    }
}

/// <summary>
/// Configuration options for the File Manager service.
/// </summary>
/// <remarks>
/// This configuration controls how files are stored and validated,
/// including storage location, maximum file size, and allowed MIME types.
/// 
/// The configuration is typically registered during application startup
/// through <see cref="DependencyInjectionExtension.AddFileManagerServices"/>.
/// </remarks>
public class FileManagerServiceConfig
{
    /// <summary>
    ///  Connection string for services upload images
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    /// <summary>Root directory on disk where physical files are stored.</summary>
    public string StorageRoot { get; set; } = "uploads";
    /// <summary>Max allowed file size in bytes. Default 100 MB.</summary>
    public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024;
    /// <summary>Allowed MIME types. Empty = allow all.</summary>
    public HashSet<string> AllowedMimeTypes { get; set; } =
    [
        // Images
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp",
        "image/svg+xml",

        // Documents
        "application/pdf",
        "text/plain",

        // Microsoft Office
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",

        // Archives
        "application/zip",
        "application/x-rar-compressed",
        "application/x-7z-compressed",

        // Media
        "video/mp4",
        "audio/mpeg"
    ];
}
