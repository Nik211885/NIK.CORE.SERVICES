namespace Nik.Core.Services.File.Models;

/// <summary>
/// Represents a logical file entity stored in the system.
/// This entity contains metadata and references to the physical file storage.
/// </summary>
public class FileEntry
{
    /// <summary>
    /// Unique identifier of the file.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name of the file (without path).
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// File extension (e.g., .pdf, .jpg).
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    /// Identifier of the folder containing this file.
    /// </summary>
    public string FolderId { get; set; }

    /// <summary>
    /// Identifier of the associated physical storage record.
    /// Used to map logical file to actual stored file.
    /// </summary>
    public string PhysicalId { get; set; }

    /// <summary>
    /// Indicates whether the file is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Additional metadata stored in JSON format.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Date and time when the file was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the file was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}