using PatientCompanion.Models;
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

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is not AppointmentViewModel vm)
            return;

        switch (vm.CurrentStep)
        {
            // Step 1 - Select Specialty
            // Exit Appointment Flow and go back to Visits Page
            case 0:
                await Shell.Current.GoToAsync("..");
                break;

            // Step 2 - Select Doctor
            case 1:
                vm.CurrentStep = 0;
                vm.CurrentStepView = new AppointmentSpecialityView(vm);
                break;

            // Step 3 - Date & Time
            case 2:
                vm.CurrentStep = 1;
                vm.CurrentStepView = new AppointmentDoctorSelectionView(vm);
                break;

            // Step 4 - Review & Confirm
            case 3:
                vm.CurrentStep = 2;
                vm.CurrentStepView = new AppointmentDateTimeView(vm);
                break;

            default:
                await Shell.Current.GoToAsync("..");
                break;
        }
    }
}