using SQLite;

namespace Todo.Models;

/// <summary>
/// Represents a single to-do item belonging to a <see cref="User"/> and stored
/// in a <see cref="TaskList"/>.  Persisted to the <c>Tasks</c> table.
/// </summary>
/// <remarks>
/// <para>
/// The class is named <see cref="TaskItem"/> rather than <c>Task</c> so it
/// does not clash with <see cref="System.Threading.Tasks.Task"/> when
/// both <c>Todo.Models</c> and <c>System.Threading.Tasks</c> are imported in
/// the same file.  The on-disk table is still called <c>Tasks</c> thanks to
/// the <see cref="TableAttribute"/> below.
/// </para>
/// <para>Schema:</para>
/// <code>
/// CREATE TABLE Tasks (
///     Id TEXT PRIMARY KEY,
///     UserId TEXT NOT NULL,
///     ListId TEXT NOT NULL,
///     Title TEXT NOT NULL,
///     Description TEXT,
///     IsCompleted BOOLEAN DEFAULT 0,
///     IsImportant BOOLEAN DEFAULT 0,
///     DueDate DATETIME NULL,
///     ReminderTime DATETIME NULL,
///     RecurrenceType TEXT,
///     DisplayOrder INTEGER,
///     CreatedAt DATETIME NOT NULL,
///     UpdatedAt DATETIME NOT NULL,
///     DeletedAt DATETIME NULL,
///     FOREIGN KEY (UserId) REFERENCES Users(Id),
///     FOREIGN KEY (ListId) REFERENCES TaskLists(Id)
/// );
/// </code>
/// </remarks>
[Table("Tasks")]
public class TaskItem
{
    /// <summary>
    /// Primary key.  GUID string; generated locally.
    /// </summary>
    [PrimaryKey]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the <see cref="User"/> that owns this task.
    /// </summary>
    [Indexed]
    [NotNull]
    [Column("UserId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the <see cref="TaskList"/> this task belongs to.
    /// </summary>
    [Indexed]
    [NotNull]
    [Column("ListId")]
    public string ListId { get; set; } = string.Empty;

    /// <summary>
    /// Short, user-visible title (1-500 characters — see
    /// <see cref="Utilities.Constants.Validation.TaskTitleMaxLength"/>).
    /// </summary>
    [NotNull]
    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional long-form description / notes for the task.
    /// </summary>
    [Column("Description")]
    public string? Description { get; set; }

    /// <summary>
    /// <see langword="true"/> when the user has ticked the task off.  A
    /// completed task is hidden from the default list view but kept in the
    /// database so it can be restored.
    /// </summary>
    [NotNull]
    [Column("IsCompleted")]
    public bool IsCompleted { get; set; }

    /// <summary>
    /// <see langword="true"/> when the user has starred / flagged the task as
    /// important.  Drives the "Important" pseudo-list filter.
    /// </summary>
    [NotNull]
    [Column("IsImportant")]
    public bool IsImportant { get; set; }

    /// <summary>
    /// Optional due date and time (UTC).  A value in the past marks the
    /// task as overdue.
    /// </summary>
    [Column("DueDate")]
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Optional reminder time (UTC).  When set, the notification service
    /// fires a local notification at this moment.
    /// </summary>
    [Column("ReminderTime")]
    public DateTime? ReminderTime { get; set; }

    /// <summary>
    /// Recurrence pattern as a string (matches
    /// <see cref="Utilities.Enums.RecurrenceType"/> names).  Persisted as a
    /// string for forward-compatibility so that adding a new enum value
    /// never breaks existing rows.
    /// </summary>
    [Column("RecurrenceType")]
    public string? RecurrenceType { get; set; }

    /// <summary>
    /// Position of the task within its <see cref="ListId"/> group
    /// (0-based).  Lower values are rendered first.
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

    /// <summary>
    /// UTC timestamp of the last completion.  <see langword="null"/> when
    /// the task has never been completed.  Used by recurring tasks to
    /// compute the next occurrence.
    /// </summary>
    [Column("CompletedAt")]
    public DateTime? CompletedAt { get; set; }
}
