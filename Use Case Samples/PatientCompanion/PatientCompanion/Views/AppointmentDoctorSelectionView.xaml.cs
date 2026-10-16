using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentDoctorSelectionView : ContentView
{
    public AppointmentDoctorSelectionView(
        AppointmentViewModel appointmentViewModel)
    {
        InitializeComponent();

        BindingContext =
            new DoctorsViewModel(
                App.Resolve<MockDataService>(),
                appointmentViewModel);
    }
}
