namespace eSIMDataManager;

/// <summary>
/// Navigation bridge used by the shared demo code. On mobile there is no Shell, so the
/// host page registers a callback; on desktop it falls back to Shell routing.
/// </summary>
internal static class AppNavigation
{
    public static Action? GoToProfiles { get; set; }

    public static Task ToProfilesAsync()
    {
        if (GoToProfiles is not null)
        {
            GoToProfiles();
            return Task.CompletedTask;
        }

        return Shell.Current?.GoToAsync("//profiles") ?? Task.CompletedTask;
    }
}
