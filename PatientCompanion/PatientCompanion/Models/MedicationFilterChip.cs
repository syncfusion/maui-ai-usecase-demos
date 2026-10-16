using CommunityToolkit.Mvvm.ComponentModel;

namespace PatientCompanion.Models;

public partial class MedicationFilterChip : ObservableObject
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private bool isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(TextColor));
    }

    public string BackgroundColor =>
        IsSelected ? "#00685F" : "#F7F9FB";

    public string TextColor =>
        IsSelected ? "#FFFFFF" : "#6B7280";
}