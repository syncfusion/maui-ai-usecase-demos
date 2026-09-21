using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace PatientCompanion.ViewModels;

public partial class HealthMetricCardViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string chartTitle = string.Empty;

    [ObservableProperty]
    private string displayValue = string.Empty;

    [ObservableProperty]
    private string unit = string.Empty;

    [ObservableProperty]
    private string icon = string.Empty;

    public ICommand? SelectCommand { get; set; }

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(CardBackgroundColor));
        OnPropertyChanged(nameof(TitleColor));
        OnPropertyChanged(nameof(ValueColor));
    }

    public Color CardBackgroundColor =>
        IsSelected
            ? Color.FromArgb("#00685F")
            : Colors.White;

    public Color TitleColor =>
        IsSelected
            ? Colors.White
            : Colors.Black;

    public Color ValueColor =>
        IsSelected
            ? Colors.White
            : Colors.Black;
}