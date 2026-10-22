using System.Globalization;

namespace SmartCropping.Converters;

/// <summary>
/// Converts a boolean to one of two arbitrary objects supplied as the
/// <c>parameter</c> in the form <c>"TrueValue|FalseValue"</c>.
/// </summary>
/// <remarks>
/// Used by the status banner to swap between the loading, success and error
/// background colors without duplicating value converters.
/// </remarks>
public sealed class BoolToObjectConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool boolean = value is bool b && b;
        string[] options = (parameter as string)?.Split('|') ?? Array.Empty<string>();

        if (options.Length == 2)
        {
            return boolean ? options[0] : options[1];
        }

        return boolean;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
