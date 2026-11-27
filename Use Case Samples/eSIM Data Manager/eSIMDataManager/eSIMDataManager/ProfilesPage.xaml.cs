using eSIMDataManager.Models;
using eSIMDataManager.ViewModels;

namespace eSIMDataManager;

public partial class ProfilesPage : ContentPage
{
    private readonly DemoPage popupActions;
    private bool? isCompactWorkspace;

    public ProfilesPage(MainViewModel viewModel)
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

    private void DashboardClicked(object? sender, EventArgs e) => Shell.Current.GoToAsync("//dashboard");
    private void StoreClicked(object? sender, EventArgs e) => Shell.Current.GoToAsync("//store");

    private void WorkspaceSizeChanged(object? sender, EventArgs e)
    {
        if (sender is Grid grid && grid.Children.Count > 1)
        {
            var compact = grid.Width < 850;
            if (isCompactWorkspace == compact)
                return;

            isCompactWorkspace = compact;
            if (compact)
            {
                grid.ColumnDefinitions[0].Width = GridLength.Star;
                grid.ColumnDefinitions[1].Width = GridLength.Star;
                grid.RowDefinitions.Clear();
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetColumn((BindableObject)grid.Children[1], 0);
                Grid.SetRow((BindableObject)grid.Children[1], 1);
            }
            else
            {
                grid.ColumnDefinitions[0].Width = new GridLength(0.85, GridUnitType.Star);
                grid.ColumnDefinitions[1].Width = new GridLength(1.7, GridUnitType.Star);
                grid.RowDefinitions.Clear();
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                Grid.SetColumn((BindableObject)grid.Children[1], 1);
                Grid.SetRow((BindableObject)grid.Children[1], 0);
            }
        }
    }

    private async void CopyIccidClicked(object? sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
            await vm.ProfilesVM.CopyIccidCommand.ExecuteAsync(null);
    }

    private async void CopySmDpClicked(object? sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
            await vm.ProfilesVM.CopySmDpCommand.ExecuteAsync(null);
    }

    private async void CopyCodeClicked(object? sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
            await vm.ProfilesVM.CopyActivationCodeCommand.ExecuteAsync(null);
    }

    private void TopUpSelectedClicked(object? sender, EventArgs e)
    {
        popupActions.TopUpItemClicked(BindingContext is MainViewModel vm ? vm.ActiveProfile : null, e);
    }

    private void RenewSelectedClicked(object? sender, EventArgs e)
    {
        popupActions.RenewItemClicked(BindingContext is MainViewModel vm ? vm.ActiveProfile : null, e);
    }

    private void ActivateItemClicked(object? sender, EventArgs e) => popupActions.ActivateItemClicked(sender, e);
    private void QrItemClicked(object? sender, EventArgs e) => popupActions.QrItemClicked(sender, e);
    private void RenewItemClicked(object? sender, EventArgs e) => popupActions.RenewItemClicked(sender, e);
    private void TopUpItemClicked(object? sender, EventArgs e) => popupActions.TopUpItemClicked(sender, e);
}
