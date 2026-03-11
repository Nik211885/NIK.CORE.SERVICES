using Microsoft.AspNetCore.Routing;

namespace NIK.CORE.SERVICES.IDENTITY.Apis;

public static class IdentityRoleMinimalApis
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder IdentityRoleApis()
        {
            return endpoints;
        }
    } 
}
