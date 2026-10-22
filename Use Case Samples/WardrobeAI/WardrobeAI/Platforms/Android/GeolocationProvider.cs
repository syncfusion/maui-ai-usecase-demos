using Microsoft.Extensions.Logging;
using WardrobeAI.Helpers;
using WardrobeAI.Services;

namespace WardrobeAI.Platforms.Android;

/// <summary>
/// Android implementation of <see cref="IGeolocationProvider"/> using MAUI's built-in
/// <see cref="Microsoft.Maui.ApplicationModel.Geolocation"/>. Location permission is
/// declared in <c>Platforms/Android/AndroidManifest.xml</c> (ACCESS_FINE_LOCATION +
/// ACCESS_COARSE_LOCATION); the runtime permission prompt is handled by
/// <see cref="Microsoft.Maui.ApplicationModel.Geolocation.RequestPermissionAsync"/>.
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
            // Run the permission request + GPS read on the platform thread when needed
            // but keep awaiting on the caller's synchronization context so the UI
            // thread is never blocked.
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
                    "Could not determine your current location. Make sure GPS is on and try again.");
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
            return Result<LocationFix>.Fail("Location.PermissionDenied",
                "Location permission was denied.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Geolocation lookup failed.");
            return Result<LocationFix>.Fail("Location.Failed", ex.Message);
        }
    }
}
