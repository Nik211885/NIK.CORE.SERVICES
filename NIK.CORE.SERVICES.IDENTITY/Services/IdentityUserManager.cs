using System.Data;
using Microsoft.Extensions.Logging;

namespace NIK.CORE.SERVICES.IDENTITY.Services;

public class IdentityUserManager
{
    private readonly IDbConnection _connection;
    private readonly ILogger<IdentityUserManager> _logger;

    public IdentityUserManager(IDbConnection connection, ILogger<IdentityUserManager> logger)
    {
        _connection = connection;
        _logger = logger;
    }
}
