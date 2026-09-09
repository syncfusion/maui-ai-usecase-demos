using System.Collections.ObjectModel;
using MediFlowSample.Models;
using MediFlowSample.Services;
using MediFlowSample.ViewModels;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.Scheduler;

namespace MediFlowSample.Views;

public partial class AppointmentsPage : ContentPage
{
    private readonly AppointmentsViewModel viewModel;

    public AppointmentsPage()
    {
        InitializeComponent();
        var dataService = Application.Current?.Handler?.MauiContext?.Services.GetService<ScheduleDataService>()
            ?? new ScheduleDataService(new PatientDataService());
        viewModel = new AppointmentsViewModel(dataService);
        BindingContext = viewModel;
        var notificationService = Application.Current?.Handler?.MauiContext?.Services.GetService<NotificationService>()
            ?? new NotificationService();
        DesktopNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
        MobileNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();

        DesktopScheduler.DaysView.TimeRegions = CreateLunchBreakRegions();
        MobileScheduler.DaysView.TimeRegions = CreateLunchBreakRegions();
    }

    private static ObservableCollection<SchedulerTimeRegion> CreateLunchBreakRegions() =>
    [
        new SchedulerTimeRegion
        {
            StartTime = DateTime.Today.AddHours(12),
            EndTime = DateTime.Today.AddHours(13),
            Text = "LUNCH BREAK",
            EnablePointerInteraction = false,
            RecurrenceRule = "FREQ=DAILY;INTERVAL=1"
        }
    ];

    private void OnPreviousDateTapped(object? sender, TappedEventArgs e) => viewModel.PreviousDateCommand.Execute(null);

    private void OnNextDateTapped(object? sender, TappedEventArgs e) => viewModel.NextDateCommand.Execute(null);

    private void OnGoToTodayTapped(object? sender, TappedEventArgs e) => viewModel.GoToTodayCommand.Execute(null);

    private void OnViewModeSelectionChanged(object? sender, Syncfusion.Maui.Buttons.SelectionChangedEventArgs e)
    {
        var view = e.NewIndex == 1 || string.Equals(e.NewValue?.ToString(), "Week", StringComparison.OrdinalIgnoreCase)
            ? SchedulerView.Week
            : SchedulerView.Day;

        viewModel.SetSchedulerView(view);
        DesktopScheduler.View = view;
        MobileScheduler.View = view;
    }

    private void OnSchedulerViewChanged(object? sender, SchedulerViewChangedEventArgs e)
    {
        if (e.NewVisibleDates is null || e.NewVisibleDates.Count == 0)
        {
            return;
        }

        viewModel.SynchronizeSchedulerState(e.NewView, e.NewVisibleDates.First());
    }

    private void OnClinicianCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Clinician clinician)
        {
            viewModel.SelectClinicianCommand.Execute(clinician);
        }
    }

    private void OnClinicianSelectionChanged(object? sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        if (e.AddedItem is Clinician clinician)
        {
            viewModel.SelectClinicianCommand.Execute(clinician);
        }
    }

    private void OnSchedulerTapped(object? sender, SchedulerTappedEventArgs e)
    {
        var appointment = e.Appointments?
            .Select(item => item is ScheduleAppointment scheduleAppointment ? scheduleAppointment
                : item is SchedulerAppointment schedulerAppointment ? schedulerAppointment.DataItem as ScheduleAppointment
                : null)
            .FirstOrDefault(item => item is not null);

        if (appointment is not null)
        {
            viewModel.SelectedAppointment = appointment;
        }
    }

    private void OnCloseAppointmentDetailsTapped(object? sender, TappedEventArgs e)
    {
        viewModel.CloseAppointmentDetailsCommand.Execute(null);
    }

    private void OnCloseAppointmentDetailsClicked(object? sender, EventArgs e)
    {
        viewModel.CloseAppointmentDetailsCommand.Execute(null);
    }

    private async void OnNotificationsTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//dashboard?showNotifications=true");
    }

}
