namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class AuditLog
{
    /// <summary>
    /// Unique identifier of the audit log
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the related user
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Identifier of the related session
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Type of event (e.g., Login, Logout, FailedLogin, PasswordChange)
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// IP address of the request
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string of the client
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Device information of the client
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Geo-resolved location of the request
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Indicates whether the event was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Reason for failure if the event was not successful
    /// </summary>
    public string? FailReason { get; set; }

    /// <summary>
    /// Additional metadata stored as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// The time when the log entry was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
