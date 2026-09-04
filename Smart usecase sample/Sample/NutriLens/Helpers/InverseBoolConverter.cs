using NutriLens.Models;
using System.Globalization;

namespace NutriLens.Helpers;

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