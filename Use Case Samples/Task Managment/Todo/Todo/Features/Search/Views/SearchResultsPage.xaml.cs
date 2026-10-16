using Todo.Common;
using Todo.Features.Notes.Models;
using Todo.Features.Search.ViewModels;

namespace Todo.Features.Search.Views;

/// <summary>
/// Search results page (Module-06 task 6-2). Renders a single search
/// input at the top, the empty state from <c>search.png</c> when the
/// query is empty, and a list of matching notes otherwise. Opens
/// modally from the header's search icon (Module-06 task 6-3).
/// </summary>
public partial class SearchResultsPage : ContentPage
{
    public SearchResultsPage(SearchResultsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    /// <summary>
    /// Routes a checkbox toggle on a result card into the ViewModel's
    /// <c>ToggleCompleteCommand</c>. See <c>NotesContentView.OnCheckChanged</c>
    /// for the same pattern in the main list (PR #37 review issue #7).
    /// </summary>
    private void OnCheckChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not BindableObject bindable || bindable.BindingContext is not Note note)
            return;
        if (note.IsCompleted == e.Value) return;
        if (BindingContext is SearchResultsViewModel vm)
            vm.ToggleCompleteCommand.Execute(note);
    }
}
