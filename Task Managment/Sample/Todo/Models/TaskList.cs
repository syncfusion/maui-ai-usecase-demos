using SQLite;

namespace Todo.Models;

/// <summary>
/// Represents a grouping of <see cref="TaskItem"/>s that belongs to a single
/// <see cref="User"/>.  Persisted to the <c>TaskLists</c> table.
/// </summary>
/// <remarks>
/// <para>Schema:</para>
/// <code>
/// CREATE TABLE TaskLists (
///     Id TEXT PRIMARY KEY,
///     UserId TEXT NOT NULL,
///     Title TEXT NOT NULL,
///     Color TEXT,
///     DisplayOrder INTEGER,
///     CreatedAt DATETIME NOT NULL,
///     UpdatedAt DATETIME NOT NULL,
///     DeletedAt DATETIME NULL,
///     FOREIGN KEY (UserId) REFERENCES Users(Id)
/// );
/// </code>
/// <para>
/// Foreign keys are declared on the <see cref="UserId"/> column so the
/// database engine can enforce referential integrity.  sqlite-net-pcl does
/// not automatically translate the <c>[ForeignKey]</c> attribute to a
/// <c>FOREIGN KEY</c> clause for every schema build, so this is enforced at
/// the application layer through the <see cref="Data.AppDbContext"/>.
/// </para>
/// </remarks>
[Table("TaskLists")]
public class TaskList
{
    /// <summary>
    /// Primary key.  GUID string; generated locally so the same value can
    /// later be used as a remote identifier without re-mapping.
    /// </summary>
    [PrimaryKey]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the <see cref="User"/> that owns this list.
    /// </summary>
    [Indexed]
    [NotNull]
    [Column("UserId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Display title shown in the navigation drawer.
    /// </summary>
    [NotNull]
    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional hex colour (for example <c>#5C4EAE</c>) used to tint the list
    /// in the UI.  Stored as a string so the value can be round-tripped
    /// through MAUI's <c>Color</c> converter without losing precision.
    /// </summary>
    [Column("Color")]
    public string? Color { get; set; }

    /// <summary>
    /// Position of the list in the navigation drawer (0-based).  Lower values
    /// are rendered first.
    /// </summary>
    [Column("DisplayOrder")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// UTC timestamp captured when the row is first persisted.
    /// </summary>
    [NotNull]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp of the most recent modification.
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
}
