using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void ProfilesClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//profiles");
    private async void StoreClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//store");
}
