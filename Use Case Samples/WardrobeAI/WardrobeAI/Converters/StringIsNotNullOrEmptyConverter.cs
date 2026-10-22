using System.Globalization;

namespace WardrobeAI.Converters;

/// <summary>
/// Returns true when the bound string is not null or empty/whitespace.
/// Used to conditionally show optional chips/labels (e.g. Season).
/// </summary>
public sealed class StringIsNotNullOrEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
