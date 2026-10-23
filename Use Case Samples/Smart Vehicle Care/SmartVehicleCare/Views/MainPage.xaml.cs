using SmartVehicleCare.ViewModels;

namespace SmartVehicleCare.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        SizeChanged += OnSizeChanged;

        // Wire quick-action alert events (view owns the dialogs, MVVM-compliant)
        viewModel.OnShowOilChangeAlert += message => _ = DisplayAlert("Oil Change", message, "OK");
        viewModel.OnShowLastFuelAlert += message => _ = DisplayAlert("Last Fuel Log", message, "OK");
        viewModel.OnShowAddActionAlert += message => _ = DisplayAlert("Add Action", message, "OK");
    }

    private MainViewModel VM => (MainViewModel)BindingContext;

    // Re-check API key prompt every time the page appears (e.g., after Welcome redirect)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateCompactDesktopState();
        VM.ResetToDashboard();
        _ = VM.CheckApiKeyPromptAsync();
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        UpdateCompactDesktopState();
    }

    private void UpdateCompactDesktopState()
    {
        VM.IsCompactDesktop = DeviceInfo.Platform == DevicePlatform.WinUI && Width > 0 && Width < 700;
    }

}
