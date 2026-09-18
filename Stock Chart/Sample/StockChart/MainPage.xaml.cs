using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using StockChart.Models;
using StockChart.Services;
using StockChart.ViewModels;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Charts;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Popup;
using Syncfusion.Maui.Sliders;

namespace StockChart
{
    public partial class MainPage : ContentPage
    {
        private const double MobileBreakpoint = 800;
        private bool _disclaimerShown;
        private bool _pageLoaded;
        private bool _isPageActive;
        private bool _isChartFullscreen;
        private bool _chartSeriesUpdateScheduled;
        private bool _isSynchronizingChartRange;
        private bool _customDateRangePopupOpen;
        private bool _windowActivated;

        private SfPopup SettingsPopup => (SfPopup)Resources["SettingsPopup"];
        private SfPopup DisclaimerPopup => (SfPopup)Resources["DisclaimerPopup"];
        private SfPopup InformationPopup => (SfPopup)Resources["InformationPopup"];
        private SfPopup AddStockPopup => (SfPopup)Resources["AddStockPopup"];
        private SfPopup NewWatchlistPopup => (SfPopup)Resources["NewWatchlistPopup"];
        private SfPopup EditWatchlistPopup => (SfPopup)Resources["EditWatchlistPopup"];
        private SfPopup WatchlistMenuPopup => (SfPopup)Resources["WatchlistMenuPopup"];
        private SfPopup DeleteWatchlistPopup => (SfPopup)Resources["DeleteWatchlistPopup"];
        private SfPopup FavoritePopup => (SfPopup)Resources["FavoritePopup"];
        private SfPopup RemoveStockPopup => (SfPopup)Resources["RemoveStockPopup"];
        private VisualElement ResetChartZoomControl => RootPage.FindByName<VisualElement>("ResetChartZoomButton");

        public MainPage()
        {
            InitializeComponent();
            var viewModel = new StockChartViewModel(new StockDataService());
            BindingContext = viewModel;
            SettingsPopup.BindingContext = viewModel;
            DisclaimerPopup.BindingContext = viewModel;
            InformationPopup.BindingContext = viewModel;
            AddStockPopup.BindingContext = viewModel;
            NewWatchlistPopup.BindingContext = viewModel;
            EditWatchlistPopup.BindingContext = viewModel;
            WatchlistMenuPopup.BindingContext = viewModel;
            DeleteWatchlistPopup.BindingContext = viewModel;
            FavoritePopup.BindingContext = viewModel;
            RemoveStockPopup.BindingContext = viewModel;
            viewModel.ChartSeries.CollectionChanged += OnChartSeriesChanged;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            SizeChanged += OnPageSizeChanged;
            Loaded += OnPageLoaded;
        }

        private void OnChartSeriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_chartSeriesUpdateScheduled)
            {
                return;
            }

