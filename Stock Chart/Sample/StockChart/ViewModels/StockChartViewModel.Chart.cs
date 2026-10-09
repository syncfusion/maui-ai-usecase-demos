namespace StockChart.ViewModels;

public partial class StockChartViewModel
{
    public void LoadSettingsDraft()
    {
        SettingsDraft.IsTooltipEnabled = IsTooltipEnabled;
        SettingsDraft.IsLogarithmicAxis = IsLogarithmicAxis;
        SettingsDraft.IsAxisInverted = IsAxisInverted;
        SettingsDraft.IsAxisOpposed = IsAxisOpposed;
        SettingsDraft.IsRangeControlEnabled = IsRangeControlEnabled;
    }

    public void ApplySettingsDraft()
    {
        IsTooltipEnabled = SettingsDraft.IsTooltipEnabled;
        IsLogarithmicAxis = SettingsDraft.IsLogarithmicAxis;
        IsAxisInverted = SettingsDraft.IsAxisInverted;
        IsAxisOpposed = SettingsDraft.IsAxisOpposed;
        IsRangeControlEnabled = SettingsDraft.IsRangeControlEnabled;
    }

    public void RefreshChart()
    {
        RefreshChartSeries();
    }
}