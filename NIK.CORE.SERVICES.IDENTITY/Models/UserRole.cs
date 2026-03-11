namespace NIK.CORE.SERVICES.IDENTITY.Models;

using System;

public class UserRole
{
    /// <summary>
    /// Identifier of the user
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identifier of the role
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// The time when the role was assigned to the user
    /// </summary>
    public DateTimeOffset AssignedAt { get; set; }

    /// <summary>
    /// Identifier of the user who assigned the role
    /// </summary>
    public Guid? AssignedBy { get; set; }

    /// <summary>
    /// Expiration time of the role (null means no expiration)
    /// </summary>
    public DateTimeOffset? ExpiredAt { get; set; }

    /// <summary>
    /// Indicates whether the role assignment is currently active
    /// </summary>
    public bool IsActive { get; set; }
}
