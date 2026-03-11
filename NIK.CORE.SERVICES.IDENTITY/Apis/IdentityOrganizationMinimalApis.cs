using Microsoft.AspNetCore.Routing;

namespace NIK.CORE.SERVICES.IDENTITY.Apis;

public static class IdentityOrganizationMinimalApis
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder IdentityOrganizationApis()
        {
            return endpoints;
        }
    } 
}
