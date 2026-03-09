using Microsoft.AspNetCore.Routing;

namespace NIK.CORE.SERVICES.FILE.Apis;

public static class FileShareMinimalApis
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder AddFileShareEndpoint()
        {
            return endpoints;
        }
    }
}