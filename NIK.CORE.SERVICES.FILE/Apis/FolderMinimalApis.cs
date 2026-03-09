using Microsoft.AspNetCore.Routing;

namespace NIK.CORE.SERVICES.FILE.Apis;

public static class FolderMinimalApis
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder AddFolderManagerEndpoint()
        {
            return endpoints;
        }
    } 
}