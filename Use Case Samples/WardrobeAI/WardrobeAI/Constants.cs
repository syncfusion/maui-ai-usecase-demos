namespace WardrobeAI;

/// <summary>
/// Project-wide constants. Values here are inlined by
/// <see cref="Services.UserContext"/> when the singleton default user row is
/// seeded on first run, so the app boots into a populated profile without
/// any sign-in flow.
/// </summary>
/// <remarks>
/// This class lives at the <c>WardrobeAI</c> namespace root (not under the
/// sibling <c>WardrobeAI.Constants</c> namespace used by
/// <see cref="ClothingOptions"/>) so the two can coexist. Refer to this class
/// as <see cref="AppConstants"/> everywhere it's consumed.
/// </remarks>
public static class AppConstants
{
    /// <summary>
    /// Full name used for the auto-created default user. Shown wherever a
    /// signed-in user's display name would normally appear (Profile header,
    /// dashboard greeting, etc.). Edit this single constant to retitle the
    /// default user everywhere.
    /// </summary>
    public const string DefaultUserName = "Alex Rivera";
}
