namespace WardrobeAI.Contracts;

/// <summary>
/// Coarse weather condition categories surfaced to the UI. The service implementation
/// maps the upstream provider's WMO weather code to one of these values so the View
/// only ever reasons about a small fixed set (drives the dashboard weather icon and
/// later the outfit-planner weather-match scoring, wardrobe.prompt §3.4.2).
/// </summary>
public enum WeatherCondition
{
    /// <summary>Default — used when no weather data is available (offline / location denied / API error).</summary>
    Unknown,
    Sunny,
    Cloudy,
    Rainy,
    Snowy,
    Stormy
}

/// <summary>
/// Dashboard-ready weather snapshot. <see cref="TemperatureDisplay"/> is the
/// pre-formatted string (e.g. "22°C") so the View doesn't need to know about units.
/// <see cref="IconKey"/> maps to a local image asset in <c>Resources/Images</c>
/// (<c>sunny.png</c>, <c>cloudy.png</c>, <c>rainny.png</c>).
/// </summary>
public sealed record WeatherInfo(
    double TemperatureC,
    WeatherCondition Condition,
    int HumidityPercent,
    string LocationLabel)
{
    /// <summary>Pre-formatted temperature string suitable for direct display.</summary>
    public string TemperatureDisplay => $"{Math.Round(TemperatureC):0}°C";

    /// <summary>Filename (no extension) of the matching icon in <c>Resources/Images</c>.</summary>
    public string IconKey => Condition switch
    {
        WeatherCondition.Sunny => "sunny",
        WeatherCondition.Cloudy => "cloudy",
        WeatherCondition.Rainy => "rainny",
        WeatherCondition.Snowy => "cloudy",   // Reuse cloudy until a snow asset is added.
        WeatherCondition.Stormy => "rainny",  // Reuse rainy for storms.
        _ => "cloudy"
    };
}

/// <summary>
/// Result of a geolocation lookup. Latitude/Longitude in WGS84 decimal degrees.
/// <see cref="Label"/> is a coarse human-readable label (e.g. "Pune, IN") so we never
/// surface exact street addresses in the UI (wardrobe.prompt §3.5 PII minimization).
/// </summary>
public sealed record GeoLocation(double Latitude, double Longitude, string Label);
