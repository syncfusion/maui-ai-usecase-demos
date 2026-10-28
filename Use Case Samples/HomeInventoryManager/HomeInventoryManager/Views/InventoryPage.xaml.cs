using System;
using HomeInventoryManager.Model;
using HomeInventoryManager.ViewModel;

namespace HomeInventoryManager.Views;

public partial class InventoryPage : ContentPage
{
	private readonly InventoryViewModel _vm;

	public InventoryPage()
		: this(new InventoryViewModel())
	{
	}

	public InventoryPage(InventoryViewModel vm)
	{
		InitializeComponent();
		_vm = vm;
		BindingContext = _vm;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
	}

	private void CategoryChipGroup_ChipClicked(object sender, EventArgs e)
	{
		try
		{
			var selected = categoryChipGroup.SelectedItem;

			if (selected == null)
				return;

			_vm.SelectedCategory = selected.ToString() ?? "All";
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[InventoryPage] CategoryChipGroup_ChipClicked failed: {ex.Message}");
		}
	}

	private async void RequestScan_Clicked(object sender, EventArgs e)
	{
		try
		{
			if (Shell.Current is not null)
				await Shell.Current.GoToAsync("//Scan");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[InventoryPage] RequestScan_Clicked failed: {ex.Message}");
		}
	}
}