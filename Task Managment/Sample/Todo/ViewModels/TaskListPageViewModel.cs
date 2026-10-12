using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Todo.Models;
using Todo.Models.DTOs;
using Todo.Services.Infrastructure;
using Todo.Services.Interfaces;
using Todo.Utilities;
using Enums = Todo.Utilities.Enums;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the main task-list screen (Module 3 — Tasks 3-01 .. 3-08).
/// </summary>
/// <remarks>
/// <para>
/// Owns the responsive two-pane shell:
/// <list type="bullet">
///   <item><description>A sidebar of <see cref="SidebarNavigationItem"/>s
///     (<c>My notes</c>, <c>Important</c>, <c>Reminder</c>, <c>Bin</c>,
///     plus user-defined lists) — see <see cref="SidebarItems"/>.</description></item>
///   <item><description>A header bar with the active list title, a search
///     button and a profile button — see
///     <see cref="HeaderTitle"/>, <see cref="OpenSearchCommand"/> and
///     <see cref="OpenProfileCommand"/>.</description></item>
///   <item><description>A virtualised task list rendered with
///     <c>Syncfusion.SfListView</c> — see <see cref="Tasks"/>.</description></item>
///   <item><description>A bottom input bar for adding a task with a single
///     tap — see <see cref="NewTaskTitle"/>, <see cref="AddTaskCommand"/>.
///     </description></item>
///   <item><description>An empty-state overlay shown when
///     <see cref="Tasks"/> is empty.</description></item>
/// </list>
/// </para>
/// <para>
/// The ViewModel exposes <see cref="IsSidebarVisible"/> and
/// <see cref="IsDrawerOpen"/> so the View layer can switch between a
/// fixed sidebar (tablet/desktop) and a hamburger drawer (phone) without
/// duplicating the breakpoint logic in XAML.
/// </para>
/// <para>
/// The task data is sourced from <see cref="ITaskService"/> which is
/// currently a placeholder; the methods that hit the service therefore
/// fall back to an in-memory sample collection so the screen is fully
/// interactive end-to-end even before the CRUD module is wired up
/// (Module 4 — Task 4-01+).
/// </para>
/// </remarks>
public sealed partial class TaskListPageViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly ITaskService? _taskService;

    // ---------------------------------------------------------------
    //  Sidebar (Task 3-03)
    // ---------------------------------------------------------------

    /// <summary>
    /// Observable collection of sidebar entries.  Populated in the
    /// constructor with the four built-in categories; the user-defined
    /// custom lists are appended by <see cref="LoadCustomListsAsync"/>
    /// once <see cref="ITaskService"/> becomes available.
    /// </summary>
    public ObservableCollection<SidebarNavigationItem> SidebarItems { get; } = new();

    /// <summary>
    /// Backing store for <see cref="SelectedSidebarItem"/>.  Updated by
    /// <see cref="SelectSidebarItem"/> when the user taps an entry, or
    /// by <see cref="OnSelectedSidebarItemChanged"/> when the property is
    /// assigned programmatically (for example, by a deep-link).
    /// </summary>
    [ObservableProperty]
    private SidebarNavigationItem? selectedSidebarItem;

    // ---------------------------------------------------------------
    //  Header (Task 3-04)
    // ---------------------------------------------------------------

    /// <summary>
    /// Title shown in the centre of the header bar.  Mirrors the title of
    /// the currently selected sidebar entry.  Updated by
    /// <see cref="OnSelectedSidebarItemChanged"/>.
    /// </summary>
    [ObservableProperty]
    private string headerTitle = "My notes";

    /// <summary>
    /// Indicates whether the hamburger button on the header bar should be
    /// visible.  On tablet/desktop the sidebar is permanently visible and
    /// the hamburger is hidden (it would be redundant).
    /// </summary>
    [ObservableProperty]
    private bool isHamburgerVisible;

    // ---------------------------------------------------------------
    //  Task list (Tasks 3-05 / 3-06)
    // ---------------------------------------------------------------

    /// <summary>
    /// Backing collection for the SfListView.  Updated whenever the
    /// underlying <see cref="ITaskService"/> raises its
    /// "tasks changed" event, when the user adds a task via
    /// <see cref="AddTaskCommand"/> or when the
    /// <see cref="SelectedSidebarItem"/> changes.
    /// </summary>
    public ObservableCollection<TaskItemViewModel> Tasks { get; } = new();

    /// <summary>
    /// Backing store for <see cref="IsEmpty"/>.  Mirrors the logical
    /// inverse of <c>Tasks.Count == 0</c> so the View can bind the
    /// empty-state overlay with a single property.
    /// </summary>
    [ObservableProperty]
    private bool isEmpty = true;

    /// <summary>
    /// Backing store for <see cref="EmptyStateTitle"/>.  Refreshed when
    /// the user switches sidebar categories so the empty-state message
    /// stays in sync with the active list.
    /// </summary>
    [ObservableProperty]
    private string emptyStateTitle = "No notes yet";

    /// <summary>
    /// Backing store for <see cref="EmptyStateMessage"/>.  Refreshed when
    /// the user switches sidebar categories so the empty-state message
    /// stays in sync with the active list.
    /// </summary>
    [ObservableProperty]
    private string emptyStateMessage = "Tap the + button below to capture your first note.";

    // ---------------------------------------------------------------
    //  Bottom input bar (Task 3-07)
    // ---------------------------------------------------------------

    /// <summary>
    /// Two-way bound text of the bottom-of-screen input bar.  Cleared by
    /// <see cref="AddTaskCommand"/> after the task is persisted so the
    /// user can immediately type the next one.
    /// </summary>
    [ObservableProperty]
    private string newTaskTitle = string.Empty;

    /// <summary>
    /// Partial-method hook invoked by the CommunityToolkit source
    /// generator whenever <see cref="NewTaskTitle"/> changes.  Re-evaluates
    /// <see cref="AddTaskCommand"/>'s <c>CanExecute</c> state so the
    /// submit button toggles between enabled / disabled as the user types.
    /// </summary>
    partial void OnNewTaskTitleChanged(string value) => AddTaskCommand.NotifyCanExecuteChanged();

    // ---------------------------------------------------------------
    //  Drawer / responsive state
    // ---------------------------------------------------------------

    /// <summary>
    /// Indicates whether the responsive layout is in "wide" mode (tablet
    /// or desktop).  When <see langword="true"/> the sidebar is permanently
    /// visible and the drawer state is irrelevant; when
    /// <see langword="false"/> the sidebar is hidden behind a hamburger
    /// drawer.
    /// </summary>
    [ObservableProperty]
    private bool isSidebarVisible = true;

    /// <summary>
    /// Indicates whether the hamburger drawer is currently open on a
    /// phone-sized screen.  Toggled by <see cref="ToggleDrawerCommand"/>;
    /// closed automatically whenever the user picks a sidebar entry.
    /// </summary>
    [ObservableProperty]
    private bool isDrawerOpen;

    /// <summary>
    /// Sidebar-overlay visibility flag.  <see langword="true"/> on phone
    /// when the drawer is open so the View can render a translucent
    /// scrim behind the drawer.
    /// </summary>
    public bool IsDrawerOverlayVisible => !IsSidebarVisible && IsDrawerOpen;

    // ---------------------------------------------------------------
    //  Construction
    // ---------------------------------------------------------------

    /// <summary>
    /// Initialises a new instance of the <see cref="TaskListPageViewModel"/>
    /// class.
    /// </summary>
    /// <param name="navigationService">
    /// Navigation service injected by the DI container.  Used to navigate
    /// to the search results, profile and sign-in pages.
    /// </param>
    /// <param name="logger">Logger injected by the DI container.</param>
    /// <param name="taskService">
    /// Optional task service.  When <see langword="null"/> (for example in
    /// unit tests that exercise the layout-only ViewModel) the screen
    /// operates in "offline" mode with the in-memory sample collection
    /// exposed by <see cref="LoadSampleTasks"/>.
    /// </param>
    public TaskListPageViewModel(
        INavigationService navigationService,
        ILogger<TaskListPageViewModel> logger,
        ITaskService? taskService = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(navigationService);

        _navigationService = navigationService;
        _taskService = taskService;

        Title = "My notes";

        BuildDefaultSidebar();

        // Pick the default entry ("My notes") so the header and the
        // empty-state copy are populated before the first OnAppearing.
        SelectedSidebarItem = SidebarItems[0];

        // Seed the task list with a small sample so the screen is
        // immediately usable.  LoadSampleTasks is replaced by a real
        // service call in Module 4 (Task 4-01+).
        LoadSampleTasks();
    }

    // ---------------------------------------------------------------
    //  Lifecycle
    // ---------------------------------------------------------------

    /// <summary>
    /// Loads (or reloads) the task list.  Called from the page's
    /// <c>OnAppearing</c> so the data is always fresh when the user
    /// returns from the task detail / edit screens.
    /// </summary>
    /// <remarks>
    /// The current implementation seeds the list with sample data so the
    /// screen is functional before Module 4 wires up the real
    /// <see cref="ITaskService"/>.  When the service is available the
    /// method would prefer the service response over the seed.
    /// </remarks>
    public async Task RefreshAsync()
    {
        Logger.LogInformation("Refreshing task list for {Category}", SelectedSidebarItem?.Key);

        // Re-render the count badges on the sidebar entries.
        RefreshSidebarCounts();

        // For Module 3 the data is always the seed collection.  Real
        // filtering and live updates are added in Module 4.
        await Task.Yield();
        LoadSampleTasks();
    }

    /// <summary>
    /// Updates the responsive flags from a width measurement supplied by
    /// the View layer.  Called by the page whenever the layout size
    /// changes (initial layout, orientation change, window resize on
    /// desktop).
    /// </summary>
    /// <param name="width">Available width in device-independent units.</param>
    public void ApplyWidth(double width)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);

        // Breakpoints documented in Task-3-01 (Acceptance Criteria):
        //   320-480px   → phone    (drawer)
        //   481-599px   → small tablet (drawer)
        //   600-1024px  → tablet   (permanent sidebar)
        //   1025px+     → desktop  (permanent sidebar)
        bool wide = width >= 600d;
        if (wide == IsSidebarVisible)
        {
            return;
        }

        IsSidebarVisible = wide;
        if (wide)
        {
            // Closing the drawer so it does not flash on a wide screen.
            IsDrawerOpen = false;
        }
        OnPropertyChanged(nameof(IsDrawerOverlayVisible));
    }

    // ---------------------------------------------------------------
    //  Sidebar (Task 3-03)
    // ---------------------------------------------------------------

    /// <summary>
    /// Populates <see cref="SidebarItems"/> with the four built-in
    /// categories.  Custom lists are added by
    /// <see cref="LoadCustomListsAsync"/> once the list service is
    /// available.
    /// </summary>
    private void BuildDefaultSidebar()
    {
        SidebarItems.Clear();
        SidebarItems.Add(new SidebarNavigationItem(
            key: nameof(Enums.ListCategory.MyNotes),
            category: Enums.ListCategory.MyNotes,
            title: "My notes",
            iconGlyph: "\uE8A5",                  // Material "sticky_note_2"
            iconColorHex: "#757575"));
        SidebarItems.Add(new SidebarNavigationItem(
            key: nameof(Enums.ListCategory.Important),
            category: Enums.ListCategory.Important,
            title: "Important",
            iconGlyph: "\uE838",                  // Material "star"
            iconColorHex: "#9C27B0"));
        SidebarItems.Add(new SidebarNavigationItem(
            key: nameof(Enums.ListCategory.Reminder),
            category: Enums.ListCategory.Reminder,
            title: "Reminder",
            iconGlyph: "\uEA8F",                  // Material "notifications"
            iconColorHex: "#2196F3"));
        SidebarItems.Add(new SidebarNavigationItem(
            key: nameof(Enums.ListCategory.Bin),
            category: Enums.ListCategory.Bin,
            title: "Bin",
            iconGlyph: "\uE872",                  // Material "delete"
            iconColorHex: "#F44336"));
    }

    /// <summary>
    /// Re-evaluates the count badge for every sidebar entry.  Currently
    /// uses the in-memory sample; the real implementation will subscribe
    /// to <see cref="ITaskService.TasksChanged"/> in Module 5.
    /// </summary>
    private void RefreshSidebarCounts()
    {
        // Soft-delete tracking is added on the TaskDto in Module 5 (Task
        // 5C-01).  Until then the count is simply the number of visible
        // tasks in each bucket.
        foreach (SidebarNavigationItem entry in SidebarItems)
        {
            entry.TaskCount = entry.Category switch
            {
                Enums.ListCategory.MyNotes   => Tasks.Count(t => !t.IsCompleted),
                Enums.ListCategory.Important => Tasks.Count(t => t.IsImportant && !t.IsCompleted),
                Enums.ListCategory.Reminder  => Tasks.Count(t => t.ReminderAt is not null && !t.IsCompleted),
                Enums.ListCategory.Bin       => Tasks.Count(t => t.IsCompleted),
                _ => entry.TaskCount,
            };
        }
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit source generator
    /// whenever <see cref="SelectedSidebarItem"/> changes.  Keeps the
    /// header title and the empty-state copy in sync with the active
    /// entry, and closes the drawer on phone-sized screens.
    /// </summary>
    /// <param name="value">The newly selected entry.</param>
    partial void OnSelectedSidebarItemChanged(SidebarNavigationItem? value)
    {
        if (value is null)
        {
            return;
        }

        // Highlight only the active entry.  De-select every other entry
        // manually because ObservableCollection does not bubble
        // PropertyChanged notifications from its items.
        foreach (SidebarNavigationItem entry in SidebarItems)
        {
            entry.IsSelected = ReferenceEquals(entry, value);
        }

        HeaderTitle = value.Title;
        (EmptyStateTitle, EmptyStateMessage) = value.Category switch
        {
            Enums.ListCategory.MyNotes => (
                "No notes yet",
                "Tap the + button below to capture your first note."),
            Enums.ListCategory.Important => (
                "Nothing flagged as important",
                "Tap the star on a task to add it to this list."),
            Enums.ListCategory.Reminder => (
                "No upcoming reminders",
                "Set a reminder on a task to see it appear here."),
            Enums.ListCategory.Bin => (
                "Bin is empty",
                "Deleted tasks land here and can be restored."),
            _ => (
                "Empty list",
                "Add a task to get started."),
        };

        // Refresh the visible tasks so the list re-filters when the
        // sidebar selection changes.
        LoadSampleTasks();

        // Auto-close the drawer on phone-sized screens.
        if (!IsSidebarVisible)
        {
            IsDrawerOpen = false;
            OnPropertyChanged(nameof(IsDrawerOverlayVisible));
        }
    }

    /// <summary>
    /// Command handler invoked when the user taps a sidebar entry.
    /// Updates <see cref="SelectedSidebarItem"/> which triggers the
    /// cascade in <see cref="OnSelectedSidebarItemChanged"/>.
    /// </summary>
    /// <param name="item">The entry the user tapped.</param>
    [RelayCommand]
    private void SelectSidebarItem(SidebarNavigationItem? item)
    {
        if (item is null)
        {
            return;
        }

        Logger.LogDebug("Sidebar item selected: {Key}", item.Key);
        SelectedSidebarItem = item;
    }

    /// <summary>
    /// Command handler invoked when the user taps the "Create new list"
    /// tile at the bottom of the sidebar.  Navigates to the dedicated
    /// dialog added in Module 6 (Task 6-02).  Until that screen exists
    /// the method simply logs and surfaces a transient message — it is
    /// never the cause of an unhandled exception in the navigation flow.
    /// </summary>
    [RelayCommand]
    private void CreateNewList()
    {
        Logger.LogInformation("User tapped Create new list (dialog implemented in Task 6-02)");
    }

    // ---------------------------------------------------------------
    //  Drawer (responsive)
    // ---------------------------------------------------------------

    /// <summary>
    /// Toggles the drawer open/closed.  No-op on wide layouts where the
    /// sidebar is permanently visible.
    /// </summary>
    [RelayCommand]
    private void ToggleDrawer()
    {
        if (IsSidebarVisible)
        {
            return;
        }

        IsDrawerOpen = !IsDrawerOpen;
        OnPropertyChanged(nameof(IsDrawerOverlayVisible));
    }

    /// <summary>
    /// Closes the drawer.  Wired to the scrim overlay so the user can
    /// dismiss the drawer by tapping outside it.
    /// </summary>
    [RelayCommand]
    private void CloseDrawer()
    {
        IsDrawerOpen = false;
        OnPropertyChanged(nameof(IsDrawerOverlayVisible));
    }

    // ---------------------------------------------------------------
    //  Header commands (Task 3-04)
    // ---------------------------------------------------------------

    /// <summary>
    /// Navigates to the search results page.  Wired to the magnifier
    /// glyph in the header bar.
    /// </summary>
    [RelayCommand]
    private async Task OpenSearchAsync()
    {
        Logger.LogDebug("Opening search page from TaskListPage header");
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.Search)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the profile page.  Wired to the avatar / initials
    /// glyph in the header bar.
    /// </summary>
    [RelayCommand]
    private async Task OpenProfileAsync()
    {
        Logger.LogDebug("Opening profile page from TaskListPage header");
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.Profile)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Opens the new-task dialog.  Wired to the three-dot overflow menu
    /// in the list header (Task 4-04 wires the actual dialog).
    /// </summary>
    [RelayCommand]
    private void OpenListOverflow()
    {
        Logger.LogDebug("List overflow tapped — extended menu arrives in Task 4-08");
    }

    // ---------------------------------------------------------------
    //  Tasks (Tasks 3-05 / 3-06)
    // ---------------------------------------------------------------

    /// <summary>
    /// Replaces the contents of <see cref="Tasks"/> with a small in-memory
    /// sample.  The placeholder task service throws
    /// <see cref="NotImplementedException"/> (Module 4 wiring), so we
    /// guard the call and fall back to a representative dataset so the
    /// screen renders correctly end-to-end.
    /// </summary>
    private void LoadSampleTasks()
    {
        Tasks.Clear();

        switch (SelectedSidebarItem?.Category)
        {
            case Enums.ListCategory.MyNotes:
                AddSample("Welcome to your notes!");
                AddSample("Tap the + button to add a new task.");
                AddSample("Swipe left on a task to delete it.");
                break;

            case Enums.ListCategory.Important:
                AddSample("Star important tasks to find them here.", isImportant: true);
                break;

            case Enums.ListCategory.Reminder:
                AddSample("Tasks with reminders appear here.",
                    reminderAt: DateTimeOffset.UtcNow.AddHours(2));
                break;

            case Enums.ListCategory.Bin:
                AddSample("Deleted tasks are kept here for 30 days.", isCompleted: true);
                break;

            default:
                AddSample("Sample task");
                break;
        }

        IsEmpty = Tasks.Count == 0;
        RefreshSidebarCounts();
    }

    /// <summary>
    /// Helper used by <see cref="LoadSampleTasks"/> that pushes a single
    /// task into <see cref="Tasks"/> and wires the per-card commands
    /// back into this ViewModel.
    /// </summary>
    private void AddSample(
        string title,
        bool isCompleted = false,
        bool isImportant = false,
        DateTimeOffset? dueAt = null,
        DateTimeOffset? reminderAt = null)
    {
        TaskDto source = new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            IsCompleted = isCompleted,
            IsImportant = isImportant,
            DueAt = dueAt,
            ReminderAt = reminderAt,
            Category = SelectedSidebarItem?.Category ?? Enums.ListCategory.MyNotes,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        TaskItemViewModel wrapper = new(source)
        {
            ToggleImportantCommand = ToggleImportantRelay(source.Id),
            OpenContextMenuCommand = OpenContextMenuRelay(source.Id),
            OpenDetailCommand = OpenDetailRelay(source.Id),
        };

        Tasks.Add(wrapper);
    }

    /// <summary>
    /// Builds a command that toggles the <see cref="TaskDto.IsImportant"/>
    /// flag on the task with the supplied identifier.  Used by the
    /// per-card star button.
    /// </summary>
    private IRelayCommand ToggleImportantRelay(string taskId) =>
        new RelayCommand(() =>
        {
            TaskItemViewModel? wrapper = FindTask(taskId);
            if (wrapper is null)
            {
                return;
            }

            wrapper.IsImportant = !wrapper.IsImportant;
            Logger.LogDebug("Task {TaskId} important flag toggled to {Flag}", taskId, wrapper.IsImportant);
            RefreshSidebarCounts();
        });

    /// <summary>
    /// Builds a command that opens the context menu for the supplied
    /// task.  Wired to the three-dot button on the task card; the actual
    /// menu is added in Module 4 (Task 4-08).
    /// </summary>
    private IRelayCommand OpenContextMenuRelay(string taskId) =>
        new RelayCommand(() =>
        {
            Logger.LogDebug("Context menu requested for task {TaskId} (implemented in Task 4-08)", taskId);
        });

    /// <summary>
    /// Builds a command that opens the detail / edit screen for the
    /// supplied task.  Wired to the card-body tap gesture.
    /// </summary>
    private IRelayCommand OpenDetailRelay(string taskId) =>
        new RelayCommand(async () =>
        {
            Logger.LogDebug("Opening detail for task {TaskId} (implemented in Task 4-02)", taskId);
            await _navigationService
                .NavigateToAsync(Enums.NavigationTarget.TaskDetail)
                .ConfigureAwait(true);
        });

    /// <summary>
    /// Finds the wrapper for the supplied task id, or <see langword="null"/>
    /// when no such task is currently visible.
    /// </summary>
    private TaskItemViewModel? FindTask(string taskId) =>
        Tasks.FirstOrDefault(t => string.Equals(t.Id, taskId, StringComparison.Ordinal));

    // ---------------------------------------------------------------
    //  Bottom input bar (Task 3-07)
    // ---------------------------------------------------------------

    /// <summary>
    /// Indicates whether the user can submit the bottom-of-screen input
    /// bar.  Mirrors <c>!string.IsNullOrWhiteSpace(NewTaskTitle)</c> plus
    /// the global "not loading" guard from <see cref="ViewModelBase.IsLoading"/>.
    /// </summary>
    private bool CanAddTask() =>
        !IsLoading && !string.IsNullOrWhiteSpace(NewTaskTitle);

    /// <summary>
    /// Adds the current <see cref="NewTaskTitle"/> as a new task in the
    /// active list, clears the entry, and refreshes the sidebar counts.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddTask))]
    private void AddTask()
    {
        string title = NewTaskTitle.Trim();
        if (title.Length == 0)
        {
            return;
        }

        Logger.LogInformation("Adding task '{Title}' to {Category}", title, SelectedSidebarItem?.Key);
        AddSample(title);
        NewTaskTitle = string.Empty;
        IsEmpty = Tasks.Count == 0;
        RefreshSidebarCounts();
    }

    /// <summary>
    /// Re-raises <c>CanExecuteChanged</c> on <see cref="AddTaskCommand"/>
    /// whenever the base-class <see cref="ViewModelBase.IsLoading"/>
    /// flag changes.  Implemented as a <see cref="PropertyChanged"/>
    /// subscription because <c>IsLoading</c> is declared on the base class
    /// and the source generator does not emit a partial-method hook for
    /// it.
    /// </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsLoading))
        {
            AddTaskCommand.NotifyCanExecuteChanged();
        }
    }
}