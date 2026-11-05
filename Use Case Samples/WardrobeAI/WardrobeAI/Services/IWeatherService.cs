using WardrobeAI.Contracts;
using WardrobeAI.Helpers;

namespace WardrobeAI.Services;

/// <summary>
/// Resolves the device's current location and fetches the live weather for the dashboard
/// greeting chip (Module 2) and the outfit-planner weather-match scoring (Module 4).
///
/// All public methods return <see cref="Result{T}"/> per wardrobe.prompt §10; the
/// ViewModel is responsible for rendering a friendly fallback when a request fails
/// (offline, permission denied, location services off).
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Resolves the device's current <see cref="GeoLocation"/>. Returns a failure
    /// (not an exception) when location permission is denied, the GPS is off, or
    /// the platform can't resolve a fix within the timeout.
    /// </summary>
    Task<Result<GeoLocation>> GetCurrentLocationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the current weather for the supplied location. Implementations should
    /// use a free public weather API (no API key required) and map the upstream
    /// condition code onto <see cref="WeatherCondition"/>.
    /// </summary>
    Task<Result<WeatherInfo>> GetCurrentWeatherAsync(GeoLocation location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience: location + weather in one call. Used by the dashboard's
    /// <c>OnAppearing</c> refresh path.
    /// </summary>
    Task<Result<WeatherInfo>> GetCurrentWeatherAsync(CancellationToken cancellationToken = default);
}
