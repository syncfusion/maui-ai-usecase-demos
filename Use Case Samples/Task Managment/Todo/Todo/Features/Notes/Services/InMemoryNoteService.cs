using System.Collections.ObjectModel;
using Todo.Common;
using Todo.Features.Notes.Models;

namespace Todo.Features.Notes.Services;

/// <summary>
/// Singleton in-memory implementation of <see cref="INoteService"/>.
/// Backed by a single <see cref="ObservableCollection{T}"/> so all
/// subscribed UI automatically reflects changes.
/// </summary>
public sealed class InMemoryNoteService : INoteService
{
    private readonly ObservableCollection<Note> _notes;

    public InMemoryNoteService()
    {
        // The My Notes page must not contain any preloaded tasks or sample
        // data on application load (per the Module-08 spec). The store
        // starts empty — the user creates notes via the bottom input bar
        // and the data is held only in memory for the lifetime of the app.
        _notes = new ObservableCollection<Note>();
        Notes = new ReadOnlyObservableCollection<Note>(_notes);
    }

    public ReadOnlyObservableCollection<Note> Notes { get; }

    public Task<Note> CreateAsync(string title, DateTime? dueDate = null, DateTime? reminderDate = null, string? listId = null, RepeatType repeatType = RepeatType.None, bool notificationsEnabled = true)
    {
        var now = DateTime.Now;
        var nextOrder = _notes.Count == 0 ? 0 : _notes.Max(n => n.DisplayOrder) + 1;
        var note = new Note
        {
            Id = Guid.NewGuid().ToString(),
            Title = (title ?? string.Empty).Trim(),
            DueDate = dueDate,
            ReminderDate = reminderDate,
            ListId = listId,
            DisplayOrder = nextOrder,
            CreatedAt = now,
            UpdatedAt = now,
            RepeatType = repeatType,
            NotificationsEnabled = notificationsEnabled
        };
        _notes.Insert(0, note);
        return Task.FromResult(note);
    }

    public Task UpdateAsync(Note note)
    {
        note.UpdatedAt = DateTime.Now;
        // ObservableCollection replaces items in-place; binding systems
        // observe property-level notifications on Note itself.
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is null)
            return Task.CompletedTask;

        existing.DeletedAt = DateTime.Now;
        existing.UpdatedAt = DateTime.Now;
        return Task.CompletedTask;
    }

    public Task RestoreAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is null)
            return Task.CompletedTask;

        existing.DeletedAt = null;
        existing.UpdatedAt = DateTime.Now;
        return Task.CompletedTask;
    }

    public Task DeletePermanentlyAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is not null)
            _notes.Remove(existing);
        return Task.CompletedTask;
    }

    public Task ToggleCompleteAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is null)
            return Task.CompletedTask;

        existing.IsCompleted = !existing.IsCompleted;
        existing.UpdatedAt = DateTime.Now;
        return Task.CompletedTask;
    }

    public Task ToggleImportantAsync(string id)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == id);
        if (existing is null)
            return Task.CompletedTask;

        existing.IsImportant = !existing.IsImportant;
        existing.UpdatedAt = DateTime.Now;
        return Task.CompletedTask;
    }

    public IReadOnlyList<Note> GetForCategory(NotesListCategory category)
    {
        IEnumerable<Note> query = _notes;
        query = category switch
        {
            // Per the design (mynotes.png), My Notes is the active list
            // and only shows notes that are not soft-deleted.
            NotesListCategory.MyNotes => query.Where(n => n.DeletedAt is null),

            // Per the design (important.png), Important shows every note
            // that has been starred – including ones that have since been
            // moved to the Bin so users can still find them via the star.
            NotesListCategory.Important => query.Where(n => n.IsImportant),

            // Per the design (remainder.png), Reminder shows every note
            // that has a reminder date set, regardless of soft-delete.
            NotesListCategory.Reminder => query.Where(n => n.ReminderDate.HasValue),

            // Per the design (bin.png), Bin shows every soft-deleted note.
            NotesListCategory.Bin => query.Where(n => n.DeletedAt is not null),
            _ => query
        };

        return query
            .OrderBy(n => n.IsCompleted)
            .ThenBy(n => n.DisplayOrder)
            .ToList();
    }

    public IReadOnlyList<Note> GetForList(string listId)
    {
        // Only active (non-deleted) notes belong to a user-defined list.
        // Soft-deleted notes are surfaced in the Bin regardless of list.
        return _notes
            .Where(n => n.DeletedAt is null && n.ListId == listId)
            .OrderBy(n => n.IsCompleted)
            .ThenBy(n => n.DisplayOrder)
            .ToList();
    }

    public Task ReorderAsync(string targetListId, int oldIndex, int newIndex)
    {
        // Reorder only the notes that match the active filter so that
        // a drop in the My Notes view does not collide with Important
        // or Bin notes. `GetForCategory` / `GetForList` are the same
        // ordering the user sees on screen, so the index from the
        // view maps directly onto the source list.
        var ordered = !string.IsNullOrEmpty(targetListId)
            ? _notes
                .Where(n => n.DeletedAt is null && n.ListId == targetListId)
                .OrderBy(n => n.IsCompleted)
                .ThenBy(n => n.DisplayOrder)
                .ToList()
            : _notes
                .Where(n => n.DeletedAt is null)
                .OrderBy(n => n.IsCompleted)
                .ThenBy(n => n.DisplayOrder)
                .ToList();

        if (oldIndex < 0 || oldIndex >= ordered.Count) return Task.CompletedTask;
        // Clamp newIndex to the last valid insertion point.
        newIndex = Math.Clamp(newIndex, 0, ordered.Count - 1);
        if (oldIndex == newIndex) return Task.CompletedTask;

        var moved = ordered[oldIndex];
        ordered.RemoveAt(oldIndex);
        ordered.Insert(newIndex, moved);

        // Rewrite DisplayOrder so the new ordering is durable across
        // every subsequent re-filter / restart.
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].DisplayOrder = i;
            ordered[i].UpdatedAt = DateTime.Now;
        }
        return Task.CompletedTask;
    }

    public Task MoveToListAsync(string noteId, string? targetListId)
    {
        var existing = _notes.FirstOrDefault(n => n.Id == noteId);
        if (existing is null) return Task.CompletedTask;

        existing.ListId = targetListId;
        existing.UpdatedAt = DateTime.Now;

        // Append the moved note to the end of its new list so the user
        // can find it in the destination sidebar entry.
        if (existing.DeletedAt is null)
        {
            var sameScope = _notes
                .Where(n => n.DeletedAt is null && n.ListId == targetListId && n.Id != existing.Id)
                .OrderBy(n => n.IsCompleted)
                .ThenBy(n => n.DisplayOrder)
                .ToList();
            var nextOrder = sameScope.Count == 0 ? 0 : sameScope.Max(n => n.DisplayOrder) + 1;
            existing.DisplayOrder = nextOrder;
        }
        return Task.CompletedTask;
    }
}