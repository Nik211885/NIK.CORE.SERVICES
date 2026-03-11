namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class UserFunctionAction
{
    /// <summary>
    /// Unique identifier of the user-function-action permission
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the user
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identifier of the related FunctionAction
    /// </summary>
    public Guid FunctionActionId { get; set; }

    /// <summary>
    /// Indicates whether the permission is granted.
    /// False means the permission is explicitly denied.
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
