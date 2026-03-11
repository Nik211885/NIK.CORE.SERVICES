namespace NIK.CORE.SERVICES.IDENTITY.Models;

public class OrganizationFunction
{
    /// <summary>
    /// Unique identifier of the organization-function relationship
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the organization
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Identifier of the function
    /// </summary>
    public Guid FunctionId { get; set; }

    /// <summary>
    /// Indicates whether the function is enabled for the organization
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The time when the function was enabled
    /// </summary>
    public DateTimeOffset EnabledAt { get; set; }

    /// <summary>
    /// Identifier of the user who enabled the function
    /// </summary>
    public Guid? EnabledBy { get; set; }

    /// <summary>
    /// The time when the function was disabled
    /// </summary>
    public DateTimeOffset? DisabledAt { get; set; }

    /// <summary>
    /// Identifier of the user who disabled the function
    /// </summary>
    public Guid? DisabledBy { get; set; }
}
