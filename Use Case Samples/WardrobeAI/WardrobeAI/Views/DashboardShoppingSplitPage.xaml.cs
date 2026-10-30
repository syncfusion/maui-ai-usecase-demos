using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WardrobeAI.ViewModels;

namespace WardrobeAI.Views;

/// <summary>
/// Desktop split-view pane: Dashboard in column 1, Smart Shopping
/// in column 2. This page used to be a full Shell destination
/// (<c>//main/dashboardshop</c>) that hosted its own side rail +
/// profile popup. It is now a child of
/// <see cref="DesktopSplitHostPage"/>, which owns the rail, the
/// popup, and the pane-switching logic.
///
/// <para>Responsibilities remaining here:
/// <list type="bullet">
///   <item>Lazily resolve <see cref="Views.Dashboard.DashboardPage"/>
///     and <see cref="Views.Shopping.ShoppingPage"/> from DI and
///     reparent their root visual trees into the two column
///     <see cref="ContentView"/> hosts declared in
///     <c>DashboardShoppingSplitPage.xaml</c>.</item>
///   <item>Expose the captured child VMs (<see cref="DashboardViewModel"/>,
///     <see cref="ShoppingViewModel"/>) so the host can refresh
///     them on pane switch.</item>
///   <item>Re-fire the user-context load on first OnAppearing so
///     the greeting + avatar reflect the currently signed-in user
///     even after a fresh sign-in (the dashboard VM does this on
///     its own ctor but a transient lifetime means we may own a
///     fresh VM after the first reparent).</item>
/// </list>
/// </para>
///
/// <para>Important — visibility lifecycle: this page lives inside
/// the host as an always-realised visual child. Its
/// <see cref="OnAppearing"/> fires exactly once (on the first time
/// it's attached to the host's visual tree). The host's
/// <see cref="DesktopSplitHostViewModel.PaneChanged"/> event is
/// what re-fires per-pane refreshes on subsequent switches, since
/// <c>OnAppearing</c> does NOT fire when only <c>IsVisible</c>
/// changes between <c>true</c> and <c>false</c>.</para>
/// </summary>
public partial class DashboardShoppingSplitPage : ContentPage
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DashboardShoppingSplitPage> _logger;

    private ContentPage? _dashboardPage;
    private ContentPage? _shoppingPage;

    public DashboardShoppingSplitPage(
        IServiceProvider services,
        ILogger<DashboardShoppingSplitPage> logger)
    {
        _services = services;
        _logger = logger;
        InitializeComponent();
    }

    /// <summary>
    /// The <see cref="DashboardViewModel"/> instance captured from
    /// the reparented <see cref="Views.Dashboard.DashboardPage"/>.
    /// Exposed to <see cref="DesktopSplitHostPage"/> so the host
    /// can call <c>LoadUserContextAsync</c> and <c>RefreshAsync</c>
    /// when the user switches panes.
    /// </summary>
    public DashboardViewModel? DashboardViewModel =>
        _dashboardPage?.BindingContext as DashboardViewModel;

    /// <summary>
    /// The <see cref="ShoppingViewModel"/> instance captured from
    /// the reparented <see cref="Views.Shopping.ShoppingPage"/>.
    /// </summary>
    public ShoppingViewModel? ShoppingViewModel =>
        _shoppingPage?.BindingContext as ShoppingViewModel;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Run the column reparenting + user-context reload on every
        // appearance, with idempotent guards inside each step. This
        // path is only hit when the pane is used standalone (e.g. a
        // fallback routing consumer that puts the page directly into
        // a Shell stack); the desktop host invokes
        // <see cref="EnsureChildrenAttached"/> from its ctor so its
        // reparenting happens before the host page is shown.
        await EnsureChildrenAttachedAsync();
    }

    /// <summary>
    /// Resolves the two child pages from DI and reparents their root
    /// visual trees into the column <see cref="ContentView"/> hosts.
    /// Idempotent: subsequent calls are no-ops once both child
    /// references are populated.
    /// </summary>
    /// <remarks>
    /// Why this is public: the desktop
    /// <see cref="DesktopSplitHostPage"/> steals this pane's
    /// <c>Content</c> in its own ctor and reparents it into an inner
    /// <c>ContentView</c> so the side rail and the popup can share a
    /// single visual tree across the two panes. That means the
    /// pane's <see cref="OnAppearing"/> no longer fires (it would
    /// only fire if the pane were itself the topmost Shell page),
    /// so the host has to drive the reparenting itself by calling
    /// this method. Each step is guarded so calling it twice (once
    /// from the host, once from <c>OnAppearing</c> in the rare
    /// standalone-Shell case) doesn't double-reparent or re-instantiate
    /// the child VMs.
    /// </remarks>
    public Task EnsureChildrenAttachedAsync()
    {
        if (_dashboardPage is null)
        {
            _dashboardPage = _services.GetRequiredService<Views.Dashboard.DashboardPage>();
            ReparentForColumn(_dashboardPage, DashboardHost);
        }

        if (_shoppingPage is null)
        {
            _shoppingPage = _services.GetRequiredService<Views.Shopping.ShoppingPage>();
            ReparentForColumn(_shoppingPage, ShoppingHost);
        }

        // The host handles the user-context refresh (via
        // DesktopSplitHostViewModel.ReloadUserContextAsync) so we
        // don't re-fire it here; the dashboard VM is shared with
        // the host, so calling it twice would just do the same
        // DB read twice on the first frame.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Pulls <paramref name="page"/>'s root view out of the page and rehosts it
    /// inside <paramref name="host"/>. The root view is forced to
    /// <c>HorizontalOptions=Fill</c> so it stretches to fill the column width
    /// on desktop — the original pages sized for a single-column phone
    /// viewport with <c>Padding</c>-based horizontal padding.
    /// </summary>
    private static void ReparentForColumn(ContentPage page, ContentView host)
    {
        if (page.Content is null)
        {
            // Defensive: page with no Content has nothing to host. The
            // empty ContentView still gets the right BindingContext so
            // any future programmatic rebuild wires up correctly.
            host.Content = null;
            return;
        }

        var root = page.Content;
        page.Content = null;

        // The page's existing BindingContext (its VM) is preserved as-is on
        // the page itself. The host adopts the same BindingContext so any
        // bindings inside the reparented visual tree continue to resolve
        // to the same VM.
        host.BindingContext = page.BindingContext;
        root.HorizontalOptions = LayoutOptions.Fill;
        host.Content = root;
    }
}
