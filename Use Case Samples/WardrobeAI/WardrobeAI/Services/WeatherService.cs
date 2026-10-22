using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WardrobeAI.Contracts;
using WardrobeAI.Helpers;

namespace WardrobeAI.Services;

/// <summary>
/// Live weather + location service.
///
/// Backed by the free <see href="https://open-meteo.com/">Open-Meteo</see> APIs
/// (forecast + geocoding), which require no API key and have a permissive usage
/// policy that fits a demo. Reverse geocoding is also Open-Meteo so the service
/// stays single-source and key-free.
///
/// <para>Pipeline:</para>
/// <list type="number">
///   <item>Resolve the device's WGS84 location via <see cref="Microsoft.Maui.ApplicationModel.Geolocation"/>
///         (MAUI Essentials — already in the project, no extra NuGet).</item>
///   <item>Look up a coarse place label ("Pune, IN") via the Open-Meteo geocoding API
///         — the dashboard chip only needs a friendly label, never a street address
///         (wardrobe.prompt §3.5 PII minimization).</item>
///   <item>Fetch the current temperature, humidity, and WMO weather code from
///         the Open-Meteo forecast API, then map to <see cref="WeatherCondition"/>.</item>
/// </list>
///
/// <para>All public methods return <see cref="Result{T}"/> — no exceptions escape
/// to the ViewModel layer. The dashboard's <c>Weather</c> property simply retains
/// its last known good value on failure (no flicker, no error toast on every
/// offline render).</para>
/// </summary>
public sealed class WeatherService : IWeatherService
{
    private const string ForecastUrl = "https://api.open-meteo.com/v1/forecast";
    private const string GeocodingUrl = "https://geocoding-api.open-meteo.com/v1/reverse";

