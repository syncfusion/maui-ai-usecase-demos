using Todo.Utilities;

namespace Todo.Services.Interfaces;

/// <summary>
/// Contract for the navigation service.  Encapsulates Shell navigation so
/// that ViewModels do not depend on the static <see cref="Shell"/> API.
/// Implementations are added as part of the navigation infrastructure
/// (Module 1 / Task 1-06).
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the page registered against <paramref name="route"/>,
    /// passing <paramref name="parameters"/> as the navigation context.
    /// </summary>
    /// <param name="route">The route name to navigate to.</param>
    /// <param name="parameters">Optional navigation parameters.</param>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>
    Task<Result> NavigateToAsync(string route, IDictionary<string, object>? parameters = null);

    /// <summary>
    /// Pops the current page off the navigation stack.
    /// </summary>
    /// <returns>A <see cref="Result"/> describing the outcome.</returns>
    Task<Result> GoBackAsync();
}
