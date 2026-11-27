using eSIMDataManager.Models;
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
        Content = new Grid
        {
            Children =
            {
                Content,
                popupActions.PopupView,
                popupActions.QrPopupView,
                popupActions.TopUpPopupView,
                popupActions.PurchasePopupView
            }
        };
        SizeChanged += OnPageSizeChanged;
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        popupActions.UpdateTopUpPopupWidth(Width, Height);
    }

    private void StoreClicked(object? sender, EventArgs e) => Shell.Current.GoToAsync("//store");
    private void ProfilesClicked(object? sender, EventArgs e) => Shell.Current.GoToAsync("//profiles");
    private void QrActiveClicked(object? sender, EventArgs e) => popupActions.QrActiveClicked(sender, e);
    private void TopUpActiveClicked(object? sender, EventArgs e) => popupActions.TopUpActiveClicked(sender, e);
}
