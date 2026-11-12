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

    private void DashboardClicked(object? sender, EventArgs e) => Shell.Current.GoToAsync("//dashboard");
    private void ProfilesClicked(object? sender, EventArgs e) => Shell.Current.GoToAsync("//profiles");
    private void BuyPlanClicked(object? sender, EventArgs e) => popupActions.BuyPlanClicked(sender, e);
    private void DestinationClicked(object? sender, EventArgs e) => popupActions.DestinationClicked(sender, e);
    private void FilterClicked(object? sender, EventArgs e) => popupActions.FilterClicked(sender, e);
    private void NetworkFilterSelected(object? sender, EventArgs e) => popupActions.NetworkFilterSelected(sender, e);
    private void QuotaFilterSelected(object? sender, EventArgs e) => popupActions.QuotaFilterSelected(sender, e);
    private void DurationFilterSelected(object? sender, EventArgs e) => popupActions.DurationFilterSelected(sender, e);
    private void SortFilterSelected(object? sender, EventArgs e) => popupActions.SortFilterSelected(sender, e);
}
