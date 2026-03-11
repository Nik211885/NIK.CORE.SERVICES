using System.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NIK.CORE.SERVICES.IDENTITY.Apis;
using NIK.CORE.SERVICES.IDENTITY.Scripts.Migrations;
using NIK.CORE.SERVICES.IDENTITY.Services;
using Npgsql;

namespace NIK.CORE.SERVICES.IDENTITY;

public static class DependencyInjectionExtension
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection AddIdentityServices(Action<IdentityConfig> identityConfig)
        {
            ArgumentNullException.ThrowIfNull(identityConfig);
            
            var identityConfigs = new IdentityConfig();
            identityConfig.Invoke(identityConfigs);
            
            ArgumentNullException.ThrowIfNull(identityConfigs.ConnectionString);
            var npgsqlConnection = new NpgsqlConnection(identityConfigs.ConnectionString);
            
            collection.AddScoped<IDbConnection>(x=>npgsqlConnection);
            collection.AddSingleton(identityConfigs);
            collection.AddScoped<IdentityActionManager>();
            collection.AddScoped<IdentityFunctionManager>();
            collection.AddScoped<IdentityModuleManager>();
            collection.AddScoped<IdentityOrganizationManager>();
            collection.AddScoped<IdentityRoleManager>();
            collection.AddScoped<IdentityUserManager>();
            
            // hold exception because it different thread in thread pool
            Task.Run(async () =>
            {
                await npgsqlConnection.InitializeDatabase();
            });
            return collection;
        }
    }

    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder AddIdentityApi()
        {
            endpoints.IdentityActionApis();
            endpoints.IdentityFunctionApis();
            endpoints.IdentityModuleApis();
            endpoints.IdentityOrganizationApis();
            endpoints.IdentityRoleApis();
            endpoints.IdentityUserApis();
            return endpoints;
        }
    }
}
/// <summary>
/// 
/// </summary>
public class IdentityConfig
{
    /// <summary>
    /// /
    /// </summary>
    public string ConnectionString { get; set; }
}
