using Todo.Common;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

/// <summary>
/// Shared base for the four built-in pages (My Notes, Important, Reminder,
/// Bin). Each page renders the same responsive shell
/// (<see cref="MainLayoutView"/>) hosting the same <see cref="NotesContentView"/>,
/// and is differentiated only by the <see cref="NotesListCategory"/> it
/// represents. All state, commands, and business logic come from the
/// shared singleton <see cref="NotesListViewModel"/>.
/// </summary>
public abstract class NotesFilterPageBase : ContentPage
{
    private readonly NotesListViewModel _viewModel;
    private readonly NotesListCategory _category;
    private MainLayoutView? _layout;

    protected NotesFilterPageBase(NotesListViewModel viewModel, NotesListCategory category)
    {
        _viewModel = viewModel;
        _category = category;

        // The actual visual tree is created in BuildLayout — defer it
        // until the binding context is set so that RelativeSource lookups
        // inside the content view resolve correctly.
        _viewModel.SelectedCategory = category;
        _viewModel.NavigationRequested += OnNavigationRequested;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Lazily build the visual tree on first appearance so the
        // BindingContext has already propagated to the page.
        if (_layout is null)
        {
            _layout = new MainLayoutView(_viewModel)
            {
                BindingContext = _viewModel
            };
            _layout.SetContent(new NotesContentView { BindingContext = _viewModel });
            Content = _layout;
        }

        // Keep the page's filter in sync with the active sidebar selection.
        _viewModel.SelectedCategory = _category;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.NavigationRequested -= OnNavigationRequested;
    }

    private async void OnNavigationRequested(object? sender, NotesListCategory category)
    {
        if (category == _category) return;

        // Navigate to the Shell route corresponding to the requested
        // category. Routes are registered on AppShell.
        var route = category switch
        {
            NotesListCategory.MyNotes => "mynotes",
            NotesListCategory.Important => "important",
            NotesListCategory.Reminder => "reminder",
            NotesListCategory.Bin => "bin",
            _ => "mynotes"
        };

        try
        {
            await Shell.Current.GoToAsync($"//{route}");
        }
        catch
        {
            // Navigation failures are non-fatal in this sample; the
            // ViewModel still reflects the new selection.
        }
    }
}
