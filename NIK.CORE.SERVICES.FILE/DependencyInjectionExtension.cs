using System.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NIK.CORE.SERVICES.FILE.Apis;
using NIK.CORE.SERVICES.FILE.Scripts.Migrations;
using NIK.CORE.SERVICES.FILE.Services;
using Npgsql;

namespace NIK.CORE.SERVICES.FILE;
/// <summary>
/// 
/// </summary>
public static class DependencyInjectionExtension
{
    extension(IServiceCollection collection)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="endpoints"></param>
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
/// 
/// </summary>
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
