using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentReviewView : ContentView
{
    public AppointmentReviewView()
    {
        InitializeComponent();
    }

    public AppointmentReviewView(
        AppointmentViewModel appointmentViewModel)
    {
        InitializeComponent();

        BindingContext =
            new AppointmentReviewViewModel(
                appointmentViewModel,
                App.Resolve<AppointmentService>());
    }
}
