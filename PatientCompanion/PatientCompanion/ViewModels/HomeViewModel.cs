using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion.Models;
using PatientCompanion.Services;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly MockDataService _mockDataService;
    private readonly AppointmentService _appointmentService;

    public HomeViewModel(MockDataService mockDataService, AppointmentService appointmentService)
    {
        _mockDataService = mockDataService;
        _appointmentService = appointmentService;

        Title = "Home";

        _appointmentService.OnAppointmentsChanged += LoadData;
        LoadData();
    }

    [ObservableProperty]
    private Patient? patient;

    [ObservableProperty]
    private VitalSummary? vitalSummary;

    [ObservableProperty]
    private Appointment? nextAppointment;

    public ObservableCollection<Appointment> NextAppointments { get; } = new();

    [ObservableProperty]
    private int medicationCount;

    [ObservableProperty]
    private string healthStatus = "Healthy & Stable";

    private void LoadData()
    {
        Patient = _mockDataService.GetPatient();

        VitalSummary = _mockDataService.GetVitalSummary();

        MedicationCount = _mockDataService
            .GetMedications()
            .Count;

        var upcoming = _appointmentService.GetAppointments()
            .Where(item => item.Status is "Upcoming" or "Confirmed" && item.ScheduledDateTime >= DateTime.Now)
            .OrderBy(item => item.SelectedDate)
            .ThenBy(item => item.SelectedTimeSlot)
            .Select(ToAppointment)
            .ToList();

        NextAppointments.Clear();
        foreach (var appointment in upcoming)
            NextAppointments.Add(appointment);

        NextAppointment = NextAppointments.FirstOrDefault();
    }

    public void Refresh()
    {
        LoadData();
    }

    private static Appointment ToAppointment(AppointmentBooking booking) => new()
    {
        AppointmentId = booking.AppointmentId,
        DoctorName = booking.DoctorName,
        Specialization = booking.SelectedDoctor?.Specialization ?? booking.SpecialityName,
        Hospital = booking.HospitalName,
        Date = booking.SelectedDate,
        Time = booking.SelectedTimeSlot,
        VisitType = booking.VisitType,
        Status = booking.Status
    };

    [RelayCommand]
    private async Task OpenDoctorsAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/VisitsPage");
    }

    [RelayCommand]
    private async Task OpenVisitsAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/RecordsPage");
    }

    [RelayCommand]
    private async Task OpenRecordsAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/MedicationsPage");
    }

    [RelayCommand]
    private async Task OpenAppointmentAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.AppointmentPage));
    }

    [RelayCommand]
    private async Task OpenMedicationsAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/MedicationsPage");
    }

    [RelayCommand]
    private async Task OpenVitalsAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/HealthPage");
    }

    [RelayCommand]
    private async Task OpenReportsAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/RecordsPage");
    }

    [RelayCommand]
    private async Task OpenAssistantAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/RecordsPage");
    }
}
