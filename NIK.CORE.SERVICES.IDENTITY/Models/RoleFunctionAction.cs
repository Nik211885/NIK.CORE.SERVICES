namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class RoleFunctionAction
{
    /// <summary>
    /// Unique identifier of the role-function-action permission
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the role
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Identifier of the related FunctionAction
    /// </summary>
    public Guid FunctionActionId { get; set; }

    /// <summary>
    /// Indicates whether the permission is granted for the role
    /// </summary>
    public bool IsGranted { get; set; }

    /// <summary>
    /// The time when the permission was granted
    /// </summary>
    public DateTimeOffset GrantedAt { get; set; }

    /// <summary>
    /// Identifier of the user who granted the permission
    /// </summary>
    public Guid? GrantedBy { get; set; }

    /// <summary>
    /// Expiration time of the permission
    /// </summary>
    public DateTimeOffset? ExpiredAt { get; set; }
}
