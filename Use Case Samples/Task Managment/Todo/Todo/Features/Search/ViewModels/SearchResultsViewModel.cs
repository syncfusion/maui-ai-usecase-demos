using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;
using Todo.Features.EditNote.ViewModels;
using Todo.Features.EditNote.Views;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.Services;
using Todo.Features.Search.Services;

namespace Todo.Features.Search.ViewModels;

/// <summary>
/// ViewModel for the Search results page (Module-06 task 6-2). The
/// page is a thin shell around a single search input and an
/// <see cref="ObservableCollection{T}"/> of matching notes — the
/// design is captured in <c>search.png</c>.
///
/// <para>
/// The ViewModel owns the search query, the filtered results, and the
/// "clear" affordance. It delegates the actual matching to
/// <see cref="INoteSearchService"/> so the same search rules can be
/// shared with any other surface that might need them later.
/// </para>
/// </summary>
public partial class SearchResultsViewModel : ViewModelBase
{
    private readonly INoteSearchService _searchService;
    private readonly INoteService _noteService;
    private readonly INavigationService _navigation;

    public SearchResultsViewModel()
        : this(new InMemoryNoteSearchService(ServiceLocator.Current),
               ServiceLocator.Current,
               ServiceLocator.CurrentNavigation)
    {
    }

    public SearchResultsViewModel(
        INoteSearchService searchService,
        INoteService noteService,
        INavigationService navigation)
    {
        _searchService = searchService;
        _noteService = noteService;
        _navigation = navigation;
        Results = new ObservableCollection<Note>();
    }

    /// <summary>
    /// Currently-displayed search results. Bound to the list in
    /// <c>SearchResultsPage.xaml</c>.
    /// </summary>
    public ObservableCollection<Note> Results { get; }

    /// <summary>
    /// True when the query is non-empty and there are no matching
    /// notes. Drives the "no results" empty state.
    /// </summary>
    public bool HasNoResults => !string.IsNullOrWhiteSpace(Query) && Results.Count == 0;

    /// <summary>
    /// True when the query is empty. Drives the "You can search the
    /// created notes here" empty state from <c>search.png</c>.
    /// </summary>
    public bool IsQueryEmpty => string.IsNullOrWhiteSpace(Query);

    [ObservableProperty]
    private string query = string.Empty;

    partial void OnQueryChanged(string value)
    {
        RefreshResults();
    }

    [RelayCommand]
    private void ClearQuery()
    {
        Query = string.Empty;
    }

    /// <summary>
    /// Re-runs the search and replaces <see cref="Results"/> with the
    /// fresh set of matches. Called whenever the query text changes.
    /// </summary>
    private void RefreshResults()
    {
        Results.Clear();
        foreach (var match in _searchService.Search(Query))
            Results.Add(match);

        OnPropertyChanged(nameof(HasNoResults));
        OnPropertyChanged(nameof(IsQueryEmpty));
    }

    // ---------------------------------------------------------------
    //  Per-result commands
    // ---------------------------------------------------------------

    [RelayCommand]
    private void ToggleComplete(Note? note)
    {
        if (note is null) return;
        _noteService.ToggleCompleteAsync(note.Id);
    }

    [RelayCommand]
    private void ToggleImportant(Note? note)
    {
        if (note is null) return;
        _noteService.ToggleImportantAsync(note.Id);
    }

    [RelayCommand]
    private async Task OpenNoteAsync(Note? note)
    {
        if (note is null) return;
        var page = new EditNotePage(new EditNoteViewModel(_noteService, _navigation, note));
        await _navigation.PushModalAsync(page);
    }
}
