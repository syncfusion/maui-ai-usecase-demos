namespace NutriLens.Models;

public sealed class HistoryItem
{
    public string Image { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string Score { get; init; } = string.Empty;
    public string ScanDate { get; init; } = string.Empty;
    public string ScoreColor { get; init; } = string.Empty;
    public string StatusIcon { get; init; } = string.Empty;
}