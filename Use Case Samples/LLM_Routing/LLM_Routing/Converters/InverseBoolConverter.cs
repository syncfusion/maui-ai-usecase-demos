using System.Globalization;

namespace LLM_Routing.Converters;

/// <summary>
/// Returns the boolean negation of the bound value. Used in XAML to flip a
/// binding like IsVisible={Binding IsGenerating, Converter=...} into "visible
/// while NOT generating".
/// </summary>
public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}
