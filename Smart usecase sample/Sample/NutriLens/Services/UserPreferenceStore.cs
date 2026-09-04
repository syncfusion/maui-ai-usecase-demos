using System.Text.Json;
using NutriLens.Models;

namespace NutriLens.Services;

public interface IUserPreferenceStore
{
    UserDietaryPreference Load();
    void Save(UserDietaryPreference preference);
}

/// <summary>
/// Backed by <see cref="Preferences"/> with a single JSON payload,
/// so adding/removing preference types later needs no schema migration.
/// </summary>
public sealed class UserPreferenceStore : IUserPreferenceStore
{
    private const string Key = "nutrilens.user.preference.v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UserDietaryPreference Load()
    {
        var json = Preferences.Get(Key, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return new UserDietaryPreference();

        try
        {
            return JsonSerializer.Deserialize<UserDietaryPreference>(json, JsonOptions)
                ?? new UserDietaryPreference();
        }
        catch
        {
            // Corrupt payload — reset to empty rather than crash.
            return new UserDietaryPreference();
        }
    }

    public void Save(UserDietaryPreference preference)
    {
        ArgumentNullException.ThrowIfNull(preference);
        var json = JsonSerializer.Serialize(preference, JsonOptions);
        Preferences.Set(Key, json);
    }
}