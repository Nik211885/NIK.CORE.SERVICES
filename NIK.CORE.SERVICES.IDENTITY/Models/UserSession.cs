namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class UserSession
{
    /// <summary>
    /// Unique identifier of the session
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the user who owns the session
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Hashed session token used for authentication
    /// </summary>
    public string SessionToken { get; set; }

    /// <summary>
    /// Refresh token used in JWT refresh flow
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Unique identifier of the device
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Device display name (e.g., Chrome on Windows)
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// Type of device (1=Web, 2=Mobile, 3=Desktop, 4=API)
    /// </summary>
    public short DeviceType { get; set; }

    /// <summary>
    /// IP address of the session
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string of the client
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Geo-resolved location of the session
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Indicates whether the user is currently online
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Last activity timestamp of the session
    /// </summary>
    public DateTimeOffset LastActivityAt { get; set; }

    /// <summary>
    /// Expiration time of the session
    /// </summary>
    public DateTimeOffset? ExpiredAt { get; set; }

    /// <summary>
    /// Time when the session was revoked
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Reason why the session was revoked (e.g., Logout, ForceLogout, Expired)
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// The time when the session was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
