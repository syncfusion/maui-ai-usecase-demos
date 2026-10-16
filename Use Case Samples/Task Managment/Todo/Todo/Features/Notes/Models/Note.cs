using CommunityToolkit.Mvvm.ComponentModel;

namespace Todo.Features.Notes.Models;

/// <summary>
/// Represents a single To-Do entry shown on the My Notes list and any
/// of its filter views (Important, Reminder, Bin).
/// </summary>
/// <remarks>
/// Inherits from <see cref="ObservableObject"/> so derived UI metadata
/// (e.g. computed visibility flags) can be exposed as observable
/// properties without external bookkeeping.
/// </remarks>
public partial class Note : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private DateTime? dueDate;

    [ObservableProperty]
    private DateTime? reminderDate;

    /// <summary>
    /// Identifier of the user-defined list the note belongs to.
    /// <c>null</c> means the note is in <c>My Notes</c>.
    /// </summary>
    [ObservableProperty]
    private string? listId;

    [ObservableProperty]
    private bool isImportant;

    [ObservableProperty]
    private bool isCompleted;

    /// <summary>
    /// Soft-delete marker. When set, the note only appears in the Bin.
    /// </summary>
    [ObservableProperty]
    private DateTime? deletedAt;

    [ObservableProperty]
    private int displayOrder;

    [ObservableProperty]
    private DateTime createdAt = DateTime.Now;

    [ObservableProperty]
    private DateTime updatedAt = DateTime.Now;

    /// <summary>
    /// Indicates whether the note has a reminder (date flag only — no
    /// OS notification fires in this sample).
    /// </summary>
    public bool HasReminder => ReminderDate.HasValue;

    /// <summary>
    /// Indicates whether the note is due today.
    /// </summary>
    public bool IsDueToday => DueDate.HasValue && DueDate.Value.Date == DateTime.Today;

    /// <summary>
    /// Whether the note is visible in the My Notes (active) view —
    /// i.e. not soft-deleted.
    /// </summary>
    public bool IsActive => DeletedAt is null;
}