using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentPage : ContentPage
{
    public AppointmentPage()
    {
        InitializeComponent();

        var service = App.Resolve<AppointmentService>();
        BindingContext = new AppointmentViewModel(service.ConsumePendingReschedule());
    }
}
