using PatientCompanion.Models;
using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentDoctorSelectionView : ContentView
{
    private readonly AppointmentViewModel appointmentViewModel;

    public AppointmentDoctorSelectionView(
        AppointmentViewModel appointmentViewModel)
    {
        InitializeComponent();

        this.appointmentViewModel = appointmentViewModel;

        BindingContext =
            new DoctorsViewModel(
                new MockDataService(),
                appointmentViewModel);
    }

    public void SelectDoctor(Doctor doctor)
    {
        if (doctor == null)
            return;

        appointmentViewModel.SelectedDoctor = doctor;

        appointmentViewModel.CurrentStep = 2;

        appointmentViewModel.CurrentStepView =
            new AppointmentDateTimeView(appointmentViewModel);
    }
}