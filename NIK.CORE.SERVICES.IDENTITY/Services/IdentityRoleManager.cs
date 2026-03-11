using System.Data;
using Microsoft.Extensions.Logging;

namespace NIK.CORE.SERVICES.IDENTITY.Services;

public class IdentityRoleManager
{
    private readonly IDbConnection _connection;
    private readonly ILogger<IdentityRoleManager> _logger;

    public IdentityRoleManager(IDbConnection connection, ILogger<IdentityRoleManager> logger)
    {
        _connection = connection;
        _logger = logger;
    }
}
