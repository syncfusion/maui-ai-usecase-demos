using Microsoft.Extensions.Logging;
using Todo.Services.Interfaces;
using Todo.Utilities;
using System.Text;
using Enums = Todo.Utilities.Enums;

namespace Todo.Services.Infrastructure;

/// <summary>
/// Default <see cref="INavigationService"/> implementation backed by
/// <see cref="Shell"/>.  All operations are routed through
/// <c>Shell.Current.GoToAsync</c> which means routes must be registered in
/// <c>AppShell.xaml.cs</c> at startup time.  The service is registered as a
/// singleton in <c>MauiProgram</c> so every ViewModel shares the same
/// instance.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="NavigationService"/>
    /// class.
    /// </summary>
    /// <param name="logger">
    /// Logger injected by the DI container (Task 1-05).  Must not be
    /// <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public NavigationService(ILogger<NavigationService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Result> NavigateToAsync(
        string route,
        IDictionary<string, object>? parameters = null)
    {
        _logger.LogWarning(
            "NavigationService.NavigateToAsync is not implemented yet (Task 1-06).");
        throw new NotImplementedException(
            "Shell navigation is implemented in Task 1-06 (Navigation Setup).");
    }

    /// <inheritdoc />
    public Task<Result> GoBackAsync()
        => throw new NotImplementedException("Implemented in Task 1-06.");
    public string? CurrentRoute =>
        Shell.Current?.CurrentState?.Location?.OriginalString;

    /// <inheritdoc />
    public bool CanGoBack =>
        Shell.Current is not null
        && Shell.Current.Navigation.NavigationStack.Count > 1;

    /// <inheritdoc />
    public Page? GetCurrentPage() =>
        Shell.Current?.CurrentPage;

    /// <inheritdoc />
    public async Task<bool> NavigateToAsync(string route, IDictionary<string, object>? parameters = null, bool clearStack = false)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            _logger.LogWarning("NavigateToAsync called with an empty route");
            return false;
        }

        Shell? shell = Shell.Current;
        if (shell is null)
        {
            _logger.LogWarning("Shell.Current is null; cannot navigate to {Route}", route);
            return false;
        }

        string uri = clearStack ? $"//{route}" : BuildRouteUri(route, parameters);
        try
        {
            _logger.LogInformation("Navigating to {Route} (clearStack={ClearStack})", uri, clearStack);
            await shell.GoToAsync(uri, animate: true).ConfigureAwait(true);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Navigation to {Route} failed", uri);
            return false;
        }
    }

    /// <inheritdoc />
    public Task<bool> NavigateToAsync<TPage>(IDictionary<string, object>? parameters = null, bool clearStack = false)
        where TPage : Page
    {
        ArgumentNullException.ThrowIfNull(typeof(TPage));

        string? route = ResolveRouteForPage<TPage>();
        if (string.IsNullOrEmpty(route))
        {
            _logger.LogWarning("No registered route found for page type {PageType}", typeof(TPage).FullName);
            return Task.FromResult(false);
        }

        return NavigateToAsync(route, parameters, clearStack);
    }

    /// <inheritdoc />
    public Task<bool> NavigateToAsync(Enums.NavigationTarget navigationTarget, bool clearStack = false)
    {
        string? route = navigationTarget switch
        {
            Enums.NavigationTarget.SignIn      => Constants.Routes.SignIn,
            Enums.NavigationTarget.SignUp      => Constants.Routes.SignUp,
            Enums.NavigationTarget.TaskList    => Constants.Routes.TaskList,
            Enums.NavigationTarget.TaskDetail  => Constants.Routes.TaskDetail,
            Enums.NavigationTarget.Profile     => Constants.Routes.Profile,
            Enums.NavigationTarget.Search      => Constants.Routes.Search,
            _ => null,
        };

        if (string.IsNullOrEmpty(route))
        {
            _logger.LogWarning("No route mapped for navigation target {Target}", navigationTarget);
            return Task.FromResult(false);
        }

        return clearStack
            ? NavigateToAsync(route, clearStack: true)
            : NavigateToAsync(route);
    }

    /// <inheritdoc />
    public async Task<bool> GoBackAsync()
    {
        Shell? shell = Shell.Current;
        if (shell is null)
        {
            _logger.LogWarning("Shell.Current is null; cannot navigate back");
            return false;
        }

        try
        {
            _logger.LogInformation("Navigating back");
            await shell.GoToAsync("..", animate: true).ConfigureAwait(true);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Back navigation failed");
            return false;
        }
    }

    /// <summary>
    /// Combines <paramref name="route"/> and <paramref name="parameters"/>
    /// into a Shell-compatible URI.  The format is
    /// <c>route?key1=value1&amp;key2=value2</c>, with values URL-escaped.
    /// </summary>
    private static string BuildRouteUri(string route, IDictionary<string, object>? parameters)
    {
        if (parameters is null || parameters.Count == 0)
        {
            return route;
        }

        StringBuilder builder = new(route);
        builder.Append('?');
        bool first = true;
        foreach (KeyValuePair<string, object> entry in parameters)
        {
            if (!first)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(entry.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(entry.Value?.ToString() ?? string.Empty));
            first = false;
        }

        return builder.ToString();
    }

    /// <summary>
    /// Resolves the Shell route key associated with <typeparamref name="TPage"/>
    /// using the in-process <see cref="RegisteredPages"/> map populated by
    /// <c>AppShell</c> at startup.  Returns <see langword="null"/> when the
    /// page has not been registered.
    /// </summary>
    private static string? ResolveRouteForPage<TPage>() where TPage : Page =>
        RegisteredPages.TryGetValue(typeof(TPage), out string? route) ? route : null;
}
