using Todo.Utilities;

namespace Todo.Services.Infrastructure;

/// <summary>
/// Abstraction over the MAUI <see cref="Microsoft.Maui.Controls.Shell"/>
/// navigation surface so that ViewModels can navigate without taking a direct
/// dependency on the static <c>Shell.Current</c> singleton.  The concrete
/// implementation lives in <see cref="NavigationService"/> and is registered
/// in <c>MauiProgram</c> as a singleton (Task 1-05).
/// </summary>
/// <remarks>
/// All methods are <see langword="async"/> and exception-safe — they never
/// throw <see cref="InvalidOperationException"/> from MAUI when a route is
/// missing, but instead return <see langword="false"/> (or wrap the failure
/// in a <see cref="Result{T}"/>) so the caller can decide how to react.
/// </remarks>
public interface INavigationService
{
    /// <summary>
    /// Gets the route of the page currently on top of the navigation stack,
    /// or <see langword="null"/> when the application is not initialised yet.
    /// </summary>
    string? CurrentRoute { get; }

    /// <summary>
    /// Navigates to the page registered against <paramref name="route"/>.
    /// </summary>
    /// <param name="route">
    /// A route string from <see cref="Constants.Routes"/>.  Route keys must be
    /// registered in <c>AppShell.xaml.cs</c> at startup time.
    /// </param>
    /// <param name="parameters">
    /// Optional dictionary of query-string parameters.  Keys map directly to
    /// the <c>[QueryProperty]</c> names declared on the destination ViewModel.
    /// </param>
    /// <param name="clearStack">
    /// When <see langword="true"/>, the navigation history is cleared before
    /// the new page is pushed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the navigation completed successfully;
    /// <see langword="false"/> when the route is unknown or Shell is not
    /// initialised.  Exceptions are never propagated to the caller.
    /// </returns>
    Task<bool> NavigateToAsync(string route, IDictionary<string, object>? parameters = null, bool clearStack = false);

    /// <summary>
    /// Navigates to the page registered against <typeparamref name="TPage"/>
    /// without requiring the route key to be passed as a magic string.  The
    /// route is resolved through <see cref="Constants.Routes"/>.
    /// </summary>
    /// <typeparam name="TPage">The page type to navigate to.</typeparam>
    /// <param name="parameters">Optional query-string parameters.</param>
    /// <param name="clearStack">
    /// When <see langword="true"/>, the navigation history is cleared
    /// before the new page is pushed.
    /// </param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    Task<bool> NavigateToAsync<TPage>(IDictionary<string, object>? parameters = null, bool clearStack = false) where TPage : Page;

    /// <summary>
    /// Navigates to the page registered against
    /// <paramref name="navigationTarget"/>, optionally clearing the navigation
    /// stack so the user cannot return to the previous page with the back
    /// gesture.  Used after sign-in / sign-out flows.
    /// </summary>
    /// <param name="navigationTarget">The destination enumerated by
    /// <see cref="NavigationTarget"/>.</param>
    /// <param name="clearStack">
    /// When <see langword="true"/>, the navigation history is cleared before
    /// the new page is pushed.
    /// </param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    Task<bool> NavigateToAsync(Enums.NavigationTarget navigationTarget, bool clearStack = false);

    /// <summary>
    /// Navigates to the previous page in the navigation stack.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the back navigation completed;
    /// <see langword="false"/> when there is no page to go back to.
    /// </returns>
    Task<bool> GoBackAsync();

    /// <summary>
    /// Returns <see langword="true"/> when the navigation stack contains at
    /// least one page that can be popped with <see cref="GoBackAsync"/>.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Returns the <see cref="Page"/> that is currently on top of the visual
    /// tree, or <see langword="null"/> when no page is active.
    /// </summary>
    Page? GetCurrentPage();
}
