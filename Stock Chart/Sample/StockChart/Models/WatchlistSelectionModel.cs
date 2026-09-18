using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.Models;

public partial class WatchlistSelectionModel : ObservableObject
{
    public WatchlistSelectionModel(WatchlistModel watchlist, bool isSelected = false)
    {
        Watchlist = watchlist;
        IsSelected = isSelected;
    }

    public WatchlistModel Watchlist { get; }
    public string Name => Watchlist.Name;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
