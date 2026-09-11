using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Maps;
using Syncfusion.Maui.Scheduler;
using Syncfusion.Maui.TreeView;
using SmartVehicleCare.Models;
using SmartVehicleCare.ViewModels;

namespace SmartVehicleCare.Views;

public partial class VehicleCenterPage : ContentView
{
    private VehicleCenterViewModel? _vm;

    public VehicleCenterPage()
    {
        InitializeComponent();

        // On mobile: override Fill with explicit widths so the grid scrolls horizontally
        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
        {
            ServiceHistoryGrid.ColumnWidthMode = ColumnWidthMode.None;
            ServiceHistoryGrid.Columns[0].Width = 120; // Date
            ServiceHistoryGrid.Columns[1].Width = 190; // ServiceType
            ServiceHistoryGrid.Columns[2].Width = 130; // Mileage
            ServiceHistoryGrid.Columns[3].Width = 90;  // Amount
            ServiceHistoryGrid.Columns[4].Width = 100; // Status
        }
    }

    // VM lives on RootGrid (declared in XAML), not on this ContentView
    private void AttachToViewModel()
    {
        var newVm = RootGrid?.BindingContext as VehicleCenterViewModel;
        if (newVm == _vm) return;

        if (_vm != null)
        {
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
            _vm.MapMarkers.CollectionChanged -= OnMapMarkersChanged;
        }

        _vm = newVm;

        if (_vm != null)
        {
            _vm.PropertyChanged += OnViewModelPropertyChanged;
            _vm.MapMarkers.CollectionChanged += OnMapMarkersChanged;
            // Sync any markers that were added before attachment
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (DesktopMapLayer != null) DesktopMapLayer.Markers = _vm.MapMarkers;
                if (MobileMapLayer  != null) MobileMapLayer.Markers  = _vm.MapMarkers;
            });
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        AttachToViewModel();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null)
            AttachToViewModel();
    }

    private void OnMapMarkersChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_vm is null) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (DesktopMapLayer != null) DesktopMapLayer.Markers = _vm.MapMarkers;
            if (MobileMapLayer  != null) MobileMapLayer.Markers  = _vm.MapMarkers;
        });
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(VehicleCenterViewModel.UserLatitude)
                                or nameof(VehicleCenterViewModel.UserLongitude)
                                or nameof(VehicleCenterViewModel.UserMapZoom)))
            return;

        if (_vm is null) return;
        var center = new MapLatLng(_vm.UserLatitude, _vm.UserLongitude);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (DesktopMapLayer != null)
            {
                DesktopMapLayer.Center = center;
                if (e.PropertyName == nameof(VehicleCenterViewModel.UserMapZoom)
                    && DesktopMapLayer.ZoomPanBehavior is { } dzpb)
                    dzpb.ZoomLevel = _vm.UserMapZoom;
            }
            if (MobileMapLayer != null)
            {
                MobileMapLayer.Center = center;
                if (e.PropertyName == nameof(VehicleCenterViewModel.UserMapZoom)
                    && MobileMapLayer.ZoomPanBehavior is { } mzpb)
                    mzpb.ZoomLevel = _vm.UserMapZoom;
            }
        });
    }

    private void OnExpenseFilterDropDownClosed(object sender, EventArgs e)
    {
        if (_vm?.SelectedExpenseFilter == "Custom")
            MainThread.BeginInvokeOnMainThread(() => ExpenseStartDatePicker.IsOpen = true);
    }

    private void OnExpenseStartDateTapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        => ExpenseStartDatePicker.IsOpen = true;

    private void OnExpenseEndDateTapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        => ExpenseEndDatePicker.IsOpen = true;

    private void OnExpenseStartDateOkClicked(object sender, EventArgs e)
    {
        if (_vm != null && ExpenseStartDatePicker.SelectedDate != default)
            _vm.CustomExpenseStartDate = ExpenseStartDatePicker.SelectedDate!.Value;

        ExpenseEndDatePicker.IsOpen = true;
    }

    private void OnExpenseEndDateOkClicked(object sender, EventArgs e)
    {
        if (_vm != null && ExpenseEndDatePicker.SelectedDate != default)
            _vm.CustomExpenseEndDate = ExpenseEndDatePicker.SelectedDate!.Value;
    }

    private void OnScheduleViewChanged(object sender, SchedulerViewChangedEventArgs e)
    {
        var visibleDates = e.NewVisibleDates?.ToList();
        if (visibleDates is not { Count: > 0 }) return;

        DateTime displayDate;
        if (e.NewView == SchedulerView.Month)
        {
            // Month view includes leading/trailing days from adjacent months.
            var month = visibleDates
                .GroupBy(date => new { date.Year, date.Month })
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key.Year)
                .ThenBy(group => group.Key.Month)
                .First()
                .Key;
            displayDate = new DateTime(month.Year, month.Month, 1);
        }
        else
        {
            displayDate = visibleDates[0];
        }
        if (_vm != null)
            _vm.CurrentMonthDisplayDate = displayDate;
    }

    private void OnServiceHistoryQueryRowHeight(object sender, DataGridQueryRowHeightEventArgs e)
    {
        if (e.RowIndex > 0) // skip header row (always index 0)
        {
            e.Height = e.GetIntrinsicRowHeight(e.RowIndex);
            if (e.Height < 52) e.Height = 52;
            e.Handled = true;
        }
    }

    private void OnEditServiceDateFieldTapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        => EditServiceDatePicker.IsOpen = true;

    private void OnEditServiceDateOkClicked(object sender, EventArgs e)
    {
        if (EditServiceDatePicker.SelectedDate.HasValue && _vm != null)
            _vm.EditServiceDate = EditServiceDatePicker.SelectedDate.Value;
    }

    private void OnEditFuelDateFieldTapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        => EditFuelDatePicker.IsOpen = true;

    private void OnEditFuelDateOkClicked(object sender, EventArgs e)
    {
        if (EditFuelDatePicker.SelectedDate.HasValue && _vm != null)
            _vm.EditFuelDate = EditFuelDatePicker.SelectedDate.Value;
    }

    private void OnPreviousSchedulePeriodTapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        => NavigateSchedulePeriod(-1);

    private void OnNextSchedulePeriodTapped(object sender, Microsoft.Maui.Controls.TappedEventArgs e)
        => NavigateSchedulePeriod(1);

    private void NavigateSchedulePeriod(int direction)
    {
        if (ScheduleCalendar == null || _vm == null)
            return;

        var current = ScheduleCalendar.DisplayDate;
        var newDate = _vm.CurrentSchedulerView switch
        {
            SchedulerView.Day => current.AddDays(direction),
            SchedulerView.Week => current.AddDays(7 * direction),
            _ => current.AddMonths(direction),
        };

        ScheduleCalendar.DisplayDate = newDate;
        _vm.CurrentMonthDisplayDate = newDate;
    }
}
