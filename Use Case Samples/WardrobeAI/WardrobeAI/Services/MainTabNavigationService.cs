using Microsoft.Extensions.Logging;
using WardrobeAI.ViewModels;

namespace WardrobeAI.Services;

/// <summary>
/// Default <see cref="INavigationService"/> implementation. Switches the
/// active <c>SfTabView</c> tab by mutating
/// <see cref="MainTabHostViewModel.SelectedIndex"/>; the
/// <c>MainTabView_SelectionChanged</c> handler in
/// <see cref="Views.Main.MainTabHostPage"/> then triggers a refresh of
/// the newly active tab and keeps the visual selection in sync.
///
/// <para>This deliberately does <b>not</b> call
/// <c>Shell.Current.GoToAsync("//main/closet")</c> — that path pushes a
/// fresh <see cref="Views.Closet.ClosetPage"/> instance on top of the
/// current page instead of switching the active tab, because the
/// <c>MainTabHostPage</c>'s <c>SfTabView</c> is a <c>View</c>, not a
/// Shell <c>TabBar</c>. The previous behaviour showed the dashboard
/// still on screen (or a duplicate page on top of it) when a user
/// tapped a Quick Action button.</para>
/// </summary>
public sealed class MainTabNavigationService : INavigationService
{
    // We resolve the host VM lazily to break a constructor-injection cycle:
    //
    //   AppShell → MainTabHostViewModel → DashboardViewModel → INavigationService
    //           → MainTabNavigationService → MainTabHostViewModel
    //
    // The runtime would otherwise throw
    // System.InvalidOperationException("A circular dependency was detected for
    // the service of type 'WardrobeAI.ViewModels.MainTabHostViewModel'") the
    // first time the container tries to construct AppShell on Android (it dies
    // with exit code 0 because the unhandled exception escapes CreateMauiApp).
    // By holding a Lazy<MainTabHostViewModel> instead of the VM itself, the
    // navigation service's constructor completes without touching the host —
    // the host is only materialised on the first call into the service (which
    // happens from a UI thread, after both singletons are fully constructed).
    private readonly Lazy<MainTabHostViewModel> _host;
    private readonly ILogger<MainTabNavigationService> _logger;

    public MainTabNavigationService(Lazy<MainTabHostViewModel> host, ILogger<MainTabNavigationService> logger)
    {
        _host = host;
        _logger = logger;
    }

    public Task NavigateToDashboardAsync() => SwitchAsync(MainTabKind.Dashboard);
    public Task NavigateToClosetAsync() => SwitchAsync(MainTabKind.Closet);
    public Task NavigateToPlannerAsync() => SwitchAsync(MainTabKind.Planner);
    public Task NavigateToShoppingAsync() => SwitchAsync(MainTabKind.Shopping);
    public Task NavigateToProfileAsync() => SwitchAsync(MainTabKind.Profile);

    private Task SwitchAsync(MainTabKind tab)
    {
        // Desktop routes Shopping taps to the persistent desktop host
        // (//main/desktop) so the Dashboard stays visible alongside the
        // Shopping column — matches the SDS §6 desktop variant. The host
        // owns the side rail and toggles between the two split-view
        // panes (Dashboard+Shopping, Closet+Planner) without Shell
        // navigation, so a re-navigation to the same route is a Shell
        // no-op (Shell deduplicates identical routes). Phones/tablets
        // skip the desktop branch and fall through to the SfTabView
        // switch below.
        if (tab == MainTabKind.Shopping && Helpers.IdiomExtensions.IsDesktop)
        {
            if (Shell.Current is not null)
            {
                // Fire-and-forget: the caller (button command) is already async.
                return Shell.Current.GoToAsync("//main/desktop");
            }
        }

        var index = TabIndex(tab);
        if (index < 0)
        {
            _logger.LogWarning("MainTabNavigationService: unknown tab {Tab}.", tab);
            return Task.CompletedTask;
        }

        // Resolve the host via the Lazy<T> — by the time any caller reaches
        // here, the host VM (and every transitive dependency, including this
        // service itself) is fully constructed. Assigning SelectedIndex raises
        // PropertyChanged; the host page's OnViewModelPropertyChanged handler
        // then sets SfTabView.SelectedIndex which raises SelectionChanged
        // which calls RefreshSelectedAsync. The whole chain runs on the UI
        // thread (we're invoked from a button command) and is synchronous in
        // MAUI.
        _host.Value.SelectedIndex = index;
        return Task.CompletedTask;
    }

    private static int TabIndex(MainTabKind tab) => tab switch
    {
        MainTabKind.Dashboard => 0,
        MainTabKind.Closet => 1,
        MainTabKind.Planner => 2,
        MainTabKind.Shopping => 3,
        MainTabKind.Profile => 4,
        _ => -1,
    };
}
