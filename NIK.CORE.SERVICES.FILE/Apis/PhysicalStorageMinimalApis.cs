using Microsoft.AspNetCore.Routing;

namespace NIK.CORE.SERVICES.FILE.Apis;

public static class PhysicalStorageMinimalApis
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder AddPhysicalStorageManagerEndpoint()
        {
            return endpoints;
        }
    } 
}