            _chartSeriesUpdateScheduled = true;
            Dispatcher.Dispatch(() =>
            {
                _chartSeriesUpdateScheduled = false;
                if (!_isPageActive || BindingContext is not StockChartViewModel viewModel || Handler is null)
                {
                    return;
                }

                var series = viewModel.ChartSeries.ToArray();
                StockChartView.SuspendSeriesNotification();
                try
                {
                    StockChartView.Series.Clear();
                    foreach (var chartSeries in series)
                    {
                        StockChartView.Series.Add(chartSeries);
                    }
                }
                catch (COMException)
                {
                    // The native chart can reject a collection mutation while its handler is detaching.
                    return;
                }
                finally
                {
                    StockChartView.ResumeSeriesNotification();
                }

                ResetChartZoomControl.IsVisible = false;
                ApplyChartRange(viewModel);
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _isPageActive = true;
            if (Window is not null)
            {
                Window.Activated += OnWindowActivated;
            }

            if (BindingContext is StockChartViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }

            if (_windowActivated)
            {
                ShowDisclaimerIfReady();
            }
        }

        protected override void OnDisappearing()
        {
            _isPageActive = false;
            if (Window is not null)
            {
                Window.Activated -= OnWindowActivated;
            }

            base.OnDisappearing();
            _disclaimerShown = false;
            DismissPopups();
        }

        private void DismissPopups()
        {
            // OnDisappearing sets _isPageActive to false before this cleanup runs.
            // Do not use SafePopupOperation here because it intentionally rejects
            // operations on inactive pages and would leave native popups attached.
            DismissPopup(SettingsPopup);
            DismissPopup(DisclaimerPopup);
            DismissPopup(InformationPopup);
            DismissPopup(AddStockPopup);
            DismissPopup(NewWatchlistPopup);
            DismissPopup(EditWatchlistPopup);
            DismissPopup(WatchlistMenuPopup);
            DismissPopup(DeleteWatchlistPopup);
            DismissPopup(FavoritePopup);
            DismissPopup(RemoveStockPopup);
            _customDateRangePopupOpen = false;
        }

        private static void DismissPopup(SfPopup popup)
        {
            try
            {
                popup.Dismiss();
            }
            catch (InvalidOperationException)
            {
                // The native popup may already be detached during shutdown.
            }
            catch (COMException)
            {
                // WinUI can reject cleanup after the native popup has closed.
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StockChartViewModel.LastSelectedStock) &&
                Width > 0 && Width < MobileBreakpoint &&
                !_isChartFullscreen)
            {
                Dispatcher.Dispatch(() =>
                {
                    if (BindingContext is StockChartViewModel { LastSelectedStock: not null } && !_isChartFullscreen)
                    {
                        StockPanel.IsVisible = false;
                        ChartPanel.IsVisible = true;
                        OpenStocksButton.Text = "Stocks & Watchlists";
                    }
                });
            }

            if (sender is StockChartViewModel changedViewModel &&
                e.PropertyName == nameof(StockChartViewModel.IsCustomDateRangeVisible) &&
                !changedViewModel.IsCustomDateRangeVisible)
            {
                _customDateRangePopupOpen = false;
            }

            if (e.PropertyName == nameof(StockChartViewModel.SelectedTimeRange) &&
                sender is StockChartViewModel viewModel &&
                viewModel.SelectedTimeRange == "Custom" &&
                CanUsePopup &&
                !_customDateRangePopupOpen)
            {
                ShowCustomDateRangePopup(viewModel);
            }
        }

