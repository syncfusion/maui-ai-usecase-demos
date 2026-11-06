using SmartVehicleCare.ViewModels;

namespace SmartVehicleCare.Views;

public partial class SettingsPage : ContentView
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is SettingsViewModel vm)
        {
            vm.OnDeleteVehicleConfirmationRequested = async vehicle =>
            {
                var page = Application.Current?.MainPage;
                return page != null && await page.DisplayAlert(
                    "Delete vehicle",
                    $"Delete {vehicle.MakeModel} and all of its service, fuel, and schedule data?",
                    "Delete",
                    "Cancel");
            };

            // The view owns the platform actions — theme application and navigation.
            vm.OnThemeChangeRequested -= OnThemeChangeRequested;
            vm.OnThemeChangeRequested += OnThemeChangeRequested;
            vm.OnNavigateToWelcomeRequested -= OnNavigateToWelcome;
            vm.OnNavigateToWelcomeRequested += OnNavigateToWelcome;
        }
    }

    private void OnThemeChangeRequested()
    {
        if (Application.Current is not { } app) return;
        app.UserAppTheme = (BindingContext as SettingsViewModel)?.SelectedTheme switch
        {
            "Light" => AppTheme.Light,
            "Dark"  => AppTheme.Dark,
            _       => AppTheme.Unspecified
        };
    }

    private async void OnNavigateToWelcome()
    {
        try { await Shell.Current.GoToAsync("///welcome"); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation failed: {ex.Message}");
        }
    }
}
