using System.Collections.ObjectModel;
using Todo.Features.CustomLists.Models;

namespace Todo.Features.CustomLists.Services;

/// <summary>
/// Abstraction over the in-memory list store. The default
/// implementation keeps an <see cref="ObservableCollection{T}"/> that
/// the shared sidebar observes and renders below the four built-in
/// categories (per the design image <c>mynotes.png</c>).
/// </summary>
public interface IListService
{
    /// <summary>
    /// Observable collection of user-defined lists, sorted by
    /// <see cref="NoteList.DisplayOrder"/>. Mutations to this
    /// collection surface change notifications to bound UI.
    /// </summary>
    ReadOnlyObservableCollection<NoteList> Lists { get; }

    /// <summary>
    /// Creates a new list with the given <paramref name="name"/> and
    /// returns the resulting <see cref="NoteList"/>. Throws
    /// <see cref="ArgumentException"/> when the name is empty or
    /// already taken by an existing list (case-insensitive match).
    /// </summary>
    Task<NoteList> CreateAsync(string name);

    /// <summary>
    /// Renames an existing list to <paramref name="newName"/>. Throws
    /// <see cref="KeyNotFoundException"/> when no list has the given
    /// <paramref name="id"/>, or <see cref="ArgumentException"/> when
    /// the new name is empty or already used by another list.
    /// </summary>
    Task RenameAsync(string id, string newName);

    /// <summary>
    /// Removes a list from the in-memory store. Notes that were in
    /// this list are returned to <c>My Notes</c> (their
    /// <c>ListId</c> is cleared) so no data is lost.
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// Returns <c>true</c> when a list with the given
    /// <paramref name="name"/> (case-insensitive) already exists.
    /// </summary>
    bool NameExists(string name, string? excludeId = null);
}