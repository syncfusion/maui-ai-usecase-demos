using Microsoft.Extensions.Logging;
using WardrobeAI.ViewModels;
using WardrobeAI.Views.Dashboard;
using WardrobeAI.Views.Closet;
using WardrobeAI.Views.OutfitPlanner;
using WardrobeAI.Views.Shopping;
using WardrobeAI.Views.Profile;

namespace WardrobeAI
{
    /// <summary>
    /// Application Shell (<c>//main</c> only — auth + onboarding were removed in
    /// P7). The five main tabs (Dashboard / Closet / Plan / Shop / Profile) are
    /// rendered inside a Syncfusion <c>SfTabView</c> hosted on
    /// <see cref="Views.Main.MainTabHostPage"/> instead of the platform Shell
    /// <c>TabBar</c> so the bottom navigation matches the rest of the design
    /// system.
    ///
    /// <para>For backward compatibility the per-tab sub-routes
    /// (<c>//main/dashboard</c>, <c>//main/closet</c>, <c>//main/planner</c>,
    /// <c>//main/shopping</c>, <c>//main/profile</c>) are still routable: when
    /// Shell resolves one of them, the <see cref="OnShellNavigated"/> handler
    /// updates <see cref="MainTabHostViewModel.SelectedIndex"/> via the host VM
    /// so the <c>SfTabView</c> jumps to the right tab.</para>
    ///
    /// <para>Desktop routing: <c>//main/desktop</c> resolves to
    /// <see cref="Views.DesktopSplitHostPage"/>, a persistent shell
    /// that hosts the side rail + the two split-view panes
    /// (<see cref="Views.DashboardShoppingSplitPage"/> and
    /// <see cref="Views.ClosetPlannerSplitPage"/>) and toggles
    /// between them via <c>IsVisible</c> rather than Shell
    /// navigation. The previous design routed each pane as its own
    /// Shell destination (<c>//main/dashboardshop</c>,
    /// <c>//main/closetplanner</c>), which caused the side rail to
    /// slide in from the right and the UI to fully rebuild on every
    /// rail tap. <see cref="App.OnWindowCreated"/> navigates to
    /// <c>//main/desktop</c> on the desktop idiom.</para>
    /// </summary>
    public partial class AppShell : Shell
    {
        private readonly MainTabHostViewModel _mainTabHost;
        private readonly ILogger<AppShell> _logger;

        public AppShell(
            MainTabHostViewModel mainTabHost,
            ILogger<AppShell> logger)
        {
            _mainTabHost = mainTabHost;
            _logger = logger;
            InitializeComponent();

            // Per-tab routes: registered as children of the //main ShellContent
            // so existing GoToAsync("//main/closet") call sites still resolve.
            // The Shell.Navigated handler below syncs the SfTabView to the
            // resolved sub-route.
            Routing.RegisterRoute("main/dashboard", typeof(DashboardPage));
            Routing.RegisterRoute("main/closet", typeof(ClosetPage));
            Routing.RegisterRoute("main/planner", typeof(OutfitPlannerPage));
            Routing.RegisterRoute("main/shopping", typeof(ShoppingPage));
            Routing.RegisterRoute("main/profile", typeof(ProfilePage));

            // Persistent desktop split-view host. Owns the side rail
            // and toggles between the DashboardShoppingSplitPage and
            // ClosetPlannerSplitPage panes by flipping IsVisible — no
            // Shell navigation between the two panes, so side-rail
            // taps don't slide a new page in from the right or rebuild
            // the UI. The two split-view pages are still routable as
            // standalone Shell pages for backward compatibility, but
            // desktop boot lands here so the rail + popup are
            // realised exactly once and shared across switches.
            Routing.RegisterRoute(
                "main/desktop",
                typeof(Views.DesktopSplitHostPage));

            Navigated += OnShellNavigated;
        }

        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            try
            {
                // The current location string is something like "//main/closet"
                // or "//main/dashboard". The last segment is the tab we want
                // to highlight in the SfTabView.
                var location = Shell.Current?.CurrentItem?.CurrentItem?.Route
                               ?? Shell.Current?.CurrentState?.Location?.OriginalString;
                if (string.IsNullOrEmpty(location))
                {
                    return;
                }

                // Strip the leading "//" + "main/" to isolate the tab segment.
                var prefix = "//main/";
                if (!location.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                var tabRoute = location.Substring(prefix.Length);
                var index = _mainTabHost.IndexForRoute(tabRoute);
                if (index >= 0 && _mainTabHost.SelectedIndex != index)
                {
                    _mainTabHost.SelectedIndex = index;
                }
            }
            catch (Exception ex)
            {
                // Navigation sync is best-effort: a failure here must not
                // crash the Shell. The user can still tap a tab manually.
                _logger.LogWarning(ex, "Shell.Navigated sync to SfTabView failed.");
            }
        }
    }
}
