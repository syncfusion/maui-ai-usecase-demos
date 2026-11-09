using ESimDataManager.ViewModels;
using Syncfusion.Maui.ListView;

namespace ESimDataManager.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
        SizeChanged += (_, _) => UpdateLayout();
        Loaded += (_, _) =>
        {
            UpdateLayout();
            SelectShellRoute();
        };
    }

    private void UpdateLayout()
    {
        if (Width <= 0) return;
        var wide = Width >= 900;
        WideNavigation.IsVisible = wide;
        MobileNavigation.IsVisible = !wide;
        Shell.SetNavBarIsVisible(this, false);
        if (Width < 700)
        {
            ((Grid)DashboardView.Children[1]).ColumnDefinitions = new ColumnDefinitionCollection { new(Star) };
            ((Grid)DashboardView.Children[1]).Children[1].SetValue(Grid.ColumnProperty, 0);
            ((Grid)DashboardView.Children[1]).Children[1].SetValue(Grid.RowProperty, 1);
            ((Grid)DashboardView.Children[1]).RowDefinitions = new RowDefinitionCollection { new(Auto), new(Auto) };
            ((Grid)DashboardView.Children[0]).ColumnDefinitions = new ColumnDefinitionCollection { new(Star) };
            ((Grid)DashboardView.Children[0]).Children[1].SetValue(Grid.ColumnProperty, 0);
            ((Grid)DashboardView.Children[0]).Children[1].SetValue(Grid.RowProperty, 1);
            ((Grid)DashboardView.Children[0]).RowDefinitions = new RowDefinitionCollection { new(Auto), new(Auto) };
        }
        else
        {
            ((Grid)DashboardView.Children[1]).ColumnDefinitions = new ColumnDefinitionCollection { new(1.5, Star), new(Star) };
            ((Grid)DashboardView.Children[1]).Children[1].SetValue(Grid.ColumnProperty, 1);
            ((Grid)DashboardView.Children[1]).Children[1].SetValue(Grid.RowProperty, 0);
            ((Grid)DashboardView.Children[1]).RowDefinitions = new RowDefinitionCollection { new(Auto) };
            ((Grid)DashboardView.Children[0]).ColumnDefinitions = new ColumnDefinitionCollection { new(Star), new(280) };
            ((Grid)DashboardView.Children[0]).Children[1].SetValue(Grid.ColumnProperty, 1);
            ((Grid)DashboardView.Children[0]).Children[1].SetValue(Grid.RowProperty, 0);
            ((Grid)DashboardView.Children[0]).RowDefinitions = new RowDefinitionCollection { new(Auto) };
        }
    }

    private void SelectShellRoute()
    {
        var route = Shell.Current?.CurrentState.Location.OriginalString ?? "";
        if (route.Contains("esims", StringComparison.OrdinalIgnoreCase)) ShowScreen("esims");
        else if (route.Contains("explore", StringComparison.OrdinalIgnoreCase)) ShowScreen("explore");
        else ShowScreen("home");
    }

    private void ShowScreen(string screen)
    {
        DashboardView.IsVisible = screen == "home";
        EsimsView.IsVisible = screen == "esims";
        ExploreView.IsVisible = screen == "explore";
        PageTitle.Text = screen switch { "esims" => "Your eSIM profiles", "explore" => "Explore plans", _ => "Your travel connectivity" };
        PageSubtitle.Text = screen switch { "esims" => "Manage your session-only sample profiles.", "explore" => "Choose a fictional sample plan for your next trip.", _ => "A simple overview of your demo eSIM plans." };
    }

    private async void OnDashboard(object sender, EventArgs e)
    {
        ShowScreen("home");
        await Shell.Current.GoToAsync("//main/home");
    }

    private async void OnEsims(object sender, EventArgs e)
    {
        ShowScreen("esims");
        await Shell.Current.GoToAsync("//main/esims");
    }

    private async void OnExplore(object sender, EventArgs e)
    {
        ShowScreen("explore");
        await Shell.Current.GoToAsync("//main/explore");
    }

    private async void OnTopUp(object sender, EventArgs e) => await viewModel.TopUpCommand.ExecuteAsync(viewModel.SelectedProfile ?? viewModel.ActiveProfile);
    private async void OnQr(object sender, EventArgs e) => await viewModel.ShowQrCommand.ExecuteAsync(viewModel.SelectedProfile ?? viewModel.ActiveProfile);
    private async void OnInstall(object sender, EventArgs e) => await viewModel.ShowQrCommand.ExecuteAsync(viewModel.SelectedProfile);
    private async void OnRenew(object sender, EventArgs e) => await viewModel.RenewCommand.ExecuteAsync(viewModel.SelectedProfile);

    private void OnProfileSelected(object? sender, ItemSelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is EsimProfile profile) viewModel.SelectedProfile = profile;
    }

    private async void OnBuyPlan(object sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: Plan plan }) await viewModel.BuyPlanCommand.ExecuteAsync(plan);
    }

    private async void OnPlanDetails(object sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: Plan plan })
            await DisplayAlert($"{plan.Country} · {plan.DataLabel}", $"{plan.Provider}\n{plan.ValidityDays} days · {plan.Network} · {plan.Coverage}\n${plan.Price:0.00} USD\n\nFictional sample plan. Coverage and performance are not real.", "Close");
    }

    private void OnDestinationSelected(object sender, EventArgs e)
    {
        if (sender is Button { Text: string country }) viewModel.SelectedCountry = viewModel.SelectedCountry == country ? null : country;
    }

    private void OnFilterSelected(object sender, EventArgs e)
    {
        if (sender is not Button { Text: string label }) return;
        switch (label)
        {
            case "Local": viewModel.SelectedCoverage = CoverageType.Local; break;
            case "Regional": viewModel.SelectedCoverage = CoverageType.Regional; break;
            case "Global": viewModel.SelectedCoverage = CoverageType.Global; break;
            case "4G": viewModel.SelectedNetwork = NetworkType.FourG; break;
            case "5G": viewModel.SelectedNetwork = NetworkType.FiveG; break;
            case "Limited": viewModel.SelectedType = PlanType.Limited; break;
            case "Unlimited": viewModel.SelectedType = PlanType.Unlimited; break;
            case "7 Days": viewModel.SelectedValidity = 7; break;
            case "15 Days": viewModel.SelectedValidity = 15; break;
            case "30 Days": viewModel.SelectedValidity = 30; break;
        }
    }
}