        private void OnTimeRangeDropDownClosed(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel &&
                viewModel.SelectedTimeRange == "Custom" &&
                !viewModel.IsCustomDateRangeVisible)
            {
                ShowCustomDateRangePopup(viewModel);
            }
        }

        private void ShowCustomDateRangePopup(StockChartViewModel viewModel)
        {
            if (!CanUsePopup)
            {
                return;
            }

            viewModel.PrepareCustomDateRange();
            viewModel.IsCustomDateRangeVisible = true;
            _customDateRangePopupOpen = true;
        }

        private bool CanUsePopup => _isPageActive && _pageLoaded && Window is not null && Handler is not null;

        private void SafePopupOperation(Action operation)
        {
            if (!CanUsePopup)
            {
                return;
            }

            try
            {
                operation();
            }
            catch (InvalidOperationException)
            {
                // WinUI can detach a Popup before MAUI raises Disappearing.
            }
            catch (COMException)
            {
                // A native popup can reject an operation during window activation or teardown.
            }
        }

        private void ShowWorkflowPopup(SfPopup popup)
        {
            if (!CanUsePopup)
            {
                return;
            }

            var isMobile = Width > 0 && Width < MobileBreakpoint;
            if (isMobile)
            {
                var display = DeviceDisplay.MainDisplayInfo;
                var screenWidth = display.Width / display.Density;
                var screenHeight = display.Height / display.Density;
                popup.WidthRequest = screenWidth;
                popup.StartX = 0;
                popup.StartY = (int)Math.Max(0, screenHeight - popup.HeightRequest);
                popup.PopupStyle.PopupBackground = Colors.Transparent;
                popup.PopupStyle.Stroke = Colors.Transparent;
                popup.PopupStyle.StrokeThickness = 0;
                popup.PopupStyle.CornerRadius = 0;
                popup.AnimationMode = PopupAnimationMode.SlideOnBottom;
                SafePopupOperation(() => popup.Show(popup.StartX, popup.StartY));
                return;
            }

            popup.PopupStyle.CornerRadius = 6;
            popup.PopupStyle.PopupBackground = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#1E293B")
                : Colors.White;
            popup.PopupStyle.Stroke = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#334155")
                : Color.FromArgb("#E2E8F0");
            popup.PopupStyle.StrokeThickness = 1;
            SafePopupOperation(() => popup.Show());
        }

        private void OnPageLoaded(object? sender, EventArgs e)
        {
            _pageLoaded = true;
            if (_windowActivated)
            {
                ShowDisclaimerIfReady();
            }
        }

        private void OnWindowActivated(object? sender, EventArgs e)
        {
            _windowActivated = true;
            ShowDisclaimerIfReady();
        }

        private void ShowDisclaimerIfReady()
        {
            if (CanUsePopup && !_disclaimerShown)
            {
                DisclaimerPopup.WidthRequest = Width > 0 && Width < MobileBreakpoint
                    ? Width
                    : 680;
                DisclaimerPopup.HeightRequest = Width > 0 && Width < MobileBreakpoint ? 315 : 250;
                Dispatcher.Dispatch(() =>
                {
                    if (CanUsePopup && !_disclaimerShown)
                    {
                        SafePopupOperation(() => DisclaimerPopup.Show());
                        _disclaimerShown = true;
                    }
                });
            }
        }

        private void OnInformationClicked(object? sender, EventArgs e)
        {
            if (CanUsePopup && sender is View informationButton && informationButton.Handler is not null)
            {
                SafePopupOperation(() => InformationPopup.ShowRelativeToView(informationButton, PopupRelativePosition.AlignBottomRight, 0, -20));
            }
        }

        private void OnMobileTimeRangeClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel) viewModel.OpenMobilePicker("TimeRange");
        }

        private void OnMobileChartTypeClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel) viewModel.OpenMobilePicker("ChartType");
        }

        private void OnMobileTrendlineClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel) viewModel.OpenMobilePicker("Trendline");
        }

        private void OnMobilePickerItemClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && sender is Element { BindingContext: MobilePickerItem item })
            {
                viewModel.SelectMobilePickerItem(item.Value);
            }
        }

        private void OnDisclaimerCloseClicked(object? sender, EventArgs e)
        {
            SafePopupOperation(DisclaimerPopup.Dismiss);
            SafePopupOperation(InformationPopup.Dismiss);
        }

        private void OnSettingsClicked(object? sender, EventArgs e)
        {
            if (!CanUsePopup || BindingContext is not StockChartViewModel)
            {
                return;
            }

            var isMobile = Width > 0 && Width < MobileBreakpoint;
            if (BindingContext is StockChartViewModel viewModel)
            {
                viewModel.LoadSettingsDraft();
            }

            if (isMobile)
            {
                var display = DeviceDisplay.MainDisplayInfo;
                var screenWidth = display.Width / display.Density;
                var screenHeight = display.Height / display.Density;
                SettingsPopup.WidthRequest = screenWidth;
                SettingsPopup.HeightRequest = Math.Min(560, screenHeight * 0.82);
                SettingsPopup.StartX = 0;
                SettingsPopup.StartY = (int)Math.Max(0, screenHeight - SettingsPopup.HeightRequest);
                SettingsPopup.PopupStyle.PopupBackground = Colors.Transparent;
                SettingsPopup.PopupStyle.Stroke = Colors.Transparent;
                SettingsPopup.PopupStyle.StrokeThickness = 0;
                SettingsPopup.PopupStyle.CornerRadius = 0;
                SettingsPopup.AnimationMode = PopupAnimationMode.SlideOnBottom;
                SafePopupOperation(() => SettingsPopup.Show(SettingsPopup.StartX, SettingsPopup.StartY));
            }
            else
            {
                SettingsPopup.WidthRequest = 625;
                SettingsPopup.HeightRequest = 570;
                SafePopupOperation(() => SettingsPopup.Show());
            }
        }

        private void OnAddStockClicked(object? sender, EventArgs e)
        {
            if (!CanUsePopup || BindingContext is not StockChartViewModel viewModel)
            {
                return;
            }

            viewModel.PrepareAddStockCommand.Execute(null);
            ConfigureSheet(AddStockPopup, 690, 450);
            ShowWorkflowPopup(AddStockPopup);
        }

        private void OnAddWatchlistFromSelectorClicked(object? sender, EventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel)
            {
                return;
            }

            WatchlistSelector.IsDropDownOpen = false;
            Dispatcher.Dispatch(() =>
            {
                if (!CanUsePopup)
                {
                    return;
                }

                viewModel.PrepareNewWatchlistCommand.Execute(null);
                ConfigureSheet(NewWatchlistPopup, 620, 350);
                ShowWorkflowPopup(NewWatchlistPopup);
            });
        }

        private void OnAddWatchlistFromSelectorTapped(object? sender, TappedEventArgs e)
        {
            OnAddWatchlistFromSelectorClicked(sender, e);
        }

        private void OnWatchlistOverflowClicked(object? sender, EventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel || viewModel.SelectedWatchlist is null)
            {
                return;
            }

            ConfigureSheet(WatchlistMenuPopup, 240, viewModel.CanDeleteSelectedWatchlist ? 105 : 52);
            Dispatcher.Dispatch(() =>
            {
                if (CanUsePopup && WatchlistOverflowButton.Handler is not null)
                {
                    SafePopupOperation(() => WatchlistMenuPopup.ShowRelativeToView(WatchlistOverflowButton, PopupRelativePosition.AlignBottomRight, 0, -20));
                }
            });
        }

        private void OnEditWatchlistFromMenuClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && viewModel.SelectedWatchlist is not null)
            {
                SafePopupOperation(WatchlistMenuPopup.Dismiss);
                Dispatcher.Dispatch(() =>
                {
                    if (!CanUsePopup)
                    {
                        return;
                    }

                    viewModel.PrepareEditWatchlistCommand.Execute(null);
                    ConfigureSheet(EditWatchlistPopup, 620, 460);
                    ShowWorkflowPopup(EditWatchlistPopup);
                });
            }
        }

        private void OnDeleteWatchlistFromMenuClicked(object? sender, EventArgs e)
        {
            SafePopupOperation(WatchlistMenuPopup.Dismiss);
            if (BindingContext is StockChartViewModel viewModel && viewModel.CanDeleteSelectedWatchlist)
            {
                Dispatcher.Dispatch(() =>
                {
                    if (!CanUsePopup)
                    {
                        return;
                    }

                    ConfigureSheet(DeleteWatchlistPopup, 530, 200);
                    ShowWorkflowPopup(DeleteWatchlistPopup);
                });
            }
        }

        private void OnDeleteWatchlistCloseClicked(object? sender, EventArgs e) => SafePopupOperation(DeleteWatchlistPopup.Dismiss);

        private void OnDeleteWatchlistConfirmClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel)
            {
                viewModel.DeleteSelectedWatchlist();
            }

            SafePopupOperation(DeleteWatchlistPopup.Dismiss);
        }

        private void OnAddStockCloseClicked(object? sender, EventArgs e) => SafePopupOperation(AddStockPopup.Dismiss);

        private void OnFavoriteClicked(object? sender, EventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel || !viewModel.CanOpenFavoritePopup)
            {
                return;
            }

            if (viewModel.IsLastSelectedStockInSelectedWatchlist)
            {
                viewModel.PrepareRemoveStock();
                ConfigureSheet(RemoveStockPopup, 530, 360);
                ShowWorkflowPopup(RemoveStockPopup);
            }
            else
            {
                viewModel.PrepareFavoriteStock();
                ConfigureSheet(FavoritePopup, 420, 360);
                ShowWorkflowPopup(FavoritePopup);
            }
        }

        private void OnStockMoreClicked(object? sender, EventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel ||
                sender is not Element { BindingContext: StockModel stock })
            {
                return;
            }

            if (viewModel.IsWatchlistSelected)
            {
                viewModel.PrepareRemoveStock(stock);
                ConfigureSheet(RemoveStockPopup, 530, 360);
                ShowWorkflowPopup(RemoveStockPopup);
            }
            else
            {
                viewModel.PrepareFavoriteStock(stock);
                ConfigureSheet(FavoritePopup, 420, 360);
                ShowWorkflowPopup(FavoritePopup);
            }
        }
        private void OnFavoriteCloseClicked(object? sender, EventArgs e) => SafePopupOperation(FavoritePopup.Dismiss);

        private void OnFavoriteApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel)
            {
                viewModel.ApplyFavoriteStock();
            }

            SafePopupOperation(FavoritePopup.Dismiss);
        }

        private void OnRemoveStockCloseClicked(object? sender, EventArgs e) => SafePopupOperation(RemoveStockPopup.Dismiss);

        private void OnRemoveStockApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel)
            {
                viewModel.ApplyRemoveStock();
            }

            SafePopupOperation(RemoveStockPopup.Dismiss);
        }

        private void OnAddStockApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && viewModel.PendingStock is not null)
            {
                viewModel.AddPendingStockToWatchlistsCommand.Execute(null);
                SafePopupOperation(AddStockPopup.Dismiss);
            }
        }
        private void OnNewWatchlistCloseClicked(object? sender, EventArgs e) => SafePopupOperation(NewWatchlistPopup.Dismiss);

        private void OnNewWatchlistCreateClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && !string.IsNullOrWhiteSpace(viewModel.NewWatchlistName))
            {
                viewModel.CreateWatchlistCommand.Execute(null);
                viewModel.PrepareAddStockCommand.Execute(null);
                SafePopupOperation(NewWatchlistPopup.Dismiss);
            }
        }
        private void OnEditWatchlistCloseClicked(object? sender, EventArgs e) => SafePopupOperation(EditWatchlistPopup.Dismiss);

        private void OnEditWatchlistApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && !string.IsNullOrWhiteSpace(viewModel.EditWatchlistName))
            {
                viewModel.SaveWatchlistEditCommand.Execute(null);
                SafePopupOperation(EditWatchlistPopup.Dismiss);
            }
        }

        private static void ConfigureSheet(SfPopup popup, double width, double height)
        {
            popup.WidthRequest = width;
            if (height > 0)
            {
                popup.HeightRequest = height;
            }
            else
            {
                popup.ClearValue(VisualElement.HeightRequestProperty);
            }
        }

        private void OnSettingsCloseClicked(object? sender, EventArgs e)
        {
            CloseSettingsPopup();
        }

        private void OnSettingsCancelClicked(object? sender, EventArgs e)
        {
            CloseSettingsPopup();
        }

        private void CloseSettingsPopup()
        {
            SafePopupOperation(SettingsPopup.Dismiss);
            SettingsPopup.ClearValue(VisualElement.WidthRequestProperty);
            SettingsPopup.ClearValue(VisualElement.HeightRequestProperty);
        }

        private void OnSettingsApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel)
            {
                viewModel.ApplySettingsDraft();
                ApplyChartSettings(viewModel);
            }

            CloseSettingsPopup();
        }

        private void ApplyChartSettings(StockChartViewModel viewModel)
        {
            foreach (var series in StockChartView.Series.OfType<ChartSeries>())
            {
                series.EnableTooltip = viewModel.IsTooltipEnabled;
            }

            RangeAxisBase axis = viewModel.IsLogarithmicAxis
                ? new LogarithmicAxis()
                : new NumericalAxis();
            axis.Name = "PriceAxis";
            axis.ShowMajorGridLines = true;
            if (viewModel.IsAxisOpposed)
            {
                axis.CrossesAt = double.MaxValue;
            }
            axis.LabelStyle = new ChartAxisLabelStyle
            {
                FontSize = 11,
                TextColor = Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#CBD5E1")
                    : Color.FromArgb("#64748B")
            };

            if (axis is NumericalAxis numericalAxis)
            {
                numericalAxis.LabelCreated += OnPriceAxisLabelCreated;
            }

            StockChartView.YAxes.Clear();
            StockChartView.YAxes.Add(axis);
            StockChartView.IsTransposed = viewModel.IsAxisInverted;

            ChartRangeSelector.IsVisible = _isChartFullscreen && viewModel.IsRangeControlEnabled;

            viewModel.RefreshChart();
        }

        private void OnRangeSelectorLabelCreated(object? sender, SliderLabelCreatedEventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel ||
                !double.TryParse(e.Text, out var index) ||
                viewModel.LastSelectedStock is null)
            {
                return;
            }

            var dataIndex = Math.Clamp((int)Math.Round(index), 0, viewModel.LastSelectedStock.Data.Count - 1);
            e.Text = viewModel.LastSelectedStock.Data[dataIndex].Date.ToString("MMM yyyy");
        }

        private void OnRangeSelectorValueChangeEnd(object? sender, EventArgs e)
        {
            if (_isSynchronizingChartRange || BindingContext is not StockChartViewModel viewModel)
            {
                return;
            }

            _isSynchronizingChartRange = true;
            try
            {
                StockChartView.ZoomPanBehavior.Reset();
                viewModel.CommitRangeSelection();
                ApplyChartRange(viewModel);
                ResetChartZoomControl.IsVisible = false;
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }
        }

        private void OnChartZoomEnd(object? sender, ChartZoomEventArgs e)
        {
            if (_isSynchronizingChartRange ||
                BindingContext is not StockChartViewModel viewModel ||
                StockChartView.XAxes.Count == 0 ||
                !ReferenceEquals(e.Axis, StockChartView.XAxes[0]))
            {
                return;
            }

            _isSynchronizingChartRange = true;
            try
            {
                viewModel.ApplyZoomSelection(e.CurrentZoomFactor, e.CurrentZoomPosition);
                ResetChartZoomControl.IsVisible = true;
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }
        }

        private void OnResetChartZoomClicked(object? sender, EventArgs e)
        {
            if (_isSynchronizingChartRange || BindingContext is not StockChartViewModel viewModel)
            {
                return;
            }

            _isSynchronizingChartRange = true;
            try
            {
                StockChartView.ZoomPanBehavior.Reset();
                viewModel.ResetZoomSelection();
                ApplyChartRange(viewModel);
                ResetChartZoomControl.IsVisible = false;
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }
        }

        private void OnTrendlineVisibilityClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && sender is Element { BindingContext: TrendlineViewModel overlay })
            {
                viewModel.ToggleTrendline(overlay);
            }
        }

        private void OnTrendlineRemoveClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && sender is Element { BindingContext: TrendlineViewModel overlay })
            {
                viewModel.RemoveTrendline(overlay);
            }
        }

        private void ApplyChartRange(StockChartViewModel viewModel)
        {
            if (viewModel.LastSelectedStock is null ||
                StockChartView.XAxes.FirstOrDefault() is not DateTimeAxis dateTimeAxis ||
                viewModel.LastSelectedStock.Data.Count == 0)
            {
                return;
            }

            var startIndex = Math.Clamp(viewModel.ChartRangeStartIndex, 0, viewModel.LastSelectedStock.Data.Count - 1);
            var endIndex = Math.Clamp(viewModel.ChartRangeEndIndex, startIndex, viewModel.LastSelectedStock.Data.Count - 1);
            _isSynchronizingChartRange = true;
            try
            {
                StockChartView.ZoomPanBehavior.ZoomByRange(
                    dateTimeAxis,
                    viewModel.LastSelectedStock.Data[startIndex].Date,
                    viewModel.LastSelectedStock.Data[endIndex].Date);
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }
        }

        private void OnPriceAxisLabelCreated(object? sender, ChartAxisLabelEventArgs e)
        {
            if (double.TryParse(e.Label, out var value))
            {
                e.Label = $"${value:N0}";
            }
        }

        private void OnOpenStocksClicked(object? sender, EventArgs e)
        {
            var showStocks = !StockPanel.IsVisible;
            StockPanel.IsVisible = showStocks;
            ChartPanel.IsVisible = !showStocks;
            OpenStocksButton.Text = showStocks ? "Back to chart" : "Stocks & Watchlists";
        }

        private void OnFullscreenClicked(object? sender, EventArgs e)
        {
            _isChartFullscreen = !_isChartFullscreen;
            ApplyResponsiveLayout();
        }

        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            ApplyResponsiveLayout();
        }

        private void ApplyResponsiveLayout()
        {
            var isMobile = Width > 0 && Width < MobileBreakpoint;
            if (BindingContext is StockChartViewModel toolbarViewModel)
            {
                toolbarViewModel.IsMobileToolbar = isMobile;
            }

            if (_isChartFullscreen)
            {
                HeaderGrid.IsVisible = false;
                StockPanel.IsVisible = false;
                OpenStocksButton.IsVisible = false;
                MainContentGrid.Padding = new Thickness(12);
                MainContentGrid.RowSpacing = 0;
                WorkspaceGrid.ColumnDefinitions.Clear();
                WorkspaceGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                Grid.SetColumn(ChartPanel, 0);
                FullscreenButton.Text = "\uE5D1";
                SemanticProperties.SetDescription(FullscreenButton, "Exit chart fullscreen");
                ChartRangeSelector.IsVisible = BindingContext is StockChartViewModel viewModel && viewModel.IsRangeControlEnabled;
                return;
            }

            HeaderGrid.IsVisible = true;
            MainContentGrid.Padding = new Thickness((double)Application.Current!.Resources["PagePadding"]);
            MainContentGrid.RowSpacing = 16;
            FullscreenButton.Text = "\uE5D0";
            SemanticProperties.SetDescription(FullscreenButton, "Enter chart fullscreen");
            ChartRangeSelector.IsVisible = false;
            StockPanel.IsVisible = !isMobile;
            OpenStocksButton.IsVisible = isMobile;

            if (isMobile && BindingContext is StockChartViewModel responsiveViewModel && responsiveViewModel.LastSelectedStock is not null && !_isChartFullscreen)
            {
                StockPanel.IsVisible = false;
                ChartPanel.IsVisible = true;
            }

            if (isMobile)
            {
                WatchlistSelector.ClearValue(VisualElement.WidthRequestProperty);
                WatchlistSelector.HorizontalOptions = LayoutOptions.Fill;
                WatchlistSelector.DropdownWidth = Math.Max(200, Width - 100);
            }
            else
            {
                WatchlistSelector.WidthRequest = 160;
                WatchlistSelector.HorizontalOptions = LayoutOptions.Start;
                WatchlistSelector.DropdownWidth = 160;
            }

            WorkspaceGrid.ColumnDefinitions.Clear();
            WorkspaceGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            if (!isMobile)
            {
                WorkspaceGrid.ColumnDefinitions.Insert(0, new ColumnDefinition(new GridLength(360)));
                Grid.SetColumn(StockPanel, 0);
                Grid.SetColumn(ChartPanel, 1);
            }
            else
            {
                Grid.SetColumn(ChartPanel, 0);
            }
        }
    }
}
