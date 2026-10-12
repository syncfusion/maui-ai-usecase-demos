using Microsoft.Extensions.Logging;
using Todo.Services.Interfaces;
using Todo.Utilities;

namespace Todo.Services.Infrastructure;

/// <summary>
/// Placeholder implementation of <see cref="INavigationService"/>.  The
/// Shell-based implementation is added as part of Task 1-06 (Navigation
/// Setup) — this skeleton simply throws because no Shell routes are
/// registered yet at this point in the build-up.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="NavigationService"/>
    /// class.
    /// </summary>
    /// <param name="logger">Logger used for structured diagnostics.</param>
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
}
