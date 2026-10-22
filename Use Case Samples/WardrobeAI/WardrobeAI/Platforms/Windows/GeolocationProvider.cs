using Microsoft.Extensions.Logging;
using WardrobeAI.Helpers;
using WardrobeAI.Services;

namespace WardrobeAI.Platforms.Windows;

/// <summary>
/// Windows implementation of <see cref="IGeolocationProvider"/> using MAUI's built-in
/// <see cref="Microsoft.Maui.ApplicationModel.Geolocation"/>. The Windows app manifest
/// must declare the <c>location</c> capability and the <c>Windows.Devices.Geolocation</c>
/// device capability (see <c>Platforms/Windows/Package.appxmanifest</c>).
/// </summary>
public sealed class GeolocationProvider : IGeolocationProvider
{
    private readonly ILogger<GeolocationProvider> _logger;

    public GeolocationProvider(ILogger<GeolocationProvider> logger)
    {
        _logger = logger;
    }

    public async Task<Result<LocationFix>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    _logger.LogWarning("Location permission denied ({Status}).", status);
                    return Result<LocationFix>.Fail("Location.PermissionDenied",
                        "Location permission was denied. Enable it in Settings to see live weather.");
                }
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request, cancellationToken);
            if (location is null)
            {
                return Result<LocationFix>.Fail("Location.Unavailable",
                    "Could not determine your current location. Make sure location is enabled and try again.");
            }

            return Result<LocationFix>.Ok(new LocationFix(location.Latitude, location.Longitude));
        }
        catch (FeatureNotSupportedException)
        {
            return Result<LocationFix>.Fail("Location.NotSupported",
                "This device does not support location services.");
        }
        catch (FeatureNotEnabledException)
        {
            return Result<LocationFix>.Fail("Location.Disabled",
                "Location services are turned off. Enable them to see live weather.");
        }
        catch (PermissionException ex)
        {
            _logger.LogError(ex, "Location permission error.");
            return Result<LocationFix>.Fail("Location.PermissionDenied", "Location permission was denied.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Geolocation lookup failed.");
            return Result<LocationFix>.Fail("Location.Failed", ex.Message);
        }
    }
}
