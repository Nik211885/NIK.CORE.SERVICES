namespace NIK.CORE.SERVICES.FILE.Models;

/// <summary>
/// Represents a physical file stored in the system.
/// Contains metadata about the stored file including hash, size,
/// storage path, and reference tracking information.
/// </summary>
public class PhysicalStorage
{
    /// <summary>
    /// Unique identifier of the physical file.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Hash value of the file content (used for deduplication and integrity check).
    /// </summary>
    public string FileHash { get; set; }

    /// <summary>
    /// Size of the file in bytes.
    /// </summary>
    public int FileSize { get; set; }

    /// <summary>
    /// Relative path where the file is stored in the file system.
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// MIME type of the file (e.g., image/png, application/pdf).
    /// </summary>
    public string MimeType { get; set; }

    /// <summary>
    /// Number of references pointing to this physical file.
    /// Used to determine whether the file can be safely deleted.
    /// </summary>
    public int RefCount { get; set; }

    /// <summary>
    /// Date and time when the file was created in storage.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}