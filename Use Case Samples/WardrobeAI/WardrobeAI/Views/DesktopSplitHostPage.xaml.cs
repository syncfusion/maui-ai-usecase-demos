using Microsoft.Extensions.Logging;
using WardrobeAI.ViewModels;

namespace WardrobeAI.Views;

/// <summary>
/// Persistent desktop shell: left side rail + two content panes
/// (Dashboard+Shopping and Closet+Planner) toggled by
/// <see cref="DesktopSplitHostViewModel.SelectedPane"/>.
///
/// <para>Routing: this page is the destination of the
/// <c>//main/desktop</c> Shell sub-route, which
/// <see cref="App.OnWindowCreated"/> navigates to on
/// <see cref="Helpers.IdiomExtensions.IsDesktop"/>. Phones and tablets
/// keep the SfTabView host (<see cref="Main.MainTabHostPage"/>) on
/// <c>//main</c>.</para>
///
/// <para>Why a single host page (not two separate Shell routes):
/// the previous design routed <c>//main/dashboardshop</c> and
/// <c>//main/closetplanner</c> as siblings and called
/// <c>Shell.Current.GoToAsync</c> on every side-rail tap. That
/// triggered a Shell push animation (the "page slides in from the
/// right" feel), tore down the side rail + reparented child pages,
/// and re-fired the lazy one-shot reparenting logic in the new
/// split view's <c>OnAppearing</c>. The result was a full UI rebuild
/// on every rail tap, which is what the user is now asking us to
/// avoid.</para>
///
/// <para>New design: the host owns the rail + the popup, and the two
/// split-view pages live inside it as always-realised children.
/// Side-rail taps just flip <c>IsVisible</c> on the two pane pages —
/// no Shell navigation, no animation, no rebuild. The host code
/// re-fires a refresh on the *active* pane whenever the user
/// switches, mirroring the SfTabView behaviour from
/// <see cref="Main.MainTabHostPage"/>.</para>
/// </summary>
public partial class DesktopSplitHostPage : ContentPage
{
    private readonly DesktopSplitHostViewModel _viewModel;
    private readonly ILogger<DesktopSplitHostPage> _logger;
    private readonly DashboardShoppingSplitPage _dashboardShoppingPane;
    private readonly ClosetPlannerSplitPage _closetPlannerPane;

    public DesktopSplitHostPage(
        DesktopSplitHostViewModel viewModel,
        ILogger<DesktopSplitHostPage> logger,
        DashboardShoppingSplitPage dashboardShoppingPane,
        ClosetPlannerSplitPage closetPlannerPane)
    {
        _viewModel = viewModel;
        _logger = logger;
        _dashboardShoppingPane = dashboardShoppingPane;
        _closetPlannerPane = closetPlannerPane;
        BindingContext = viewModel;
        InitializeComponent();

        // Host the two pane ContentPages inside the ContentView
        // placeholders. ContentPage derives from Page (not View),
        // so it cannot be a child of Grid in XAML — the XAML
        // compiler enforces XLS0504 / MAUIG1001. The workaround
        // mirrors `MainTabHostPage.BuildTabs`: steal the pane's
        // root view (a real View) and assign it to the
        // ContentView.Content. The pane's OnAppearing then fires
        // once on first attach and reparents its child pages
        // (DashboardPage / ShoppingPage / etc.) into the inner
        // column hosts.
        //
        // The host drives the reparenting itself (rather than
        // relying on the pane's `OnAppearing`) because once the
        // pane's `Content` is stolen above, the pane is no longer
        // attached to any visual tree and its `OnAppearing` will
        // never fire. The two `EnsureChildrenAttached*` methods on
        // the panes are the public hooks for that — they resolve
        // the child pages from DI and reparent them into the
        // inner column hosts. `ClosetPlanner` is sync (no DB
        // work); `DashboardShopping` is async because the original
        // `OnAppearing` also kicked the user-context refresh off
        // the dashboard VM. That call is now a no-op (the host's
        // `OnAppearing` handles user-context refresh) but the
        // signature stays async for forward compatibility.
        // Steal each pane's root Grid and rehost it inside the host's pane
        // ContentView. The pane page is a ContentPage whose root is a
        // Grid (declared in DashboardShoppingSplitPage.xaml /
        // ClosetPlannerSplitPage.xaml), and the host's pane host is a
        // plain ContentView that can only own one child. Setting the
        // pane's Content to null first detaches the Grid from the split
        // page before we move it under the pane host — otherwise MAUI
        // sees the Grid already parented to the split page and emits
        //   "Microsoft.Maui.Controls.Grid is already a child of
        //    WardrobeAI.Views.DashboardShoppingSplitPage. Remove ... from
        //    ... before adding to Microsoft.Maui.Controls.ContentView."
        // on every measure pass. That double-parent state puts the same
        // Grid in two visual subtrees, which makes the WinUI layout
        // engine loop invalidating it (the cascade of
        // 'System.InvalidOperationException' / ArgumentException /
        // TargetInvocationException in WinRT.Runtime.dll seen in the
        // desktop build) and eventually tears the process down.
        DashboardShoppingPaneHost.Content = StealPaneRoot(_dashboardShoppingPane);
        ClosetPlannerPaneHost.Content = StealPaneRoot(_closetPlannerPane);

        _closetPlannerPane.EnsureChildrenAttached();
        _ = EnsureDashboardShoppingChildrenAttachedAsync();

        // Apply the active row up front so the side rail reads
        // correctly *before* the first OnAppearing completes (the
        // "Dashboard & Shopping" row was already styled active in
        // XAML; this is a defensive backstop in case brush
        // resolution ever changes).
        HighlightActiveMenuRow();

        // Mirror MainTabHostPage's per-tab refresh-on-switch
        // behaviour for the two desktop panes: when the user
        // switches panes, refresh the *now visible* pane so its
        // data is current. The hidden pane is left alone — its
        // OnAppearing refresh already ran during the first show,
        // and replaying it would race the user's tap.
        _viewModel.PaneChanged += OnPaneChanged;
    }

