using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.ViewModels;

public partial class ChartSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsTooltipEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsLogarithmicAxis { get; set; }

    [ObservableProperty]
    public partial bool IsAxisInverted { get; set; }

    [ObservableProperty]
    public partial bool IsAxisOpposed { get; set; }

    [ObservableProperty]
    public partial bool IsRangeControlEnabled { get; set; }
}