using System.Data;
using Microsoft.Extensions.Logging;

namespace NIK.CORE.SERVICES.IDENTITY.Services;

public class IdentityModuleManager
{   
    private readonly ILogger<IdentityModuleManager> _logger;
    private readonly IDbConnection _connection;

    public IdentityModuleManager(ILogger<IdentityModuleManager> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }
}