    /// <summary>
    /// Pulls <paramref name="pane"/>'s root view out of the page and
    /// returns it for rehosting inside the host's pane
    /// <see cref="ContentView"/>. Nulls the pane's <c>Content</c> first
    /// so the root view is detached from its original parent before it
    /// is handed to the new parent — see the comment on the two
    /// <c>PaneHost.Content = …</c> assignments in the ctor for why this
    /// matters.
    /// </summary>
    private static View? StealPaneRoot(ContentPage pane)
    {
        var root = pane.Content;
        pane.Content = null;
        return root;
    }

    /// <summary>
    /// Fire-and-forget wrapper around the dashboard+shopping
    /// pane's reparenting method. The pane page is registered
    /// transient, so this is a single-shot call; the method
    /// itself is idempotent and guarded against double-reparent.
    /// Failures are logged but never crash the host.
    /// </summary>
    private async Task EnsureDashboardShoppingChildrenAttachedAsync()
    {
        try
        {
            await _dashboardShoppingPane.EnsureChildrenAttachedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dashboard+Shopping pane reparenting failed during host ctor.");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        HighlightActiveMenuRow();

        // The host drives the column reparenting in its ctor
        // (see the EnsureChildrenAttached* calls there) because
        // stealing the pane's `Content` detaches the pane from
        // any visual tree and its `OnAppearing` would never
        // fire. So by the time we get here, both panes have
        // already reparented their child pages into the inner
        // column hosts.
        //
        // The pane pages are also reparented into the visual
        // tree only on first attach — flipping `IsVisible`
        // between the two `ContentView` placeholders does NOT
        // fire `OnAppearing` on the hidden pane. So we must do
        // the equivalent of a "manual OnAppearing" for the
        // hidden pane the first time the user toggles to it,
        // by calling RefreshAsync on its ViewModels via the
        // PaneChanged event. The host VM's PaneChanged event
        // handles the first switch and every switch after that.
        //
        // We also kick off a refresh of the *currently visible*
        // (default) pane right here: `PaneChanged` only fires on
        // a real change, so without this the default
        // `DashboardShopping` pane would never load its data on
        // the very first show (the original design relied on
        // the pane's own `OnAppearing` for that — which we
        // bypassed in the ctor).
        //
        // What we DO also need to do here is refresh the user
        // context once (it powers the side rail avatar + popup).
        // The dashboard VM does this itself on first
        // construction; we re-fire it so the side rail
        // reflects the currently signed-in user immediately on
        // desktop boot.
        try
        {
            // The dashboard VM lives behind the host VM (forwarded
            // into the side rail + popup). Loading the user context
            // here repopulates ProfileImageBase64 / InitialsAvatarName
            // / etc. on the dashboard VM, which the host VM forwards
            // up via PropertyChanged (see DesktopSplitHostViewModel
            // .OnDashboardPropertyChanged).
            await _viewModel.ReloadUserContextAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Desktop host user-context refresh failed.");
        }

        // Refresh the default pane so its data is loaded the very
        // first time the host appears. The PaneChanged event
        // handler covers every subsequent switch; this covers
        // the initial show.
        try
        {
            await RefreshPaneAsync(_viewModel.SelectedPane);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Default-pane refresh failed on first appear.");
        }
    }

    private async void OnPaneChanged(object? sender, DesktopSplitPane pane)
    {
        HighlightActiveMenuRow();

        try
        {
            // Re-fire the per-pane refresh on switch so the
            // newly-visible pane reloads its data. Mirrors the
            // SfTabView SelectionChanged → RefreshSelectedCommand
            // pattern in MainTabHostPage.
            await RefreshPaneAsync(pane);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Pane refresh failed for {Pane}.", pane);
        }
    }

    /// <summary>
    /// Calls <see cref="ITabRefreshable.RefreshAsync"/> on every
    /// view model that the named pane owns. The hidden pane is
    /// intentionally NOT refreshed — its data was loaded when it
    /// was first shown and replaying the refresh on a hidden
    /// view tree can cause a flash of empty content if the
    /// results aren't back before the user switches back.
    /// </summary>
    private Task RefreshPaneAsync(DesktopSplitPane pane)
    {
        switch (pane)
        {
            case DesktopSplitPane.DashboardShopping:
                return RefreshDashboardShoppingPaneAsync();
            case DesktopSplitPane.ClosetPlanner:
                return RefreshClosetPlannerPaneAsync();
            default:
                return Task.CompletedTask;
        }
    }

    private async Task RefreshDashboardShoppingPaneAsync()
    {
        if (_dashboardShoppingPane is null)
        {
            return;
        }
        if (_dashboardShoppingPane.DashboardViewModel is { } dashboard)
        {
            await SafeRefreshAsync(dashboard, "dashboard");
        }
        if (_dashboardShoppingPane.ShoppingViewModel is { } shopping)
        {
            await SafeLoadAsync(shopping, "shopping");
        }
    }

    private async Task RefreshClosetPlannerPaneAsync()
    {
        if (_closetPlannerPane is null)
        {
            return;
        }
        if (_closetPlannerPane.ClosetViewModel is { } closet)
        {
            await SafeRefreshAsync(closet, "closet");
        }
        if (_closetPlannerPane.OutfitPlannerViewModel is { } planner)
        {
            await SafeRefreshAsync(planner, "planner");
        }
    }

    private async Task SafeRefreshAsync(ITabRefreshable vm, string label)
    {
        try
        {
            await vm.RefreshAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Label} refresh failed inside desktop host.", label);
        }
    }

