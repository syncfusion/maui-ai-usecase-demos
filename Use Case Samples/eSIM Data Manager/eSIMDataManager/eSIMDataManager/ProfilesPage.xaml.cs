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
    private void ActivateItemClicked(object? sender, EventArgs e) => popupActions.ActivateItemClicked(sender, e);
    private void QrItemClicked(object? sender, EventArgs e) => popupActions.QrItemClicked(sender, e);
    private void RenewItemClicked(object? sender, EventArgs e) => popupActions.RenewItemClicked(sender, e);
    private void TopUpItemClicked(object? sender, EventArgs e) => popupActions.TopUpItemClicked(sender, e);
}
