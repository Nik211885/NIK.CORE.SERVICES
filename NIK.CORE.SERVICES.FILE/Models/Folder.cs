namespace NIK.CORE.SERVICES.FILE.Models;

/// <summary>
/// Represents a logical folder in the storage system.
/// Folders can be organized hierarchically using the ParentId property.
/// </summary>
public class Folder
{
    /// <summary>
    /// Unique identifier of the folder.
    /// </summary>
    public string Id  { get; set; }

    /// <summary>
    /// Display name of the folder.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Identifier of the parent folder.
    /// Null or empty if this folder is a root folder.
    /// </summary>
    public string ParentId { get; set; }

    /// <summary>
    /// Date and time when the folder was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the folder was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}