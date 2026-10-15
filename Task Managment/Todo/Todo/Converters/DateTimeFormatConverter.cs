using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Formats a nullable <see cref="DateTime"/> using a culture-aware
/// pattern. Returns an empty string for <c>null</c>.
///
/// <para>
/// Output format is selected via the <c>parameter</c> on the binding:
/// </para>
/// <list type="bullet">
///   <item><c>date</c> (default) – <c>ddd d MMM</c> (e.g. "Wed 18 Feb")</item>
///   <item><c>time</c> – <c>HH:mm</c> (e.g. "10:00")</item>
///   <item><c>datetime</c> – <c>ddd d MMM, HH:mm</c> (e.g. "Wed 18 Feb, 10:00")</item>
/// </list>
/// </summary>
public sealed class DateTimeFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime dt)
            return string.Empty;

        var mode = (parameter as string ?? "date").ToLowerInvariant();
        return mode switch
        {
            "time"     => dt.ToString("HH:mm", culture),
            "datetime" => dt.ToString("ddd d MMM, HH:mm", culture),
            _          => dt.ToString("ddd d MMM", culture)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
