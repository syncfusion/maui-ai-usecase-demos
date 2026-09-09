using System.Text.Json;
using NutriLens.Models;

namespace NutriLens.Services;

public interface IUserPreferenceStore
{
    UserDietaryPreference Load();
    void Save(UserDietaryPreference preferences);
    void Clear();
}

public sealed class UserPreferenceStore : IUserPreferenceStore
{
    private const string StorageKey = "nutrilens_user_preferences_v1";

    public UserDietaryPreference Load()
    {
        try
        {
            var json = Preferences.Get(StorageKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return new UserDietaryPreference();

            return JsonSerializer.Deserialize<UserDietaryPreference>(
                       json,
                       new JsonSerializerOptions
                       {
                           PropertyNameCaseInsensitive = true
                       })
                   ?? new UserDietaryPreference();
        }
        catch
        {
            return new UserDietaryPreference();
        }
    }

    public void Save(UserDietaryPreference preferences)
    {
        try
        {
            var json = JsonSerializer.Serialize(preferences, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            Preferences.Set(StorageKey, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserPreferenceStore] Save failed: {ex}");
        }
    }

    public void Clear()
    {
        try
        {
            Preferences.Remove(StorageKey);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UserPreferenceStore] Clear failed: {ex}");
        }
    }
}