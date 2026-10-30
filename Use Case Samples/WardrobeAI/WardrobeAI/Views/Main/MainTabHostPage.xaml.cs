using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.TabView;
using WardrobeAI.ViewModels;
using WardrobeAI.Views.Closet;
using WardrobeAI.Views.Dashboard;
using WardrobeAI.Views.OutfitPlanner;
using WardrobeAI.Views.Profile;
using WardrobeAI.Views.Shopping;

namespace WardrobeAI.Views.Main;

/// <summary>
/// Host page for the post-onboarding main experience. Wraps the five tab
/// pages in a Syncfusion <c>SfTabView</c> so the bottom navigation matches
/// the design system used by the rest of the app (chips, gauges, etc.) and
/// the Shell <c>TabBar</c> can be turned off.
///
/// <para>Routing: registered as <c>//main</c> in <see cref="AppShell"/>. The
/// per-tab routes (<c>//main/dashboard</c>, <c>//main/closet</c>, etc.) still
/// resolve via the Shell URI parser and are re-routed back to this host by
/// the <c>Shell.Navigated</c> handler installed in <see cref="AppShell"/>
/// ctor — that handler then sets <c>SelectedIndex</c> via two-way binding
/// to the host VM.</para>
///
/// <para>Refresh semantics: the original Shell <c>TabBar</c> fired
/// <c>OnAppearing</c> on each child page when its tab was tapped. With the
/// <c>SfTabView</c> host, the children are realised exactly once and the
/// host fires <see cref="Syncfusion.Maui.TabView.SfTabView.SelectionChanged"/>
/// on every tab change. The host's handler invokes
/// <see cref="MainTabHostViewModel.RefreshSelectedCommand"/>, which dispatches
/// the right <c>RefreshAsync</c> to the active child VM (all five implement
/// <see cref="ITabRefreshable"/>).</para>
///
/// <para>Per-tab content hosting: <see cref="SfTabItem.Content"/> is typed
/// <c>View</c> and <c>ContentPage</c> is a <c>Page</c> (not a <c>View</c>),
/// so the child pages cannot be assigned directly in XAML (XLS0504). The
/// code-behind builds each tab's content at <see cref="BuildTabs"/> time by
/// reparenting the child page's root <c>Content</c> (a <c>View</c>) into a
/// <see cref="ContentView"/> whose <c>BindingContext</c> is set to the
/// child VM. The child <see cref="ContentPage"/> instances still exist as
/// real Shell-routable pages (so <c>GoToAsync("//main/closet")</c> keeps
/// working), and the host's <see cref="OnAppearing"/> is the single place
/// that drives per-tab refreshes through <see cref="ITabRefreshable"/>.</para>
/// </summary>
public partial class MainTabHostPage : ContentPage
{
    private readonly MainTabHostViewModel _viewModel;
    private readonly ILogger<MainTabHostPage> _logger;
    private readonly DashboardPage _dashboardPage;
    private readonly ClosetPage _closetPage;
    private readonly OutfitPlannerPage _plannerPage;
    private readonly ShoppingPage _shoppingPage;
    private readonly ProfilePage _profilePage;
    private bool _initialized;

