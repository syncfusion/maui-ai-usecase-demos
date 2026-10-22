using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WardrobeAI.ViewModels;

namespace WardrobeAI.ViewModels;

/// <summary>
/// View model for <see cref="WardrobeAI.Views.Main.MainTabHostPage"/>. Owns the
/// <c>SfTabView</c> selection and dispatches a refresh call to the currently
/// selected tab's view model whenever the user switches tabs (which mirrors the
/// <c>OnAppearing</c> behavior the Shell <c>TabBar</c> previously provided for free).
///
/// <para>Routing: the host page is registered at <c>//main</c>. Sub-routes
/// (<c>//main/dashboard</c>, <c>//main/closet</c>, etc.) still resolve via the
/// Shell URI parser so existing <c>GoToAsync("//main/closet")</c> call sites
/// (e.g. <see cref="DashboardViewModel.NavigateAsync"/>) keep working — the
/// <see cref="WardrobeAI.AppShell"/>'s <c>Shell.Navigated</c> listener syncs
/// <see cref="SelectedIndex"/> to the resolved route.</para>
/// </summary>
public partial class MainTabHostViewModel : ObservableObject
{
    /// <summary>
    /// Stable route names for each tab. Order here = visual order in the
    /// <c>SfTabView</c>. Keep aligned with <see cref="Tabs"/>.
    /// </summary>
    public const string DashboardRoute = "dashboard";
    public const string ClosetRoute = "closet";
    public const string PlannerRoute = "planner";
    public const string ShoppingRoute = "shopping";
    public const string ProfileRoute = "profile";

    private readonly DashboardViewModel _dashboard;
    private readonly ClosetViewModel _closet;
    private readonly OutfitPlannerViewModel _planner;
    private readonly ShoppingViewModel _shopping;
    private readonly ProfileViewModel _profile;
    private readonly ILogger<MainTabHostViewModel> _logger;

    /// <summary>
    /// Currently selected tab index. Two-way bound to the <c>SfTabView.SelectedIndex</c>
    /// so the XAML can drive selection and code-behind can react to changes.
    /// </summary>
    [ObservableProperty]
    private int selectedIndex;

    /// <summary>
    /// Lightweight metadata for each tab. The <c>SfTabView</c> itself holds the
    /// header text + content (so it can render the right ContentPage per tab),
    /// but the VM exposes the collection for any code-side lookups (analytics,
    /// tests, deep links) and to keep the route → VM mapping in one place.
    /// </summary>
    public ObservableCollection<MainTabDescriptor> Tabs { get; } = new();

    public MainTabHostViewModel(
        DashboardViewModel dashboard,
        ClosetViewModel closet,
        OutfitPlannerViewModel planner,
        ShoppingViewModel shopping,
        ProfileViewModel profile,
        ILogger<MainTabHostViewModel> logger)
    {
        _dashboard = dashboard;
        _closet = closet;
        _planner = planner;
        _shopping = shopping;
        _profile = profile;
        _logger = logger;

        Tabs.Add(new MainTabDescriptor(DashboardRoute, "Dashboard", _dashboard));
        Tabs.Add(new MainTabDescriptor(ClosetRoute, "Closet", _closet));
        Tabs.Add(new MainTabDescriptor(PlannerRoute, "Plan", _planner));
        Tabs.Add(new MainTabDescriptor(ShoppingRoute, "Shop", _shopping));
        Tabs.Add(new MainTabDescriptor(ProfileRoute, "Profile", _profile));

        // Default to the Dashboard so the page lands on the same view it did
        // when Shell's TabBar owned navigation.
        SelectedIndex = 0;
    }

    /// <summary>
    /// Returns the tab index that matches <paramref name="route"/>, or
    /// <c>-1</c> if the route is not one of the 5 main tabs. Used by the host
    /// page when syncing <c>SfTabView</c> selection with Shell navigation.
    /// </summary>
    public int IndexForRoute(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return -1;
        }
        for (var i = 0; i < Tabs.Count; i++)
        {
            if (string.Equals(Tabs[i].Route, route, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Returns the route for a tab index, or <c>null</c> if out of range.
    /// </summary>
    public string? RouteForIndex(int index)
    {
        if (index < 0 || index >= Tabs.Count)
        {
            return null;
        }
        return Tabs[index].Route;
    }

    /// <summary>
    /// Returns the view model associated with the given tab index, or
    /// <c>null</c> if the index is out of range. Used by the host page to
    /// dispatch refresh calls.
    /// </summary>
    public ObservableObject? ViewModelForIndex(int index) => index switch
    {
        0 => _dashboard,
        1 => _closet,
        2 => _planner,
        3 => _shopping,
        4 => _profile,
        _ => null,
    };

    /// <summary>
    /// Reloads the data for the currently selected tab. Called by the host
    /// page on <c>OnAppearing</c> and whenever the SfTabView selection
    /// changes (so per-tab "refresh on tab switch" semantics are preserved
    /// now that we no longer use the Shell <c>TabBar</c>).
    /// </summary>
    [RelayCommand]
    public async Task RefreshSelectedAsync()
    {
        var vm = ViewModelForIndex(SelectedIndex);
        if (vm is null)
        {
            return;
        }
        try
        {
            await ((ITabRefreshable)vm).RefreshAsync();
        }
        catch (Exception ex)
        {
            // Per-tab refresh failures must not crash the host. Each child VM
            // already logs its own errors; we just log here so the loop is
            // visible in the trace.
            _logger?.LogWarning(ex, "Tab refresh failed for index {Index}.", SelectedIndex);
        }
    }
}

/// <summary>
/// One row in the main tab strip. Carries the Shell route, display label, and
/// the child view model so the host VM can hand the right VM to the
/// <c>RefreshAsync</c> dispatcher without a parallel switch statement.
/// </summary>
public sealed record MainTabDescriptor(string Route, string DisplayName, ObservableObject ViewModel);
