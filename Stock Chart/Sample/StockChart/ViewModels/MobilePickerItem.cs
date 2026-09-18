using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.ViewModels;

public partial class MobilePickerItem : ObservableObject
{
    public MobilePickerItem(string value)
    {
        Value = value;
    }

    public string Value { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
