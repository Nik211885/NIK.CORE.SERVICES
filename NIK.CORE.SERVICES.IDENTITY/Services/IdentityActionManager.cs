using System.Data;
using Microsoft.Extensions.Logging;

namespace NIK.CORE.SERVICES.IDENTITY.Services;

public class IdentityActionManager
{
    private readonly ILogger<IdentityFunctionManager> _logger;
    private readonly IDbConnection _connection;

    public IdentityActionManager(ILogger<IdentityFunctionManager> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }
}
