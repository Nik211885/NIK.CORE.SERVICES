namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class Role
{
    /// <summary>
    /// Primary key - UUID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Role name (e.g., Admin, User, Manager)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Normalized role name (usually uppercase for searching)
    /// </summary>
    public string NormalizedName { get; set; }

    /// <summary>
    /// Description of the role
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Concurrency token used to detect concurrent updates
    /// </summary>
    public string ConcurrencyStamp { get; set; }

    /// <summary>
    /// Indicates whether this role is a system role (cannot be deleted/modified easily)
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Indicates whether the role is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Soft delete flag (true if deleted)
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who created this role
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// User ID who last updated this role
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
