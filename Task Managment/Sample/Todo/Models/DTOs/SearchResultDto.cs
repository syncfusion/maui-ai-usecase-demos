namespace Todo.Models.DTOs;

/// <summary>
/// Bundle of results returned by <see cref="Todo.Services.Interfaces.ISearchService"/>.
/// </summary>
public sealed record SearchResultDto
{
    /// <summary>The original query string the user typed.</summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>Tasks matching the query.</summary>
    public IReadOnlyList<TaskDto> Tasks { get; init; } = Array.Empty<TaskDto>();

    /// <summary>Lists matching the query by name.</summary>
    public IReadOnlyList<ListDto> Lists { get; init; } = Array.Empty<ListDto>();
}
