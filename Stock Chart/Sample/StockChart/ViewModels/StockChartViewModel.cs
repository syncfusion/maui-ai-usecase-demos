using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using StockChart.Models;
using StockChart.Services;
using Syncfusion.Maui.Charts;

namespace StockChart.ViewModels;

public partial class StockChartViewModel : ObservableObject
{
    private readonly StockDataService _stockDataService;
    private bool _hasLoaded;

    public StockChartViewModel(StockDataService stockDataService)
    {
        _stockDataService = stockDataService;
        SelectedEditWatchlistStocks.CollectionChanged += OnSelectedEditWatchlistStocksChanged;
    }

    public ObservableCollection<StockModel> Stocks { get; } = [];
    public ObservableCollection<StockModel> FilteredStocks { get; } = [];
    public ObservableCollection<CandleDataModel> VisibleCandles { get; } = [];
    public ObservableCollection<WatchlistModel> Watchlists { get; } = [];
    public ObservableCollection<StockModel> AddStockCandidates { get; } = [];
    public ObservableCollection<WatchlistSelectionModel> AddStockWatchlists { get; } = [];
    public ObservableCollection<StockSelectionModel> EditWatchlistStocks { get; } = [];
    public ObservableCollection<StockSelectionModel> SelectedEditWatchlistStocks { get; } = [];
    public ObservableCollection<StockSelectionModel> EditWatchlistCandidates { get; } = [];
    public IReadOnlyList<string> AvailableTimeRanges { get; } = ["1M", "3M", "5M", "1Y", "All", "Custom"];
    public IReadOnlyList<string> AvailableChartTypes { get; } = ["Area", "Candle", "Hollow candle", "Column", "Line", "HiLo", "OHLC", "Range Column", "Step Area", "Step Line"];
    public IReadOnlyList<string> AvailableTrendlines { get; } = ["Trendline", "Linear", "Exponential", "Logarithmic", "Polynomial", "Power", "Moving Average"];
    public ObservableCollection<ChartSeries> ChartSeries { get; } = [];
    public ObservableCollection<TrendlineViewModel> ActiveTrendlines { get; } = [];
    public ObservableCollection<MobilePickerItem> MobilePickerItems { get; } = [];
    public ChartSettingsViewModel SettingsDraft { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StockModel? SelectedStock { get; set; }

    [ObservableProperty]
    public partial StockModel? LastSelectedStock { get; set; }

    [ObservableProperty]
    public partial bool IsWatchlistSelected { get; set; }

    [ObservableProperty]
    public partial int SelectedViewIndex { get; set; }

    [ObservableProperty]
    public partial string SelectedTimeRange { get; set; } = "1Y";

    [ObservableProperty]
    public partial DateTime CustomStartDate { get; set; }

    [ObservableProperty]
    public partial DateTime CustomEndDate { get; set; }

    [ObservableProperty]
    public partial bool IsCustomDateRangeVisible { get; set; }

    [ObservableProperty]
    public partial bool IsMobilePickerVisible { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomStartCalendarVisible))]
    [NotifyPropertyChangedFor(nameof(IsCustomEndCalendarVisible))]
    public partial bool IsSelectingCustomStartDate { get; set; } = true;

    [ObservableProperty]
    public partial string MobilePickerTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MobilePickerKind { get; set; } = string.Empty;

    private string _lastStandardTimeRange = "1Y";

    [ObservableProperty]
    public partial string SelectedChartType { get; set; } = "Candle";

    [ObservableProperty]
    public partial string SelectedTrendline { get; set; } = "Trendline";
    private bool _isResettingTrendlineSelection;

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDesktopToolbar))]
    public partial bool IsMobileToolbar { get; set; }

    [ObservableProperty]
    public partial double RangeMinimum { get; set; }

    [ObservableProperty]
    public partial double RangeMaximum { get; set; } = 1;

    [ObservableProperty]
    public partial double RangeStart { get; set; }

    [ObservableProperty]
    public partial double RangeEnd { get; set; } = 1;

    private bool _isUpdatingRange;
    private int _chartRangeStartIndex;
    private int _chartRangeEndIndex;
    private int _selectedRangeStartIndex;
    private int _selectedRangeEndIndex;

    [ObservableProperty]
    public partial StockModel? MenuStock { get; set; }

    [ObservableProperty]
    public partial WatchlistModel? SelectedWatchlist { get; set; }

    [ObservableProperty]
    public partial StockModel? PendingStock { get; set; }

    [ObservableProperty]
    public partial string AddStockSearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateWatchlist))]
    public partial string NewWatchlistName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewWatchlistSearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StockModel? NewWatchlistStock { get; set; }

    [ObservableProperty]
    public partial string EditWatchlistName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EditWatchlistSearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StockSelectionModel? EditWatchlistCandidate { get; set; }

    public bool IsNotBusy => !IsBusy;
    public bool IsDesktopToolbar => !IsMobileToolbar;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasLoaded => _hasLoaded;
    public string StockCountText => $"{FilteredStocks.Count} stocks";
    public bool IsChartVisible => LastSelectedStock is not null;
    public bool ShowChartPlaceholder => LastSelectedStock is null;
    public string ChartPlaceholderText => "Select a stock to view its chart.";
    public bool CanOpenFavoritePopup => LastSelectedStock is not null;
    public bool IsLastSelectedStockInSelectedWatchlist => LastSelectedStock is not null &&
        SelectedWatchlist?.Symbols.Contains(LastSelectedStock.Symbol) == true;
    public string SelectedStockPrice => LastSelectedStock?.PriceText ?? "--";
    public string SelectedStockChange => LastSelectedStock?.ChangeText ?? "--";
    public Color SelectedStockChangeColor
    {
        get
        {
            if (LastSelectedStock is null)
            {
                return Colors.Gray;
            }

            var latest = LastSelectedStock.LatestCandle;
            return latest.Close >= latest.Open ? Color.FromArgb("#16A34A") : Color.FromArgb("#DC2626");
        }
    }
    public string ViewModeText => IsWatchlistSelected ? "Watchlist" : "All Stocks";
    public bool ShowEmptyWatchlistMessage => IsWatchlistSelected && FilteredStocks.Count == 0;
    public bool CanDeleteSelectedWatchlist => Watchlists.Count > 1 && SelectedWatchlist is not null;
    public string DeleteWatchlistConfirmationText => SelectedWatchlist is null
        ? "Are you sure you want to delete this watchlist?"
        : $"Are you sure you want to delete \"{SelectedWatchlist.Name}\"?";
    public bool CanCreateWatchlist => !string.IsNullOrWhiteSpace(NewWatchlistName);
    public bool HasNewWatchlistNameError => !CanCreateWatchlist;
    public string NewWatchlistNameError => "Enter a watchlist name.";
    public int ChartRangeStartIndex => _chartRangeStartIndex;
    public int ChartRangeEndIndex => _chartRangeEndIndex;
    public bool IsCustomStartCalendarVisible => !IsMobileToolbar || IsSelectingCustomStartDate;
    public bool IsCustomEndCalendarVisible => !IsMobileToolbar || !IsSelectingCustomStartDate;

    partial void OnIsMobileToolbarChanged(bool value)
    {
        OnPropertyChanged(nameof(IsCustomStartCalendarVisible));
        OnPropertyChanged(nameof(IsCustomEndCalendarVisible));
    }

    [RelayCommand]
    private void SelectCustomStartDate() => IsSelectingCustomStartDate = true;

    [RelayCommand]
    private void SelectCustomEndDate() => IsSelectingCustomStartDate = false;

    public void OpenMobilePicker(string kind)
    {
        MobilePickerKind = kind;
        MobilePickerTitle = kind switch
        {
            "TimeRange" => "Select duration",
            "ChartType" => "Select chart type",
            "Trendline" => "Select trendline",
            _ => string.Empty
        };

        MobilePickerItems.Clear();
        foreach (var item in kind switch
        {
            "TimeRange" => AvailableTimeRanges,
            "ChartType" => AvailableChartTypes,
            "Trendline" => AvailableTrendlines,
            _ => []
        })
        {
            MobilePickerItems.Add(new MobilePickerItem(item)
            {
                IsSelected = string.Equals(item, kind switch
                {
                    "TimeRange" => SelectedTimeRange,
                    "ChartType" => SelectedChartType,
                    "Trendline" => SelectedTrendline,
                    _ => string.Empty
                }, StringComparison.Ordinal)
            });
        }

        IsMobilePickerVisible = MobilePickerItems.Count > 0;
    }

    public void SelectMobilePickerItem(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        switch (MobilePickerKind)
        {
            case "TimeRange": SelectedTimeRange = value; break;
            case "ChartType": SelectedChartType = value; break;
            case "Trendline": SelectedTrendline = value; break;
        }

        if (MobilePickerKind == "TimeRange" && value == "Custom")
        {
            PrepareCustomDateRange();
            IsCustomDateRangeVisible = true;
        }

        IsMobilePickerVisible = false;
    }

    [RelayCommand]
    private void CloseMobilePicker() => IsMobilePickerVisible = false;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy || _hasLoaded)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(HasError));

        try
        {
            var stocks = await _stockDataService.LoadAsync();
            Stocks.Clear();
            FilteredStocks.Clear();

            foreach (var stock in stocks)
            {
                Stocks.Add(stock);
                FilteredStocks.Add(stock);
            }

            var defaultWatchlist = new WatchlistModel("Watchlist 1");
            Watchlists.Clear();
            Watchlists.Add(defaultWatchlist);
            SelectedWatchlist = defaultWatchlist;

            SelectedStock = Stocks.FirstOrDefault(stock => stock.Stock.Equals("apple", StringComparison.OrdinalIgnoreCase))
                ?? Stocks.FirstOrDefault();
            _hasLoaded = true;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RetryAsync()
    {
        _hasLoaded = false;
        await LoadAsync();
    }

    partial void OnNewWatchlistNameChanged(string value)
    {
        OnPropertyChanged(nameof(HasNewWatchlistNameError));
    }

    partial void OnEditWatchlistSearchTextChanged(string value)
    {
        var query = value.Trim();
        var matches = string.IsNullOrEmpty(query)
            ? EditWatchlistStocks
            : EditWatchlistStocks.Where(stock => stock.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                                  stock.Company.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                                  stock.OfficialName.Contains(query, StringComparison.OrdinalIgnoreCase));

        EditWatchlistCandidates.Clear();
        foreach (var stock in matches)
        {
            EditWatchlistCandidates.Add(stock);
        }
    }

    partial void OnEditWatchlistCandidateChanged(StockSelectionModel? value)
    {
        if (value is null)
        {
            return;
        }

        value.IsSelected = true;
        if (!SelectedEditWatchlistStocks.Contains(value))
        {
            SelectedEditWatchlistStocks.Add(value);
        }

        EditWatchlistCandidate = null;
        EditWatchlistSearchText = string.Empty;
    }

    partial void OnLastSelectedStockChanged(StockModel? value)
    {
        OnPropertyChanged(nameof(SelectedStockPrice));
        OnPropertyChanged(nameof(SelectedStockChange));
        OnPropertyChanged(nameof(SelectedStockChangeColor));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));
        OnPropertyChanged(nameof(AvailableDataStartDate));
        OnPropertyChanged(nameof(AvailableDataEndDate));
    }

    partial void OnRangeStartChanged(double value)
    {
    }

    partial void OnRangeEndChanged(double value)
    {
    }

    partial void OnSelectedChartTypeChanged(string value)
    {
        RefreshChartSeries();
    }

    partial void OnSelectedTrendlineChanged(string value)
    {
        if (_isResettingTrendlineSelection || value == "Trendline")
        {
            return;
        }

        if (!ActiveTrendlines.Any(trendline => trendline.Name == value))
        {
            ActiveTrendlines.Add(new TrendlineViewModel(value));
        }

        _isResettingTrendlineSelection = true;
        SelectedTrendline = "Trendline";
        _isResettingTrendlineSelection = false;
        RefreshChartSeries();
    }

    partial void OnIsTooltipEnabledChanged(bool value)
    {
    }

    private void ApplyTimeRange(string range)
    {
        if (LastSelectedStock is null)
        {
            VisibleCandles.Clear();
            return;
        }

        var data = LastSelectedStock.Data;
        var lastDate = data[^1].Date;
        if (range == "Custom")
        {
            ApplyCustomDateRange();
            return;
        }

        var firstDate = range switch
        {
            "1M" => lastDate.AddMonths(-1),
            "3M" => lastDate.AddMonths(-3),
            "5M" => lastDate.AddMonths(-5),
            "1Y" => lastDate.AddYears(-1),
            _ => data[0].Date
        };

        var startIndex = data.Select((candle, index) => (candle, index))
            .FirstOrDefault(item => item.candle.Date >= firstDate).index;
        var endIndex = data.Count - 1;

        _isUpdatingRange = true;
        RangeMinimum = 0;
        RangeMaximum = Math.Max(1, endIndex);
        RangeStart = startIndex;
        RangeEnd = endIndex;
        _isUpdatingRange = false;

        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
        _selectedRangeStartIndex = startIndex;
        _selectedRangeEndIndex = endIndex;
        LoadAllCandles();
        RefreshChartSeries();
    }

    public void PrepareCustomDateRange()
    {
        if (LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        var availableStart = LastSelectedStock.Data[0].Date.Date;
        var availableEnd = LastSelectedStock.Data[^1].Date.Date;
        var selectedStart = CustomStartDate == default ? availableStart : CustomStartDate.Date;
        var selectedEnd = CustomEndDate == default ? availableEnd : CustomEndDate.Date;
        CustomStartDate = selectedStart < availableStart ? availableStart : selectedStart > availableEnd ? availableEnd : selectedStart;
        CustomEndDate = selectedEnd < CustomStartDate ? CustomStartDate : selectedEnd > availableEnd ? availableEnd : selectedEnd;
    }

    public DateTime AvailableDataStartDate => LastSelectedStock?.Data.FirstOrDefault()?.Date.Date ?? DateTime.Today;
    public DateTime AvailableDataEndDate => LastSelectedStock?.Data.LastOrDefault()?.Date.Date ?? DateTime.Today;

    [RelayCommand]
    public void ApplyCustomDateRange()
    {
        if (LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        var startDate = CustomStartDate.Date < AvailableDataStartDate ? AvailableDataStartDate : CustomStartDate.Date;
        var endDate = CustomEndDate.Date > AvailableDataEndDate ? AvailableDataEndDate : CustomEndDate.Date;
        if (endDate < startDate)
        {
            endDate = startDate;
        }

        CustomStartDate = startDate;
        CustomEndDate = endDate;
        var startIndex = LastSelectedStock.Data.FindIndex(candle => candle.Date.Date >= startDate);
        var endIndex = LastSelectedStock.Data.FindLastIndex(candle => candle.Date.Date <= endDate);
        startIndex = Math.Max(0, startIndex);
        endIndex = Math.Max(startIndex, endIndex);
        SetRangeSelection(startIndex, endIndex);
        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
        _selectedRangeStartIndex = startIndex;
        _selectedRangeEndIndex = endIndex;
        LoadAllCandles();
        RefreshChartSeries();
        IsCustomDateRangeVisible = false;
    }

    [RelayCommand]
    public void CancelCustomDateRange()
    {
        SelectedTimeRange = _lastStandardTimeRange;
        IsCustomDateRangeVisible = false;
    }

    public void CommitRangeSelection()
    {
        if (_isUpdatingRange || LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        if (LastSelectedStock.Data.Count == 1)
        {
            SetRangeSelection(0, 0);
            _chartRangeStartIndex = 0;
            _chartRangeEndIndex = 0;
            _selectedRangeStartIndex = 0;
            _selectedRangeEndIndex = 0;
            LoadAllCandles();
            return;
        }

        var lastIndex = LastSelectedStock.Data.Count - 1;
        var startIndex = Math.Clamp((int)Math.Floor(RangeStart), 0, Math.Max(0, lastIndex - 1));
        var endIndex = Math.Clamp((int)Math.Ceiling(RangeEnd), startIndex + 1, lastIndex);
        SetRangeSelection(startIndex, endIndex);
        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
        _selectedRangeStartIndex = startIndex;
        _selectedRangeEndIndex = endIndex;
        LoadAllCandles();
    }

    public void ResetZoomSelection()
    {
        if (LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        _chartRangeStartIndex = _selectedRangeStartIndex;
        _chartRangeEndIndex = _selectedRangeEndIndex;
        SetRangeSelection(_chartRangeStartIndex, _chartRangeEndIndex);
    }

    public void ApplyZoomSelection(double zoomFactor, double zoomPosition)
    {
        if (_isUpdatingRange || LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        if (LastSelectedStock.Data.Count == 1)
        {
            return;
        }

        var lastIndex = LastSelectedStock.Data.Count - 1;
        var currentStart = Math.Clamp(_chartRangeStartIndex, 0, Math.Max(0, lastIndex - 1));
        var currentEnd = Math.Clamp(_chartRangeEndIndex, currentStart + 1, lastIndex);
        var currentSpan = Math.Max(1, currentEnd - currentStart);

        var visibleSpan = Math.Clamp(zoomFactor, 0, 1) * currentSpan;
        var start = currentStart + (Math.Clamp(zoomPosition, 0, 1) * currentSpan);
        var end = start + visibleSpan;
        var startIndex = Math.Clamp((int)Math.Round(start), currentStart, Math.Max(0, currentEnd - 1));
        var endIndex = Math.Clamp((int)Math.Round(end), startIndex + 1, currentEnd);

        SetRangeSelection(startIndex, endIndex);
        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
    }

    private void SetRangeSelection(int startIndex, int endIndex)
    {
        _isUpdatingRange = true;
        RangeStart = startIndex;
        RangeEnd = endIndex;
        _isUpdatingRange = false;
    }

    private void LoadAllCandles()
    {
        VisibleCandles.Clear();
        foreach (var candle in LastSelectedStock!.Data)
        {
            VisibleCandles.Add(candle);
        }
    }

    private void RefreshChartSeries()
    {
        ChartSeries.Clear();

        if (LastSelectedStock is null)
        {
            return;
        }

        ChartSeries series = SelectedChartType switch
        {
            "Area" => CreateValueSeries(new AreaSeries()),
            "Column" => CreateValueSeries(new ColumnSeries()),
            "Line" => CreateValueSeries(new LineSeries()),
            "Step Area" => CreateValueSeries(new StepAreaSeries()),
            "Step Line" => CreateValueSeries(new StepLineSeries()),
            "Range Column" => new RangeColumnSeries
            {
                ItemsSource = VisibleCandles,
                XBindingPath = nameof(CandleDataModel.Date),
                High = nameof(CandleDataModel.High),
                Low = nameof(CandleDataModel.Low),
                EnableTooltip = IsTooltipEnabled
            },
            "HiLo" or "OHLC" => new HiLoOpenCloseSeries
            {
                ItemsSource = VisibleCandles,
                XBindingPath = nameof(CandleDataModel.Date),
                Open = nameof(CandleDataModel.Open),
                High = nameof(CandleDataModel.High),
                Low = nameof(CandleDataModel.Low),
                Close = nameof(CandleDataModel.Close),
                EnableTooltip = IsTooltipEnabled
            },
            "Hollow candle" => new CandleSeries
            {
                ItemsSource = VisibleCandles,
                XBindingPath = nameof(CandleDataModel.Date),
                Open = nameof(CandleDataModel.Open),
                High = nameof(CandleDataModel.High),
                Low = nameof(CandleDataModel.Low),
                Close = nameof(CandleDataModel.Close),
                EnableSolidCandle = false,
                BullishFill = Color.FromArgb("#16A34A"),
                BearishFill = Color.FromArgb("#DC2626"),
                Width = 0.75,
                Spacing = 0.15,
                EnableTooltip = IsTooltipEnabled
            },
            _ => new CandleSeries
            {
                ItemsSource = VisibleCandles,
                XBindingPath = nameof(CandleDataModel.Date),
                Open = nameof(CandleDataModel.Open),
                High = nameof(CandleDataModel.High),
                Low = nameof(CandleDataModel.Low),
                Close = nameof(CandleDataModel.Close),
                EnableSolidCandle = true,
                BullishFill = Color.FromArgb("#16A34A"),
                BearishFill = Color.FromArgb("#DC2626"),
                Width = 0.75,
                Spacing = 0.15,
                EnableTooltip = IsTooltipEnabled
            }
        };

        if (series is CartesianSeries cartesianSeries)
        {
            cartesianSeries.YAxisName = "PriceAxis";
        }

        ChartSeries.Add(series);

        // Financial and range series do not expose a single YBindingPath. A transparent
        // close-price line is used as the trendline host so every chart option supports
        // the same overlays without changing the primary rendering.
        foreach (var overlay in ActiveTrendlines)
        {
            var trendlineHost = CreateValueSeries(new LineSeries
            {
                StrokeWidth = 0,
                IsVisible = overlay.IsVisible,
                EnableTooltip = false
            });
            trendlineHost.Trendlines.Add(CreateTrendline(overlay.Name));
            ChartSeries.Add(trendlineHost);
        }

    }

    private XYDataSeries CreateValueSeries(XYDataSeries series)
    {
        series.ItemsSource = VisibleCandles;
        series.XBindingPath = nameof(CandleDataModel.Date);
        series.YBindingPath = nameof(CandleDataModel.Close);
        series.EnableTooltip = IsTooltipEnabled;
        return series;
    }

    private ChartTrendline CreateTrendline(string name)
    {
        ChartTrendline trendline = name switch
        {
            "Linear" => new LinearTrendline(),
            "Exponential" => new ExponentialTrendline(),
            "Logarithmic" => new LogarithmicTrendline(),
            "Polynomial" => new PolynomialTrendline { Order = 3 },
            "Power" => new PowerTrendline(),
            "Moving Average" => new MovingAverageTrendline { Period = 5 },
            _ => new LinearTrendline()
        };

        trendline.Stroke = Color.FromArgb("#2196F3");
        trendline.StrokeWidth = 2;
        return trendline;
    }

    public void ToggleTrendline(TrendlineViewModel? overlay)
    {
        if (overlay is null)
        {
            return;
        }

        overlay.IsVisible = !overlay.IsVisible;
        RefreshChartSeries();
    }

    public void RemoveTrendline(TrendlineViewModel? overlay)
    {
        if (overlay is null)
        {
            return;
        }

        ActiveTrendlines.Remove(overlay);
        RefreshChartSeries();
    }

    partial void OnSelectedViewIndexChanged(int value)
    {
        IsWatchlistSelected = value == 1;
    }

    [RelayCommand]
    private void ShowAllStocks()
    {
        IsWatchlistSelected = false;
    }

    [RelayCommand]
    private void ShowWatchlist()
    {
        IsWatchlistSelected = true;
    }

    [RelayCommand]
    private void PrepareAddStock()
    {
        AddStockSearchText = string.Empty;
        PendingStock = null;
        AddStockCandidates.Clear();
        foreach (var stock in Stocks)
        {
            AddStockCandidates.Add(stock);
        }

        AddStockWatchlists.Clear();
        foreach (var watchlist in Watchlists)
        {
            AddStockWatchlists.Add(new WatchlistSelectionModel(watchlist, ReferenceEquals(watchlist, SelectedWatchlist)));
        }
    }

    [RelayCommand]
    private void AddPendingStockToWatchlists()
    {
        if (PendingStock is null)
        {
            return;
        }

        foreach (var selection in AddStockWatchlists.Where(item => item.IsSelected))
        {
            if (!selection.Watchlist.Symbols.Contains(PendingStock.Symbol))
            {
                selection.Watchlist.Symbols.Add(PendingStock.Symbol);
            }
        }

        OnSearchTextChanged(SearchText);
    }

    [RelayCommand]
    private void PrepareNewWatchlist()
    {
        NewWatchlistName = $"Watchlist {Watchlists.Count + 1}";
        NewWatchlistSearchText = string.Empty;
        NewWatchlistStock = null;
    }

    [RelayCommand]
    private void CreateWatchlist()
    {
        var name = NewWatchlistName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var watchlist = new WatchlistModel(name);
        if (NewWatchlistStock is not null)
        {
            watchlist.Symbols.Add(NewWatchlistStock.Symbol);
        }

        Watchlists.Add(watchlist);
        OnPropertyChanged(nameof(CanDeleteSelectedWatchlist));
        SelectedWatchlist = watchlist;
        OnSearchTextChanged(SearchText);
    }

    public void DeleteSelectedWatchlist()
    {
        if (!CanDeleteSelectedWatchlist || SelectedWatchlist is null)
        {
            return;
        }

        var selectedIndex = Watchlists.IndexOf(SelectedWatchlist);
        Watchlists.Remove(SelectedWatchlist);
        SelectedWatchlist = Watchlists[Math.Min(selectedIndex, Watchlists.Count - 1)];
        OnPropertyChanged(nameof(CanDeleteSelectedWatchlist));
        OnSearchTextChanged(SearchText);
    }

    [RelayCommand]
    private void PrepareEditWatchlist()
    {
        EditWatchlistName = SelectedWatchlist?.Name ?? string.Empty;
        EditWatchlistSearchText = string.Empty;
        EditWatchlistCandidate = null;
        EditWatchlistStocks.Clear();
        SelectedEditWatchlistStocks.Clear();

        foreach (var stock in Stocks)
        {
            var selection = new StockSelectionModel(
                stock,
                SelectedWatchlist?.Symbols.Contains(stock.Symbol) == true);
            EditWatchlistStocks.Add(selection);
            if (selection.IsSelected)
            {
                SelectedEditWatchlistStocks.Add(selection);
            }
        }

        OnEditWatchlistSearchTextChanged(EditWatchlistSearchText);
    }

    [RelayCommand]
    private void SaveWatchlistEdit()
    {
        if (SelectedWatchlist is null)
        {
            return;
        }

        var name = EditWatchlistName.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            SelectedWatchlist.Name = name;
        }

        SelectedWatchlist.Symbols.Clear();
        foreach (var stock in EditWatchlistStocks.Where(stock => stock.IsSelected))
        {
            SelectedWatchlist.Symbols.Add(stock.Symbol);
        }

        OnPropertyChanged(nameof(SelectedWatchlist));
        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));

        OnSearchTextChanged(SearchText);
    }

    [RelayCommand]
    private void RemoveEditWatchlistStock(StockSelectionModel? stock)
    {
        if (stock is not null)
        {
            stock.IsSelected = false;
        }
    }

    public void SyncEditWatchlistChipSelection()
    {
        foreach (var stock in EditWatchlistStocks)
        {
            stock.IsSelected = SelectedEditWatchlistStocks.Contains(stock);
        }
    }

    private void OnSelectedEditWatchlistStocksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            foreach (StockSelectionModel stock in e.OldItems)
            {
                stock.IsSelected = false;
            }
        }
    }

    [RelayCommand]
    private void AddSelectedToWatchlist()
    {
        var watchlist = SelectedWatchlist ?? Watchlists.FirstOrDefault();
        if (LastSelectedStock is null || watchlist is null)
        {
            return;
        }

        if (!watchlist.Symbols.Contains(LastSelectedStock.Symbol))
        {
            watchlist.Symbols.Add(LastSelectedStock.Symbol);
        }

        if (IsWatchlistSelected)
        {
            OnSearchTextChanged(SearchText);
        }
    }

    public void PrepareFavoriteStock()
    {
        PrepareFavoriteStock(LastSelectedStock);
    }

    public void PrepareFavoriteStock(StockModel? stock)
    {
        if (stock is null)
        {
            return;
        }

        PendingStock = stock;
        AddStockWatchlists.Clear();
        foreach (var watchlist in Watchlists)
        {
            AddStockWatchlists.Add(new WatchlistSelectionModel(
                watchlist,
                watchlist.Symbols.Contains(stock.Symbol)));
        }
    }

    public void PrepareRemoveStock()
    {
        PrepareRemoveStock(LastSelectedStock);
    }

    public void PrepareRemoveStock(StockModel? stock)
    {
        if (stock is null)
        {
            return;
        }

        PendingStock = stock;
        AddStockWatchlists.Clear();
        foreach (var watchlist in Watchlists)
        {
            AddStockWatchlists.Add(new WatchlistSelectionModel(
                watchlist,
                watchlist.Symbols.Contains(stock.Symbol)));
        }
    }

    public void ApplyRemoveStock()
    {
        if (PendingStock is null)
        {
            return;
        }

        foreach (var selection in AddStockWatchlists.Where(item => item.IsSelected))
        {
            selection.Watchlist.Symbols.Remove(PendingStock.Symbol);
        }

        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));
        OnSearchTextChanged(SearchText);
    }

    public void ApplyFavoriteStock()
    {
        if (PendingStock is null)
        {
            return;
        }

        foreach (var selection in AddStockWatchlists)
        {
            var hasStock = selection.Watchlist.Symbols.Contains(PendingStock.Symbol);
            if (selection.IsSelected && !hasStock)
            {
                selection.Watchlist.Symbols.Add(PendingStock.Symbol);
            }
            else if (!selection.IsSelected && hasStock)
            {
                selection.Watchlist.Symbols.Remove(PendingStock.Symbol);
            }
        }

        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));
        OnSearchTextChanged(SearchText);
    }

    [RelayCommand]
    private void ToggleAddToWatchlistMenu(StockModel? stock)
    {
        MenuStock = ReferenceEquals(MenuStock, stock) ? null : stock;
    }

    [RelayCommand]
    private void AddStockToWatchlist(StockModel? stock)
    {
        var watchlist = SelectedWatchlist ?? Watchlists.FirstOrDefault();
        if (stock is null || watchlist is null)
        {
            return;
        }

        if (!watchlist.Symbols.Contains(stock.Symbol))
        {
            watchlist.Symbols.Add(stock.Symbol);
        }

        MenuStock = null;

        if (IsWatchlistSelected)
        {
            OnSearchTextChanged(SearchText);
        }
    }
}
