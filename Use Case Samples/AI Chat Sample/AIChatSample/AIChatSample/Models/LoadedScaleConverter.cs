using System.Globalization;

namespace AIChatSample;

/// <summary>
/// Maps a boolean (loading-done flag) to a 0.85 / 1.0 scale factor so the
/// Configure Agent preview pane "pops" in once the agent data is ready.
/// Used by <c>AgentConfigurePage.xaml</c> via the <c>CALoadedScale</c> key.
/// </summary>
public sealed class LoadedScaleConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? 1.0 : 0.85;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
