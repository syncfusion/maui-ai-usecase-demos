namespace NutriLens.Models;

public sealed class ProfileGoal
{
    public string Title { get; init; } = string.Empty;
    public bool IsSelected { get; init; }
}

public sealed class ProfileSetting
{
    public string Icon { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}