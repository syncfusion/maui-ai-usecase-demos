using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;
using Todo.Features.CustomLists.Services;
using Todo.Features.Dialogs.ViewModels;
using Todo.Features.Dialogs.Views;
using Todo.Features.EditNote.ViewModels;
using Todo.Features.EditNote.Views;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.Services;
using Todo.Features.Search.ViewModels;
using Todo.Features.Search.Views;

namespace Todo.Features.Notes.ViewModels;

/// <summary>
/// Central ViewModel that powers the My Notes, Important, Reminder and
/// Bin filter views. A single instance is registered as a singleton so
/// all four views share the same state, command surface, and notes
/// collection — eliminating duplicate ViewModels and business logic.
///
/// <para>
/// Module-03 adds Create / Edit / Soft-delete / Restore / Permanent-delete
/// flows on top of the Module-02 list-and-filter shell. All dialogs are
/// pushed through <see cref="INavigationService"/>; the ViewModel itself
/// stays UI-framework agnostic.
/// </para>
/// </summary>
public partial class NotesListViewModel : ViewModelBase
{
    private readonly INoteService _noteService;
    private readonly IListService _listService;
    private readonly INavigationService _navigation;

    /// <summary>
    /// The currently-displayed list of notes after applying the active
    /// category filter. Bound to <c>SfListView.ItemsSource</c>.
    /// </summary>
    public ObservableCollection<Note> FilteredNotes { get; } = new();

    public NotesListViewModel()
        : this(ServiceLocator.Current,
               ServiceLocator.CurrentListService,
               new MauiNavigationService())
    {
    }

    public NotesListViewModel(INoteService noteService, IListService listService, INavigationService navigation)
    {
        _noteService = noteService;
        _listService = listService;
        _navigation = navigation;

        // Listen to the underlying store and refresh the visible list.
        if (_noteService.Notes is INotifyCollectionChanged notifying)
        {
            notifying.CollectionChanged += OnNotesCollectionChanged;
        }
        foreach (var note in _noteService.Notes)
        {
            note.PropertyChanged += OnNotePropertyChanged;
        }

        // Re-filter when a user-defined list is renamed (header title
        // changes) or removed (the user may have been viewing it).
        if (_listService.Lists is INotifyCollectionChanged listNotifying)
        {
            listNotifying.CollectionChanged += OnListsCollectionChanged;
        }
        foreach (var list in _listService.Lists)
        {
            list.PropertyChanged += OnListPropertyChanged;
        }

        ApplyFilter();
    }

    // ---------------------------------------------------------------
    //  Category filter state (sidebar selection)
    // ---------------------------------------------------------------
    [ObservableProperty]
    private NotesListCategory selectedCategory;

    [ObservableProperty]
    private string headerTitle = NotesListCategory.MyNotes.ToDisplayName();

    /// <summary>
    /// Identifier of the user-defined list currently being shown.
    /// <c>null</c> means a built-in category is active (My Notes,
    /// Important, Reminder, Bin). Set by the sidebar when the user
    /// selects a custom list (Module-05).
    /// </summary>
    [ObservableProperty]
    private string? selectedListId;

    /// <summary>
    /// Available sidebar entries. Static for Module-02; later modules
    /// can append user-defined lists here.
    /// </summary>
    public IReadOnlyList<NotesListCategory> AvailableCategories { get; } =
        new[] { NotesListCategory.MyNotes, NotesListCategory.Important, NotesListCategory.Reminder, NotesListCategory.Bin };

    /// <summary>
    /// User-defined lists bound from the shared <see cref="IListService"/>.
    /// The sidebar renders these below the four built-in entries
    /// (Module-05).
    /// </summary>
    public ReadOnlyObservableCollection<Todo.Features.CustomLists.Models.NoteList> CustomLists => _listService.Lists;

    /// <summary>
    /// Indicates whether the active filter is a user-defined list
    /// (rather than a built-in category). Drives header copy and
    /// background image selection.
    /// </summary>
    public bool IsCustomListSelected => !string.IsNullOrEmpty(SelectedListId);

