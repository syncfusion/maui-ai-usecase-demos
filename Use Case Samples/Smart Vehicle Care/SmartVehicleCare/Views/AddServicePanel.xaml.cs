using SmartVehicleCare.ViewModels;

namespace SmartVehicleCare.Views;

public partial class AddServicePanel : ContentView
{
    public AddServicePanel()
    {
        InitializeComponent();
    }

    private void OnServiceDateFieldTapped(object sender, TappedEventArgs e)
        => ServiceDatePicker.IsOpen = true;

    private void OnServiceDateOkClicked(object sender, EventArgs e)
    {
        if (ServiceDatePicker.SelectedDate.HasValue && BindingContext is AddServiceViewModel vm)
            vm.ServiceDate = ServiceDatePicker.SelectedDate.Value;
    }
}
