using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace WardrobeAI.ViewModels;

/// <summary>
/// Identifies which desktop split-view pane is currently visible inside
/// <see cref="Views.DesktopSplitHostPage"/>. The host has exactly two
/// panes — Dashboard+Shopping and Closet+Planner — and the side rail
/// flips between them via <see cref="DesktopSplitHostViewModel.SelectedPane"/>.
/// </summary>
public enum DesktopSplitPane
{
    /// <summary>
    /// Dashboard column + Smart Shopping column.
    /// </summary>
    DashboardShopping = 0,

    /// <summary>
    /// Closet column + Outfit Planner column.
    /// </summary>
    ClosetPlanner = 1,
}

/// <summary>
/// View model for the persistent desktop host page. Drives the side rail
/// selection and the profile-popup bindings (which were previously
/// duplicated inside <c>DashboardShoppingSplitPage</c> and
/// <c>ClosetPlannerSplitPage</c>).
///
/// <para>This VM owns:
/// <list type="bullet">
///   <item><see cref="SelectedPane"/> — the currently visible pane;
///     <see cref="IsDashboardShoppingPaneVisible"/> and
///     <see cref="IsClosetPlannerPaneVisible"/> are derived computed
///     properties the XAML binds to via <c>IsVisible</c>.</item>
///   <item><see cref="IsProfilePopupOpen"/> / <see cref="ToggleProfilePopupCommand"/>
///     — the profile popup overlay declared in
///     <c>DesktopSplitHostPage.xaml</c> binds to these.</item>
///   <item><see cref="HasProfileImage"/>, <see cref="ProfileImageBase64"/>,
///     <see cref="InitialsAvatarName"/>, <see cref="ProfileStyleBadge"/> —
///     forwarded from a <see cref="DashboardViewModel"/> resolved once
///     at startup so the side rail and popup share the same source of
///     truth as before.</item>
/// </list>
/// </para>
///
/// <para>The two split-view pages
/// (<see cref="Views.DashboardShoppingSplitPage"/> and
/// <see cref="Views.ClosetPlannerSplitPage"/>) no longer carry any of
/// this state — they're now just the two-column content panes and are
/// reparented once into the host's right-hand column.</para>
/// </summary>
public partial class DesktopSplitHostViewModel : ObservableObject
{
    private readonly DashboardViewModel _dashboard;
    private readonly ILogger<DesktopSplitHostViewModel> _logger;

    private DesktopSplitPane _selectedPane = DesktopSplitPane.DashboardShopping;
    private bool _isProfilePopupOpen;

    /// <summary>
    /// Raised when the host switches panes so the host code-behind can
    /// re-fire the active pane's refresh (replacing the per-page
    /// <c>OnAppearing</c> the old split views relied on for that).
    /// </summary>
    public event EventHandler<DesktopSplitPane>? PaneChanged;

    public DesktopSplitHostViewModel(
        DashboardViewModel dashboard,
        ILogger<DesktopSplitHostViewModel> logger)
    {
        _dashboard = dashboard;
        _logger = logger;

        // Forward profile-popup state from the dashboard VM so the
        // side rail + popup + side-rail avatar all stay in lock-step
        // with the same source of truth. The dashboard VM is shared
        // with the DashboardShoppingPane (it reparents the same
        // DashboardPage instance), so the user's avatar / name in the
        // side rail are guaranteed to match the dashboard column.
        _dashboard.PropertyChanged += OnDashboardPropertyChanged;

        ToggleProfilePopupCommand = new RelayCommand(() => IsProfilePopupOpen = !IsProfilePopupOpen);
        SwitchPaneCommand = new RelayCommand<DesktopSplitPane>(pane => SelectedPane = pane);
    }

    // ---- Pane selection ----------------------------------------------------

    public DesktopSplitPane SelectedPane
    {
        get => _selectedPane;
        set
        {
            if (_selectedPane == value)
            {
                return;
            }
            _selectedPane = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDashboardShoppingPaneVisible));
            OnPropertyChanged(nameof(IsClosetPlannerPaneVisible));

            try
            {
                PaneChanged?.Invoke(this, value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Desktop split pane switch handler threw.");
            }
        }
    }

    /// <summary>
    /// Bound to <c>IsVisible</c> on the
    /// <c>DashboardShoppingSplitPage</c> pane. Computed rather than a
    /// stored bool so it can never desync from
    /// <see cref="SelectedPane"/>.
    /// </summary>
    public bool IsDashboardShoppingPaneVisible => _selectedPane == DesktopSplitPane.DashboardShopping;

    /// <summary>
    /// Bound to <c>IsVisible</c> on the
    /// <c>ClosetPlannerSplitPage</c> pane.
    /// </summary>
    public bool IsClosetPlannerPaneVisible => _selectedPane == DesktopSplitPane.ClosetPlanner;

    // ---- Profile popup (forwarded from DashboardViewModel) -----------------

    public bool IsProfilePopupOpen
    {
        get => _isProfilePopupOpen;
        set
        {
            if (_isProfilePopupOpen == value)
            {
                return;
            }
            _isProfilePopupOpen = value;
            OnPropertyChanged();
        }
    }

    public ICommand ToggleProfilePopupCommand { get; }

    public ICommand SwitchPaneCommand { get; }

    /// <summary>
    /// Re-fires the dashboard VM's user-context load so the side
    /// rail avatar / initials reflect the currently signed-in user
    /// immediately on desktop boot. The dashboard VM is also
    /// captured by <see cref="Views.DashboardShoppingSplitPage"/> for
    /// its own reparented <c>DashboardPage</c>, so this single call
    /// keeps the dashboard column, the side rail, and the popup
    /// perfectly aligned.
    /// </summary>
    public Task ReloadUserContextAsync() => _dashboard.LoadUserContextAsync();

    public string? ProfileImageBase64 => _dashboard.ProfileImageBase64;
    public bool HasProfileImage => _dashboard.HasProfileImage;
    public string InitialsAvatarName => _dashboard.InitialsAvatarName;
    public string ProfileStyleBadge => _dashboard.ProfileStyleBadge;

    private void OnDashboardPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Bubble up the four profile-related properties so any XAML
        // binding on the host (side rail avatar, etc.) refreshes
        // whenever the dashboard VM reloads user context.
        switch (e.PropertyName)
        {
            case nameof(DashboardViewModel.ProfileImageBase64):
                OnPropertyChanged(nameof(ProfileImageBase64));
                OnPropertyChanged(nameof(HasProfileImage));
                break;
            case nameof(DashboardViewModel.HasProfileImage):
                OnPropertyChanged(nameof(HasProfileImage));
                break;
            case nameof(DashboardViewModel.InitialsAvatarName):
                OnPropertyChanged(nameof(InitialsAvatarName));
                break;
            case nameof(DashboardViewModel.ProfileStyleBadge):
                OnPropertyChanged(nameof(ProfileStyleBadge));
                break;
        }
    }

    // OnPropertyChanged is inherited from ObservableObject (CommunityToolkit.Mvvm).
    // We do not redeclare the event so the base class implementation is used.
}
