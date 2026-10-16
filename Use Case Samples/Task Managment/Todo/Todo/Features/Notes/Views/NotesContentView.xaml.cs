using Syncfusion.Maui.ListView;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class NotesContentView : ContentView
{
    public NotesContentView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Routes a tap on the content-area title to the
    /// <c>RenameCurrentListCommand</c> on the ViewModel, so the user
    /// can rename the active list by tapping the title (matches the
    /// Microsoft To Do behavior).
    /// </summary>
    private void OnTitleTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is NotesListViewModel vm
            && vm.RenameCurrentListCommand.CanExecute(null))
        {
            vm.RenameCurrentListCommand.Execute(null);
        }
    }

    // -----------------------------------------------------------------
    //  Checkbox → ToggleComplete command (PR #37 review issue #7)
    // -----------------------------------------------------------------

    /// <summary>
    /// Routes a checkbox toggle on a note card into the ViewModel's
    /// <c>ToggleCompleteCommand</c> so the mutation flows through the
    /// service layer (rather than the data-binding setter on
    /// <c>Note.IsCompleted</c>, which would skip the <c>UpdatedAt</c>
    /// refresh). The IsChecked binding is intentionally OneWay so the
    /// toggle is the only mutation path.
    /// </summary>
    private void OnCheckChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not BindableObject bindable || bindable.BindingContext is not Note note)
            return;

        // Skip the round-trip when the binding-driven refresh of
        // IsChecked arrives at the same value the model already has —
        // otherwise every property change would re-toggle the note.
        if (note.IsCompleted == e.Value) return;

        if (BindingContext is NotesListViewModel vm)
            vm.ToggleCompleteCommand.Execute(note);
    }

    // -----------------------------------------------------------------
    //  Module-07 task 7-2: cross-list drag-and-drop (MAUI gestures)
    // -----------------------------------------------------------------

    /// <summary>
    /// Called when the user long-presses a note card and the MAUI
    /// drag is starting. We set the dragged note id on the
    /// <c>DataPackage</c> so the sidebar <c>DropGestureRecognizer</c>
    /// can resolve the move target, and update the ViewModel state
    /// to show the lifted card visual.
    /// </summary>
    private void OnNoteDragStarting(object? sender, DragStartingEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        if (sender is not BindableObject bindable || bindable.BindingContext is not Note note) return;

        // The MAUI DataPackage is the only channel that travels with
        // the drag. We use a custom key so the sidebar drop handler
        // can read the note id and call the move command.
        e.Data.Properties[NotesListViewModel.DragPayloadNoteIdKey] = note.Id;
        e.Data.Text = note.Title ?? string.Empty;
        vm.DraggingNoteId = note.Id;
    }

    /// <summary>
    /// Called when the MAUI drag completes (drop or cancel). Clears
    /// the in-flight drag state on the ViewModel so the lifted-card
    /// visual and the drop-target highlight disappear.
    /// </summary>
    private void OnNoteDropCompleted(object? sender, DropCompletedEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        vm.NoteDragCompletedCommand.Execute(null);
    }

    /// <summary>
    /// Module-07 task 7-1: handles the in-list drag-reorder lifecycle
    /// from the Syncfusion <c>SfListView</c>. The event fires three
    /// times per gesture — <see cref="DragAction.Start"/> when the
    /// user grabs a card, <see cref="DragAction.Dragging"/> while the
    /// card is being moved, and <see cref="DragAction.Drop"/> when
    /// the gesture completes (whether dropped on the list or
    /// cancelled). We use the ViewModel to track
    /// <c>DraggingNoteId</c> (for the lifted visual) and to
    /// persist the new order on drop.
    /// </summary>
    private void OnItemDragging(object? sender, ItemDraggingEventArgs e)
    {
        if (BindingContext is not NotesListViewModel vm) return;
        switch (e.Action)
        {
            case DragAction.Start:
                if (e.DataItem is Note note)
                {
                    vm.DraggingNoteId = note.Id;
                    vm.DropGapIndex = -1;
                }
                break;
            case DragAction.Dragging:
                vm.DropGapIndex = e.NewIndex;
                break;
            case DragAction.Drop:
                // Persist the new ordering. The SfListView has
                // already moved the item in the source collection;
                // we just need to re-author DisplayOrder so the new
                // order survives re-filtering.
                var request = new DragReorderRequest(e.OldIndex, e.NewIndex);
                vm.ReorderNoteCommand.Execute(request);

                // Gesture finished — clear the in-flight state so
                // the lifted card visual and the gap highlight
                // disappear.
                ResetDrag(vm);
                break;
        }
    }

    private static void ResetDrag(NotesListViewModel vm)
    {
        vm.DraggingNoteId = null;
        vm.DropGapIndex = -1;
        vm.DropTargetKey = null;
    }
}
