using Syncfusion.Maui.Picker;
using SmartVehicleCare.ViewModels;

namespace SmartVehicleCare.Views;

public partial class AddSchedulePanel : ContentView
{
    public AddSchedulePanel()
    {
        InitializeComponent();
    }

    private void OnDueDateFieldTapped(object sender, TappedEventArgs e)
        => DueDatePicker.IsOpen = true;

    private void OnDueDateOkClicked(object sender, EventArgs e)
    {
        if (DueDatePicker.SelectedDate.HasValue && BindingContext is AddScheduleViewModel vm)
            vm.DueDate = DueDatePicker.SelectedDate.Value;
    }

    private void OnTimeFieldTapped(object sender, TappedEventArgs e)
        => TimePicker.IsOpen = true;

    private void OnTimeOkClicked(object sender, EventArgs e)
    {
        if (TimePicker.SelectedTime.HasValue && BindingContext is AddScheduleViewModel vm)
            vm.DueTime = TimePicker.SelectedTime.Value;
    }
}
