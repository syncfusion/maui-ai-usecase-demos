using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class ProfilesPage : ContentPage
{
    private readonly DemoPage popupActions;

    public ProfilesPage(MainViewModel viewModel)
    {
        InitializeComponent();
        popupActions = new DemoPage(viewModel);
        BindingContext = viewModel;
        Content = new Grid { Children = { Content, popupActions.PopupView } };
    }

    private void DashboardClicked(object? sender, EventArgs e) => Shell.Current.GoToAsync("//dashboard");
    private void StoreClicked(object? sender, EventArgs e) => Shell.Current.GoToAsync("//store");
    private void WorkspaceSizeChanged(object? sender, EventArgs e)
    {
        if (sender is Grid grid)
        {
            var compact = grid.Width < 850;
            grid.ColumnDefinitions[0].Width = compact ? new GridLength(1, GridUnitType.Star) : new GridLength(0.85, GridUnitType.Star);
            grid.ColumnDefinitions[1].Width = compact ? new GridLength(1, GridUnitType.Star) : new GridLength(1.7, GridUnitType.Star);
            if (grid.RowDefinitions.Count < 2)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }
            grid.SetColumn(grid.Children[1], compact ? 0 : 1);
            grid.SetRow(grid.Children[1], compact ? 1 : 0);
        }
    }
    private async void CopyIccidClicked(object? sender, EventArgs e) { await Clipboard.Default.SetTextAsync("898102004819501234F"); await DisplayAlert("Copied", "Sample ICCID copied.", "OK"); }
    private async void CopySmDpClicked(object? sender, EventArgs e) { await Clipboard.Default.SetTextAsync("rsp.global-esim.net"); await DisplayAlert("Copied", "Sample SM-DP+ address copied.", "OK"); }
    private async void CopyCodeClicked(object? sender, EventArgs e) { await Clipboard.Default.SetTextAsync("LPA:1$rsp.global-esim.net$JAP-89182390"); await DisplayAlert("Copied", "Sample activation code copied.", "OK"); }
    private void TopUpSelectedClicked(object? sender, EventArgs e) => popupActions.TopUpItemClicked(BindingContext is ViewModels.MainViewModel vm ? vm.ActiveProfile : null, e);
    private void RenewSelectedClicked(object? sender, EventArgs e) => popupActions.RenewItemClicked(BindingContext is ViewModels.MainViewModel vm ? vm.ActiveProfile : null, e);
    private void ActivateItemClicked(object? sender, EventArgs e) => popupActions.ActivateItemClicked(sender, e);
    private void QrItemClicked(object? sender, EventArgs e) => popupActions.QrItemClicked(sender, e);
    private void RenewItemClicked(object? sender, EventArgs e) => popupActions.RenewItemClicked(sender, e);
    private void TopUpItemClicked(object? sender, EventArgs e) => popupActions.TopUpItemClicked(sender, e);
}
