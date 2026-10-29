using System.Collections.Specialized;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.Models;

public partial class WatchlistModel : ObservableObject
{
    public WatchlistModel(string name)
    {
        Name = name;
        Symbols.CollectionChanged += OnSymbolsCollectionChanged;
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    public ObservableCollection<string> Symbols { get; } = [];

    public event EventHandler? SymbolsChanged;

    private void OnSymbolsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => SymbolsChanged?.Invoke(this, EventArgs.Empty);
}