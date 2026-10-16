using Todo.Common;
using Todo.Features.CustomLists.Models;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class SidebarView : ContentView
{
    // Strongly-typed colors used for the drop-target highlight. We
    // resolve them once so the drop visual does not have to walk the
    // resource dictionary on every DragOver event.
    private static readonly Color DropTargetFill = Color.FromArgb("#EEEAFB");
    private static readonly Color DropTargetStroke = Color.FromArgb("#5C4EAE");

    public SidebarView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Forwards sidebar selection of a built-in category to the
    /// centralized ViewModel so it can navigate between the four
    /// built-in pages. The page's <c>SelectedCategory</c> is updated
    /// as a side effect of
    /// <see cref="NotesListViewModel.NavigateToCategoryCommand"/>.
    /// </summary>
    private void OnSidebarSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (e.CurrentSelection.FirstOrDefault() is not NotesListCategory category) return;

        // Always invoke the navigation command so the host page
        // receives the NavigationRequested event. Skip if the user
        // re-selected the currently-active category (avoids redundant
        // navigations and any self-navigation loops).
        if (vm.SelectedCategory == category) return;
        vm.NavigateToCategoryCommand.Execute(category);
    }

    /// <summary>
    /// Forwards selection of a user-defined list (Module-05) to the
    /// centralized ViewModel. Custom lists reuse the My Notes chrome,
    /// so the only state change is the active <c>SelectedListId</c>.
    /// </summary>
    private void OnCustomListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (e.CurrentSelection.FirstOrDefault() is not NoteList list) return;

        // Skip if the user re-selected the currently-active list.
        if (string.Equals(vm.SelectedListId, list.Id, StringComparison.Ordinal)) return;
        vm.NavigateToListCommand.Execute(list.Id);
    }

    // -----------------------------------------------------------------
    //  Module-07 task 7-2: drag-and-drop drop targets
    // -----------------------------------------------------------------

    /// <summary>
    /// Built-in category drag-over handler. Highlights the entry and
    /// publishes the encoded drop-target key on the ViewModel so the
    /// rest of the page (and the ViewModel) can react.
    /// </summary>
    private void OnSidebarDragOver(object? sender, DragEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is not Border border) return;
        if (border.BindingContext is not NotesListCategory category) return;

        // Publish the encoded key so other UI (e.g. notes list) can
        // coordinate the drop state.
        var key = NotesListViewModel.EncodeDropTarget(category);
        vm.DropTargetKey = key;
        ApplyDropVisual(border, active: true);
        // MAUI's DataPackageOperation only exposes None/Copy (see
        // Microsoft.Maui.Controls DragAndDrop). "Move" is not part of
        // the cross-platform enum; we use Copy so the drop is accepted
        // and the in-app move command runs to relocate the note.
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnSidebarDragLeave(object? sender, DragEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is Border border) ApplyDropVisual(border, active: false);
        vm.DropTargetKey = null;
    }

    private void OnSidebarDrop(object? sender, DropEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is not Border border) return;
        if (border.BindingContext is not NotesListCategory category) return;

        // Pull the dragged note id out of the DataPackage.
        if (!e.Data.Properties.TryGetValue(NotesListViewModel.DragPayloadNoteIdKey, out var raw))
        {
            ResetDropVisual(border);
            vm.NoteDragCompletedCommand.Execute(null);
            return;
        }
        var noteId = raw as string;
        if (string.IsNullOrEmpty(noteId)) { ResetDropVisual(border); return; }

        var key = NotesListViewModel.EncodeDropTarget(category);
        var request = new DropTargetRequest(noteId, key);
        vm.MoveNoteToTargetCommand.Execute(request);
        ResetDropVisual(border);
        vm.NoteDragCompletedCommand.Execute(null);
    }

    /// <summary>
    /// User-defined list drag-over handler. Same pattern as the
    /// built-in entries, but encodes the <see cref="NoteList.Id"/>
    /// instead of the enum value.
    /// </summary>
    private void OnCustomListDragOver(object? sender, DragEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is not Border border) return;
        if (border.BindingContext is not NoteList list) return;

        var key = NotesListViewModel.EncodeDropTarget(list.Id);
        vm.DropTargetKey = key;
        ApplyDropVisual(border, active: true);
        // MAUI's DataPackageOperation only exposes None/Copy; use Copy
        // to accept the drop (see handler in OnSidebarDragOver).
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnCustomListDragLeave(object? sender, DragEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is Border border) ApplyDropVisual(border, active: false);
        vm.DropTargetKey = null;
    }

    private void OnCustomListDrop(object? sender, DropEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is not Border border) return;
        if (border.BindingContext is not NoteList list) return;

        if (!e.Data.Properties.TryGetValue(NotesListViewModel.DragPayloadNoteIdKey, out var raw))
        {
            ResetDropVisual(border);
            vm.NoteDragCompletedCommand.Execute(null);
            return;
        }
        var noteId = raw as string;
        if (string.IsNullOrEmpty(noteId)) { ResetDropVisual(border); return; }

        var key = NotesListViewModel.EncodeDropTarget(list.Id);
        var request = new DropTargetRequest(noteId, key);
        vm.MoveNoteToTargetCommand.Execute(request);
        ResetDropVisual(border);
        vm.NoteDragCompletedCommand.Execute(null);
    }

    // ----- Visual helpers -------------------------------------------

    private static void ApplyDropVisual(Border border, bool active)
    {
        if (active)
        {
            border.BackgroundColor = DropTargetFill;
            border.Stroke = DropTargetStroke;
            border.StrokeThickness = 2;
        }
        else
        {
            ResetDropVisual(border);
        }
    }

    private static void ResetDropVisual(Border border)
    {
        border.BackgroundColor = Colors.Transparent;
        border.StrokeThickness = 0;
    }
}