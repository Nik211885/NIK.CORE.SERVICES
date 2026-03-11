namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class UserOrganization
{
    /// <summary>
    /// Unique identifier of the user-organization relationship
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
    /// Indicates whether this organization is the default for the user
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Indicates whether the user is currently active in this organization
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The time when the user joined the organization
    /// </summary>
    public DateTimeOffset JoinedAt { get; set; }

    /// <summary>
    /// Identifier of the user who added the membership
    /// </summary>
    public Guid? JoinedBy { get; set; }

    /// <summary>
    /// The time when the user left the organization
    /// </summary>
    public DateTimeOffset? LeftAt { get; set; }
}
