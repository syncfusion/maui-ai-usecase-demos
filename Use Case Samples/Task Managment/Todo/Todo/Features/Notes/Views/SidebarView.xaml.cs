using Todo.Common;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class SidebarView : ContentView
{
    public SidebarView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Forwards sidebar selection to the centralized ViewModel so it
    /// can navigate between the four built-in pages. The page's
    /// <c>SelectedCategory</c> is updated as a side effect of the
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
}
