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

        var appointmentService =
            Application.Current!
                .Handler!
                .MauiContext!
                .Services
                .GetService<AppointmentService>();

        BindingContext =
            new AppointmentReviewViewModel(
                appointmentViewModel,
                appointmentService!);
    }

}