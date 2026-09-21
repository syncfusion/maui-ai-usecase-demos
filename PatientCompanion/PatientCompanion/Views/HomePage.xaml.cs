using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
        : this(App.Resolve<HomeViewModel>())
    {
    }

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is HomeViewModel viewModel)
            viewModel.Refresh();
    }
}