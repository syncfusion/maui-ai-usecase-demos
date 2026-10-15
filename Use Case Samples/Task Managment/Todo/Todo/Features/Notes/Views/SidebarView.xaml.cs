using Todo.Common;
using Todo.Features.CustomLists.Models;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class SidebarView : ContentView
{
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
}