    private readonly IGeolocationProvider _location;
    private readonly HttpClient _http;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(IGeolocationProvider location, ILogger<WeatherService> logger)
    {
        _location = location;
        _logger = logger;

        // Single shared client with a 10s ceiling so the dashboard never blocks
        // the UI thread waiting on a slow upstream. We build it on top of a
        // SocketsHttpHandler with a finite PooledConnectionLifetime so we
        // never inherit a "Socket closed" IOException from a long-lived pooled
        // keep-alive socket that the upstream (or an intermediate LB) already
        // tore down — that's the classic symptom on .NET 10 Windows when the
        // DNS or the connection-idle timeout reaper on Azure/Open-Meteo wins
        // the race against our next request.
        _http = new HttpClient(
            new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                ConnectTimeout = TimeSpan.FromSeconds(10)
            })
        {
            Timeout = TimeSpan.FromSeconds(100)
        };
        if (!_http.DefaultRequestHeaders.UserAgent.TryParseAdd("WardrobeAI/1.0 (+https://wardrobeai.example)"))
        {
            // Best-effort: a few platforms reject the user-agent header; the
            // request still works without it.
        }
    }

    public async Task<Result<GeoLocation>> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var locationResult = await _location.GetCurrentAsync(cancellationToken);
            if (!locationResult.IsSuccess)
            {
                return Result<GeoLocation>.Fail(locationResult.Error!.Code, locationResult.Error!.Message);
            }

            var fix = locationResult.Value!;
            var label = await ResolvePlaceLabelAsync(fix.Latitude, fix.Longitude, cancellationToken)
                        ?? "Your location";

            return Result<GeoLocation>.Ok(new GeoLocation(fix.Latitude, fix.Longitude, label));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCurrentLocationAsync failed.");
            return Result<GeoLocation>.Fail("Weather.LocationFailed", ex.Message);
        }
    }

    public async Task<Result<WeatherInfo>> GetCurrentWeatherAsync(GeoLocation location, CancellationToken cancellationToken = default)
    {
        if (location is null)
        {
            return Result<WeatherInfo>.Fail("Weather.NullLocation", "Location was null.");
        }

        try
        {
            var url =
                $"{ForecastUrl}" +
                $"?latitude={location.Latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&longitude={location.Longitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&current=temperature_2m,relative_humidity_2m,weather_code";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Open-Meteo forecast returned {Status} for {Lat},{Lon}.", (int)response.StatusCode, location.Latitude, location.Longitude);
                return Result<WeatherInfo>.Fail("Weather.ApiError", $"Weather provider returned {(int)response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var current = doc.RootElement.GetProperty("current");

            var tempC = current.GetProperty("temperature_2m").GetDouble();
            var humidity = current.TryGetProperty("relative_humidity_2m", out var h) ? h.GetInt32() : 0;
            var code = current.GetProperty("weather_code").GetInt32();

            var info = new WeatherInfo(
                TemperatureC: tempC,
                Condition: MapWeatherCode(code),
                HumidityPercent: humidity,
                LocationLabel: location.Label);

            return Result<WeatherInfo>.Ok(info);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning("Open-Meteo forecast request timed out.");
            return Result<WeatherInfo>.Fail("Weather.Timeout", "Weather request timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCurrentWeatherAsync failed for {Lat},{Lon}.", location.Latitude, location.Longitude);
            return Result<WeatherInfo>.Fail("Weather.Failed", ex.Message);
        }
    }

    public async Task<Result<WeatherInfo>> GetCurrentWeatherAsync(CancellationToken cancellationToken = default)
    {
        var locationResult = await GetCurrentLocationAsync(cancellationToken);
        if (!locationResult.IsSuccess)
        {
            return Result<WeatherInfo>.Fail(locationResult.Error!.Code, locationResult.Error!.Message);
        }
        return await GetCurrentWeatherAsync(locationResult.Value!, cancellationToken);
    }

    /// <summary>
    /// Reverse-geocodes the device's lat/lon into a coarse "City, CC" label via
    /// Open-Meteo's reverse-geocoding endpoint. Returns null on any failure so
    /// the caller can fall back to a neutral string.
    /// </summary>
    private async Task<string?> ResolvePlaceLabelAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        try
        {
            var url =
                $"{GeocodingUrl}" +
                $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&count=1&language=en&format=json";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                return null;
            }

            var first = results[0];
            var name = first.TryGetProperty("name", out var n) ? n.GetString() : null;
            var country = first.TryGetProperty("country_code", out var c) ? c.GetString() : null;
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            return string.IsNullOrWhiteSpace(country) ? name! : $"{name}, {country}";
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Reverse geocoding failed; falling back to neutral label.");
            return null;
        }
    }

    /// <summary>
    /// Maps the Open-Meteo WMO weather code onto the dashboard's coarse
    /// <see cref="WeatherCondition"/>. WMO codes follow the
    /// <see href="https://open-meteo.com/en/docs">Open-Meteo WMO table</see>
    /// (also documented in WMO Publication No. 4677).
    ///
    /// <para>Notes on the freezing-precipitation codes (56, 57, 66, 67):
    /// these are technically frozen water but the user-facing experience is
    /// "wet and cold", so we map them to <see cref="WeatherCondition.Rainy"/>
    /// — same as every mainstream consumer app. Temperature drives the coat
    /// decision in the outfit planner regardless, so the outfit score stays
    /// consistent whether we pick Rainy or Snowy here.</para>
    /// </summary>
    internal static WeatherCondition MapWeatherCode(int code) => code switch
    {
        // Clear sky / Mainly clear
        0 or 1 => WeatherCondition.Sunny,
        // Partly cloudy / Overcast
        2 or 3 => WeatherCondition.Cloudy,
        // Fog / Depositing rime fog (treated as "cloudy" — no fog asset on the dashboard)
        45 or 48 => WeatherCondition.Cloudy,
        // Drizzle (light, moderate, dense) + Freezing drizzle (light, dense)
        51 or 53 or 55 or 56 or 57 => WeatherCondition.Rainy,
        // Rain (slight, moderate, heavy) + Freezing rain (light, heavy)
        61 or 63 or 65 or 66 or 67 => WeatherCondition.Rainy,
        // Snow fall (slight, moderate, heavy) + Snow grains
        71 or 73 or 75 or 77 => WeatherCondition.Snowy,
        // Snow showers (slight, heavy)
        85 or 86 => WeatherCondition.Snowy,
        // Rain showers (slight, moderate, violent)
        80 or 81 or 82 => WeatherCondition.Rainy,
        // Thunderstorm (slight/moderate, with slight hail, with heavy hail)
        95 or 96 or 99 => WeatherCondition.Stormy,
        _ => WeatherCondition.Unknown
    };
}
