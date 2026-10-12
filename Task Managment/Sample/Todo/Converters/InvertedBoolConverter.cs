using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Inverts a <see cref="bool"/> value (returns <c>!value</c>).  Useful for
/// <c>IsEnabled</c> bindings on the opposite of a "loading" / "busy" flag
/// without having to expose a second computed property on the ViewModel.
/// </summary>
/// <remarks>
/// Non-bool values resolve to <see langword="true"/> so that bindings
/// degrade gracefully (e.g. a null source is treated as "not busy", so the
/// control is enabled).
/// </remarks>
public sealed class InvertedBoolConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;
}
