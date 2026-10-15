using Todo.Common;
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
}
