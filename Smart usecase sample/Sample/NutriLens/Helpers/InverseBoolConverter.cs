using NutriLens.Models;
using System.Globalization;

namespace NutriLens.Helpers;

public sealed class HistoryScoreBrushConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var colorText = value?.ToString();

        if (string.IsNullOrWhiteSpace(colorText))
            return new SolidColorBrush(Color.FromArgb("#047857"));

        try
        {
            var color = Color.FromArgb(colorText);
            return new SolidColorBrush(color);
        }
        catch
        {
            return new SolidColorBrush(Color.FromArgb("#047857"));
        }
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
public sealed class HistoryScoreConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is int integerScore)
            return Math.Clamp(integerScore, 0, 100);

        if (value is double doubleScore)
            return Math.Clamp(doubleScore, 0, 100);

        if (value is not string scoreText)
            return 0d;

        var numericPart = scoreText
            .Trim()
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.Trim();

        return double.TryParse(
            numericPart,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var score)
            ? Math.Clamp(score, 0, 100)
            : 0d;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
 
public sealed class BoolToPeriodBackgroundConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is true
            ? "#047857"
            : "Transparent";
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class BoolToPeriodTextColorConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is true
            ? "White"
            : "Black";
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
public sealed class ProfileSelectedBackgroundConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is true ? "#19C37D" : "#DCEAFF";
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class ProfileSelectedTextConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is true ? "#0B3D2C" : "#1E3A8A";
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class PreferenceBackgroundConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is true ? "#F5A623" : "#DCEAFF";
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class PreferenceTextConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is true ? "#6A3A00" : "#1E3A8A";
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
public static class SelectedImageHolder
{
    public static FileResult? Current { get; set; }
}
public static class AnalysisNavigationData
{
    public static IngredientAnalysisResult? CurrentResult { get; set; }

    /// <summary>
    /// Short-lived handoff bucket used when Scan → Review navigation
    /// needs to carry the extracted OCR text and the source image.
    /// Read once and cleared by the Review page — never reused.
    /// </summary>
    public static PendingReviewData? PendingReview { get; set; }
}
public sealed class PendingReviewData
{
    public required string ExtractedText { get; init; }
    public required FileResult Image { get; init; }
}
public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is bool booleanValue && !booleanValue;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}