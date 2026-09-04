using CommunityToolkit.Mvvm.ComponentModel;

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