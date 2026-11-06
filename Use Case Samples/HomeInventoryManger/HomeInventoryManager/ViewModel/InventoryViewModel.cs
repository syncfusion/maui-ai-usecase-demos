using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeInventoryManager.Model;
using HomeInventoryManager.Services;

namespace HomeInventoryManager.ViewModel
{
	/// <summary>
	/// Backs the Inventory page. Holds the full inventory, an active search term
	/// and a category filter, and exposes a tap command for the page to surface
	/// the selected item to its owner.
	/// </summary>
	public partial class InventoryViewModel : ObservableObject
	{
		private static readonly List<InventoryItem> SharedItems = new();
		private static bool _isSeeded;

		[ObservableProperty]
		private string searchText = string.Empty;

		[ObservableProperty]
		private string selectedCategory = "All";

		[ObservableProperty]
		private bool isBusy;

		[ObservableProperty]
		private bool isRefreshing;

		[ObservableProperty]
		private int totalCount;

		[ObservableProperty]
		private int filteredCount;

		public event EventHandler<InventoryItem>? ItemTapped;

		public event EventHandler? ScanRequested;

		[ObservableProperty]
		private string? selectedCategories;

		public ObservableCollection<string> Categories { get; } = new()
		{
			"All",
			"Electronics",
			"Furniture",
			"Kitchen",
			"Appliances",
			"Smart Devices",
			"Outdoor"
		};

		public ObservableCollection<InventoryItem> FilteredItems { get; } = new();

		public InventoryViewModel() : this(new MockAIService())
		{
		}

		public InventoryViewModel(IAIService aiService)
		{
			selectedCategories ??= Categories[0];
			SeedIfNeeded();
			UpdateFilter();
		}

		public IReadOnlyList<InventoryItem> AllItems => SharedItems;

		public bool HasItems => FilteredItems.Count > 0;

		public void AddItem(InventoryItem item)
		{
			if (item is null) return;
			SharedItems.Add(item);
			UpdateFilter();
		}

		public async Task RefreshAsync()
		{
			try
			{
				IsRefreshing = true;
				await Task.Delay(600);
				UpdateFilter();
			}
			finally
			{
				IsRefreshing = false;
			}
		}

		[RelayCommand]
		private void OpenItem(InventoryItem? item)
		{
			if (item is null) return;
			ItemTapped?.Invoke(this, item);
		}

		[RelayCommand]
		private void RequestScan() => ScanRequested?.Invoke(this, EventArgs.Empty);

		[RelayCommand]
		private void ClearSearch() => SearchText = string.Empty;

		[RelayCommand]
		public void SelectCategory(string? category)
		{
			if (string.IsNullOrWhiteSpace(category)) return;
			SelectedCategory = category;
		}

		partial void OnSearchTextChanged(string value) => UpdateFilter();
		partial void OnSelectedCategoryChanged(string value) => UpdateFilter();

		private void UpdateFilter()
		{
			FilteredItems.Clear();

			IEnumerable<InventoryItem> query = SharedItems;

			if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "All")
				query = query.Where(x => string.Equals(x.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase));

			if (!string.IsNullOrWhiteSpace(SearchText))
			{
				var term = SearchText.Trim();
				query = query.Where(x =>
					(x.Name ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase) ||
					(x.Brand ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase) ||
					(x.Category ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase));
			}

			foreach (var item in query)
				FilteredItems.Add(item);

			TotalCount = SharedItems.Count;
			FilteredCount = FilteredItems.Count;
			OnPropertyChanged(nameof(HasItems));
		}