    partial void OnSelectedCategoryChanged(NotesListCategory value)
    {
        // Selecting a built-in category clears any active custom list.
        SelectedListId = null;
        HeaderTitle = value.ToDisplayName();
        ApplyFilter();
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
        OnPropertyChanged(nameof(AccentColorKey));
        OnPropertyChanged(nameof(BackgroundImage));
    }

    partial void OnSelectedListIdChanged(string? value)
    {
        // Selecting a custom list falls back to My Notes chrome (the
        // four built-in backgrounds are the documented design tokens).
        HeaderTitle = string.IsNullOrEmpty(value)
            ? SelectedCategory.ToDisplayName()
            : ResolveCustomListName(value) ?? "List";
        ApplyFilter();
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
        OnPropertyChanged(nameof(IsCustomListSelected));
        OnPropertyChanged(nameof(AccentColorKey));
        OnPropertyChanged(nameof(BackgroundImage));
    }

    private string? ResolveCustomListName(string listId)
        => _listService.Lists.FirstOrDefault(l => l.Id == listId)?.Name;

    /// <summary>
    /// Resets <see cref="SelectedListId"/> back to <c>null</c> when the
    /// currently-displayed list has just been deleted. Called by the
    /// sidebar delete flow (Module-05).
    /// </summary>
    public void ClearSelectedListIfDeleted(string deletedListId)
    {
        if (SelectedListId == deletedListId)
            SelectedListId = null;
    }

    /// <summary>
    /// Resource key for the sidebar accent color of the active filter.
    /// </summary>
    public string AccentColorKey => SelectedCategory.ToAccentColorKey();

    /// <summary>
    /// Filename of the background image to display behind the list.
    /// </summary>
    public string BackgroundImage => SelectedCategory.ToBackgroundImage();

    // ---------------------------------------------------------------
    //  Empty-state copy (matched to design)
    // ---------------------------------------------------------------
    /// <summary>
    /// Empty-state title text (varies by filter)
    /// </summary>
    public string EmptyStateTitle
    {
        get
        {
            if (!string.IsNullOrEmpty(SelectedListId))
            {
                var name = ResolveCustomListName(SelectedListId!) ?? "this list";
                return $"No notes in '{name}'";
            }
            return SelectedCategory switch
            {
                NotesListCategory.MyNotes => "No list was found.",
                NotesListCategory.Important => "No important notes",
                NotesListCategory.Reminder => "No reminders",
                NotesListCategory.Bin => "Bin is empty",
                _ => string.Empty
            };
        }
    }

    /// <summary>
    /// Empty-state subtitle / supporting copy.
    /// </summary>
    public string EmptyStateMessage
    {
        get
        {
            if (!string.IsNullOrEmpty(SelectedListId))
                return "Add a note using the input below.";
            return SelectedCategory switch
            {
                NotesListCategory.MyNotes => "Please create a to-do list.",
                NotesListCategory.Important => "Tap the star on a note to mark it important.",
                NotesListCategory.Reminder => "Notes with a reminder date will appear here.",
                NotesListCategory.Bin => "Soft-deleted notes can be restored from the Bin.",
                _ => string.Empty
            };
        }
    }

    public bool IsEmpty => FilteredNotes.Count == 0;

    // ---------------------------------------------------------------
    //  Sidebar expand / collapse state
    // ---------------------------------------------------------------
    [ObservableProperty]
    private bool isSidebarCollapsed;

    /// <summary>
    /// Expanded sidebar width (used by the responsive grid column
    /// defined in <c>MainLayoutView.xaml</c>).
    /// </summary>
    public const double SidebarExpandedWidth = 220;

    [RelayCommand]
    private void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

    // ---------------------------------------------------------------
    //  Navigation between the four built-in pages
    // ---------------------------------------------------------------
    /// <summary>
    /// Raised when the user selects a sidebar entry. The host page
    /// listens for this to perform Shell navigation between the four
    /// built-in pages while reusing the same ViewModel state.
    /// </summary>
    public event EventHandler<NotesListCategory>? NavigationRequested;

