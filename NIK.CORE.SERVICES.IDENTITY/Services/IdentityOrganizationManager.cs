using System.Data;
using Microsoft.Extensions.Logging;

namespace NIK.CORE.SERVICES.IDENTITY.Services;

public class IdentityOrganizationManager
{
    private readonly ILogger<IdentityOrganizationManager> _logger;
    private  readonly IDbConnection _connection;

    public IdentityOrganizationManager(ILogger<IdentityOrganizationManager> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }
}
