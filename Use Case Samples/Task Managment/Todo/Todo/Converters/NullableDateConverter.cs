using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Round-trips between a <see cref="DateTime"/> (used by the
/// <see cref="DatePicker"/>) and a nullable <see cref="DateTime"/>
/// (used by the Edit Note ViewModel for "no due date"). When the
/// picker writes today's date to a <c>null</c> source, the binding
/// sets the bound nullable to today so the user can clear it via
/// the on/off switch.
/// </summary>
public sealed class NullableDateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt) return dt;
        return DateTime.Today;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt) return dt;
        return DateTime.Today;
    }
}

/// <summary>
/// Round-trips between <see cref="TimeSpan"/> (used by the
/// <see cref="TimePicker"/>) and a nullable <see cref="TimeSpan"/>
/// (used by the Edit Note ViewModel for "no time"). Falls back to
/// midnight when the source is null.
/// </summary>
public sealed class NullableTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts) return ts;
        return TimeSpan.Zero;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts) return ts;
        return TimeSpan.Zero;
    }
}
