namespace Todo.Utilities;

/// <summary>
/// Process-wide, mutable map of <see cref="Type"/> (a Page) to the Shell
/// route key that it is registered against.  Populated by
/// <c>AppShell.xaml.cs</c> at startup and read by
/// <see cref="Todo.Services.Infrastructure.NavigationService"/> when callers
/// use the strongly-typed <c>NavigateToAsync&lt;TPage&gt;</c> overload.
/// </summary>
/// <remarks>
/// The map is intentionally non-readonly so additional pages can be
/// registered after the Shell has been constructed (for example, when a
/// feature module is loaded lazily).
/// </remarks>
public static class RegisteredPages
{
    private static readonly Dictionary<Type, string> _map = new();

    /// <summary>
    /// Adds or overwrites the route key associated with
    /// <paramref name="pageType"/>.
    /// </summary>
    /// <param name="pageType">The page type being registered.</param>
    /// <param name="route">The Shell route key it should be reachable from.</param>
    public static void Register(Type pageType, string route)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        _map[pageType] = route;
    }

    /// <summary>
    /// Attempts to resolve the route key registered for
    /// <paramref name="pageType"/>.
    /// </summary>
    /// <param name="pageType">The page type to look up.</param>
    /// <param name="route">The resolved route, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when a route is registered.</returns>
    public static bool TryGetValue(Type pageType, out string? route)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        return _map.TryGetValue(pageType, out route);
    }

    /// <summary>
    /// Clears every registration.  Intended for test scenarios that re-create
    /// the Shell; production code should not call this.
    /// </summary>
    public static void Clear() => _map.Clear();
}
