using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.Services;

namespace Todo.Features.Notes.ViewModels;

/// <summary>
/// Central ViewModel that powers the My Notes, Important, Reminder and
/// Bin filter views. A single instance is registered as a singleton so
/// all four views share the same state, command surface, and notes
/// collection — eliminating duplicate ViewModels and business logic.
/// </summary>
public partial class NotesListViewModel : ViewModelBase
{
    private readonly INoteService _noteService;

    /// <summary>
    /// The currently-displayed list of notes after applying the active
    /// category filter. Bound to <c>SfListView.ItemsSource</c>.
    /// </summary>
    public ObservableCollection<Note> FilteredNotes { get; } = new();

    public NotesListViewModel()
        : this(ServiceLocator.Current)
    {
    }

    public NotesListViewModel(INoteService noteService)
    {
        _noteService = noteService;

        // Listen to the underlying store and refresh the visible list.
        if (_noteService.Notes is INotifyCollectionChanged notifying)
        {
            notifying.CollectionChanged += OnNotesCollectionChanged;
        }
        foreach (var note in _noteService.Notes)
        {
            note.PropertyChanged += OnNotePropertyChanged;
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
    /// Available sidebar entries. Static for Module-02; later modules
    /// can append user-defined lists here.
    /// </summary>
    public IReadOnlyList<NotesListCategory> AvailableCategories { get; } =
        new[] { NotesListCategory.MyNotes, NotesListCategory.Important, NotesListCategory.Reminder, NotesListCategory.Bin };

    partial void OnSelectedCategoryChanged(NotesListCategory value)
    {
        HeaderTitle = value.ToDisplayName();
        ApplyFilter();
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
        OnPropertyChanged(nameof(AccentColorKey));
        OnPropertyChanged(nameof(BackgroundImage));
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
    public string EmptyStateTitle => SelectedCategory switch
    {
        NotesListCategory.MyNotes => "No list was found.",
        NotesListCategory.Important => "No important notes",
        NotesListCategory.Reminder => "No reminders",
        NotesListCategory.Bin => "Bin is empty",
        _ => string.Empty
    };

    /// <summary>
    /// Empty-state subtitle / supporting copy.
    /// </summary>
    public string EmptyStateMessage => SelectedCategory switch
    {
        NotesListCategory.MyNotes => "Please create a to-do list.",
        NotesListCategory.Important => "Tap the star on a note to mark it important.",
        NotesListCategory.Reminder => "Notes with a reminder date will appear here.",
        NotesListCategory.Bin => "Soft-deleted notes can be restored from the Bin.",
        _ => string.Empty
    };

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

        await _noteService.CreateAsync(title);
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
    //  Filtering
    // ---------------------------------------------------------------
    private void ApplyFilter()
    {
        FilteredNotes.Clear();
        foreach (var note in _noteService.GetForCategory(SelectedCategory))
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
        // DeletedAt / ReminderDate of any note changes.
        if (e.PropertyName is nameof(Note.IsImportant)
                              or nameof(Note.IsCompleted)
                              or nameof(Note.DeletedAt)
                              or nameof(Note.ReminderDate)
                              or nameof(Note.DisplayOrder))
        {
            ApplyFilter();
        }
    }
}
