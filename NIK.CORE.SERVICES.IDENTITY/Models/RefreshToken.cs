namespace NIK.CORE.SERVICES.IDENTITY.Models;

/// <summary>
/// Refresh token used to issue new access tokens.
/// Stored in cache or database depending on security policy.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Refresh token value (should be hashed before storing).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// User who owns the token.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Device or client identifier.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Time token was issued.
    /// </summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// Token expiration time.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indicates token was revoked (logout or rotation).
    /// </summary>
    public bool IsRevoked { get; set; }
}
