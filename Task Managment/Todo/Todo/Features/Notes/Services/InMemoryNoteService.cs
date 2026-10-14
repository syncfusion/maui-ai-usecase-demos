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
        // My Notes must be empty on initial application load (Module-02).
        // No sample / mock / placeholder notes are seeded; the user creates
        // every note via the bottom input bar.
        _notes = new ObservableCollection<Note>();
        Notes = new ReadOnlyObservableCollection<Note>(_notes);
    }

    public ReadOnlyObservableCollection<Note> Notes { get; }

    public Task<Note> CreateAsync(string title, DateTime? dueDate = null, DateTime? reminderDate = null)
    {
        var now = DateTime.Now;
        var nextOrder = _notes.Count == 0 ? 0 : _notes.Max(n => n.DisplayOrder) + 1;
        var note = new Note
        {
            Id = Guid.NewGuid().ToString(),
            Title = (title ?? string.Empty).Trim(),
            DueDate = dueDate,
            ReminderDate = reminderDate,
            DisplayOrder = nextOrder,
            CreatedAt = now,
            UpdatedAt = now
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
            NotesListCategory.MyNotes => query.Where(n => n.DeletedAt is null),
            NotesListCategory.Important => query.Where(n => n.DeletedAt is null && n.IsImportant),
            NotesListCategory.Reminder => query.Where(n => n.DeletedAt is null && n.ReminderDate.HasValue),
            NotesListCategory.Bin => query.Where(n => n.DeletedAt is not null),
            _ => query
        };

        return query
            .OrderBy(n => n.IsCompleted)
            .ThenBy(n => n.DisplayOrder)
            .ToList();
    }
}