namespace NutriLens.Models;

/// <summary>One point on the Trends page health-score chart.</summary>
public sealed class HealthTrendPoint
{
    public string Day { get; init; } = string.Empty;
    public double Score { get; init; }
}

/// <summary>One frequently-occurring additive on the Trends page.</summary>
public sealed class AdditiveTrendItem
{
    public string Name { get; init; } = string.Empty;
    public string Percentage { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Color { get; init; } = "#F59E0B";
}