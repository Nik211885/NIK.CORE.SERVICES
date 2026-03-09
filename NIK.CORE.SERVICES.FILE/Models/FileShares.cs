namespace NIK.CORE.SERVICES.FILE.Models;

/// <summary>
/// Represents a file sharing record.
/// Allows a file to be shared externally using a secure token,
/// with optional password protection and expiration.
/// </summary>
public class FileShare
{
    /// <summary>
    /// Unique identifier of the share record.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Identifier of the shared file.
    /// </summary>
    public string FileId { get; set; } 

    /// <summary>
    /// Public token used to access the shared file.
    /// This value should be unique and hard to guess.
    /// </summary>
    public string ShareToken { get; set; } 

    /// <summary>
    /// Expiration date and time of the share link.
    /// After this time, the link becomes invalid.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Hashed password protecting the share link (optional).
    /// Null if the share does not require a password.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Number of times the shared file has been downloaded.
    /// </summary>
    public int DownloadCount { get; set; }

    /// <summary>
    /// Date and time when the share link was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}