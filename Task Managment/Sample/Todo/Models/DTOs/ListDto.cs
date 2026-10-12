using Todo.Utilities;

namespace Todo.Models.DTOs;

/// <summary>
/// Transport-layer representation of a list (the sidebar grouping used to
/// organise tasks).  The full schema is fleshed out in Module 5.
/// </summary>
public sealed record ListDto
{
    /// <summary>Stable, opaque identifier for the list.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>User-visible name of the list.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional colour used to render the list chip in the sidebar.</summary>
    public string? Color { get; init; }

    /// <summary>Top-level category bucket the list belongs to.</summary>
    public Enums.ListCategory Category { get; init; } = Enums.ListCategory.Custom;

    /// <summary>UTC timestamp at which the list was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>UTC timestamp at which the list was last modified.</summary>
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}
