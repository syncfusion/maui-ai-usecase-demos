namespace WardrobeAI.Services;

/// <summary>
/// Strongly-typed navigation surface used by view models. Wraps the
/// concrete navigation host so VMs do not have to know whether the app
/// uses a Shell <c>TabBar</c> or an <c>SfTabView</c> for its main tabs.
///
/// <para>Defined in <c>WardrobeAI.Services</c> (not in
/// <c>WardrobeAI.ViewModels</c>) so the dashboard view model can depend
/// on it without taking a reverse dependency on
/// <see cref="ViewModels.MainTabHostViewModel"/> — which already depends
/// on the dashboard VM for tab refresh dispatch. Without this indirection
/// the DI graph would deadlock at first resolution.</para>
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Switches the main tab to <see cref="MainTabKind.Closet"/>.
    /// </summary>
    Task NavigateToClosetAsync();

    /// <summary>
    /// Switches the main tab to <see cref="MainTabKind.Planner"/>.
    /// </summary>
    Task NavigateToPlannerAsync();

    /// <summary>
    /// Switches the main tab to <see cref="MainTabKind.Shopping"/>.
    /// </summary>
    Task NavigateToShoppingAsync();

    /// <summary>
    /// Switches the main tab to <see cref="MainTabKind.Profile"/>.
    /// </summary>
    Task NavigateToProfileAsync();

    /// <summary>
    /// Switches the main tab to <see cref="MainTabKind.Dashboard"/>.
    /// </summary>
    Task NavigateToDashboardAsync();
}

/// <summary>
/// Identifies one of the five main tabs hosted by the
/// <c>MainTabHostPage</c>'s <c>SfTabView</c>. The string values mirror
/// the route suffixes registered in <c>AppShell</c> so logs and
/// telemetry stay consistent with the existing
/// <c>GoToAsync("//main/closet")</c> call sites.
/// </summary>
public enum MainTabKind
{
    Dashboard,
    Closet,
    Planner,
    Shopping,
    Profile,
}
