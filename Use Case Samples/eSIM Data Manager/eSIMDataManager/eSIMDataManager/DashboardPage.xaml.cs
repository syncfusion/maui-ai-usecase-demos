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
        SizeChanged += (_, _) => popupActions.PopupView.WidthRequest = Math.Min(580, Width - 32);
    }

    private async void StoreClicked(object? sender, EventArgs e) => await Navigation.PushAsync(new StorePage((MainViewModel)BindingContext));
    private async void ProfilesClicked(object? sender, EventArgs e) => await Navigation.PushAsync(new ProfilesPage((MainViewModel)BindingContext));
    private void QrActiveClicked(object? sender, EventArgs e) => popupActions.QrActiveClicked(sender, e);
    private void TopUpActiveClicked(object? sender, EventArgs e) => popupActions.TopUpActiveClicked(sender, e);
}
