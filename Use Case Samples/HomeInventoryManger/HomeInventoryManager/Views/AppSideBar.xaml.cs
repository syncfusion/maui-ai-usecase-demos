using HomeInventoryManager.Model;

namespace HomeInventoryManager.Views;

public partial class AppSideBar : ContentView
{
	public AppSideBar()
	{
		InitializeComponent();
	}

	public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
   nameof(ActiveTab), typeof(NavigationTab), typeof(MainPage), NavigationTab.Dashboard);

	public NavigationTab ActiveTab
	{
		get => (NavigationTab)GetValue(ActiveTabProperty);
		set => SetValue(ActiveTabProperty, value);
	}

	private async void OnDashboardTapped(object sender, TappedEventArgs e)
	{
		Application.Current.Windows[0].Page = new DashboardPage();
		//await GoToAsyncSafe(NavigationTab.Dashboard);
	}

	private async void OnInventoryTapped(object sender, TappedEventArgs e)
	{
		Application.Current.Windows[0].Page = new InventoryPage();
		//await GoToAsyncSafe(NavigationTab.Inventory);
	}

	private async void OnScanTapped(object sender, TappedEventArgs e)
	{
		Application.Current.Windows[0].Page = new ScanPage();
		//await GoToAsyncSafe(NavigationTab.Scan);
	}

	private async void OnWarrantyTapped(object sender, TappedEventArgs e)
	{
		Application.Current.Windows[0].Page = new WarrantyPage();
		//await GoToAsyncSafe(NavigationTab.Warranty);
	}

	private async Task GoToAsyncSafe(NavigationTab tab)
	{
		Page page = tab switch
		{
			NavigationTab.Dashboard => new DashboardPage(),
			NavigationTab.Inventory => new InventoryPage(),
			NavigationTab.Scan => new ScanPage(),
			NavigationTab.Warranty => new WarrantyPage(),
			_ => new DashboardPage()
		};

		await Application.Current.Windows[0]
			.Page.Navigation.PushAsync(page);
	}
}