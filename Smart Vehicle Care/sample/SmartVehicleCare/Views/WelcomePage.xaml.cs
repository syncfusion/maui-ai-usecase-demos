using SmartVehicleCare.ViewModels;

namespace SmartVehicleCare.Views;

public partial class WelcomePage : ContentPage
{
    private readonly WelcomeViewModel _vm;

    public WelcomePage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        _vm = viewModel;
        BindingContext = _vm;
        _vm.SetupCompleted += OnSetupCompleted;
        _vm.ExploreWithSampleDataRequested += OnExploreWithSampleDataRequested;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.ResetWelcomeFlow();
    }

    // Vehicle/demo-data creation lives in WelcomeViewModel; this page only navigates once setup finishes.
    private async void OnSetupCompleted()
    {
        await Task.Delay(1500);
        await Shell.Current.GoToAsync("///main");
    }

    private async void OnExploreWithSampleDataRequested()
    {
        await Shell.Current.GoToAsync("///main");
    }
}

