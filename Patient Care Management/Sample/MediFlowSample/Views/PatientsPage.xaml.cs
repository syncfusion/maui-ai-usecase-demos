using MediFlowSample.Models;
using MediFlowSample.Services;
using MediFlowSample.ViewModels;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Core;

namespace MediFlowSample.Views;

public partial class PatientsPage : ContentPage
{
    private readonly PatientsViewModel viewModel;

    public PatientsPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        var patientDataService = services?.GetService<PatientDataService>() ?? new PatientDataService();
        var scheduleDataService = services?.GetService<ScheduleDataService>() ?? new ScheduleDataService(patientDataService);
        viewModel = new PatientsViewModel(patientDataService, scheduleDataService);
        BindingContext = viewModel;
        var notificationService = services?.GetService<NotificationService>() ?? new NotificationService();
        DesktopNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
        MobileNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();

        foreach (var chip in MobileFilterChips.Items)
        {
            MobileFilterChips.SelectedItem = chip;
            break;
        }
        SetDesktopFilterTab("All Patients");
    }

    private void OnPatientCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Patient patient)
        {
            viewModel.OpenPatientCommand.Execute(patient);
        }
    }

    private async void OnPatientCarePlanTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Patient patient)
        {
            await Shell.Current.GoToAsync($"//careplans?patientId={patient.PatientId}");
        }
    }

    private async void OnCloseProfileTapped(object? sender, TappedEventArgs e)
    {
        var returnRoute = viewModel.ReturnRoute;
        viewModel.CloseProfileCommand.Execute(null);

        if (!string.IsNullOrEmpty(returnRoute))
        {
            viewModel.ReturnRoute = string.Empty;
            await Shell.Current.GoToAsync($"//{returnRoute}");
        }
    }

    private async void OnCloseProfileClicked(object? sender, EventArgs e)
    {
        await CloseProfileAsync();
    }

    private async Task CloseProfileAsync()
    {
        var returnRoute = viewModel.ReturnRoute;
        viewModel.CloseProfileCommand.Execute(null);

        if (!string.IsNullOrEmpty(returnRoute))
        {
            viewModel.ReturnRoute = string.Empty;
            await Shell.Current.GoToAsync($"//{returnRoute}");
        }
    }

    private void OnFilterSelectionChanged(object? sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        if (e.AddedItem is SfChip chip)
        {
            viewModel.SelectedFilter = chip.Text;
        }
    }

    private void OnDesktopFilterTabClicked(object? sender, EventArgs e)
    {
        if (sender is SfButton button && button.CommandParameter is string filter)
        {
            viewModel.SelectedFilter = filter;
            SetDesktopFilterTab(filter);
        }
    }

    private void SetDesktopFilterTab(string filter)
    {
        var activeBackground = Color.FromArgb("#087F73");
        var inactiveBackground = Colors.Transparent;
        var activeText = Colors.White;
        var inactiveText = Color.FromArgb("#14213D");

        foreach (var tab in new[] { DesktopAllPatientsTab, DesktopHighPriorityTab, DesktopFollowUpTab })
        {
            var isActive = (string)tab.CommandParameter == filter;
            tab.Background = isActive ? activeBackground : inactiveBackground;
            tab.Stroke = isActive ? activeBackground : Color.FromArgb("#DCE4F2");
            tab.TextColor = isActive ? activeText : inactiveText;
        }
    }

    private async void OnNotificationsTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//dashboard?showNotifications=true");
    }
}
