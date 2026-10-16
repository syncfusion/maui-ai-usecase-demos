using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Returns <c>true</c> when the bound string is not null, not empty,
/// and not pure whitespace. Used by the Search page (Module-06) to
/// reveal the inline clear (×) button only when the user has typed
/// something.
/// </summary>
public sealed class StringNotEmptyToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
