namespace NIK.CORE.SERVICES.IDENTITY.Models;

/// <summary>
/// User entity mapping with table "Users"
/// </summary>
public class User
{
    /// <summary>
    /// Primary key - UUID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username used for login
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Normalized username for searching (usually uppercase)
    /// </summary>
    public string NormalizedUserName { get; set; }

    /// <summary>
    /// Email address of the user
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Normalized email (uppercase for searching)
    /// </summary>
    public string NormalizedEmail { get; set; }

    /// <summary>
    /// Indicates whether the email has been confirmed
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Hashed password (stored securely)
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Security stamp used for invalidating sessions when credentials change
    /// </summary>
    public string SecurityStamp { get; set; }

    /// <summary>
    /// Concurrency token used to detect concurrent updates
    /// </summary>
    public string ConcurrencyStamp { get; set; }

    /// <summary>
    /// Phone number of the user
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Indicates whether phone number has been confirmed
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; }

    /// <summary>
    /// Indicates whether two-factor authentication is enabled
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Lockout end time if the account is temporarily locked
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// Indicates whether lockout is enabled for this user
    /// </summary>
    public bool LockoutEnabled { get; set; }

    /// <summary>
    /// Number of failed login attempts
    /// </summary>
    public int AccessFailedCount { get; set; }

    // =========================
    // Profile Information
    // =========================

    /// <summary>
    /// Full name of the user
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// URL to user's avatar image
    /// </summary>
    public string AvatarUrl { get; set; }

    /// <summary>
    /// User status
    /// 1 = Active
    /// 2 = Inactive
    /// 3 = Suspended
    /// 4 = Banned
    /// </summary>
    public short Status { get; set; }

    // =========================
    // Soft Delete & Audit
    // =========================

    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Soft delete flag (true if deleted)
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Deleted timestamp (for soft delete)
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// User ID who created this record
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// User ID who last updated this record
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}
public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Banned = 4
}
