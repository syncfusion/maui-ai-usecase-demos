using System.Globalization;
using Todo.Features.Notes.Models;

namespace Todo.Converters;

/// <summary>
/// Converts a <see cref="RepeatType"/> to a human-readable label
/// ("Daily" / "Weekly" / ...) or, with <c>ConverterParameter=visibility</c>,
/// to a <see cref="bool"/> that drives label visibility (false when
/// the value is <see cref="RepeatType.None"/>).
/// </summary>
public sealed class RepeatTypeToGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var rt = value is RepeatType t ? t : RepeatType.None;

        if (parameter is string s && string.Equals(s, "visibility", StringComparison.OrdinalIgnoreCase))
            return rt != RepeatType.None;

        return rt switch
        {
            RepeatType.Daily => "Daily",
            RepeatType.Weekly => "Weekly",
            RepeatType.Monthly => "Monthly",
            RepeatType.Yearly => "Yearly",
            _ => string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
