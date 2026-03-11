namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class RoleOrganizationAccess
{
    /// <summary>
    /// Unique identifier of the role-organization access record
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the role
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Identifier of the organization
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Type of access (1 = Read, 2 = Write, 3 = Full)
    /// </summary>
    public short AccessType { get; set; }

    /// <summary>
    /// Indicates whether the access is granted
    /// </summary>
    public bool IsGranted { get; set; }

    /// <summary>
    /// The time when the access was granted
    /// </summary>
    public DateTimeOffset GrantedAt { get; set; }

    /// <summary>
    /// Identifier of the user who granted the access
    /// </summary>
    public Guid? GrantedBy { get; set; }

    /// <summary>
    /// Expiration time of the access
    /// </summary>
    public DateTimeOffset? ExpiredAt { get; set; }
}
