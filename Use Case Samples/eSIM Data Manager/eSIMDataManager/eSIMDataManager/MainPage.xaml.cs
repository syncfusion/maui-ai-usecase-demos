using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void DashboardClicked(object? sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void ProfilesClicked(object? sender, EventArgs e) => await Navigation.PushAsync(new ProfilesPage((MainViewModel)BindingContext));
    private async void StoreClicked(object? sender, EventArgs e) => await Navigation.PushAsync(new StorePage((MainViewModel)BindingContext));
}
