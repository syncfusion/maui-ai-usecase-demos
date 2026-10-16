using Todo.Common;
using Todo.Features.Notes.ViewModels;
using Todo.Features.Profile.Views;

namespace Todo.Features.Notes.Views;

/// <summary>
/// Hosts the application shell: header + sidebar + main content area.
/// The sidebar column collapses to zero width when
/// <see cref="NotesListViewModel.IsSidebarCollapsed"/> is true, so the
/// main content area automatically occupies the full available width.
/// </summary>
public partial class MainLayoutView : ContentView
{
    // Module-08 task 8-1: expanded sidebar matches the wide layout in
    // mynotes.png. Phone layout starts collapsed (matches collapsed.png);
    // the hamburger button in the header toggles between the two.
    private const double ExpandedSidebarWidth = 220;

    private readonly NotesListViewModel _viewModel;
    private readonly INavigationService _navigation;

    public MainLayoutView(NotesListViewModel viewModel)
        : this(viewModel, ResolveNavigationService())
    {
    }

    public MainLayoutView(NotesListViewModel viewModel, INavigationService navigation)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _navigation = navigation;
        BindingContext = viewModel;

        // Module-08 task 8-1: on Phone idiom the XAML sets the sidebar
        // column to 0 (matches collapsed.png). For non-Phone idioms we
        // honor the ViewModel flag so a user who collapsed the sidebar
        // on a tablet sees the same state on the next appearance.
        var startCollapsed = DeviceIdiomMatches(DeviceIdiom.Phone) || viewModel.IsSidebarCollapsed;
        viewModel.IsSidebarCollapsed = startCollapsed;
        UpdateSidebarColumn(startCollapsed);
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(NotesListViewModel.IsSidebarCollapsed))
                UpdateSidebarColumn(viewModel.IsSidebarCollapsed);
        };

        // Wire the header avatar tap to the profile dropdown overlay.
        Header.ProfileTapped += (_, _) => ShowProfileMenu();

        // Wire the toast notification channel.
        viewModel.ShowToastRequested += (_, msg) => _ = Toast.ShowAsync(msg);
    }

    /// <summary>
    /// Swaps the page-specific content view (e.g. a notes list rendered
    /// for a given filter) in the main content area.
    /// </summary>
    public void SetContent(View content) => MainContent.Content = content;

    private void UpdateSidebarColumn(bool isCollapsed)
    {
        // Collapse the sidebar column completely so the main content
        // area occupies the full available width.
        RootGrid.ColumnDefinitions[0].Width = isCollapsed ? new GridLength(0) : new GridLength(ExpandedSidebarWidth);
    }

    private static bool DeviceIdiomMatches(DeviceIdiom idiom) =>
        DeviceInfo.Current.Idiom == idiom;

    private static INavigationService ResolveNavigationService()
    {
        // Fall back to a local navigation service if the host page did
        // not inject one (e.g. design-time / previews).
        return new MauiNavigationService();
    }

    // -----------------------------------------------------------------
    //  Profile menu overlay
    // -----------------------------------------------------------------
    private void ShowProfileMenu()
    {
        var menu = new ProfileMenuView(_navigation, onDismiss: HideProfileMenu);
        ProfileMenuHost.Content = menu;
        ProfileOverlay.IsVisible = true;
    }

    private void HideProfileMenu()
    {
        ProfileOverlay.IsVisible = false;
        ProfileMenuHost.Content = null;
    }

    private void OnProfileOverlayTapped(object? sender, TappedEventArgs e)
    {
        // Dismiss on any tap on the dim background. The menu items
        // route their own taps through the onDismiss callback above so
        // tapping an item never bubbles up to the overlay.
        if (ProfileMenuHost.Content is View)
        {
            // Only dismiss if the tap is not on the menu itself. The
            // XAML places the menu as a child of the overlay, so the
            // tap recognizer on the overlay receives taps that did not
            // hit a control with its own gesture.
            HideProfileMenu();
        }
    }
}
