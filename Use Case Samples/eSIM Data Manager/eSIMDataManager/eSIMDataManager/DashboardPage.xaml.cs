using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class DashboardPage : ContentPage
{
    private readonly DemoPage popupActions;

    public DashboardPage(MainViewModel viewModel)
    {
        InitializeComponent();
        popupActions = new DemoPage(viewModel);
        BindingContext = viewModel;
        Content = new Grid { Children = { Content, popupActions.PopupView } };
    }

    private void StoreClicked(object? sender, EventArgs e) => popupActions.StoreClicked(sender, e);
    private void QrActiveClicked(object? sender, EventArgs e) => popupActions.QrActiveClicked(sender, e);
    private void TopUpActiveClicked(object? sender, EventArgs e) => popupActions.TopUpActiveClicked(sender, e);
}
