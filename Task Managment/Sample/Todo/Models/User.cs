using SQLite;

namespace Todo.Models;

/// <summary>
/// Represents an authenticated account that owns one or more <see cref="TaskList"/>
/// instances.  Persisted to the local SQLite database as a row in the
/// <c>Users</c> table (see Task 1-07 — <c>Database Setup</c>).
/// </summary>
/// <remarks>
/// <para>Schema:</para>
/// <code>
/// CREATE TABLE Users (
///     Id TEXT PRIMARY KEY,
///     Email TEXT UNIQUE NOT NULL,
///     PasswordHash TEXT NOT NULL,
///     DisplayName TEXT,
///     CreatedAt DATETIME NOT NULL,
///     UpdatedAt DATETIME NOT NULL,
///     DeletedAt DATETIME NULL
/// );
/// </code>
/// <para>
/// The <see cref="DeletedAt"/> column backs a <em>soft delete</em> pattern —
/// a non-null value marks the record as deleted and is filtered out by
/// repository queries.  A <see cref="PasswordHash"/> is required; never store
/// the clear-text password.
/// </para>
/// </remarks>
[Table("Users")]
public class User
{
    /// <summary>
    /// Primary key.  Generated as a GUID string so that the same identifier can
    /// be used across devices and any future remote API without
    /// re-mapping.  Defaults to a new GUID on construction.
    /// </summary>
    [PrimaryKey]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// E-mail address used to sign in.  Enforced unique at the SQLite level
    /// so that two users cannot share the same address.
    /// </summary>
    [Unique]
    [NotNull]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash of the user's password (PBKDF2 / bcrypt / argon2 — whichever the
    /// authentication service picks; this model only stores the result).
    /// </summary>
    [NotNull]
    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Friendly name shown in the UI (greeting, list owner, …).  May be
    /// <see langword="null"/> until the user fills in their profile.
    /// </summary>
    [Column("DisplayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// UTC timestamp captured when the row is first persisted.  Defaults to
    /// <see cref="DateTime.UtcNow"/> so the field is never <see langword="null"/>
    /// when an un-saved instance is created.
    /// </summary>
    [NotNull]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp of the most recent modification.  Repositories are
    /// expected to bump this on every <c>Update</c>.
    /// </summary>
    [NotNull]
    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp of the soft delete, or <see langword="null"/> when the
    /// row is still active.  Repositories must filter out rows with a
    /// non-null value.
    /// </summary>
    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// UTC timestamp of the most recent successful sign-in, or
    /// <see langword="null"/> when the account has never signed in (for
    /// example, immediately after sign-up the value is set to the
    /// sign-up timestamp).  Used by the UI to render "last seen" labels.
    /// </summary>
    [Column("LastSignedInAt")]
    public DateTimeOffset? LastSignedInAt { get; set; }
}
