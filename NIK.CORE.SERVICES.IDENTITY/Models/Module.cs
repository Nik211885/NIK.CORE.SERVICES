namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class Module
{
    /// <summary>
    /// Unique identifier of the module
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique code of the module
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Display name of the module
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the module
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon URL representing the module
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Sort order used for displaying modules
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Indicates whether the module is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates whether the module is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The time when the module was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The time when the module was last updated
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who created the module
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Identifier of the user who last updated the module
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
