using Todo.Common;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views.Templates;

/// <summary>
/// Code-behind for <see cref="NoteItemTemplate"/>. Routes the
/// "more" (three-dot) tap into the right action-sheet for the
/// current filter view, so the user sees the contextual actions
/// shown in <c>deletepermently.png</c> and <c>mynotes.png</c>.
/// </summary>
public partial class NoteItemTemplate : DataTemplate
{
    public NoteItemTemplate()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Toggles the important flag on the note. The action routes through
    /// the ViewModel so the active filter (Important / Reminder / Bin)
    /// re-evaluates and the note is removed from the visible list if
    /// the new state no longer matches the filter.
    /// </summary>
    private void OnStarClicked(object? sender, EventArgs e)
    {
        if (sender is not BindableObject bindable || bindable.BindingContext is not Note note)
            return;

        var page = GetHostPage(bindable);
        if (page?.BindingContext is NotesListViewModel vm)
            vm.ToggleImportantCommand.Execute(note);
    }

    /// <summary>
    /// Opens the contextual action-sheet for a note. The set of actions
    /// is derived from the currently active sidebar filter:
    /// <list type="bullet">
    ///   <item><b>Bin</b> – Mark as completed · Edit · Restore · Delete permanently</item>
    ///   <item><b>Other</b> – Mark as completed · Edit · Delete</item>
    /// </list>
    /// </summary>
    private async void OnMoreClicked(object? sender, EventArgs e)
    {
        if (sender is not BindableObject bindable || bindable.BindingContext is not Note note)
            return;

        var page = GetHostPage(bindable);
        if (page?.BindingContext is not NotesListViewModel vm)
            return;

        var isBin = vm.SelectedCategory == NotesListCategory.Bin;
        var title = note.Title ?? string.Empty;

        if (isBin)
        {
            var action = await page.DisplayActionSheetAsync(
                title,
                "Cancel",
                null,
                "Mark as completed",
                "Edit note",
                "Restore note",
                "Delete permanently");

            switch (action)
            {
                case "Mark as completed":
                    if (!note.IsCompleted) vm.ToggleCompleteCommand.Execute(note);
                    break;
                case "Edit note":
                    vm.EditNoteCommand.Execute(note);
                    break;
                case "Restore note":
                    vm.RestoreCommand.Execute(note);
                    break;
                case "Delete permanently":
                    vm.RequestDeletePermanentlyCommand.Execute(note);
                    break;
            }
        }
        else
        {
            var action = await page.DisplayActionSheetAsync(
                title,
                "Cancel",
                null,
                "Mark as completed",
                "Edit note",
                "Delete");

            switch (action)
            {
                case "Mark as completed":
                    if (!note.IsCompleted) vm.ToggleCompleteCommand.Execute(note);
                    break;
                case "Edit note":
                    vm.EditNoteCommand.Execute(note);
                    break;
                case "Delete":
                    vm.DeleteCommand.Execute(note);
                    break;
            }
        }
    }

    private static Page? GetHostPage(BindableObject bindable)
    {
        // Walk up the visual tree to find the host Page. BindableObject
        // can be either an Element (visual) or a plain data object; only
        // the Element path exposes a Parent we can walk.
        Element? current = bindable as Element;
        while (current is not null)
        {
            if (current is Page page)
                return page;
            current = current.Parent;
        }
        return null;
    }
}
