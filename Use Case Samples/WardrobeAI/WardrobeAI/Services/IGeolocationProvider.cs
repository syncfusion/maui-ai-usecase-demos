using WardrobeAI.Helpers;

namespace WardrobeAI.Services;

/// <summary>
/// Abstraction over the platform's GPS lookup. The default implementation lives in
/// <c>Platforms/*/GeolocationProvider.cs</c> and uses
/// <see cref="Microsoft.Maui.ApplicationModel.Geolocation"/> from MAUI Essentials.
/// Wrapping it in an interface keeps <see cref="WeatherService"/> unit-testable
/// and lets us swap providers (e.g. a fused location provider on Android) without
/// touching the weather pipeline.
/// </summary>
public interface IGeolocationProvider
{
    /// <summary>
    /// Resolves the device's current WGS84 lat/lon. Returns a failure (not an exception)
    /// when the user has denied location permission, the platform reports no fix
    /// within the timeout, or location services are disabled.
    /// </summary>
    Task<Result<LocationFix>> GetCurrentAsync(CancellationToken cancellationToken = default);
}

/// <summary>Plain WGS84 lat/lon fix. Kept separate from <see cref="Contracts.GeoLocation"/>
/// so the geolocation layer doesn't depend on the weather contract.</summary>
public sealed record LocationFix(double Latitude, double Longitude);
