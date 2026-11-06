using Microsoft.Maui.Graphics;

namespace SmartVehicleCare.Models;

public class ExpenseDataPoint
{
    public string Month { get; set; } = string.Empty;
    public string RangeLabel { get; set; } = string.Empty;
    public double Amount { get; set; }
}

public class ActivityItem
{
    public string IconGlyph { get; set; } = "\uE531";
    public Color IconColor { get; set; } = Colors.Blue;
    public Color IconBgColor { get; set; } = Color.FromArgb("#1E3A5F");
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public Color TimestampColor { get; set; } = Color.FromArgb("#999999");
}

public class HealthItem
{
    public string IconGlyph { get; set; } = "\uE531";
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public double ValueNormalized => Value / 100.0;
    public Color BarColor { get; set; } = Colors.Blue;
    public Color PercentageColor { get; set; } = Colors.White;
    public string Percentage => $"{(int)Value}%";
    public string StatusText { get; set; } = string.Empty;
    public Color StatusColor { get; set; } = Color.FromArgb("#22C55E");
    public Color StatusBgColor { get; set; } = Color.FromArgb("#DCFCE7");
}

public class AiInsightItem
{
    public Color AccentColor { get; set; } = Color.FromArgb("#EF4444");
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ActionLabel { get; set; } = string.Empty;
    public Color ActionBgColor { get; set; } = Color.FromArgb("#1E3A5F");
    public Color ActionTextColor { get; set; } = Color.FromArgb("#60A5FA");
}

public class QuickActionItem
{
    public string IconGlyph { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class DashboardTableRow
{
    public string Date { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Color StatusBgColor => Status switch
    {
        "Valid"          => Color.FromArgb("#DCFCE7"),
        "Completed"      => Color.FromArgb("#DCFCE7"),
        "Scheduled"      => Color.FromArgb("#DBEAFE"),
        "Pending"        => Color.FromArgb("#FEF3C7"),
        "Expiring Soon"  => Color.FromArgb("#FEF3C7"),
        "Expired"        => Color.FromArgb("#FEE2E2"),
        "Overdue"        => Color.FromArgb("#FEE2E2"),
        _                => Color.FromArgb("#F1F5F9"),
    };

    public Color StatusColor => Status switch
    {
        "Valid"          => Color.FromArgb("#15803D"),
        "Completed"      => Color.FromArgb("#15803D"),
        "Scheduled"      => Color.FromArgb("#1D4ED8"),
        "Pending"        => Color.FromArgb("#D97706"),
        "Expiring Soon"  => Color.FromArgb("#B45309"),
        "Expired"        => Color.FromArgb("#DC2626"),
        "Overdue"        => Color.FromArgb("#DC2626"),
        _                => Color.FromArgb("#64748B"),
    };
}

public class HealthIssueItem
{
    public string Prefix { get; set; } = "✅";
    public string Text { get; set; } = string.Empty;
    public Color PrefixColor { get; set; } = Color.FromArgb("#22C55E");
    public bool IsWarning { get; set; }
}
