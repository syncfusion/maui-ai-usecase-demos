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
    private void CoverageSelectionChanged(object? sender, Syncfusion.Maui.Buttons.SelectionChangedEventArgs e)
    {
        if (e.NewValue is Syncfusion.Maui.Buttons.SfSegmentItem segment && BindingContext is MainViewModel viewModel)
            viewModel.SelectCoverage(segment.Text);
    }
    private void NetworkFilterSelected(object? sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) => popupActions.NetworkFilterSelected(sender, e);
    private void QuotaFilterSelected(object? sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) => popupActions.QuotaFilterSelected(sender, e);
    private void DurationFilterSelected(object? sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) => popupActions.DurationFilterSelected(sender, e);
    private void SortFilterSelected(object? sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) => popupActions.SortFilterSelected(sender, e);
}
