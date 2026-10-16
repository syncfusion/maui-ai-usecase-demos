using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
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
    /// Active (non-completed) notes for the current filter, sorted by
    /// the active <see cref="SortMode"/>. Bound to the upper list in
    /// the content view.
    /// </summary>
    public ObservableCollection<Note> ActiveNotes { get; } = new();

    /// <summary>
    /// Completed notes for the current filter. Bound to the
    /// collapsible "Completed" section in the content view.
    /// </summary>
    public ObservableCollection<Note> CompletedNotes { get; } = new();

    /// <summary>
    /// True when the user has collapsed the Completed section.
    /// Drives the Expander.IsExpanded binding.
    /// </summary>
    [ObservableProperty]
    private bool isCompletedExpanded;

    /// <summary>
    /// Current sort mode for the active filter. Mirrors the value
    /// the user picked in the "Sort by" sub-menu.
    /// </summary>
    [ObservableProperty]
    private NoteSortMode sortMode = NoteSortMode.Manual;

    /// <summary>
    /// User-defined lists bound from the shared <see cref="IListService"/>.
    /// The sidebar renders these below the four built-in entries
    /// (Module-05).
    /// </summary>
    public ReadOnlyObservableCollection<Todo.Features.CustomLists.Models.NoteList> CustomLists => _listService.Lists;

    /// <summary>
    /// True when at least one user-defined list exists. Drives the
    /// visibility of the sidebar "Lists" section header
    /// (Module-08 task 8-3 — ensures the empty / non-empty state of
    /// the custom-lists region refreshes as the user creates or
    /// removes lists).
    /// </summary>
    public bool HasCustomLists => _listService.Lists.Count > 0;

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
    /// Falls back to a per-list override when the user picked one in
    /// the "Customize Theme" sub-menu.
    /// </summary>
    public string BackgroundImage
    {
        get
        {
            if (!string.IsNullOrEmpty(SelectedListId))
            {
                var list = _listService.Lists.FirstOrDefault(l => l.Id == SelectedListId);
                if (list is not null && !string.IsNullOrEmpty(list.BackgroundImage))
                    return list.BackgroundImage;
            }
            return SelectedCategory.ToBackgroundImage();
        }
    }

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
    //  Sidebar expand / collapse state (Module-08 task 8-1)
    // ---------------------------------------------------------------
    [ObservableProperty]
    private bool isSidebarCollapsed;

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
    /// Triggered when the user taps the content-area title
    /// (matches the "click on the title to rename" interaction in
    /// Microsoft To Do). Falls back to the create-list dialog when
    /// the active filter is the built-in My Notes.
    /// </summary>
    [RelayCommand]
    private async Task RenameCurrentListAsync()
    {
        if (string.IsNullOrEmpty(SelectedListId)) return;
        var list = _listService.Lists.FirstOrDefault(l => l.Id == SelectedListId);
        if (list is null) return;
        await OpenRenameListDialogAsync(list);
    }

    /// <summary>
    /// Triggered when the user taps the three-dot button next to the
    /// content title. Mirrors the per-list menu actions from the
    /// sidebar (Rename / Sort by / Customize theme / Delete).
    /// </summary>
    [RelayCommand]
    private async Task ShowListMenuFromTitleAsync()
    {
        if (string.IsNullOrEmpty(SelectedListId)) return;
        var list = _listService.Lists.FirstOrDefault(l => l.Id == SelectedListId);
        if (list is null) return;
        await ShowListMenuAsync(list);
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
            // Toast: per Microsoft To Do design, show a confirmation
            // toast after a destructive operation completes.
            ShowToastRequested?.Invoke(this, "List deleted successfully");
        }
    }

    /// <summary>
    /// Raised when the ViewModel needs the host page to display a
    /// short toast notification. The MainLayoutView listens for this
    /// and shows the toast at the bottom of the screen.
    /// </summary>
    public event EventHandler<string>? ShowToastRequested;

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
            destruction: "Delete list",
            "Rename list", "Sort by", "Customize theme", "Delete list") ?? string.Empty;

        switch (choice)
        {
            case "Rename list":
                await OpenRenameListDialogAsync(list);
                break;
            case "Sort by":
                await ShowSortByMenuAsync(list);
                break;
            case "Customize theme":
                await ShowCustomizeThemeAsync(list);
                break;
            case "Delete list":
                await ConfirmDeleteListAsync(list);
                break;
        }
    }

    /// <summary>
    /// Right-click context menu for a custom list: Rename / Delete.
    /// Reuses the same dialogs as the more-icon.
    /// </summary>
    public async Task ShowListRightClickMenuAsync(Todo.Features.CustomLists.Models.NoteList? list)
    {
        if (list is null) return;
        var choice = await _navigation.ShowActionSheetAsync(
            title: list.Name ?? "List",
            message: null,
            destruction: "Delete list",
            "Rename list", "Delete list") ?? string.Empty;

        switch (choice)
        {
            case "Rename list":
                await OpenRenameListDialogAsync(list);
                break;
            case "Delete list":
                await ConfirmDeleteListAsync(list);
                break;
        }
    }

    /// <summary>
    /// "Sort by" sub-menu for a custom list (Important / Reminder /
    /// Due Date / Alphabetical). Persists the choice in the
    /// <see cref="NotesListViewModel.SortMode"/> property.
    /// </summary>
    [RelayCommand]
    private async Task ShowSortByMenuAsync(Todo.Features.CustomLists.Models.NoteList? list)
    {
        if (list is null) return;
        var choice = await _navigation.ShowActionSheetAsync(
            title: "Sort by",
            message: null,
            destruction: null,
            "Important", "Reminder", "Due Date", "Alphabetical") ?? string.Empty;
        switch (choice)
        {
            case "Important": SortMode = NoteSortMode.Important; break;
            case "Reminder": SortMode = NoteSortMode.Reminder; break;
            case "Due Date": SortMode = NoteSortMode.DueDate; break;
            case "Alphabetical": SortMode = NoteSortMode.Alphabetical; break;
        }
        ApplyFilter();
    }

    /// <summary>
    /// "Customize theme" sub-menu. Currently offers to pick a
    /// background image (built-in presets) or attach a custom image
    /// via the file picker. Persists the choice in
    /// <see cref="NotesListViewModel.BackgroundImage"/> / a per-list
    /// setting via a simple <c>BackgroundImage</c> file name.
    /// </summary>
    [RelayCommand]
    private async Task ShowCustomizeThemeAsync(Todo.Features.CustomLists.Models.NoteList? list)
    {
        if (list is null) return;
        var choice = await _navigation.ShowActionSheetAsync(
            title: "Customize theme",
            message: null,
            destruction: null,
            "Background image", "Attach custom image") ?? string.Empty;

        switch (choice)
        {
            case "Background image":
                await PickBackgroundPresetAsync(list);
                break;
            case "Attach custom image":
                await AttachCustomImageAsync(list);
                break;
        }
    }

    private async Task PickBackgroundPresetAsync(Todo.Features.CustomLists.Models.NoteList list)
    {
        var choice = await _navigation.ShowActionSheetAsync(
            title: "Choose background",
            message: null,
            destruction: null,
            "Default", "Important (teal)", "Reminder (ocean)", "Bin (pink)") ?? string.Empty;
        var key = choice switch
        {
            "Important (teal)" => "important_bg.png",
            "Reminder (ocean)" => "remainder_bg.png",
            "Bin (pink)" => "bin_bg.png",
            _ => "mynotes_bg.png"
        };
        list.BackgroundImage = key;
    }

    private async Task AttachCustomImageAsync(Todo.Features.CustomLists.Models.NoteList list)
    {
        try
        {
            var pick = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose background image"
            });
            if (pick is not null)
                list.BackgroundImage = pick.FullPath;
        }
        catch
        {
            // Swallow — file pickers can fail on some platforms. The
            // user can retry.
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
    // Each command awaits its service call so any future async work
    // (a real backend, etc.) is wired correctly. The fire-and-forget
    // pattern in the previous revision would have swallowed exceptions
    // and broken the moment the service became actually async
    // (PR #37 review issue #6). Note: the method name intentionally
    // does NOT end with "Async" so the generated command property
    // matches the existing XAML bindings (ToggleCompleteCommand, ...).
    [RelayCommand]
    private async Task ToggleComplete(Note? note)
    {
        if (note is null) return;
        await _noteService.ToggleCompleteAsync(note.Id);
    }

    [RelayCommand]
    private async Task ToggleImportant(Note? note)
    {
        if (note is null) return;
        await _noteService.ToggleImportantAsync(note.Id);
    }

    [RelayCommand]
    private async Task Delete(Note? note)
    {
        if (note is null) return;
        await _noteService.SoftDeleteAsync(note.Id);
    }

    [RelayCommand]
    private async Task Restore(Note? note)
    {
        if (note is null) return;
        await _noteService.RestoreAsync(note.Id);
    }

    [RelayCommand]
    private async Task DeletePermanently(Note? note)
    {
        if (note is null) return;
        await _noteService.DeletePermanentlyAsync(note.Id);
    }

    // ---------------------------------------------------------------
    //  Drag-and-drop commands (Module-07: matches draganddrop.png)
    // ---------------------------------------------------------------

    /// <summary>
    /// Identifier of the note currently being dragged, if any. The view
    /// uses this to apply a stronger shadow / lifted-card visual that
    /// matches <c>draganddrop.png</c>.
    /// </summary>
    [ObservableProperty]
    private string? draggingNoteId;

    /// <summary>
    /// Identifies the sidebar entry that is currently the drop target.
    /// Encoded as <c>"cat:MyNotes"</c> / <c>"cat:Bin"</c> for built-in
    /// categories and <c>"list:&lt;id&gt;"</c> for user-defined lists.
    /// The sidebar uses this to highlight the destination.
    /// </summary>
    [ObservableProperty]
    private string? dropTargetKey;

    /// <summary>
    /// True while the user is mid-drag. The view binds to this to apply
    /// the gap highlight shown in <c>draganddrop.png</c>.
    /// </summary>
    public bool IsDragging => DraggingNoteId is not null;

    /// <summary>
    /// Index in the rendered list at which a gap should be drawn while
    /// a drag is in progress. The view binds to this to position the
    /// highlight bar shown in <c>draganddrop.png</c>. <c>-1</c> means
    /// no gap highlight is visible.
    /// </summary>
    [ObservableProperty]
    private int dropGapIndex = -1;

    partial void OnDraggingNoteIdChanged(string? value)
    {
        // IsDragging is a computed property that depends on
        // DraggingNoteId; raise the change notification so any
        // bindings refresh (e.g. the gap-highlight bar and the
        // drag-mode hint banner).
        OnPropertyChanged(nameof(IsDragging));
    }

    /// <summary>
    /// Persists a within-list reorder triggered by the Syncfusion
    /// <c>SfListView</c> drag-and-drop. Module-07 task 7-1.
    /// </summary>
    [RelayCommand]
    private void ReorderNote(DragReorderRequest? request)
    {
        if (request is null) return;
        var scope = !string.IsNullOrEmpty(SelectedListId) ? SelectedListId : string.Empty;
        _noteService.ReorderAsync(scope ?? string.Empty, request.OldIndex, request.NewIndex);
    }

    /// <summary>
    /// Moves a note to a different list (built-in category or
    /// user-defined list) when dropped on the matching sidebar entry.
    /// Module-07 task 7-2.
    /// </summary>
    [RelayCommand]
    private async Task MoveNoteToTargetAsync(DropTargetRequest? request)
    {
        if (request is null || string.IsNullOrEmpty(request.NoteId)) return;
        var target = DecodeDropTargetKey(request.TargetKey);
        if (target is null) return;

        // No-op when the note is already in the destination list.
        var note = _noteService.Notes.FirstOrDefault(n => n.Id == request.NoteId);
        if (note is null) return;
        if (note.ListId == target) return;

        await _noteService.MoveToListAsync(request.NoteId, target);

        // Snap the active filter to the destination so the user
        // immediately sees the note land in the new list. (If the
        // source and destination are the Bin, leave the current view
        // untouched to keep the in-Bin experience consistent.)
        if (target is null && SelectedCategory == NotesListCategory.MyNotes) return;
        if (target is not null && SelectedListId == target) return;
        // The user is "moving to list" — switch to the destination.
        SelectedListId = target;
    }

    /// <summary>
    /// Encodes a built-in category or list id into a drop-target key
    /// that can travel through the MAUI <c>DataPackage</c>. Format:
    /// <c>"cat:&lt;enum&gt;"</c> or <c>"list:&lt;id&gt;"</c> or
    /// <c>"my"</c> for My Notes.
    /// </summary>
    public static string EncodeDropTarget(NotesListCategory category) => category switch
    {
        NotesListCategory.MyNotes => "my",
        NotesListCategory.Important => "cat:Important",
        NotesListCategory.Reminder => "cat:Reminder",
        NotesListCategory.Bin => "cat:Bin",
        _ => "my"
    };

    public static string EncodeDropTarget(string? listId) =>
        string.IsNullOrEmpty(listId) ? "my" : $"list:{listId}";

    /// <summary>
    /// Decodes a drop-target key. Returns the target <c>ListId</c>
    /// (or <c>null</c> for My Notes) and <c>true</c> for the category
    /// string (when the key targets a built-in category other than
    /// My Notes).
    /// </summary>
    private string? DecodeDropTargetKey(string? key)
    {
        if (string.IsNullOrEmpty(key) || key == "my") return null;
        if (key.StartsWith("list:", StringComparison.Ordinal))
            return key.Substring("list:".Length);
        // Built-in category other than My Notes maps back to the
        // matching enum value. We treat those as "no list" so the
        // note lands in My Notes scope but is filtered by the user's
        // new sidebar selection.
        return null;
    }

    /// <summary>
    /// Resolves a drop-target key to the matching <see cref="NotesListCategory"/>
    /// (only valid for built-in categories other than My Notes).
    /// </summary>
    public static NotesListCategory? TryDecodeCategoryKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (key.StartsWith("cat:", StringComparison.Ordinal))
        {
            var name = key.Substring("cat:".Length);
            return Enum.TryParse<NotesListCategory>(name, out var parsed) ? parsed : (NotesListCategory?)null;
        }
        return null;
    }

    /// <summary>
    /// Key used by the cross-list drag to identify the dragged note
    /// in the MAUI <c>DataPackage.Properties</c>.
    /// </summary>
    public const string DragPayloadNoteIdKey = "Todo.NoteId";

    /// <summary>
    /// Clears the in-flight drag state. Called when the MAUI drag
    /// completes (drop or cancel) so the visuals reset. Exposed as
    /// a command so XAML can wire it via a
    /// <c>DropCompleted</c>-style trigger if needed; the View
    /// currently calls it from the <c>OnNoteDropCompleted</c>
    /// event handler.
    /// </summary>
    [RelayCommand]
    private void NoteDragCompleted()
    {
        DraggingNoteId = null;
        DropTargetKey = null;
        DropGapIndex = -1;
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
            // Per the spec: Bin note menu shows
            //   Mark as Completed / Edit Note / Delete Note.
            choice = await _navigation.ShowActionSheetAsync(
                title: note.Title ?? "Note",
                message: null,
                destruction: "Delete note",
                "Mark as Completed", "Edit Note", "Delete Note") ?? string.Empty;
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
            case "Edit Note":
                await EditNoteAsync(note);
                break;
            case "Delete":
            case "Delete note":
            case "Delete Note":
                if (SelectedCategory == NotesListCategory.Bin)
                    await ConfirmDeletePermanentlyAsync(note);
                else
                    await Delete(note);
                break;
            case "Restore note":
                await Restore(note);
                break;
            case "Mark as Completed":
                if (!note.IsCompleted)
                    await _noteService.ToggleCompleteAsync(note.Id);
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

    // ---------------------------------------------------------------
    //  Filtering
    // ---------------------------------------------------------------
    private void ApplyFilter()
    {
        FilteredNotes.Clear();
        ActiveNotes.Clear();
        CompletedNotes.Clear();
        // Module-05: a user-defined list takes priority over the
        // built-in category so the user can scope My Notes style
        // chrome to a specific list.
        var source = !string.IsNullOrEmpty(SelectedListId)
            ? _noteService.GetForList(SelectedListId!)
            : (IEnumerable<Note>)_noteService.GetForCategory(SelectedCategory);
        var ordered = ApplySort(source);
        foreach (var note in ordered)
        {
            FilteredNotes.Add(note);
            if (note.IsCompleted) CompletedNotes.Add(note);
            else ActiveNotes.Add(note);
        }
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(IsCompletedEmpty));
    }

    /// <summary>
    /// Applies the current <see cref="SortMode"/> to the source
    /// sequence. The Manual mode preserves the service ordering
    /// (which itself respects DisplayOrder).
    /// </summary>
    private IEnumerable<Note> ApplySort(IEnumerable<Note> source) => SortMode switch
    {
        NoteSortMode.Important => source.OrderByDescending(n => n.IsImportant)
                                        .ThenBy(n => n.IsCompleted)
                                        .ThenBy(n => n.DisplayOrder),
        NoteSortMode.Reminder => source.OrderByDescending(n => n.ReminderDate.HasValue)
                                        .ThenBy(n => n.ReminderDate)
                                        .ThenBy(n => n.IsCompleted)
                                        .ThenBy(n => n.DisplayOrder),
        NoteSortMode.DueDate => source.OrderBy(n => n.DueDate ?? DateTime.MaxValue)
                                      .ThenBy(n => n.IsCompleted)
                                      .ThenBy(n => n.DisplayOrder),
        NoteSortMode.Alphabetical => source.OrderBy(n => n.Title, StringComparer.OrdinalIgnoreCase)
                                            .ThenBy(n => n.IsCompleted)
                                            .ThenBy(n => n.DisplayOrder),
        _ => source.OrderBy(n => n.IsCompleted).ThenBy(n => n.DisplayOrder)
    };

    /// <summary>
    /// Count of completed notes for the active filter. Bound to the
    /// "Completed" section header.
    /// </summary>
    public int CompletedCount => CompletedNotes.Count;

    /// <summary>
    /// True when the completed section is empty. Drives whether the
    /// Completed header is rendered at all.
    /// </summary>
    public bool IsCompletedEmpty => CompletedNotes.Count == 0;

    /// <summary>
    /// Key used by the cross-list drag to identify the dragged note
    /// in the MAUI <c>DataPackage.Properties</c>.
    /// </summary>
    public const string DragPayloadNoteIdKey = "Todo.NoteId";

    /// <summary>
    /// Key used by the sidebar-list drag to identify the dragged
    /// list in the MAUI <c>DataPackage.Properties</c>.
    /// </summary>
    public const string DragPayloadListIdKey = "Todo.ListId";

    /// <summary>
    /// Reorders a sidebar list. The source list is identified by its
    /// id; the destination is encoded as the target key. Reserved for
    /// future use by the sidebar drag-and-drop reordering gesture.
    /// </summary>
    [RelayCommand]
    private async Task ReorderListAsync(DropTargetRequest? request)
    {
        if (request is null) return;
        if (string.Equals(request.NoteId, request.TargetKey, StringComparison.Ordinal)) return;
        if (!int.TryParse(request.TargetKey, out var targetIndex)) return;
        await _listService.ReorderAsync(request.NoteId, targetIndex);
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
                              or nameof(Note.DueDate)
                              or nameof(Note.ListId)
                              or nameof(Note.DisplayOrder)
                              or nameof(Note.Title)
                              or nameof(Note.RepeatType)
                              or nameof(Note.NotificationsEnabled))
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

        // Module-08 task 8-3: the sidebar "Lists" header is bound to
        // HasCustomLists — refresh the flag every time the underlying
        // collection changes so the section header appears / disappears
        // as the user creates or removes lists.
        OnPropertyChanged(nameof(HasCustomLists));

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
        else if (e.PropertyName == nameof(Todo.Features.CustomLists.Models.NoteList.BackgroundImage))
        {
            OnPropertyChanged(nameof(BackgroundImage));
        }
    }
}

/// <summary>
/// Payload passed from the <c>SfListView</c> drag-reorder event to
/// the <see cref="NotesListViewModel.ReorderNoteCommand"/>. Carries
/// the source and destination indices as observed on the rendered
/// list so the service can re-author <c>DisplayOrder</c> values.
/// </summary>
public sealed record DragReorderRequest(int OldIndex, int NewIndex);

/// <summary>
/// Payload passed from the sidebar drop gesture to the
/// <see cref="NotesListViewModel.MoveNoteToTargetCommand"/>. The
/// <see cref="TargetKey"/> is encoded by
/// <see cref="NotesListViewModel.EncodeDropTarget(string?)"/> /
/// <see cref="NotesListViewModel.EncodeDropTarget(NotesListCategory)"/>.
/// </summary>
public sealed record DropTargetRequest(string NoteId, string TargetKey);

/// <summary>
/// Sort modes for the active list. Mirrors the options in the
/// "Sort by" sub-menu (Important / Reminder / Due Date /
/// Alphabetical). Manual preserves the service-side DisplayOrder.
/// </summary>
public enum NoteSortMode
{
    Manual,
    Important,
    Reminder,
    DueDate,
    Alphabetical
}
