using System.Collections.ObjectModel;
using Todo.Common;
using Todo.Features.Notes.Models;

namespace Todo.Features.Notes.Services;

/// <summary>
/// Abstraction over the in-memory note store. The implementation keeps a
/// single <see cref="ObservableCollection{T}"/> that the shared
/// <c>NotesListViewModel</c> observes and filters.
/// </summary>
public interface INoteService
{
    /// <summary>
    /// Observable collection shared with all consumers (e.g. the central
    /// ViewModel). Mutations to this collection surface change
    /// notifications to bound UI.
    /// </summary>
    ReadOnlyObservableCollection<Note> Notes { get; }

    /// <summary>
    /// Adds a new note to the store. The supplied <paramref name="note"/>
    /// receives a fresh <see cref="Note.Id"/> and timestamps. The
    /// optional <paramref name="listId"/> attaches the note to a
    /// user-defined list (Module-05).
    /// </summary>
    Task<Note> CreateAsync(string title, DateTime? dueDate = null, DateTime? reminderDate = null, string? listId = null);

    /// <summary>
    /// Replaces the editable fields of an existing note.
    /// </summary>
    Task UpdateAsync(Note note);

    /// <summary>
    /// Marks a note as soft-deleted (sets <see cref="Note.DeletedAt"/>).
    /// The note is hidden from <c>My Notes</c> and only visible in
    /// <c>Bin</c>.
    /// </summary>
    Task SoftDeleteAsync(string id);

    /// <summary>
    /// Reverses a soft-delete: clears <see cref="Note.DeletedAt"/>.
    /// </summary>
    Task RestoreAsync(string id);

    /// <summary>
    /// Removes the note from the in-memory store entirely.
    /// </summary>
    Task DeletePermanentlyAsync(string id);

    /// <summary>
    /// Toggles <see cref="Note.IsCompleted"/> for the given note.
    /// </summary>
    Task ToggleCompleteAsync(string id);

    /// <summary>
    /// Toggles <see cref="Note.IsImportant"/> for the given note.
    /// </summary>
    Task ToggleImportantAsync(string id);

    /// <summary>
    /// Returns a filtered snapshot of the notes for the supplied
    /// <see cref="NotesListCategory"/>. The result is a fresh list and
    /// safe to bind from XAML.
    /// </summary>
    IReadOnlyList<Note> GetForCategory(NotesListCategory category);

    /// <summary>
    /// Returns the notes that belong to the supplied user-defined list
    /// (<paramref name="listId"/>). Used by the sidebar's custom lists
    /// (Module-05).
    /// </summary>
    IReadOnlyList<Note> GetForList(string listId);
}