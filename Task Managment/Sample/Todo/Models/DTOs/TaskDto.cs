using Todo.Utilities;

namespace Todo.Models.DTOs;

/// <summary>
/// Transport-layer representation of a single task.  The full schema (notes,
/// attachments, sub-tasks, etc.) is added in Module 4 (Task 4-01).
/// </summary>
public sealed record TaskDto
{
    /// <summary>Stable, opaque identifier for the task.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Identifier of the list the task belongs to.</summary>
    public string? ListId { get; init; }

    /// <summary>Short, single-line summary of the task.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Optional longer, multi-line description.</summary>
    public string? Description { get; init; }

    /// <summary>Whether the task has been completed by the user.</summary>
    public bool IsCompleted { get; init; }

    /// <summary>Whether the task is flagged as important (star icon).</summary>
    public bool IsImportant { get; init; }

    /// <summary>Optional due date (UTC).</summary>
    public DateTimeOffset? DueAt { get; init; }

    /// <summary>Optional reminder date (UTC).</summary>
    public DateTimeOffset? ReminderAt { get; init; }

    /// <summary>Recurrence pattern applied to the task.</summary>
    public Enums.RecurrenceType Recurrence { get; init; } = Enums.RecurrenceType.None;

    /// <summary>Top-level category used by the sidebar filter.</summary>
    public Enums.ListCategory Category { get; init; } = Enums.ListCategory.MyNotes;

    /// <summary>UTC timestamp at which the task was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>UTC timestamp at which the task was last modified.</summary>
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}
