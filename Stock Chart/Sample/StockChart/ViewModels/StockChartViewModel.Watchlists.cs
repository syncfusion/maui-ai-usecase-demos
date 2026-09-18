using StockChart.Models;

namespace StockChart.ViewModels;

public partial class StockChartViewModel
{
    partial void OnSelectedWatchlistChanged(WatchlistModel? value)
    {
        OnPropertyChanged(nameof(CanDeleteSelectedWatchlist));
        OnPropertyChanged(nameof(DeleteWatchlistConfirmationText));
        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(ChartPlaceholderText));
        if (IsWatchlistSelected) OnSearchTextChanged(SearchText);
    }

    partial void OnIsWatchlistSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(ViewModeText));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(ChartPlaceholderText));
        OnSearchTextChanged(SearchText);
        SelectedStock = null;
        if (LastSelectedStock is not null && FilteredStocks.Contains(LastSelectedStock))
        {
            SelectedStock = LastSelectedStock;
        }
    }
}