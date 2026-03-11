namespace NIK.CORE.SERVICES.IDENTITY.Models;


public class Organization
{
    /// <summary>
    /// Unique identifier of the organization
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the parent organization
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Unique code of the organization
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Display name of the organization
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Description of the organization
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Organization type (1 = Company, 2 = Branch, 3 = Department)
    /// </summary>
    public short Type { get; set; }

    /// <summary>
    /// URL of the organization logo
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Address of the organization
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Contact email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Tax code of the organization
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// Indicates whether the organization is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates whether the organization is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The time when the organization was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The time when the organization was last updated
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// The time when the organization was deleted
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Identifier of the user who created the organization
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Identifier of the user who last updated the organization
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