    [RelayCommand]
    private void NavigateToCategory(NotesListCategory? category)
    {
        if (category is null) return;
        // The active category is always kept in sync, even if the host
        // doesn't navigate (so the sidebar selection / header text
        // remain consistent with the destination page).
        SelectedCategory = category.Value;
        NavigationRequested?.Invoke(this, category.Value);
    }

    /// <summary>
    /// Selects a user-defined list (Module-05). Custom lists reuse
    /// the My Notes chrome — they don't have a separate route — so
    /// this only updates <see cref="SelectedListId"/>.
    /// </summary>
    [RelayCommand]
    private void NavigateToList(string? listId)
    {
        if (string.IsNullOrEmpty(listId)) return;
        SelectedListId = listId;
    }

    // ---------------------------------------------------------------
    //  Custom-list commands (Module-05: create / rename / delete)
    // ---------------------------------------------------------------

    /// <summary>
    /// Opens the centered create-list dialog (matches
    /// <c>createnewlist.png</c>) and adds the new list on confirm.
    /// </summary>
    [RelayCommand]
    private async Task OpenCreateListDialogAsync()
    {
        var vm = new Todo.Features.CustomLists.ViewModels.CreateListDialogViewModel(_listService, _navigation);
        var page = new Todo.Features.CustomLists.Views.CreateListDialogPage(vm);
        await _navigation.PushModalAsync(page);
    }

    /// <summary>
    /// Opens the centered rename-list dialog (matches
    /// <c>renamelist.png</c>) pre-populated with the current name.
    /// </summary>
    [RelayCommand]
    private async Task OpenRenameListDialogAsync(Todo.Features.CustomLists.Models.NoteList? list)
    {
        if (list is null) return;
        var vm = new Todo.Features.CustomLists.ViewModels.RenameListDialogViewModel(_listService, _navigation, list);
        var page = new Todo.Features.CustomLists.Views.RenameListDialogPage(vm);
        await _navigation.PushModalAsync(page);
    }

    /// <summary>
    /// Shows a confirmation dialog and removes the list on confirm.
    /// Notes in the deleted list are returned to My Notes.
    /// </summary>
    [RelayCommand]
    private async Task ConfirmDeleteListAsync(Todo.Features.CustomLists.Models.NoteList? list)
    {
        if (list is null) return;

        var vm = new ConfirmDialogViewModel(_navigation)
        {
            Title = "Delete list?",
            Message = $"The '{list.Name}' list will be removed. Notes in this list will move back to My Notes.",
            ConfirmText = "Delete",
            IsDestructive = true
        };
        var page = new ConfirmDialogPage(vm);
        var result = await _navigation.PushModalAsync(page);
        if (result is true)
        {
            await _listService.DeleteAsync(list.Id);
            ClearSelectedListIfDeleted(list.Id);
        }
    }

    /// <summary>
    /// Three-dot "more" menu on a custom list entry. Opens a native
    /// action sheet with Rename / Delete options.
    /// </summary>
    [RelayCommand]
    private async Task ShowListMenuAsync(Todo.Features.CustomLists.Models.NoteList? list)
    {
        if (list is null) return;

        var choice = await _navigation.ShowActionSheetAsync(
            title: list.Name ?? "List",
            message: null,
            destruction: "Delete",
            "Rename list", "Delete") ?? string.Empty;

        switch (choice)
        {
            case "Rename list":
                await OpenRenameListDialogAsync(list);
                break;
            case "Delete":
                await ConfirmDeleteListAsync(list);
                break;
        }
    }

    // ---------------------------------------------------------------
    //  Search (Module-06)
    // ---------------------------------------------------------------

    /// <summary>
    /// Opens the Search results page (matches <c>search.png</c>) as a
    /// modal. The page has its own ViewModel so the global notes
    /// state is not polluted by the search query.
    /// </summary>
    [RelayCommand]
    private async Task OpenSearchAsync()
    {
        var searchService = new Todo.Features.Search.Services.InMemoryNoteSearchService(_noteService);
        var page = new SearchResultsPage(new SearchResultsViewModel(searchService, _noteService, _navigation));
        await _navigation.PushModalAsync(page);
    }