    private async Task SafeLoadAsync(ShoppingViewModel vm, string label)
    {
        try
        {
            await vm.LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Label} load failed inside desktop host.", label);
        }
    }

    // ---- Side-rail menu handlers -----------------------------------------
    //
    // Tapping a row just flips the SelectedPane enum on the host VM.
    // No Shell navigation. No page rebuild. The two pane pages
    // already exist as visual children of the host's right column
    // and stay realised across switches.

    private void OnDashboardShoppingTapped(object? sender, TappedEventArgs e)
    {
        _viewModel.SelectedPane = DesktopSplitPane.DashboardShopping;
    }

    private void OnClosetPlannerTapped(object? sender, TappedEventArgs e)
    {
        _viewModel.SelectedPane = DesktopSplitPane.ClosetPlanner;
    }

    /// <summary>
    /// Applies the active state to the currently selected rail row
    /// and resets every other row to the inactive colour + icon.
    /// Idempotent and cheap — called from <see cref="OnAppearing"/>
    /// and from <see cref="OnPaneChanged"/>.
    /// </summary>
    private void HighlightActiveMenuRow()
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
        {
            return;
        }
        var activeBg = (Color)resources["Primary"];
        var activeFg = (Color)resources["OnPrimary"];
        var inactiveBg = Colors.Transparent;
        var inactiveFg = (Color)(resources["OnSurface"] ?? Colors.Black);

        // Each rail row is independently active iff *its own* pane is
        // selected. The two checks must be mutually exclusive so a tap
        // lights up exactly one row. (An earlier version drove both
        // rows from a single tuple of icon tags, which produced the
        // "both rows look selected" symptom on initial load.)
        var isDashboardActive = _viewModel.SelectedPane == DesktopSplitPane.DashboardShopping;
        DashboardShoppingRowBackground.BackgroundColor = isDashboardActive ? activeBg : inactiveBg;
        DashboardShoppingRowLabel.TextColor = isDashboardActive ? activeFg : inactiveFg;
        DashboardShoppingRowIcon.Source = isDashboardActive ? "dashboard_white.png" : "dashboard.png";

        var isClosetActive = _viewModel.SelectedPane == DesktopSplitPane.ClosetPlanner;
        ClosetPlannerRowBackground.BackgroundColor = isClosetActive ? activeBg : inactiveBg;
        ClosetPlannerRowLabel.TextColor = isClosetActive ? activeFg : inactiveFg;
        ClosetPlannerRowIcon.Source = isClosetActive ? "hanger_white.png" : "hanger.png";
    }
}
