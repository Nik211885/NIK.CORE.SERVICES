using Microsoft.AspNetCore.Routing;

namespace NIK.CORE.SERVICES.IDENTITY.Apis;

public static class IdentityModuleMinimalApis
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder IdentityModuleApis()
        {
            return endpoints;
        }
    } 
}
