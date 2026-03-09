namespace NIK.CORE.SERVICES.FILE.Dtos;

/// <summary>
/// Represents a request to create a new physical storage record for a file.
/// This record stores metadata about the file stored on disk.
/// </summary>
public class CreatePhysicalStorageRequest
{
    /// <summary>
    /// Hash value of the file content used to detect duplicate files
    /// and ensure file integrity.
    /// </summary>
    public string FileHash { get; set; }

    /// <summary>
    /// Size of the file in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Relative path to the file location within the storage root directory.
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// MIME type of the file (e.g., image/png, application/pdf).
    /// </summary>
    public string? MimeType { get; set; }
}