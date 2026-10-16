using NutriLens.ViewModels;

namespace NutriLens.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
        : this(new ProfileViewModel())
    {
    }

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (BindingContext is ProfileViewModel vm)
                await vm.LoadScanInsightsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Profile] Scan insight load failed: {ex}");
            await DisplayAlertAsync("Profile", "Scan insights could not be loaded.", "OK");
        }
    }
}