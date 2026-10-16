using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion.Models;
using PatientCompanion.Services;
using PatientCompanion.Views;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class DoctorsViewModel : BaseViewModel
{
    private readonly MockDataService mockDataService;
    private readonly AppointmentViewModel appointmentViewModel;

    public DoctorsViewModel(
        MockDataService mockDataService,
        AppointmentViewModel appointmentViewModel)
    {
        this.mockDataService = mockDataService;
        this.appointmentViewModel = appointmentViewModel;

        Title = "Doctors";

        LoadData();
    }

    public ObservableCollection<Doctor> Doctors { get; } = new();

    [ObservableProperty]
    private Doctor? selectedDoctor;

    private void LoadData()
    {
        Doctors.Clear();

        var selectedSpeciality = appointmentViewModel.SelectedSpeciality?.Name;

        foreach (var doctor in mockDataService.GetDoctors()
            .Where(item => string.Equals(
                item.Specialization,
                selectedSpeciality,
                StringComparison.OrdinalIgnoreCase)))
        {
            Doctors.Add(doctor);
        }
    }

    [RelayCommand]
    private Task SelectDoctor(Doctor doctor)
    {
        if (doctor == null)
            return Task.CompletedTask;

        SelectedDoctor = doctor;

        appointmentViewModel.Booking.SelectedDoctor = doctor;
        appointmentViewModel.CurrentStep = 2;
        // pass the appointment view model so the next view has access to shared Booking state
        appointmentViewModel.CurrentStepView = new AppointmentDateTimeView(appointmentViewModel);

        return Task.CompletedTask;
    }
}
