using System.Globalization;

namespace AIChatSample.Converters;

/// <summary>
/// Returns <c>true</c> when the bound string is non-null and non-whitespace.
/// Used by the chat UI to toggle visibility of empty-state placeholders,
/// avatars, etc.
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
