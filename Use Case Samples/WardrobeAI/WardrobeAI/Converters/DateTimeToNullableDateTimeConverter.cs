using System.Globalization;

namespace WardrobeAI.Converters;

/// <summary>
/// Bridges <see cref="DateTime"/> (non-nullable, used by view models) and
/// <see cref="Nullable{DateTime}"/> (used by Syncfusion's <c>SfDatePicker.SelectedDate</c>).
/// When converting back, <c>null</c> is treated as <see cref="DateTime.Today"/> so the
/// picker never pushes a null into a non-nullable view-model property.
/// </summary>
public sealed class DateTimeToNullableDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dt ? (DateTime?)dt : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime dt ? dt : DateTime.Today;
}
