using System.Globalization;

namespace SmartCropping.Converters;

/// <summary>
/// Converts a boolean value to its inverse.
/// </summary>
/// <remarks>
/// Used to bind the <c>IsEnabled</c> state of action buttons to the
/// <c>IsBusy</c> flag of the view model without writing extra inverse
/// properties on every view model.
/// </remarks>
public sealed class InverseBoolConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return !boolean;
        }

        return false;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return !boolean;
        }

        return false;
    }
}
