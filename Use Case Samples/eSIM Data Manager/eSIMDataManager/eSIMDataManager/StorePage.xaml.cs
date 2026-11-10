using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class StorePage : ContentPage
{
    private readonly DemoPage popupActions;

    public StorePage(MainViewModel viewModel)
    {
        InitializeComponent();
        popupActions = new DemoPage(viewModel);
        BindingContext = viewModel;
        Content = new Grid { Children = { Content, popupActions.PopupView } };
    }

    private async void DashboardClicked(object? sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void ProfilesClicked(object? sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
        await Navigation.PushAsync(new ProfilesPage((MainViewModel)BindingContext));
    }
    private void BuyPlanClicked(object? sender, EventArgs e) => popupActions.BuyPlanClicked(sender, e);
    private void DestinationClicked(object? sender, EventArgs e) => popupActions.DestinationClicked(sender, e);
    private void FilterClicked(object? sender, EventArgs e) => popupActions.FilterClicked(sender, e);
}
