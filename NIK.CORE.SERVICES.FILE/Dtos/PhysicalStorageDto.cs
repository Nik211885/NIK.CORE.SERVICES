namespace NIK.CORE.SERVICES.FILE.Dtos;

/// <summary>
/// Represents the physical storage information of a file in the system.
/// </summary>
public class PhysicalStorageDto
{
    /// <summary>
    /// Unique identifier of the stored file.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Hash value of the file used to verify integrity and detect duplicates.
    /// </summary>
    public string FileHash { get; set; }

    /// <summary>
    /// Size of the file in bytes.
    /// </summary>
    public int FileSize { get; set; }

    /// <summary>
    /// Relative path to the file location in the storage system.
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// MIME type of the file (e.g., image/png, application/pdf).
    /// </summary>
    public string MimeType { get; set; }

    /// <summary>
    /// Number of references pointing to this physical file.
    /// </summary>
    public int RefCount { get; set; }

    /// <summary>
    /// The date and time when the file was created in the storage system.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}