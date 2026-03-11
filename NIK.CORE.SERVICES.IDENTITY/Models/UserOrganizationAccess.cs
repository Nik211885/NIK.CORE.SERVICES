namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class UserOrganizationAccess
{
    /// <summary>
    /// Unique identifier of the user-organization access record
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the user
    /// </summary>
    public Guid UserId { get; set; }

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
