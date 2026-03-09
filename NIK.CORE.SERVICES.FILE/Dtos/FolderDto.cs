namespace NIK.CORE.SERVICES.FILE.Dtos;

/// <summary>
/// Represents a folder in the file management system.
/// </summary>
/// <remarks>
/// A folder can contain files and other subfolders.
/// Folders are organized in a hierarchical structure using the <see cref="ParentId"/> property.
/// </remarks>
public class FolderDto
{
    /// <summary>
    /// Unique identifier of the folder.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Name of the folder.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Identifier of the parent folder.
    /// </summary>
    /// <remarks>
    /// If <c>null</c> or empty, the folder is considered a root-level folder.
    /// </remarks>
    public string ParentId { get; set; }

    /// <summary>
    /// The date and time when the folder was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the folder was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
