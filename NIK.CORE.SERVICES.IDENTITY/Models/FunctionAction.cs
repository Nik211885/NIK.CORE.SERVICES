namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class FunctionAction
{
    /// <summary>
    /// Unique identifier of the function-action mapping
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the related function
    /// </summary>
    public Guid FunctionId { get; set; }

    /// <summary>
    /// Identifier of the related action
    /// </summary>
    public Guid ActionId { get; set; }

    /// <summary>
    /// Indicates whether this function-action mapping is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The time when the mapping was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who created the mapping
    /// </summary>
    public Guid? CreatedBy { get; set; }
}
