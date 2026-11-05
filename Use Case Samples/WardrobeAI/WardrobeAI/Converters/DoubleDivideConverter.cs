using System.Globalization;

namespace WardrobeAI.Converters;

/// <summary>
/// Divides a numeric binding value (e.g. an integer percentage) by a constant
/// <c>double</c> divisor passed via the converter parameter, returning a
/// <c>double</c> result clamped to <c>[0, 1]</c>.
///
/// <para>Used to map <c>CompletionScore</c> (0–100) onto
/// <c>ProgressBar.Progress</c> (0–1) so the visual fill matches the
/// percentage the view model already reports.</para>
/// </summary>
public sealed class DoubleDivideConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double numerator = value switch
        {
            double d => d,
            int i => i,
            decimal m => (double)m,
            float f => f,
            long l => l,
            _ => 0d,
        };

        double divisor = 1d;
        if (parameter is not null)
        {
            // Accept either a raw number ("100") or a culture-formatted one.
            var raw = parameter.ToString();
            if (!string.IsNullOrWhiteSpace(raw)
                && double.TryParse(raw, NumberStyles.Any, culture, out var parsed))
            {
                divisor = parsed;
            }
        }

        if (divisor == 0d)
        {
            return 0d;
        }

        var ratio = numerator / divisor;
        if (ratio < 0d)
        {
            return 0d;
        }
        if (ratio > 1d)
        {
            return 1d;
        }
        return ratio;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double d ? d : 0d;
}
