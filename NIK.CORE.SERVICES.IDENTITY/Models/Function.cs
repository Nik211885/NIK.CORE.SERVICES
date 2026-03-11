namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class Function
{
    /// <summary>
    /// Unique identifier of the function
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the module this function belongs to
    /// </summary>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Unique code representing the function
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Display name of the function
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the function
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL or route associated with the function
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Sort order used for displaying functions
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Indicates whether the function is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates whether the function is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The time when the function was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The time when the function was last updated
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who created the function
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Identifier of the user who last updated the function
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
