using Microsoft.Extensions.Logging;
using Todo.Models.DTOs;
using Todo.Services.Interfaces;
using Todo.Utilities;

namespace Todo.Services.Search;

/// <summary>
/// Placeholder implementation of <see cref="ISearchService"/>.  The real
/// implementation is added in Module 7 (Task 7-xx).
/// </summary>
public sealed class SearchService : ISearchService
{
    private readonly ILogger<SearchService> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="SearchService"/> class.
    /// </summary>
    /// <param name="logger">Logger used for structured diagnostics.</param>
    public SearchService(ILogger<SearchService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<Result<SearchResultDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "SearchService.SearchAsync is not implemented yet (Module 7).");
        throw new NotImplementedException(
            "Search is implemented in Module 7 (Task 7-xx).");
    }
}