    // ---------------------------------------------------------------
    //  Bottom-input bar state
    // ---------------------------------------------------------------
    [ObservableProperty]
    private string newNoteTitle = string.Empty;

    partial void OnNewNoteTitleChanged(string value)
    {
        AddNoteCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanAddNote))]
    private async Task AddNoteAsync()
    {
        var title = (NewNoteTitle ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(title))
            return;

        // Notes created from the bottom input are attached to the
        // currently-active custom list, if any (Module-05).
        await _noteService.CreateAsync(title, listId: SelectedListId);
        NewNoteTitle = string.Empty;
    }

    private bool CanAddNote() => !string.IsNullOrWhiteSpace(NewNoteTitle);

    // ---------------------------------------------------------------
    //  Item-template commands (check / star / delete / restore)
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
    private void Delete(Note? note)
    {
        if (note is null) return;
        _noteService.SoftDeleteAsync(note.Id);
    }

    [RelayCommand]
    private void Restore(Note? note)
    {
        if (note is null) return;
        _noteService.RestoreAsync(note.Id);
    }

    [RelayCommand]
    private void DeletePermanently(Note? note)
    {
        if (note is null) return;
        _noteService.DeletePermanentlyAsync(note.Id);
    }

    // ---------------------------------------------------------------
    //  Dialog / action sheet commands (Module-03)
    // ---------------------------------------------------------------

    /// <summary>
    /// Opens the EditNote dialog (matches the centered
    /// <c>createnewlist.png</c> / <c>renamelist.png</c> chrome) to
    /// create a brand new note. Falls back to inline create when the
    /// navigation service is unavailable (e.g. design-time).
    /// </summary>
    [RelayCommand]
    private async Task OpenCreateNoteAsync()
    {
        var page = new EditNotePage(new EditNoteViewModel(_noteService, _navigation));
        await _navigation.PushModalAsync(page);
    }

    /// <summary>
    /// Tapping a note card opens the EditNote dialog pre-filled with
    /// that note's values.
    /// </summary>
    [RelayCommand]
    private async Task EditNoteAsync(Note? note)
    {
        if (note is null) return;
        var page = new EditNotePage(new EditNoteViewModel(_noteService, _navigation, note));
        await _navigation.PushModalAsync(page);
    }

    /// <summary>
    /// Three-dot "more" menu on a note card. Opens a native action
    /// sheet whose items depend on the active filter (active notes
    /// show "Edit note" / "Delete"; bin shows "Restore" /
    /// "Delete permanently").
    /// </summary>
    [RelayCommand]
    private async Task ShowNoteMenuAsync(Note? note)
    {
        if (note is null) return;

        string choice;
        if (SelectedCategory == NotesListCategory.Bin)
        {
            choice = await _navigation.ShowActionSheetAsync(
                title: note.Title ?? "Note",
                message: null,
                destruction: "Delete permanently",
                "Restore note", "Delete permanently") ?? string.Empty;
        }
        else
        {
            choice = await _navigation.ShowActionSheetAsync(
                title: note.Title ?? "Note",
                message: null,
                destruction: "Delete",
                "Edit note", "Delete") ?? string.Empty;
        }

        switch (choice)
        {
            case "Edit note":
                await EditNoteAsync(note);
                break;
            case "Delete":
                Delete(note);
                break;
            case "Restore note":
                Restore(note);
                break;
            case "Delete permanently":
                await ConfirmDeletePermanentlyAsync(note);
                break;
        }
    }

    /// <summary>
    /// Shows the destructive confirmation dialog (matches
    /// <c>deletepermently.png</c>) and permanently deletes the note
    /// on confirm.
    /// </summary>
    [RelayCommand]
    private async Task ConfirmDeletePermanentlyAsync(Note? note)
    {
        if (note is null) return;
        var vm = new ConfirmDialogViewModel(_navigation)
        {
            Title = "Delete permanently?",
            Message = "This note will be permanently deleted and cannot be recovered.",
            ConfirmText = "Delete",
            IsDestructive = true
        };
        var page = new ConfirmDialogPage(vm);
        var result = await _navigation.PushModalAsync(page);
        if (result is true)
        {
            await _noteService.DeletePermanentlyAsync(note.Id);
        }
    }

    //  Context-menu actions (more button on each card)
    // ---------------------------------------------------------------

    /// <summary>
    /// Shows the permanent-delete confirmation dialog (per
    /// <c>deletepermently.png</c>). When the user confirms, the note is
    /// removed from the in-memory store. When the user cancels, the note
    /// is left untouched.
    /// </summary>
    [RelayCommand]
    private async Task RequestDeletePermanentlyAsync(Note? note)
    {
        if (note is null) return;
        if (ConfirmDeletePermanently is null)
        {
            // No dialog hook available – fall back to direct delete so
            // the rest of the flow still works.
            await _noteService.DeletePermanentlyAsync(note.Id);
            return;
        }

        var confirmed = await ConfirmDeletePermanently.Invoke(note);
        if (confirmed)
            await _noteService.DeletePermanentlyAsync(note.Id);
    }

    /// <summary>
    /// Hook used by the View to show the permanent-delete dialog.
    /// Returns <c>true</c> if the user confirmed the deletion.
    /// </summary>
    public Func<Note, Task<bool>>? ConfirmDeletePermanently { get; set; }

    // ---------------------------------------------------------------
    //  Filtering
    // ---------------------------------------------------------------
    private void ApplyFilter()
    {
        FilteredNotes.Clear();
        // Module-05: a user-defined list takes priority over the
        // built-in category so the user can scope My Notes style
        // chrome to a specific list.
        var source = !string.IsNullOrEmpty(SelectedListId)
            ? _noteService.GetForList(SelectedListId!)
            : (IEnumerable<Note>)_noteService.GetForCategory(SelectedCategory);
        foreach (var note in source)
            FilteredNotes.Add(note);
    }

    private void OnNotesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (Note n in e.NewItems)
                n.PropertyChanged += OnNotePropertyChanged;
        if (e.OldItems is not null)
            foreach (Note n in e.OldItems)
                n.PropertyChanged -= OnNotePropertyChanged;

        ApplyFilter();
        OnPropertyChanged(nameof(IsEmpty));
    }

    private void OnNotePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Re-evaluate the filter when the IsImportant / IsCompleted /
        // DeletedAt / ReminderDate / ListId of any note changes.
        if (e.PropertyName is nameof(Note.IsImportant)
                              or nameof(Note.IsCompleted)
                              or nameof(Note.DeletedAt)
                              or nameof(Note.ReminderDate)
                              or nameof(Note.ListId)
                              or nameof(Note.DisplayOrder))
        {
            ApplyFilter();
        }
    }

    private void OnListsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (Todo.Features.CustomLists.Models.NoteList l in e.NewItems)
                l.PropertyChanged += OnListPropertyChanged;
        if (e.OldItems is not null)
            foreach (Todo.Features.CustomLists.Models.NoteList l in e.OldItems)
                l.PropertyChanged -= OnListPropertyChanged;

        // If the currently-active list has been removed, snap back to
        // the built-in My Notes view.
        if (!string.IsNullOrEmpty(SelectedListId)
            && _listService.Lists.All(l => l.Id != SelectedListId))
        {
            SelectedListId = null;
        }
        else
        {
            // Otherwise the header title may need a refresh.
            OnPropertyChanged(nameof(HeaderTitle));
            OnPropertyChanged(nameof(EmptyStateTitle));
        }
    }

    private void OnListPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Todo.Features.CustomLists.Models.NoteList.Name))
        {
            OnPropertyChanged(nameof(HeaderTitle));
            OnPropertyChanged(nameof(EmptyStateTitle));
        }
    }
}
