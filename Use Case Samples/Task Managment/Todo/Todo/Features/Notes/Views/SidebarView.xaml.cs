using System.Collections.Specialized;
using System.ComponentModel;
using Todo.Common;
using Todo.Features.CustomLists.Models;
using Todo.Features.CustomLists.Services;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class SidebarView : ContentView
{
    // Strongly-typed colors used for the drop-target highlight. We
    // resolve them once so the drop visual does not have to walk the
    // resource dictionary on every DragOver event.
    private static readonly Color DropTargetFill = Color.FromArgb("#EEEAFB");
    private static readonly Color DropTargetStroke = Color.FromArgb("#5C4EAE");

    private readonly NotesListViewModel? _trackedVm;

    public SidebarView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is NotesListViewModel vm)
        {
            // Re-render the empty-state label when the custom-lists
            // collection is mutated by the host page (Module-08 task 8-3).
            if (vm.CustomLists is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= OnCustomListsChanged;
                ncc.CollectionChanged += OnCustomListsChanged;
            }
        }
    }

    private void OnCustomListsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // The HasCustomLists property drives the visibility of the
        // "Lists" header and the "No lists found" placeholder. The
        // ViewModel already raises PropertyChanged for it; nothing to
        // do here other than make sure we stay subscribed.
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

        if (vm.SelectedCategory == category) return;
        vm.NavigateToCategoryCommand.Execute(category);
    }

    /// <summary>
    /// Forwards selection of a user-defined list (Module-05) to the
    /// centralized ViewModel.
    /// </summary>
    private void OnCustomListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (e.CurrentSelection.FirstOrDefault() is not NoteList list) return;

        if (string.Equals(vm.SelectedListId, list.Id, StringComparison.Ordinal)) return;
        vm.NavigateToListCommand.Execute(list.Id);
    }

    // -----------------------------------------------------------------
    //  Module-07 task 7-2: drag-and-drop drop targets
    // -----------------------------------------------------------------

    private void OnSidebarDragOver(object? sender, DragEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is not Border border) return;
        if (border.BindingContext is not NotesListCategory category) return;

        var key = NotesListViewModel.EncodeDropTarget(category);
        vm.DropTargetKey = key;
        ApplyDropVisual(border, active: true);
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

    private void OnCustomListDragOver(object? sender, DragEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is not Border border) return;
        if (border.BindingContext is not NoteList list) return;

        var key = NotesListViewModel.EncodeDropTarget(list.Id);
        vm.DropTargetKey = key;
        ApplyDropVisual(border, active: true);
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

    // -----------------------------------------------------------------
    //  Sidebar list drag-and-drop reorder
    // -----------------------------------------------------------------

    private void OnCustomListDragStarting(object? sender, DragStartingEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is not BindableObject bindable || bindable.BindingContext is not NoteList list) return;

        // Encode the list id as the drag payload. The drop target is
        // the same CollectionView, so the ViewModel uses its existing
        // encoding to resolve the target.
        e.Data.Properties[NotesListViewModel.DragPayloadListIdKey] = list.Id;
        e.Data.Text = list.Name ?? string.Empty;
    }

    private void OnCustomListDropCompleted(object? sender, DropCompletedEventArgs e)
    {
        // Currently no-op: the actual reorder is done by the
        // CollectionView's own drop handler.
    }

    /// <summary>
    /// Right-click on a sidebar list shows a context menu with
    /// "Rename List" and "Delete List" (per the Microsoft To Do design
    /// intent — also matches the action sheet for the more-icon).
    /// </summary>
    private async void OnCustomListRightTapped(object? sender, PointerEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is not BindableObject bindable || bindable.BindingContext is not NoteList list) return;
        if (e.Button != PointerButtons.Secondary) return;

        var choice = await vm.ShowListRightClickMenuAsync(list);
        // ShowListRightClickMenuAsync already routes the choice; the
        // method exists so the view can stay free of UI knowledge.
        _ = choice;
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
