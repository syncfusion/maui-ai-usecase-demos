using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private MainViewModel ViewModel => (MainViewModel)BindingContext;

    private async void DashboardClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//dashboard");
    private async void ProfilesClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//profiles");
    private async void StoreClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//store");
    private async void OrdersClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//orders");
    private async void QrActiveClicked(object? sender, EventArgs e) => await ViewModel.ShowQrCommand.ExecuteAsync(ViewModel.ActiveProfile);
    private async void TopUpActiveClicked(object? sender, EventArgs e) => await ViewModel.TopUpCommand.ExecuteAsync(ViewModel.ActiveProfile);
    private async void QrItemClicked(object? sender, EventArgs e) => await ViewModel.ShowQrCommand.ExecuteAsync((sender as Element)?.BindingContext as eSIMDataManager.Models.EsimProfile);
    private async void TopUpItemClicked(object? sender, EventArgs e) => await ViewModel.TopUpCommand.ExecuteAsync((sender as Element)?.BindingContext as eSIMDataManager.Models.EsimProfile);
    private async void ActivateItemClicked(object? sender, EventArgs e) => await ViewModel.InstallProfileCommand.ExecuteAsync((sender as Element)?.BindingContext as eSIMDataManager.Models.EsimProfile);
    private async void RenewItemClicked(object? sender, EventArgs e) => await ViewModel.RenewCommand.ExecuteAsync((sender as Element)?.BindingContext as eSIMDataManager.Models.EsimProfile);
    private async void QrSelectedClicked(object? sender, EventArgs e) => await ViewModel.ShowQrCommand.ExecuteAsync(ViewModel.SelectedProfile);

    private void DestinationClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.Text is not null) ViewModel.SelectCountry(button.Text);
    }

    private void FilterClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.Text is not null) ViewModel.ToggleFilter(button.Text);
    }
}
