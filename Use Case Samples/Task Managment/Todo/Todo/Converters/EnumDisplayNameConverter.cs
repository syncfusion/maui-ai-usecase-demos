using System.Globalization;
using Todo.Common;

namespace Todo.Converters;

/// <summary>
/// Returns the human-friendly display name for a
/// <see cref="NotesListCategory"/> value.
/// </summary>
public sealed class EnumDisplayNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is NotesListCategory c ? c.ToDisplayName() : value?.ToString() ?? string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}