using System;
using System.Linq;
using HomeInventoryManager.Model;
using HomeInventoryManager.ViewModel;

namespace HomeInventoryManager.Views;

public partial class WarrantyPage : ContentPage
{
	private readonly WarrantyViewModel _vm;

	public WarrantyPage()
		: this(new WarrantyViewModel())
	{
	}

	public WarrantyPage(WarrantyViewModel vm)
	{
		InitializeComponent();
		_vm = vm;
		BindingContext = _vm;

		_vm.WarrantyTapped += OnWarrantyTapped;
		_vm.ExpirationTapped += OnExpirationTapped;
		_vm.ViewAllRequested += OnViewAllRequested;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_vm.WarrantyTapped -= OnWarrantyTapped;
		_vm.ExpirationTapped -= OnExpirationTapped;
		_vm.ViewAllRequested -= OnViewAllRequested;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		try
		{
			await _vm.RefreshAsync();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[WarrantyPage] OnAppearing refresh failed: {ex.Message}");
		}
	}

	private void Warranties_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
	{
		try
		{
			if (e?.DataItem is WarrantyEntry entry)
			{
				if (_vm.OpenWarrantyCommand.CanExecute(entry))
					_vm.OpenWarrantyCommand.Execute(entry);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[WarrantyPage] Warranties_ItemTapped failed: {ex.Message}");
		}
	}

	private async void OnWarrantyTapped(object? sender, WarrantyEntry entry)
	{
		try
		{
			var msg =
				$"Brand: {entry.Brand}\n" +
				$"Coverage remaining: {entry.CoverageRemaining}/100\n" +
				$"AMC: {entry.AmcLabel}";

			await DisplayAlertAsync(entry.ProductName, msg, "Close");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[WarrantyPage] OnWarrantyTapped failed: {ex.Message}");
		}
	}

	private async void OnExpirationTapped(object? sender, UpcomingExpiration exp)
	{
		try
		{
			var answer = await DisplayAlertAsync(
				$"{exp.Bucket}",
				$"{exp.ProductName} – {exp.DaysRemaining} days left (ends {exp.EndDate:dd MMM yyyy}).\n\nRenew now?",
				"Renew",
				"Later");

			if (answer)
				await DisplayAlertAsync("Renewal Initiated",
					$"Renewal queued for {exp.ProductName}.", "OK");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[WarrantyPage] OnExpirationTapped failed: {ex.Message}");
		}
	}

	private async void OnViewAllRequested(object? sender, EventArgs e)
	{
		try
		{
			await DisplayAlertAsync("All Warranties",
				$"You have {_vm.Warranties.Count} warranties tracked.",
				"OK");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[WarrantyPage] OnViewAllRequested failed: {ex.Message}");
		}
	}
}
