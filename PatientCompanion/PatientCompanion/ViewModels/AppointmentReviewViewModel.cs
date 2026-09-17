using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion.Models;
using PatientCompanion.Services;

namespace PatientCompanion.ViewModels;

public partial class AppointmentReviewViewModel : ObservableObject
{
    private readonly AppointmentViewModel appointmentViewModel;
    private readonly AppointmentService appointmentService;

    public AppointmentReviewViewModel(
        AppointmentViewModel appointmentViewModel,
        AppointmentService appointmentService)
    {
        this.appointmentViewModel = appointmentViewModel;
        this.appointmentService = appointmentService;

        Booking = appointmentViewModel.Booking;
    }

    public AppointmentBooking Booking { get; }

    public Doctor? SelectedDoctor => Booking.SelectedDoctor;

    public string DoctorName =>
        Booking.SelectedDoctor?.Name ?? string.Empty;

    public string DoctorImage =>
        Booking.DoctorImage;

    public string Specialization =>
        Booking.SelectedDoctor?.Specialization ?? string.Empty;

    public string HospitalName =>
        Booking.SelectedDoctor?.HospitalName ?? string.Empty;

    public string Address =>
        Booking.SelectedDoctor?.Address ?? string.Empty;

    public double MapLatitude =>
        Booking.SelectedDoctor is { Latitude: not 0 } doctor
            ? doctor.Latitude
            : 13.0827;

    public double MapLongitude =>
        Booking.SelectedDoctor is { Longitude: not 0 } doctor
            ? doctor.Longitude
            : 80.2707;

    public string VisitType =>
        Booking.VisitType;

    public string AppointmentDate =>
        Booking.SelectedDate.ToString("dddd, dd MMM yyyy");

    public string AppointmentTime =>
        Booking.SelectedTimeSlot;

    public string AppointmentSummary =>
        $"{Booking.SelectedDate:dd MMM yyyy} • {Booking.SelectedTimeSlot}";

    public string PatientName => "Maya Chen";

    public string PurposeOfVisit => "Follow-up on heart rate monitoring";

    [RelayCommand]
    private async Task ConfirmAppointment()
    {
        appointmentService.AddAppointment(Booking);

        await Shell.Current.GoToAsync("//MainTabs/VisitsPage");
    }

    [RelayCommand]
    private void EditAppointment()
    {
        appointmentViewModel.CurrentStep = 2;

        appointmentViewModel.CurrentStepView =
            new Views.AppointmentDateTimeView(
                appointmentViewModel);
    }
}