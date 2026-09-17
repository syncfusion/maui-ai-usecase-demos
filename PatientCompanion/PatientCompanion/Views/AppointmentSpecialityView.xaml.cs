using PatientCompanion.Models;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentSpecialityView : ContentView
{
    public AppointmentSpecialityView(
        AppointmentViewModel appointmentViewModel)
    {
        InitializeComponent();
        BindingContext = new AppointmentSpecialityViewModel(appointmentViewModel);
    }

    private void SpecialityChipGroup_SelectionChanged(
        object sender,
        EventArgs e)
    {
        if (BindingContext is AppointmentSpecialityViewModel vm)
        {
            var chipGroup =
                sender as Syncfusion.Maui.Core.SfChipGroup;

            var selectedItem =
                chipGroup?.SelectedItem as SpecialtyItem;

            if (selectedItem != null)
            {
                vm.SelectedSpecialty = selectedItem;
            }
        }
    }
}