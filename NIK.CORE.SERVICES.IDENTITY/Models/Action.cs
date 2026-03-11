namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class Action
{
    /// <summary>
    /// Unique identifier of the action
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique code representing the action
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Display name of the action
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the action
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates whether the action is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The time when the action was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The time when the action was last updated
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who created the action
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Identifier of the user who last updated the action
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
