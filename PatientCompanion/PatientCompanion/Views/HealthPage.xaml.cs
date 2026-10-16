using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class HealthPage : ContentPage
{
    public HealthPage()
        : this(App.Resolve<HealthViewModel>())
    {
    }

    public HealthPage(HealthViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}