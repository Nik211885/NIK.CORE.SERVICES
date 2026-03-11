using Microsoft.AspNetCore.Routing;

namespace NIK.CORE.SERVICES.IDENTITY.Apis;

public static class IdentityUserMinimalApis
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder IdentityUserApis()
        {
            return endpoints;
        }
    } 
}
