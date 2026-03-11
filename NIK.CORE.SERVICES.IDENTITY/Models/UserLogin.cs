namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class UserLogin
{
    /// <summary>
    /// The login provider (e.g., Google, Facebook, Microsoft)
    /// </summary>
    public string LoginProvider { get; set; }

    /// <summary>
    /// The unique key provided by the login provider
    /// </summary>
    public string ProviderKey { get; set; }

    /// <summary>
    /// Display name of the login provider
    /// </summary>
    public string? ProviderDisplayName { get; set; }

    /// <summary>
    /// Identifier of the user associated with this login
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The time when this external login was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
