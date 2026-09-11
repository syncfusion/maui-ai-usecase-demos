using SmartVehicleCare.ViewModels;

namespace SmartVehicleCare.Views;

public partial class AddFuelPanel : ContentView
{
    public AddFuelPanel()
    {
        InitializeComponent();
    }

    private void OnFuelDateFieldTapped(object sender, TappedEventArgs e)
        => FuelDatePicker.IsOpen = true;

    private void OnFuelDateOkClicked(object sender, EventArgs e)
    {
        if (FuelDatePicker.SelectedDate.HasValue && BindingContext is AddFuelViewModel vm)
            vm.FuelDate = FuelDatePicker.SelectedDate.Value;
    }
}