		private static void SeedIfNeeded()
		{
			if (_isSeeded)
				return;

			_isSeeded = true;
			var _allItems = SharedItems;
			_allItems.Add(CreateSeedItem(
					inventoryId: 1,
					name: "Bespoke 4-Door Flex Refrigerator",
					brand: "SAMSUNG",
					category: "Appliances",
					purchasePrice: 2850,
					purchaseDate: new DateTime(2022, 1, 1),
					warrantyOffsetDaysFromToday: 180,
					image: "refrigerator.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 2,
					name: "65\" Class C2 OLED TV",
					brand: "LG",
					category: "Electronics",
					purchasePrice: 1400,
					purchaseDate: new DateTime(2018, 6, 15),
					warrantyOffsetDaysFromToday: -10,
					image: "tv.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 3,
					name: "MacBook Pro 16 M1 Max",
					brand: "APPLE",
					category: "Electronics",
					purchasePrice: 2100,
					purchaseDate: new DateTime(2023, 2, 10),
					warrantyOffsetDaysFromToday: 180,
					image: "laptop.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 4,
					name: "Barista Touch Espresso Machine",
					brand: "BREVILLE",
					category: "Kitchen",
					purchasePrice: 1099,
					purchaseDate: new DateTime(2022, 5, 20),
					warrantyOffsetDaysFromToday: 180,
					image: "coffee.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 5,
					name: "Dyson V15 Detect",
					brand: "DYSON",
					category: "Appliances",
					purchasePrice: 799,
					purchaseDate: new DateTime(2023, 1, 12),
					warrantyOffsetDaysFromToday: 180,
					image: "vacuum.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 6,
					name: "iPhone 15 Pro",
					brand: "APPLE",
					category: "Smart Devices",
					purchasePrice: 1299,
					purchaseDate: new DateTime(2024, 1, 5),
					warrantyOffsetDaysFromToday: 180,
					image: "phone.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 7,
					name: "Galaxy Watch 6",
					brand: "SAMSUNG",
					category: "Smart Devices",
					purchasePrice: 399,
					purchaseDate: new DateTime(2023, 9, 10),
					warrantyOffsetDaysFromToday: 180,
					image: "watch.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 8,
					name: "Wood Dining Table",
					brand: "IKEA",
					category: "Furniture",
					purchasePrice: 650,
					purchaseDate: new DateTime(2024, 8, 15),
					warrantyOffsetDaysFromToday: 180,
					image: "table.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 9,
					name: "3-Seater Sofa",
					brand: "IKEA",
					category: "Furniture",
					purchasePrice: 1200,
					purchaseDate: new DateTime(2025, 3, 12),
					warrantyOffsetDaysFromToday: 180,
					image: "sofa.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 10,
					name: "Microwave Oven",
					brand: "PANASONIC",
					category: "Kitchen",
					purchasePrice: 350,
					purchaseDate: new DateTime(2022, 11, 8),
					warrantyOffsetDaysFromToday: 25,
					image: "microwave.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 11,
					name: "Bluetooth Speaker",
					brand: "SONY",
					category: "Electronics",
					purchasePrice: 280,
					purchaseDate: new DateTime(2023, 7, 25),
					warrantyOffsetDaysFromToday: 180,
					image: "speaker.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 12,
					name: "Air Conditioner",
					brand: "DAIKIN",
					category: "Appliances",
					purchasePrice: 1600,
					purchaseDate: new DateTime(2021, 4, 4),
					warrantyOffsetDaysFromToday: -10,
					image: "ac.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 13,
					name: "Outdoor Grill",
					brand: "WEBER",
					category: "Outdoor",
					purchasePrice: 700,
					purchaseDate: new DateTime(2022, 9, 18),
					warrantyOffsetDaysFromToday: 28,
					image: "grill.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 14,
					name: "Robot Vacuum",
					brand: "iRobot",
					category: "Smart Devices",
					purchasePrice: 599,
					purchaseDate: new DateTime(2024, 2, 15),
					warrantyOffsetDaysFromToday: 180,
					image: "vacuum.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 15,
					name: "Smart Refrigerator",
					brand: "LG",
					category: "Appliances",
					purchasePrice: 2100,
					purchaseDate: new DateTime(2025, 12, 1),
					warrantyOffsetDaysFromToday: 180,
					image: "refrigerator.png"));

			_allItems.Add(CreateSeedItem(
					inventoryId: 16,
					name: "Espresso Machine",
					brand: "De'Longhi",
					category: "Kitchen",
					purchasePrice: 850,
					purchaseDate: new DateTime(2017, 4, 10),
					warrantyOffsetDaysFromToday: -10,
					image: "coffee.png"));
		}

		private static InventoryItem CreateSeedItem(
			int inventoryId,
			string name,
			string brand,
			string category,
			decimal purchasePrice,
			DateTime purchaseDate,
			int warrantyOffsetDaysFromToday,
			string image)
		{
			var warrantyEndDate = ComputeWarrantyEndDate(purchaseDate, warrantyOffsetDaysFromToday);
			var warrantyStatus = ComputeWarrantyStatus(warrantyEndDate);
			var statusColor = ComputeStatusColor(warrantyStatus);

			return new InventoryItem
			{
				InventoryId = inventoryId,
				Name = name,
				Brand = brand,
				Category = category,
				PurchasePrice = purchasePrice,
				PurchaseDate = purchaseDate,
				WarrantyEndDate = warrantyEndDate,
				WarrantyStatus = warrantyStatus,
				StatusColor = statusColor,
				Image = image
			};
		}

		private static DateTime ComputeWarrantyEndDate(DateTime purchaseDate, int warrantyOffsetDaysFromToday)
		{
			var targetEndDate = DateTime.Today.AddDays(warrantyOffsetDaysFromToday).Date;
			var durationDays = (targetEndDate - purchaseDate.Date).Days;
			return purchaseDate.Date.AddDays(durationDays);
		}

		private static string ComputeWarrantyStatus(DateTime warrantyEndDate)
		{
			var daysRemaining = (warrantyEndDate.Date - DateTime.Today).TotalDays;
			if (daysRemaining < 0) return "Expired";
			if (daysRemaining < 90) return "Expiring Soon";
			return "Protected";
		}

		private static string ComputeStatusColor(string warrantyStatus) => warrantyStatus switch
		{
			"Expired" => "#DC2626",
			"Expiring Soon" => "#EA580C",
			_ => "#1D4ED8"
		};
	}
}
