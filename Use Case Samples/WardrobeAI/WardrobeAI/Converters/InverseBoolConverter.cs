using System.Globalization;

namespace WardrobeAI.Converters;

/// <summary>
/// Inverts a boolean. <c>true</c> → <c>false</c> and vice versa.
/// Used to disable a CTA while a busy indicator is visible.
/// </summary>
public sealed class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}
