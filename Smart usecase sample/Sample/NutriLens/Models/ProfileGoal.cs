using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace NutriLens.Models;

public partial class ProfileGoal : ObservableObject
{
    public string Title { get; init; } = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}

public sealed class ProfileSetting
{
    public string Icon { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}

public sealed class UserDietaryPreference
{
    public List<string> DietaryGoals { get; set; } = [];
    public List<string> AllergiesAndPreferences { get; set; } = [];
    public List<string> HealthConsiderations { get; set; } = [];
    public string AvatarPath { get; set; } = string.Empty;
    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public bool IsEmpty =>
        DietaryGoals.Count == 0 &&
        AllergiesAndPreferences.Count == 0 &&
        HealthConsiderations.Count == 0 &&
        string.IsNullOrWhiteSpace(AvatarPath);
}