using System.Data;
using Microsoft.Extensions.Logging;

namespace NIK.CORE.SERVICES.IDENTITY.Services;

public class IdentityFunctionManager
{
    private readonly ILogger<IdentityFunctionManager> _logger;
    private readonly IDbConnection _connection;

    public IdentityFunctionManager(ILogger<IdentityFunctionManager> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }
}
