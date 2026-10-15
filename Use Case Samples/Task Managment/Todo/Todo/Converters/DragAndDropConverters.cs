using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Module-07 helper. Multiplies the bound numeric value by the
/// factor in <c>ConverterParameter</c> (parsed as a double). Used
/// to convert an item index into a pixel offset
/// (e.g. <c>DropGapIndex * 70</c>) for the gap-highlight bar that
/// appears under the dragged card in the notes list.
/// </summary>
public sealed class MultiplyByConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var factor = 1.0;
        if (parameter is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            factor = parsed;
        else if (parameter is double d) factor = d;
        else if (parameter is int i) factor = i;

        return value switch
        {
            int idx => idx * factor,
            double dd => dd * factor,
            long ll => ll * factor,
            _ => 0d
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Module-07 helper used in a <see cref="MultiBinding"/> on the
/// note card. The first binding supplies the note's <c>Id</c>, the
/// second supplies the ViewModel's <c>DraggingNoteId</c>, and the
/// converter returns the dragged-state color when the two match
/// or the resting color otherwise. Both colors are passed in
/// <see cref="ConverterParameter"/> as
/// <c>"resting|dragged"</c> hex pairs to keep the XAML compact.
/// </summary>
public sealed class DragStateColorConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length < 2) return Colors.Transparent;
        var id = values[0] as string;
        var dragging = values[1] as string;
        var isDragged = !string.IsNullOrEmpty(id)
            && !string.IsNullOrEmpty(dragging)
            && string.Equals(id, dragging, StringComparison.Ordinal);

        // Default to a subtle highlight when the parameter is missing.
        if (parameter is not string param || string.IsNullOrEmpty(param))
            return isDragged ? Color.FromArgb("#EEEAFB") : Colors.Transparent;

        var parts = param.Split('|');
        if (parts.Length != 2) return Colors.Transparent;
        var resting = Color.FromArgb(parts[0]);
        var dragged = Color.FromArgb(parts[1]);
        return isDragged ? dragged : resting;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
