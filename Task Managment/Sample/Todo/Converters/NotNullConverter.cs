using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Returns <see langword="true"/> when the source value is non-null.  Used
/// by the task-card template to toggle the visibility of the optional
/// metadata rows (due date, reminder) without exposing a separate
/// "HasMetadata" property on every wrapper.
/// </summary>
/// <remarks>
/// For <see cref="string"/> sources the converter additionally treats
/// <see cref="string.Empty"/> as null so blank strings are also hidden.
/// </remarks>
public sealed class NotNullConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return false;
        }

        if (value is string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        return true;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("NotNullConverter only supports Convert.");
}