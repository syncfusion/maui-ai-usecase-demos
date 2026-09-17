namespace PatientCompanion.Models;

public class HealthMetricCardModel
{
    public string Title { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public bool IsSelected { get; set; }

    public string DisplayValue =>
        string.IsNullOrWhiteSpace(Unit)
            ? Value
            : $"{Value} {Unit}";

    public Color BackgroundColor =>
        IsSelected
            ? Color.FromArgb("#00685F")
            : Colors.White;

    public Color TitleColor =>
        IsSelected
            ? Colors.White
            : Color.FromArgb("#526171");

    public Color ValueColor =>
        IsSelected
            ? Colors.White
            : Color.FromArgb("#1F2937");
}