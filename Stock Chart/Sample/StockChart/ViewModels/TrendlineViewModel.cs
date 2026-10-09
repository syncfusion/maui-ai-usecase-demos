using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.ViewModels;

public partial class TrendlineViewModel : ObservableObject
{
    public TrendlineViewModel(string name)
    {
        Name = name;
    }

    public string Name { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisibilityIcon))]
    public partial bool IsVisible { get; set; } = true;

    public string VisibilityIcon => IsVisible ? "\uE8F4" : "\uE8F5";
}