    public MainTabHostPage(
        MainTabHostViewModel viewModel,
        ILogger<MainTabHostPage> logger,
        DashboardPage dashboardPage,
        ClosetPage closetPage,
        OutfitPlannerPage plannerPage,
        ShoppingPage shoppingPage,
        ProfilePage profilePage)
    {
        _viewModel = viewModel;
        _logger = logger;
        _dashboardPage = dashboardPage;
        _closetPage = closetPage;
        _plannerPage = plannerPage;
        _shoppingPage = shoppingPage;
        _profilePage = profilePage;
        BindingContext = viewModel;
        InitializeComponent();

        // Each tab's content is a ContentView that hosts the child
        // ContentPage's root visual tree (the child page's Content
        // property). Reparenting happens here so the children keep their
        // DI-created singleton lifetimes and stay routable through the
        // Shell while still satisfying SfTabItem.Content's View contract.
        BuildTabs();

        // Keep the SfTabView selection in lock-step with the host VM so
        // external nav (GoToAsync("//main/closet")) can drive the tab
        // programmatically via the Shell.Navigated handler in AppShell.
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void BuildTabs()
    {
        DashboardTab.Content = CreateTabHost(_dashboardPage);
        ClosetTab.Content = CreateTabHost(_closetPage);
        PlannerTab.Content = CreateTabHost(_plannerPage);
        ShopTab.Content = CreateTabHost(_shoppingPage);
        ProfileTab.Content = CreateTabHost(_profilePage);
    }

    /// <summary>
    /// Wraps <paramref name="page"/>'s root visual tree in a
    /// <see cref="ContentView"/> so it can be assigned to
    /// <see cref="SfTabItem.Content"/> (which is typed <c>View</c>, while
    /// <see cref="ContentPage"/> is a <c>Page</c>). The wrapper adopts the
    /// child VM as its <c>BindingContext</c> so the child bindings keep
    /// resolving exactly the same way they did when the page was a top-level
    /// <see cref="ContentPage"/>.
    /// </summary>
    private static ContentView CreateTabHost(ContentPage page)
    {
        if (page.Content is null)
        {
            // Defensive guard: a page with no Content has nothing to host.
            // The empty ContentView still gets the right BindingContext
            // so any future programmatic rebuild wires up correctly.
            return new ContentView { BindingContext = page.BindingContext };
        }

        // Capture the page's root view before reparenting so we can hand it
        // to the ContentView. Setting page.Content = null detaches the view
        // from the page (MAUI's ContentPage.Content setter is a regular
        // property; assignment removes the child from the page's logical
        // tree). The page itself stays alive as a singleton so Shell
        // routing (Routing.RegisterRoute("main/closet", typeof(ClosetPage)))
        // still resolves to the same instance and the per-tab VM wiring
        // (e.g. ClothingItemRepository / OnAppearing hooks) keeps working
        // when the user pushes a sub-page from inside the tab.
        var root = page.Content;
        page.Content = null;

        var host = new ContentView
        {
            // Preserve the page's own BindingContext (which is the child
            // VM) so every binding inside the reparented visual tree
            // continues to resolve identically.
            BindingContext = page.BindingContext,
            Content = root,
        };
        return host;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Apply the current SelectedIndex the first time we appear, in case
        // a GoToAsync("//main/closet") already updated the VM before the
        // SfTabView was realised.
        if (!_initialized)
        {
            MainTabView.SelectedIndex = _viewModel.SelectedIndex;
            _initialized = true;
        }

        // Refresh the active tab every time the host re-appears (covers the
        // sign-in / onboarding → main transition where the data wasn't yet
        // loaded for the signed-in user).
        await _viewModel.RefreshSelectedAsync();
    }

    private async void MainTabView_SelectionChanged(object? sender, TabSelectionChangedEventArgs e)
    {
        // Sync the VM with the new index on every selection change — this
        // covers both user taps and programmatic changes routed from the
        // Shell.Navigated handler in AppShell. SelectionChanged is only
        // raised when the new index actually differs, so the equality
        // check below is a defensive guard against the SfTabView raising
        // a same-index event in some edge cases.
        var newIndex = (int)e.NewIndex;
        if (sender is SfTabView view && (int)view.SelectedIndex == newIndex
            && _viewModel.SelectedIndex == newIndex)
        {
            return;
        }
        _viewModel.SelectedIndex = newIndex;
        await _viewModel.RefreshSelectedAsync();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainTabHostViewModel.SelectedIndex))
        {
            return;
        }

        // Programmatic selection (from Shell.Navigated) — sync the SfTabView
        // but do NOT call RefreshSelectedAsync, since the SelectionChanged
        // event raised by setting SelectedIndex will dispatch the refresh
        // exactly once. SfTabView.SelectedIndex is a double in this
        // Syncfusion version, so cast both sides for the comparison.
        if ((int)MainTabView.SelectedIndex != _viewModel.SelectedIndex)
        {
            MainTabView.SelectedIndex = _viewModel.SelectedIndex;
        }
    }
}
