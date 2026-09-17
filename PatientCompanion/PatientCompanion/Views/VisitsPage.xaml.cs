using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class VisitsPage : ContentPage
{
    public VisitsPage()
        : this(App.Resolve<VisitsViewModel>())
    {
    }

    public VisitsPage(VisitsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBookAppointmentTapped(object sender, TappedEventArgs e)
{
await Shell.Current.GoToAsync(nameof(AppointmentPage));
}
}