using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.Models;

public partial class WatchlistModel : ObservableObject
{
    public WatchlistModel(string name)
    {
        Name = name;
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    public ObservableCollection<string> Symbols { get; } = [];
}