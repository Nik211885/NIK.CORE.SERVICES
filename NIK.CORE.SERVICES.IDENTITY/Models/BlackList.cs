namespace NIK.CORE.SERVICES.IDENTITY.Models;

/// <summary>
/// Blacklist entry for revoked access tokens.
/// Stored in cache with TTL until token expiration.
/// </summary>
public class BlackList
{
    /// <summary>
    /// JWT unique identifier (jti claim).
    /// </summary>
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    /// User who owned the token.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Time token was revoked.
    /// </summary>
    public DateTime RevokedAt { get; set; }

    /// <summary>
    /// Original expiration time of the token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
