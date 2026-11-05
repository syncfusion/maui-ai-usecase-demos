using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WardrobeAI.ViewModels;

namespace WardrobeAI.Views;

/// <summary>
/// Desktop split-view pane: <see cref="Closet.ClosetPage"/> in
/// column 1, <see cref="OutfitPlanner.OutfitPlannerPage"/> in
/// column 2. This page used to be a full Shell destination
/// (<c>//main/closetplanner</c>) that hosted its own side rail +
/// profile popup. It is now a child of
/// <see cref="DesktopSplitHostPage"/>, which owns the rail, the
/// popup, and the pane-switching logic.
///
/// <para>Responsibilities remaining here:
/// <list type="bullet">
///   <item>Lazily resolve <see cref="Closet.ClosetPage"/> and
///     <see cref="OutfitPlanner.OutfitPlannerPage"/> from DI and
///     reparent their root visual trees into the two column
///     <see cref="ContentView"/> hosts.</item>
///   <item>Expose the captured child VMs
///     (<see cref="ClosetViewModel"/>, <see cref="OutfitPlannerViewModel"/>)
///     so the host can refresh them on pane switch.</item>
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
public partial class ClosetPlannerSplitPage : ContentPage
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ClosetPlannerSplitPage> _logger;

    private ContentPage? _closetPage;
    private ContentPage? _plannerPage;

    public ClosetPlannerSplitPage(
        IServiceProvider services,
        ILogger<ClosetPlannerSplitPage> logger)
    {
        _services = services;
        _logger = logger;
        InitializeComponent();
    }

    /// <summary>
    /// The <see cref="ClosetViewModel"/> instance captured from
    /// the reparented <see cref="Closet.ClosetPage"/>. Exposed
    /// to <see cref="DesktopSplitHostPage"/> so the host can
    /// call <c>RefreshAsync</c> when the user switches panes.
    /// </summary>
    public ClosetViewModel? ClosetViewModel =>
        _closetPage?.BindingContext as ClosetViewModel;

    /// <summary>
    /// The <see cref="OutfitPlannerViewModel"/> instance captured
    /// from the reparented <see cref="OutfitPlanner.OutfitPlannerPage"/>.
    /// </summary>
    public OutfitPlannerViewModel? OutfitPlannerViewModel =>
        _plannerPage?.BindingContext as OutfitPlannerViewModel;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Run the column reparenting on every appearance, guarded so
        // it only happens once. This path is only hit when the pane
        // is used standalone (e.g. a fallback routing consumer that
        // puts the page directly into a Shell stack); the desktop
        // host invokes <see cref="EnsureChildrenAttached"/> from
        // its ctor so its reparenting happens before the host page
        // is shown.
        EnsureChildrenAttached();
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
    public void EnsureChildrenAttached()
    {
        if (_closetPage is null)
        {
            _closetPage = _services.GetRequiredService<Closet.ClosetPage>();
            ReparentForColumn(_closetPage, ClosetHost);
        }

        if (_plannerPage is null)
        {
            _plannerPage = _services.GetRequiredService<OutfitPlanner.OutfitPlannerPage>();
            ReparentForColumn(_plannerPage, PlannerHost);
        }
    }

    /// <summary>
    /// Pulls <paramref name="page"/>'s root view out of the page and
    /// rehosts it inside <paramref name="host"/>. Mirrors
    /// <see cref="DashboardShoppingSplitPage"/> so the sibling split
    /// panes behave identically.
    /// </summary>
    private static void ReparentForColumn(ContentPage page, ContentView host)
    {
        if (page.Content is null)
        {
            host.Content = null;
            return;
        }

        var root = page.Content;
        page.Content = null;

        // The page's existing BindingContext (its VM) is preserved as-is
        // on the page itself. The host adopts the same BindingContext so
        // any bindings inside the reparented visual tree continue to
        // resolve to the same VM.
        host.BindingContext = page.BindingContext;
        root.HorizontalOptions = LayoutOptions.Fill;
        host.Content = root;
    }
}
