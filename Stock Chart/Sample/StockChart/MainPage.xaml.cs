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
        private bool _mobileStockDrawerExpanded;
        private bool _mobileStockDrawerPanMoved;
        private double _mobileStockDrawerPanStartHeight;
        private bool _mobileStockDrawerPanUpdateScheduled;
        private double _mobileStockDrawerPendingHeight;
        private bool _chartRangeUpdateScheduled;
        private int _pendingChartRangeStart;
        private int _pendingChartRangeEnd;

        private const double MobileStockDrawerPreviewHeight = 52;
        private const double MobileStockDrawerGap = 16;

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
                UpdatePriceAxisRange(viewModel);
                ScheduleChartRangeApplication(viewModel);
            });
        }

        private void ScheduleChartRangeApplication(StockChartViewModel viewModel)
        {
            ApplyChartRangeWhenReady(viewModel, 0);
        }

        private void ApplyChartRangeWhenReady(StockChartViewModel viewModel, int attempt)
        {
            if (!_isPageActive || Handler is null || !ReferenceEquals(BindingContext, viewModel))
            {
                return;
            }

            if (StockChartView.Width > 0 &&
                StockChartView.Height > 0 &&
                StockChartView.Series.Count > 0 &&
                StockChartView.XAxes.Count > 0)
            {
                ApplyChartRange(viewModel);
            }

            if (attempt < 5)
            {
                var delay = TimeSpan.FromMilliseconds(50 * (attempt + 1));
                Dispatcher.DispatchDelayed(delay, () => ApplyChartRangeWhenReady(viewModel, attempt + 1));
            }
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

        // Native chart/axis mutations can be rejected mid-teardown (e.g. navigating away or
        // switching stocks while a drag/zoom is still in flight); swallow and log rather than crash.
        private static void SafeChartOperation(Action operation)
        {
            try
            {
                operation();
            }
            catch (InvalidOperationException exception)
            {
                System.Diagnostics.Trace.TraceWarning($"Chart operation rejected: {exception.Message}");
            }
            catch (COMException exception)
            {
                System.Diagnostics.Trace.TraceWarning($"Chart operation rejected: {exception.Message}");
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
                        _mobileStockDrawerExpanded = false;
                        StockPanel.IsVisible = true;
                        ChartPanel.IsVisible = true;
                        UpdateMobileStockDrawerLayout();
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

            ConfigureDesktopPopupStyle(popup);
            popup.PopupStyle.PopupBackground = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#1E293B")
                : Colors.White;
            popup.PopupStyle.Stroke = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#334155")
                : Color.FromArgb("#E2E8F0");
            popup.PopupStyle.StrokeThickness = 1;
            SafePopupOperation(() => popup.Show());
        }

        private static void ConfigureDesktopPopupStyle(SfPopup popup)
        {
            popup.PopupStyle.CornerRadius = 20;
            popup.PopupStyle.StrokeThickness = 1;
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
                // Width can still be 0/-1 the first time this runs (before the page's initial
                // layout pass); fall back to the device idiom so phones never get the compact
                // desktop-sized popup.
                var isMobile = Width > 0
                    ? Width < MobileBreakpoint
                    : DeviceInfo.Current.Idiom == DeviceIdiom.Phone;

                DisclaimerPopup.WidthRequest = isMobile && Width > 0 ? Width : isMobile ? 420 : 680;
                // Size to the message content rather than a fraction of the screen height,
                // otherwise the card is left with a large empty area below the text.
                DisclaimerPopup.HeightRequest = isMobile ? 300 : 200;
                ConfigureDesktopPopupStyle(DisclaimerPopup);
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
                if (Width >= MobileBreakpoint)
                {
                    InformationPopup.WidthRequest = 350;
                    InformationPopup.HeightRequest = 220;
                    ConfigureDesktopPopupStyle(InformationPopup);
                }
                else
                {
                    InformationPopup.WidthRequest = Math.Max(280, Width - 32);
                    InformationPopup.HeightRequest = 220;
                    InformationPopup.PopupStyle.CornerRadius = 12;
                }
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
                SettingsPopup.HeightRequest = Math.Min(520, screenHeight * 0.78);
                SettingsPopup.StartX = 0;
                SettingsPopup.StartY = (int)Math.Max(0, screenHeight - SettingsPopup.HeightRequest);
                SettingsPopup.PopupStyle.PopupBackground = Colors.Transparent;
                SettingsPopup.PopupStyle.Stroke = Colors.Transparent;
                SettingsPopup.PopupStyle.StrokeThickness = 0;
                SettingsPopup.PopupStyle.CornerRadius = new CornerRadius(20, 20, 0, 0);
                SettingsPopup.AnimationMode = PopupAnimationMode.SlideOnBottom;
                SafePopupOperation(() => SettingsPopup.Show(SettingsPopup.StartX, SettingsPopup.StartY));
            }
            else
            {
                SettingsPopup.WidthRequest = 625;
                SettingsPopup.HeightRequest = 540;
                ConfigureDesktopPopupStyle(SettingsPopup);
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
                ConfigureSheet(NewWatchlistPopup, 620, 360);
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
            if (Width >= MobileBreakpoint)
            {
                ConfigureDesktopPopupStyle(WatchlistMenuPopup);
            }
            else
            {
                WatchlistMenuPopup.PopupStyle.CornerRadius = 0;
            }
            WatchlistMenuPopup.PopupStyle.Stroke = Colors.Transparent;
            WatchlistMenuPopup.PopupStyle.StrokeThickness = 0;
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
                    ConfigureSheet(EditWatchlistPopup, 620, Width > 0 && Width < MobileBreakpoint ? 440 : 460);
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

                    ConfigureSheet(DeleteWatchlistPopup, 530, Width > 0 && Width < MobileBreakpoint ? 215 : 200);
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
                viewModel.PrepareRemoveStock(stock, viewModel.SelectedWatchlist);
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
            var isDarkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;
            var borderColor = isDarkTheme ? Color.FromArgb("#334155") : Color.FromArgb("#E2E8F0");

            RangeAxisBase axis = viewModel.IsLogarithmicAxis
                ? new LogarithmicAxis()
                : new NumericalAxis();
            axis.Name = "PriceAxis";
            axis.ShowMajorGridLines = true;
            axis.MajorGridLineStyle = new ChartLineStyle { Stroke = new SolidColorBrush(borderColor), StrokeWidth = 0.8 };
            axis.AxisLineStyle = new ChartLineStyle { Stroke = new SolidColorBrush(borderColor), StrokeWidth = 1 };
            axis.MajorTickStyle = new ChartAxisTickStyle { Stroke = new SolidColorBrush(borderColor), StrokeWidth = 1 };
            if (viewModel.IsAxisOpposed)
            {
                axis.CrossesAt = double.MaxValue;
            }
            axis.LabelStyle = new ChartAxisLabelStyle
            {
                FontSize = 11,
                TextColor = isDarkTheme
                    ? Color.FromArgb("#CBD5E1")
                    : Color.FromArgb("#64748B")
            };

            if (axis is NumericalAxis numericalAxis)
            {
                numericalAxis.RangePadding = NumericalPadding.Auto;
                numericalAxis.LabelCreated += OnPriceAxisLabelCreated;
            }

            SafeChartOperation(() =>
            {
                foreach (var series in StockChartView.Series.OfType<ChartSeries>())
                {
                    series.EnableTooltip = viewModel.IsTooltipEnabled;
                }

                StockChartView.YAxes.Clear();
                StockChartView.YAxes.Add(axis);
                StockChartView.IsTransposed = viewModel.IsAxisInverted;
            });

            ChartRangeSelector.IsVisible = _isChartFullscreen && viewModel.IsRangeControlEnabled;

            viewModel.RefreshChart();
            UpdatePriceAxisRange(viewModel);
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

        private void OnRangeSelectorValueChanged(object? sender, RangeSelectorValueChangedEventArgs e)
        {
            if (_isSynchronizingChartRange ||
                BindingContext is not StockChartViewModel viewModel ||
                viewModel.LastSelectedStock is null ||
                viewModel.LastSelectedStock.Data.Count == 0)
            {
                return;
            }

            var lastIndex = viewModel.LastSelectedStock.Data.Count - 1;
            var startIndex = Math.Clamp((int)Math.Floor(e.NewRangeStart), 0, lastIndex);
            var endIndex = Math.Clamp((int)Math.Ceiling(e.NewRangeEnd), startIndex, lastIndex);

            _isSynchronizingChartRange = true;
            try
            {
                viewModel.ApplyVisibleRangeSelection(startIndex, endIndex);
                ResetChartZoomControl.IsVisible = true;
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }

            // ZoomByRange and axis rescaling are the expensive part of this update; coalesce
            // rapid drag ticks into at most one native chart update per dispatcher cycle.
            ScheduleChartRangeUpdate(viewModel, startIndex, endIndex);
        }

        private void ScheduleChartRangeUpdate(StockChartViewModel viewModel, int startIndex, int endIndex)
        {
            _pendingChartRangeStart = startIndex;
            _pendingChartRangeEnd = endIndex;

            if (_chartRangeUpdateScheduled)
            {
                return;
            }

            _chartRangeUpdateScheduled = true;
            Dispatcher.Dispatch(() =>
            {
                _chartRangeUpdateScheduled = false;
                if (!_isPageActive || !ReferenceEquals(BindingContext, viewModel))
                {
                    return;
                }

                ApplyChartRange(viewModel, _pendingChartRangeStart, _pendingChartRangeEnd);
                UpdatePriceAxisRange(viewModel, _pendingChartRangeStart, _pendingChartRangeEnd);
            });
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
                viewModel.CommitRangeSelection();
                ApplyChartRange(viewModel);
                UpdatePriceAxisRange(viewModel);
                // Keep any still-queued drag update (from ScheduleChartRangeUpdate) in sync with the commit
                // so it can't overwrite this final range with a stale mid-drag value.
                _pendingChartRangeStart = viewModel.ChartRangeStartIndex;
                _pendingChartRangeEnd = viewModel.ChartRangeEndIndex;
                ResetChartZoomControl.IsVisible = true;
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
                var dateTimeAxis = (DateTimeAxis)e.Axis;
                var visibleRange = GetVisibleDataRange(viewModel, dateTimeAxis);
                viewModel.ApplyVisibleRangeSelection(visibleRange.Start, visibleRange.End);
                UpdatePriceAxisRange(viewModel, visibleRange.Start, visibleRange.End);
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
                var selectedRange = viewModel.RestoreCommittedRange();
                ChartRangeSelector.RangeStart = selectedRange.Start;
                ChartRangeSelector.RangeEnd = selectedRange.End;
                ApplyChartRange(viewModel, selectedRange.Start, selectedRange.End);
                UpdatePriceAxisRange(viewModel, selectedRange.Start, selectedRange.End);
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

            ApplyChartRange(viewModel, viewModel.ChartRangeStartIndex, viewModel.ChartRangeEndIndex);
        }

        private void ApplyChartRange(StockChartViewModel viewModel, int startIndex, int endIndex)
        {
            if (viewModel.LastSelectedStock is null ||
                StockChartView.XAxes.FirstOrDefault() is not DateTimeAxis dateTimeAxis ||
                viewModel.LastSelectedStock.Data.Count == 0)
            {
                return;
            }

            startIndex = Math.Clamp(startIndex, 0, viewModel.LastSelectedStock.Data.Count - 1);
            endIndex = Math.Clamp(endIndex, startIndex, viewModel.LastSelectedStock.Data.Count - 1);
            var rangeStartDate = viewModel.LastSelectedStock.Data[startIndex].Date;
            var rangeEndDate = viewModel.LastSelectedStock.Data[endIndex].Date;
            _isSynchronizingChartRange = true;
            try
            {
                SafeChartOperation(() => StockChartView.ZoomPanBehavior.ZoomByRange(dateTimeAxis, rangeStartDate, rangeEndDate));
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }
        }

        private void UpdatePriceAxisRange(StockChartViewModel viewModel)
        {
            UpdatePriceAxisRange(viewModel, viewModel.ChartRangeStartIndex, viewModel.ChartRangeEndIndex);
        }

        private void UpdatePriceAxisRange(StockChartViewModel viewModel, int startIndex, int endIndex)
        {
            if (StockChartView.YAxes.FirstOrDefault(axis => axis.Name == "PriceAxis") is not NumericalAxis numericalAxis ||
                viewModel.LastSelectedStock is null ||
                viewModel.LastSelectedStock.Data.Count == 0)
            {
                return;
            }

            var data = viewModel.LastSelectedStock.Data;
            startIndex = Math.Clamp(startIndex, 0, data.Count - 1);
            endIndex = Math.Clamp(endIndex, startIndex, data.Count - 1);

            var minimum = double.MaxValue;
            var maximum = double.MinValue;
            for (var i = startIndex; i <= endIndex; i++)
            {
                var candle = data[i];
                if (candle.Low < minimum) minimum = candle.Low;
                if (candle.High > maximum) maximum = candle.High;
            }

            var padding = Math.Max((maximum - minimum) * 0.05, 0.01);
            var axisMinimum = Math.Max(0.01, minimum - padding);
            var axisMaximum = maximum + padding;

            SafeChartOperation(() =>
            {
                numericalAxis.Minimum = axisMinimum;
                numericalAxis.Maximum = axisMaximum;
                numericalAxis.RangePadding = NumericalPadding.None;
            });
        }

        private void UpdatePriceAxisRangeFromVisibleAxis(StockChartViewModel viewModel, DateTimeAxis dateTimeAxis)
        {
            var visibleRange = GetVisibleDataRange(viewModel, dateTimeAxis);
            UpdatePriceAxisRange(viewModel, visibleRange.Start, visibleRange.End);
        }

        private static (int Start, int End) GetVisibleDataRange(StockChartViewModel viewModel, DateTimeAxis dateTimeAxis)
        {
            if (viewModel.LastSelectedStock is null || viewModel.LastSelectedStock.Data.Count == 0)
            {
                return (0, 0);
            }

            var data = viewModel.LastSelectedStock.Data;
            var visibleStart = DateTime.FromOADate(dateTimeAxis.VisibleMinimum);
            var visibleEnd = DateTime.FromOADate(dateTimeAxis.VisibleMaximum);
            var startIndex = data.FindIndex(candle => candle.Date >= visibleStart);
            var endIndex = data.FindLastIndex(candle => candle.Date <= visibleEnd);
            startIndex = Math.Clamp(startIndex < 0 ? 0 : startIndex, 0, data.Count - 1);
            endIndex = Math.Clamp(endIndex < 0 ? data.Count - 1 : endIndex, startIndex, data.Count - 1);
            return (startIndex, endIndex);
        }

        private void OnPriceAxisLabelCreated(object? sender, ChartAxisLabelEventArgs e)
        {
            if (double.TryParse(e.Label, out var value))
            {
                e.Label = $"${value:N0}";
            }
        }

        private void OnMobileStockDrawerTapped(object? sender, TappedEventArgs e)
        {
            if (_mobileStockDrawerPanMoved)
            {
                return;
            }

            ToggleMobileStockDrawer();
        }

        private void OnMobileStockDrawerPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (BindingContext is not StockChartViewModel { LastSelectedStock: not null } || _isChartFullscreen)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _mobileStockDrawerPanMoved = false;
                    StockPanel.IsVisible = true;
                    _mobileStockDrawerPanStartHeight = StockPanel.Height;
                    break;
                case GestureStatus.Running:
                    _mobileStockDrawerPanMoved = Math.Abs(e.TotalY) > 4;
                    var maxHeight = Math.Max(MobileStockDrawerPreviewHeight, WorkspaceGrid.Height - MobileStockDrawerGap);
                    _mobileStockDrawerPendingHeight = Math.Clamp(
                        _mobileStockDrawerPanStartHeight - e.TotalY,
                        MobileStockDrawerPreviewHeight,
                        maxHeight);
                    ScheduleMobileStockDrawerHeightUpdate();
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    SnapMobileStockDrawer();
                    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () => _mobileStockDrawerPanMoved = false);
                    break;
            }
        }

        // Coalesces rapid pan-gesture ticks into at most one layout pass per dispatcher cycle.
        private void ScheduleMobileStockDrawerHeightUpdate()
        {
            if (_mobileStockDrawerPanUpdateScheduled)
            {
                return;
            }

            _mobileStockDrawerPanUpdateScheduled = true;
            Dispatcher.Dispatch(() =>
            {
                _mobileStockDrawerPanUpdateScheduled = false;
                StockPanel.HeightRequest = _mobileStockDrawerPendingHeight;
            });
        }

        private void ToggleMobileStockDrawer()
        {
            if (BindingContext is not StockChartViewModel { LastSelectedStock: not null } || _isChartFullscreen)
            {
                return;
            }

            SetMobileStockDrawerExpanded(!_mobileStockDrawerExpanded, true);
        }

        private void SnapMobileStockDrawer()
        {
            var maxHeight = Math.Max(MobileStockDrawerPreviewHeight, WorkspaceGrid.Height - MobileStockDrawerGap);
            var expanded = StockPanel.Height > (MobileStockDrawerPreviewHeight + maxHeight) / 2;
            SetMobileStockDrawerExpanded(expanded, true);
        }

        private void SetMobileStockDrawerExpanded(bool expanded, bool animate)
        {
            _mobileStockDrawerExpanded = expanded;
            var target = expanded
                ? Math.Max(MobileStockDrawerPreviewHeight, WorkspaceGrid.Height - MobileStockDrawerGap)
                : MobileStockDrawerPreviewHeight;
            StockPanel.IsVisible = true;

            if (!animate)
            {
                StockPanel.HeightRequest = target;
                return;
            }

            this.AbortAnimation("MobileStockDrawer");
            var animation = new Animation(
                value => StockPanel.HeightRequest = value,
                StockPanel.Height,
                target);
            animation.Commit(this, "MobileStockDrawer", 16, 220, Easing.CubicOut);
        }

        private void UpdateMobileStockDrawerLayout()
        {
            var isMobile = Width > 0 && Width < MobileBreakpoint;
            var hasSelectedStock = BindingContext is StockChartViewModel { LastSelectedStock: not null };
            if (!isMobile || _isChartFullscreen)
            {
                this.AbortAnimation("MobileStockDrawer");
                StockPanel.ClearValue(VisualElement.HeightRequestProperty);
                MobileStockDrawerHandle.IsVisible = false;
                return;
            }

            StockPanel.IsVisible = true;
            MobileStockDrawerHandle.IsVisible = hasSelectedStock;
            StockPanel.HeightRequest = _mobileStockDrawerExpanded
                ? Math.Max(MobileStockDrawerPreviewHeight, WorkspaceGrid.Height - MobileStockDrawerGap)
                : MobileStockDrawerPreviewHeight;
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

        private void OnWorkspaceSizeChanged(object? sender, EventArgs e)
        {
            if (Width > 0 && Width < MobileBreakpoint && !_isChartFullscreen)
            {
                UpdateMobileStockDrawerLayout();
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), UpdateMobileStockDrawerLayout);
            }
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
                MobileStockDrawerHandle.IsVisible = false;
                MainContentGrid.Padding = new Thickness(12);
                MainContentGrid.RowSpacing = 0;
                WorkspaceGrid.ColumnDefinitions.Clear();
                WorkspaceGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                WorkspaceGrid.RowDefinitions.Clear();
                WorkspaceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                Grid.SetRow(StockPanel, 0);
                Grid.SetColumn(ChartPanel, 0);
                FullscreenButton.Text = "\uE5D1";
                SemanticProperties.SetDescription(FullscreenButton, "Exit chart fullscreen");
                MobileFullscreenButton.Text = "\uE5D1";
                SemanticProperties.SetDescription(MobileFullscreenButton, "Exit chart fullscreen");
                ChartRangeSelector.IsVisible = BindingContext is StockChartViewModel viewModel && viewModel.IsRangeControlEnabled;
                return;
            }

            HeaderGrid.IsVisible = true;
            MainContentGrid.Padding = isMobile
                ? new Thickness((double)Application.Current!.Resources["PagePadding"], 20, (double)Application.Current!.Resources["PagePadding"], 4)
                : new Thickness((double)Application.Current!.Resources["PagePadding"]);
            MainContentGrid.RowSpacing = 16;
            FullscreenButton.Text = "\uE5D0";
            SemanticProperties.SetDescription(FullscreenButton, "Enter chart fullscreen");
            MobileFullscreenButton.Text = "\uE5D0";
            SemanticProperties.SetDescription(MobileFullscreenButton, "Enter chart fullscreen");
            ChartRangeSelector.IsVisible = false;
            StockPanel.IsVisible = true;

            if (isMobile)
            {
                WorkspaceGrid.RowDefinitions.Clear();
                WorkspaceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                WorkspaceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                WorkspaceGrid.RowSpacing = MobileStockDrawerGap;
                Grid.SetRow(ChartPanel, 0);
                Grid.SetRow(StockPanel, 1);
                StockPanel.Margin = new Thickness(0);
                WatchlistSelectorBorder.WidthRequest = 160;
                WatchlistSelector.HorizontalOptions = LayoutOptions.Start;
                WatchlistSelector.DropdownWidth = Math.Max(200, Width - 100);
            }
            else
            {
                WorkspaceGrid.RowDefinitions.Clear();
                WorkspaceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                WorkspaceGrid.RowSpacing = 0;
                Grid.SetRow(ChartPanel, 0);
                Grid.SetRow(StockPanel, 0);
                StockPanel.Margin = new Thickness(0);
                WatchlistSelectorBorder.WidthRequest = 160;
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

            UpdateMobileStockDrawerLayout();
        }
    }
}
