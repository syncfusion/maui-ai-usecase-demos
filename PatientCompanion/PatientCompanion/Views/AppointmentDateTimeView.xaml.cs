using PatientCompanion.Models;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentDateTimeView : ContentView
{
    public Picker MonthPicker => monthPicker;
    public AppointmentDateTimeView()
    {
        InitializeComponent();

        BindingContext = new AppointmentDateTimeViewModel();
    }

    public AppointmentDateTimeView(AppointmentViewModel appointmentViewModel)
    {
        InitializeComponent();

        BindingContext = new AppointmentDateTimeViewModel(appointmentViewModel);
    }

    private void VisitType_SelectionChanged(object sender, EventArgs e)
    {
        if (BindingContext is AppointmentDateTimeViewModel viewModel &&
            sender is Syncfusion.Maui.Core.SfChipGroup chipGroup)
        {
            viewModel.ApplyVisitType(chipGroup.SelectedItem as string);
        }
    }

    private void Time_SelectionChanged(object sender, EventArgs e)
    {
        if (BindingContext is AppointmentDateTimeViewModel viewModel &&
            sender is Syncfusion.Maui.Core.SfChipGroup chipGroup)
        {
            viewModel.ApplyTimeOption(chipGroup.SelectedItem as AppointmentTimeOption);
        }
    }

    private void CalendarTapped(object sender, TappedEventArgs e)
    {
        monthPicker.Focus();
    }

    private void MonthPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (BindingContext is not AppointmentDateTimeViewModel viewModel)
            return;

        if (monthPicker.SelectedItem is string monthText)
            viewModel.SetMonth(monthText);
    }
}
