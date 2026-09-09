using MediFlowSample.Models;
using MediFlowSample.Services;
using MediFlowSample.ViewModels;

namespace MediFlowSample.Views;

public partial class CarePlansPage : ContentPage
{
    private readonly CarePlansViewModel viewModel;

    public CarePlansPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        var carePlanDataService = services?.GetService<CarePlanDataService>() ?? new CarePlanDataService();
        var patientDataService = services?.GetService<PatientDataService>() ?? new PatientDataService();
        viewModel = new CarePlansViewModel(carePlanDataService, patientDataService);
        BindingContext = viewModel;
        var notificationService = services?.GetService<NotificationService>() ?? new NotificationService();
        DesktopNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
        MobileNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
    }

    private void OnPatientChipTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Patient patient)
        {
            viewModel.SelectPatientCommand.Execute(patient);
        }
    }

    private async void OnNotificationsTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//dashboard?showNotifications=true");
    }
}
