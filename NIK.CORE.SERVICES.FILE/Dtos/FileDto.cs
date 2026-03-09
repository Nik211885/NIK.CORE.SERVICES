namespace NIK.CORE.SERVICES.FILE.Dtos;

/// <summary>
/// Represents a file entity returned to clients, including logical file information
/// and related physical storage details.
/// </summary>
public class FileDto
{
    /// <summary>
    /// Unique identifier of the file.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the file as shown to users.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// File extension (e.g., .png, .pdf, .docx).
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    /// Identifier of the folder that contains this file.
    /// </summary>
    public string FolderId { get; set; }

    /// <summary>
    /// Identifier of the associated physical storage record.
    /// </summary>
    public string PhysicalId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Optional metadata associated with the file, typically stored as JSON.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Timestamp when the file record was created in the system.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the file content was uploaded.
    /// </summary>
    public DateTimeOffset UploadAt { get; set; }

    /// <summary>
    /// Size of the file in bytes. Retrieved from the physical storage record.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME type of the file (e.g., image/png, application/pdf).
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Relative path of the file within the storage system.
    /// </summary>
    public string? RelativePath { get; set; }
}
