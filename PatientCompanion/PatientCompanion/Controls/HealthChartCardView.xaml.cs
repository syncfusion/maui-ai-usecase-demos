using PatientCompanion.ViewModels;

namespace PatientCompanion.Controls;

public partial class HealthChartCardView : ContentView
{
    public HealthChartCardView()
    {
        InitializeComponent();

        BindingContextChanged += HealthChartCardView_BindingContextChanged;
    }

    private void HealthChartCardView_BindingContextChanged(
        object? sender,
        EventArgs e)
    {
        if (BindingContext is HealthViewModel vm)
        {
            vm.PropertyChanged -= ViewModel_PropertyChanged;

            vm.PropertyChanged += ViewModel_PropertyChanged;

            UpdateAxis(vm);
        }
    }

    private void ViewModel_PropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (BindingContext is not HealthViewModel vm)
            return;

        if (e.PropertyName == nameof(HealthViewModel.SelectedRangeIndex))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateAxis(vm);
            });
        }
    }

    private void UpdateAxis(HealthViewModel vm)
    {
        DateTime minimum;
        DateTime maximum;
        double interval;

        switch (vm.SelectedRangeIndex)
        {
            case 0:
                minimum = DateTime.Today.AddDays(-6);
                maximum = DateTime.Today;
                interval = 1;
                break;

            case 1:
                minimum = DateTime.Today.AddDays(-29);
                maximum = DateTime.Today;
                interval = 7;
                break;

            default:
                minimum = DateTime.Today.AddDays(-89);
                maximum = DateTime.Today;
                interval = 20;
                break;
        }

        DateAxis.Minimum = minimum;
        DateAxis.Maximum = maximum;
        DateAxis.Interval = interval;
    }
}