using System;
using System.Linq;
using System.Threading.Tasks;
using HomeInventoryManager.Model;
using HomeInventoryManager.ViewModel;

namespace HomeInventoryManager.Views;

public partial class DashboardPage : ContentPage
{
	private readonly DashboardViewModel _vm;

	public DashboardPage()
		: this(new DashboardViewModel())
	{
	}

	public DashboardPage(DashboardViewModel vm)
	{
		InitializeComponent();
		_vm = vm;
		BindingContext = _vm;

		_vm.NavigateRequested += OnNavigateRequested;
		_vm.RecommendationTapped += OnRecommendationTapped;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_vm.NavigateRequested -= OnNavigateRequested;
		_vm.RecommendationTapped -= OnRecommendationTapped;
	}

	private async void OnNavigateRequested(object? sender, string route)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(route) || Shell.Current is null)
				return;

			await Shell.Current.GoToAsync($"//{route}");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DashboardPage] OnNavigateRequested({route}) failed: {ex.Message}");
		}
	}

	private async void OnRecommendationTapped(object? sender, AIRecommendation recommendation)
	{
		try
		{
			if (Shell.Current is null)
				return;

			await Shell.Current.GoToAsync("//AIAdvisor");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DashboardPage] OnRecommendationTapped failed: {ex.Message}");
		}
	}

	private void Recommendations_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
	{
		try
		{
			if (e?.DataItem is AIRecommendation rec)
			{
				if (_vm.OpenRecommendationCommand.CanExecute(rec))
					_vm.OpenRecommendationCommand.Execute(rec);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DashboardPage] Recommendations_ItemTapped failed: {ex.Message}");
		}
	}

	private void UpcomingActions_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
	{
		try
		{
			if (e?.DataItem is UpcomingAction action)
			{
				if (_vm.OpenUpcomingActionCommand.CanExecute(action))
					_vm.OpenUpcomingActionCommand.Execute(action);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DashboardPage] UpcomingActions_ItemTapped failed: {ex.Message}");
		}
	}

	private async void OnScanTapped(object? sender, TappedEventArgs e)
	{
		try
		{
			if (Shell.Current is null)
				return;

			await Shell.Current.GoToAsync("//Scan");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DashboardPage] OnScanTapped failed: {ex.Message}");
		}
	}

	private async void ViewWarranty_Clicked(object sender, EventArgs e)
	{
		try
		{
			if (Shell.Current is null)
				return;

			await Shell.Current.GoToAsync("//Warranty");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DashboardPage] ViewWarranty_Clicked failed: {ex.Message}");
		}
	}

	private async void OpenAdvisor_Clicked(object sender, EventArgs e)
	{
		try
		{
			if (Shell.Current is null)
				return;

			await Shell.Current.GoToAsync("//AIAdvisor");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[DashboardPage] OpenAdvisor_Clicked failed: {ex.Message}");
		}
	}
}