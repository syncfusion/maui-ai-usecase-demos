namespace NutriLens.Models;

public sealed class RecentScanItem
{
    public string ProductName { get; set; } = string.Empty;

    public string ScanTime { get; set; } = string.Empty;

    public int Score { get; set; }

    public string Rating { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public string RatingColor { get; set; } = "#047857";
}