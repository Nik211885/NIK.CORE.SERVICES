using Microsoft.AspNetCore.Routing;

namespace NIK.CORE.SERVICES.IDENTITY.Apis;

public static class IdentityFunctionMinimalApis
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder IdentityFunctionApis()
        {
            return endpoints;
        }
    } 
}
