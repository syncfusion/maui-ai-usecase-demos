using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion.Models;
using PatientCompanion.Services;
using PatientCompanion.Views;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class VisitsViewModel : BaseViewModel
{
    private readonly MockDataService mockDataService;

    private readonly AppointmentService appointmentService;

    [ObservableProperty]
    private bool isUpcomingSelected = true;

    public ObservableCollection<Appointment> UpcomingVisits { get; } = new();

    public ObservableCollection<Appointment> PastVisits { get; } = new();

    public ObservableCollection<Appointment> DisplayVisits { get; } = new();

    public VisitsViewModel(
        MockDataService mockDataService,
        AppointmentService appointmentService)
    {
        this.mockDataService = mockDataService;
        this.appointmentService = appointmentService;

        Title = "Visits";

        appointmentService.OnAppointmentsChanged += LoadData;

        LoadData();
    }

    private void LoadData()
    {
        var showUpcoming = IsUpcomingSelected;

        UpcomingVisits.Clear();
        PastVisits.Clear();

        foreach (var booking in appointmentService.GetAppointments())
        {
            if (booking.Status == "Cancelled")
                continue;

            var appointment = new Appointment
            {
                AppointmentId = booking.AppointmentId,
                DoctorName = booking.DoctorName,
                Specialization = booking.SelectedDoctor?.Specialization ?? "",
                Hospital = booking.HospitalName,
                Date = booking.SelectedDate,
                Time = booking.SelectedTimeSlot,
                VisitType = booking.VisitType,
                Status =
                    booking.Status,
            };

            if (appointment.Status is "Confirmed" or "Upcoming" && booking.ScheduledDateTime >= DateTime.Now)
            {
                UpcomingVisits.Add(appointment);
            }
            else
            {
                PastVisits.Add(appointment);
            }
        }

        DisplayVisits.Clear();
        var visits = showUpcoming ? UpcomingVisits : PastVisits;
        foreach (var visit in visits)
            DisplayVisits.Add(visit);
    }

    [RelayCommand]
    private void Cancel(Appointment? appointment)
    {
        if (appointment != null)
            appointmentService.CancelAppointment(appointment.AppointmentId);
    }

    [RelayCommand]
    private async Task Reschedule(Appointment? appointment)
    {
        if (appointment == null)
            return;

        appointmentService.RequestReschedule(appointment.AppointmentId);
        await Shell.Current.GoToAsync(nameof(AppointmentPage));
    }

    [RelayCommand]
    private void ShowUpcoming()
    {
        IsUpcomingSelected = true;

        DisplayVisits.Clear();

        foreach (var item in UpcomingVisits)
        {
            DisplayVisits.Add(item);
        }
    }

    [RelayCommand]
    private void ShowPast()
    {
        IsUpcomingSelected = false;

        DisplayVisits.Clear();

        foreach (var item in PastVisits)
        {
            DisplayVisits.Add(item);
        }
    }

    public Color UpcomingBackground =>
        IsUpcomingSelected
            ? Color.FromArgb("#008378")
            : Color.FromArgb("#E5E7EB");

    public Color UpcomingTextColor =>
        IsUpcomingSelected
            ? Colors.White
            : Color.FromArgb("#3D4947");

    public Color PastBackground =>
        !IsUpcomingSelected
            ? Color.FromArgb("#008378")
            : Color.FromArgb("#E5E7EB");

    public Color PastTextColor =>
        !IsUpcomingSelected
            ? Colors.White
            : Color.FromArgb("#3D4947");

    partial void OnIsUpcomingSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(UpcomingBackground));
        OnPropertyChanged(nameof(UpcomingTextColor));
        OnPropertyChanged(nameof(PastBackground));
        OnPropertyChanged(nameof(PastTextColor));
    }
}