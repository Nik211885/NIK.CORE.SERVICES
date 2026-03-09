namespace NIK.CORE.SERVICES.FILE.Apis;
using Microsoft.AspNetCore.Routing;

public static class FileMinimalApis
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder AddFileManagerEndpoint()
        {
            return endpoints;
        }
    } 
}