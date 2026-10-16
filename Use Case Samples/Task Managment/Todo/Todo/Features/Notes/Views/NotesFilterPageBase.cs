using Microsoft.Extensions.Logging;
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
    private bool _hasAppeared;

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

        // Only seed the category on the very first appearance. Re-seeding
        // on every appearance used to clobber a custom-list selection the
        // user had active on My Notes when they tapped away and back
        // (PR #37 review issue #5).
        if (!_hasAppeared)
        {
            _hasAppeared = true;
            _viewModel.SelectedCategory = _category;
        }
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
        catch (Exception ex)
        {
            // PR #37 review issue #5: log the failure instead of
            // silently swallowing it. The ViewModel still reflects the
            // new selection so the UI stays consistent, but the cause
            // is now visible in the device log.
            LoggerFactory
                .Create(b => b.AddDebug())
                .CreateLogger<NotesFilterPageBase>()
                .LogWarning(ex, "Shell navigation to {Route} failed", route);
        }
    }
}
