using WardrobeAI.Helpers;

namespace WardrobeAI.Services;

/// <summary>
/// Fallback <see cref="IGeolocationProvider"/> used when the host platform doesn't
/// have a real implementation (e.g. running under a unit-test host or a future
/// platform target that hasn't been wired up yet). Always reports a friendly
/// failure so the dashboard renders the placeholder weather chip rather than
/// crashing.
/// </summary>
public sealed class NullGeolocationProvider : IGeolocationProvider
{
    public Task<Result<LocationFix>> GetCurrentAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result<LocationFix>.Fail(
            "Location.NotSupported",
            "Location services aren't available on this platform target."));
}
