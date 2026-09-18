using System.Collections.ObjectModel;
using StockChart.Models;

namespace StockChart.ViewModels;

public partial class StockChartViewModel
{
    partial void OnSearchTextChanged(string value)
    {
        var query = value.Trim();
        var source = IsWatchlistSelected
            ? Stocks.Where(stock => SelectedWatchlist?.Symbols.Contains(stock.Symbol) == true)
            : Stocks;
        var matches = string.IsNullOrEmpty(query) ? source : source.Where(MatchesStockQuery(query));
        ReplaceCollection(FilteredStocks, matches);
        OnPropertyChanged(nameof(StockCountText));
        OnPropertyChanged(nameof(ShowEmptyWatchlistMessage));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(ChartPlaceholderText));
    }

    partial void OnAddStockSearchTextChanged(string value)
    {
        var query = value.Trim();
        ReplaceCollection(AddStockCandidates, string.IsNullOrEmpty(query) ? Stocks : Stocks.Where(MatchesStockQuery(query)));
    }

    partial void OnSelectedStockChanged(StockModel? value)
    {
        if (value is null) return;

        foreach (var stock in Stocks)
        {
            stock.IsSelected = ReferenceEquals(stock, value);
        }

        LastSelectedStock = value;
        PrepareCustomDateRange();
        ApplyTimeRange(SelectedTimeRange);
        MenuStock = null;
        OnPropertyChanged(nameof(SelectedStockPrice));
        OnPropertyChanged(nameof(SelectedStockChange));
        OnPropertyChanged(nameof(SelectedStockChangeColor));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(ChartPlaceholderText));
    }

    partial void OnSelectedTimeRangeChanged(string value)
    {
        if (value != "Custom")
        {
            _lastStandardTimeRange = value;
            ApplyTimeRange(value);
        }
    }

    private static Func<StockModel, bool> MatchesStockQuery(string query)
        => stock => stock.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    stock.Company.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    stock.OfficialName.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source) target.Add(item);
    }
}