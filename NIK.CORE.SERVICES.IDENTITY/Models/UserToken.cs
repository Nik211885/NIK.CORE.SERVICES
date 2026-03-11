namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class UserToken
{
    /// <summary>
    /// Unique identifier of the token
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the user associated with the token
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Login provider related to the token (e.g., System, Google)
    /// </summary>
    public string LoginProvider { get; set; }

    /// <summary>
    /// Token name indicating its purpose (e.g., EmailConfirmation, PasswordReset, 2FA)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Token value
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Expiration time of the token
    /// </summary>
    public DateTimeOffset? ExpiredAt { get; set; }

    /// <summary>
    /// Indicates whether the token has been used
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// The time when the token was used
    /// </summary>
    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>
    /// The time when the token was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
