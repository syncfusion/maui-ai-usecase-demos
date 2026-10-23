using HomeInventoryManager.Model;

namespace HomeInventoryManager.Views;

public partial class AppSideBar : ContentView
{
	public AppSideBar()
	{
		InitializeComponent();
	}

	public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
		nameof(ActiveTab), typeof(NavigationTab), typeof(AppSideBar), NavigationTab.Dashboard);

	public NavigationTab ActiveTab
	{
		get => (NavigationTab)GetValue(ActiveTabProperty);
		set => SetValue(ActiveTabProperty, value);
	}

	private async void OnDashboardTapped(object sender, TappedEventArgs e)
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("//Dashboard");
	}

	private async void OnInventoryTapped(object sender, TappedEventArgs e)
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("//Inventory");
	}

	private async void OnScanTapped(object sender, TappedEventArgs e)
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("//Scan");
	}

	private async void OnWarrantyTapped(object sender, TappedEventArgs e)
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("//Warranty");
	}
}
