using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeInventoryManager.Model;
using HomeInventoryManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace HomeInventoryManager.ViewModel
{
	/// <summary>
	/// Backs the Warranty Center page. Drives the AI Insight callout,
	/// the 2x2 Summary grid, the Upcoming Expirations timeline,
	/// the All Warranties list and the Brand Coverage analytics.
	/// </summary>
	public partial class WarrantyViewModel : ObservableObject
	{
		private const string FilterAll = "All";
		private const string FilterHealthy = "Healthy";
		private const string FilterMonitor = "Monitor";
		private const string FilterAttention = "Attention";
		private const string FilterHighRisk = "High Risk";

		private readonly IAIService _aiService;
		private readonly InventoryViewModel _inventoryViewModel;

		private bool _isBusy;

		private bool _isBusyData;
		public bool IsBusy
		{
			get => _isBusy;
			set => SetProperty(ref _isBusy, value);
		}

		public bool IsBusyData
		{
			get => _isBusyData;
			set => SetProperty(ref _isBusyData, value);
		}

		private int _averageHealthScore;
		public int AverageHealthScore
		{
			get => _averageHealthScore;
			set => SetProperty(ref _averageHealthScore, value);
		}

		private int _healthyCount;
		public int HealthyCount
		{
			get => _healthyCount;
			set => SetProperty(ref _healthyCount, value);
		}

		private int _monitorCount;
		public int MonitorCount
		{
			get => _monitorCount;
			set => SetProperty(ref _monitorCount, value);
		}

		private int _attentionCount;
		public int AttentionCount
		{
			get => _attentionCount;
			set => SetProperty(ref _attentionCount, value);
		}

		private int _highRiskCount;
		public int HighRiskCount
		{
			get => _highRiskCount;
			set => SetProperty(ref _highRiskCount, value);
		}

		private string _topRiskSummary = string.Empty;
		public string TopRiskSummary
		{
			get => _topRiskSummary;
			set => SetProperty(ref _topRiskSummary, value);
		}

		private string _forecastKpiValue = "0";
		public string ForecastKpiValue
		{
			get => _forecastKpiValue;
			set => SetProperty(ref _forecastKpiValue, value);
		}

		private string _selectedFilter = FilterAll;
		public string SelectedFilter
		{
			get => _selectedFilter;
			set => SetProperty(ref _selectedFilter, value);
		}

		private ObservableCollection<FilterChipItem> _filterChips = new();
		public ObservableCollection<FilterChipItem> FilterChips
		{
			get => _filterChips;
			set => SetProperty(ref _filterChips, value);
		}

		private FilterChipItem? _selectedFilterChip;
		public FilterChipItem? SelectedFilterChip
		{
			get => _selectedFilterChip;
			set
			{
				if (SetProperty(ref _selectedFilterChip, value) && value is not null)
				{
					if (!string.Equals(SelectedFilter, value.Key, StringComparison.OrdinalIgnoreCase))
						ApplyFilter(value.Key);
				}
			}
		}

		private ObservableCollection<AgeDistributionBucket> _ageDistribution = new();
		public ObservableCollection<AgeDistributionBucket> AgeDistribution
		{
			get => _ageDistribution;
			set => SetProperty(ref _ageDistribution, value);
		}

		private ObservableCollection<ForecastSlice> _forecastSlices = new();
		public ObservableCollection<ForecastSlice> ForecastSlices
		{
			get => _forecastSlices;
			set => SetProperty(ref _forecastSlices, value);
		}

		private ObservableCollection<ApplianceHealth> _filteredAppliances = new();
		public ObservableCollection<ApplianceHealth> FilteredAppliances
		{
			get => _filteredAppliances;
			set => SetProperty(ref _filteredAppliances, value);
		}

		public ObservableCollection<ApplianceHealth> Appliances { get; } = new();

		public ObservableCollection<AdvisorSummary> SummaryCards { get; } = new();

		public ObservableCollection<AdvisorAction> Actions { get; } = new();

		private int _aiInsightRequestId;

		private int _activeCount;
		public int ActiveCount
		{
			get => _activeCount;
			set => SetProperty(ref _activeCount, value);
		}

		private int _expiringSoonCount;
		public int ExpiringSoonCount
		{
			get => _expiringSoonCount;
			set => SetProperty(ref _expiringSoonCount, value);
		}

		private int _expiredCount;
		public int ExpiredCount
		{
			get => _expiredCount;
			set => SetProperty(ref _expiredCount, value);
		}

		private int _amcCount;
		public int AmcCount
		{
			get => _amcCount;
			set => SetProperty(ref _amcCount, value);
		}

		private string _aiInsightText = string.Empty;
		public string AiInsightText
		{
			get => _aiInsightText;
			set => SetProperty(ref _aiInsightText, value);
		}

		/// <summary>Raised when the user taps a warranty in the list.</summary>
		public event EventHandler<WarrantyEntry>? WarrantyTapped;

		/// <summary>Raised when the user taps a timeline entry.</summary>
		public event EventHandler<UpcomingExpiration>? ExpirationTapped;

		/// <summary>Raised when the user taps "View All".</summary>
		public event EventHandler? ViewAllRequested;

		public ObservableCollection<WarrantyStat> Summary { get; } = new();

		public ObservableCollection<UpcomingExpiration> UpcomingExpirations { get; } = new();

		public ObservableCollection<WarrantyEntry> Warranties { get; } = new();

		public ObservableCollection<WarrantyEntry> FilteredWarranties { get; private set; } = new();

		public ObservableCollection<BrandCoverage> BrandCoverage { get; } = new();

		public ObservableCollection<WarrantyExpirationYearTrend> YearlyExpirationTrends { get; } = new();

		public int TotalExpiringSoon { get; private set; }
		public int TotalPeakExpiry { get; private set; }

		public ObservableCollection<string> FilterOptions { get; } =
		[
			"All",
			"Active",
			"ExpiringSoon",
			"Expired"
		];

		public WarrantyViewModel() : this(new MockAIService())
		{
		}

		public WarrantyViewModel(IAIService aiService)
		{
			_aiService = aiService;
			_inventoryViewModel = new InventoryViewModel(aiService);
			RebuildFromInventory();
			ApplyFilter();
			RecomputeSummary();
			SeedFilterChips();
		}

		private void SeedFilterChips()
		{
			FilterChips = new ObservableCollection<FilterChipItem>
			{
				new(FilterAll,        "All",       "#94A3B8"),
				new(FilterHealthy,    "Healthy",   "#16A34A"),
				new(FilterMonitor,    "Monitor",   "#EA580C"),
				new(FilterAttention,  "Attention", "#F59E0B"),
				new(FilterHighRisk,   "High Risk", "#DC2626"),
			};
			// Defer assigning the selected chip until the binding has
			// actually attached to the SfChipGroup; assigning it in the
			// constructor fires the change callback before the
			// ItemsSource has been pushed into the visual tree, which
			// leaves the chip group without a selected chip.
			_ = EnsureDefaultFilterAsync();
		}

		private async Task EnsureDefaultFilterAsync()
		{
			await Task.Yield();
			SelectedFilterChip ??= FilterChips[0];
			ApplyFilter(SelectedFilter);
		}

		public async Task RefreshAsync()
		{
			try
			{
				IsBusy = true;
				IsBusyData = false;

				await Task.Delay(300);
				RebuildFromInventory();
				ApplyFilter();
				RecomputeSummary();
				await RecomputeAiInsightTextAsync();

				var sourceItems = _inventoryViewModel.FilteredItems.Any()
					? _inventoryViewModel.FilteredItems
					: _inventoryViewModel.AllItems;

				var healths = await _aiService.EvaluateApplianceHealthAsync(sourceItems);

				// Enrich entries with the new card-display properties.
				EnrichForCardDisplay(healths);

				Appliances.Clear();
				foreach (var h in healths.OrderBy(h => h.HealthScore))
					Appliances.Add(h);

				AverageHealthScore = healths.Count == 0
					? 0
					: (int)Math.Round(healths.Average(h => (double)h.HealthScore));

				HealthyCount = healths.Count(h => h.RiskCategory == HealthCategory.Healthy);
				MonitorCount = healths.Count(h => h.RiskCategory == HealthCategory.Monitor);
				AttentionCount = healths.Count(h => h.RiskCategory == HealthCategory.AttentionNeeded);
				HighRiskCount = healths.Count(h => h.RiskCategory == HealthCategory.HighReplacementRisk);

				//RebuildSummary();
				RebuildAgeDistribution();
				RebuildForecast();
				RebuildTopRiskSummary();

				// Make sure the SfChipGroup always has a selected item
				// (defaults to "All" on the very first load).
				if (SelectedFilterChip is null && FilterChips.Count > 0)
					SelectedFilterChip = FilterChips[0];

				RebuildActions(healths);
				ApplyFilter(SelectedFilterChip?.Key ?? SelectedFilter);
			}
			finally
			{
				IsBusy = false;
				IsBusyData = true;
			}
		}

		private void RebuildActions(IReadOnlyList<ApplianceHealth> healths)
		{
			Actions.Clear();

			if (healths.Count == 0)
				return;

			var healthy = healths.Count(h => h.RiskCategory == HealthCategory.Healthy);
			var monitor = healths.Count(h => h.RiskCategory == HealthCategory.Monitor);
			var attention = healths.Count(h => h.RiskCategory == HealthCategory.AttentionNeeded);
			var highRisk = healths.Count(h => h.RiskCategory == HealthCategory.HighReplacementRisk);

			if (highRisk > 0)
			{
				var worst = healths.Where(h => h.RiskCategory == HealthCategory.HighReplacementRisk)
					.OrderBy(h => h.HealthScore)
					.FirstOrDefault();

				Actions.Add(new AdvisorAction
				{
					Title = "Upgrade Appliance",
					Description = worst is null
						? $"Upgrade {highRisk} high-risk appliances."
						: $"Upgrade {worst.ProductName} to reduce unexpected failures.",
					Icon = "expired.png",
					BackgroundColor = "#FEE2E2",
					IconColor = "#DC2626"
				});
			}

			if (attention > 0)
			{
				var most = healths.Where(h => h.RiskCategory == HealthCategory.AttentionNeeded)
					.OrderBy(h => h.HealthScore)
					.FirstOrDefault();

				Actions.Add(new AdvisorAction
				{
					Title = "Replace Appliance",
					Description = most is null
						? $"Replace {attention} attention-needed appliances."
						: $"Replace {most.ProductName} based on its depreciation risk.",
					Icon = "attention.png",
					BackgroundColor = "#FEF3C7",
					IconColor = "#F59E0B"
				});
			}

			if (monitor > 0)
			{
				var most = healths.Where(h => h.RiskCategory == HealthCategory.Monitor)
					.OrderBy(h => h.HealthScore)
					.FirstOrDefault();

				Actions.Add(new AdvisorAction
				{
					Title = "Renew AMC",
					Description = most is null
						? $"Renew AMC for {monitor} monitor appliances."
						: $"Renew AMC for {most.ProductName} for better coverage.",
					Icon = "monitor.png",
					BackgroundColor = "#E0F2FE",
					IconColor = "#0EA5E9"
				});
			}

			if (healthy > 0 && Actions.Count < 4)
			{
				var best = healths.Where(h => h.RiskCategory == HealthCategory.Healthy)
					.OrderByDescending(h => h.HealthScore)
					.FirstOrDefault();

				Actions.Add(new AdvisorAction
				{
					Title = "Service Appliance",
					Description = best is null
						? $"Schedule routine checks for {healthy} healthy units."
						: $"Schedule a routine check for {best.ProductName}.",
					Icon = "total.png",
					BackgroundColor = "#DCFCE7",
					IconColor = "#16A34A"
				});
			}
		}

		/// <summary>
		/// Filter the appliance list by the given status key
		/// ("All" | "Healthy" | "Monitor" | "Attention" | "High Risk").
		/// </summary>
		public void ApplyFilter(string? statusKey)
		{
			var key = string.IsNullOrWhiteSpace(statusKey) ? FilterAll : statusKey;
			SelectedFilter = key;

			// Keep the SfChipGroup selection in sync when the filter is
			// applied programmatically (e.g. on first load or via the
			// Refresh command).
			if (SelectedFilterChip is null || !string.Equals(SelectedFilterChip.Key, key, StringComparison.OrdinalIgnoreCase))
			{
				var match = FilterChips.FirstOrDefault(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));
				if (match is not null)
					SelectedFilterChip = match;
			}

			FilteredAppliances.Clear();
			IEnumerable<ApplianceHealth> source = Appliances;
			if (!string.Equals(key, FilterAll, StringComparison.OrdinalIgnoreCase))
			{
				source = Appliances.Where(a => string.Equals(a.StatusKey, key, StringComparison.OrdinalIgnoreCase));
			}
			foreach (var item in source)
				FilteredAppliances.Add(item);
		}

		//private void RebuildSummary()
		//{
		//	SummaryCards.Clear();

		//	SummaryCards.Add(new AdvisorSummary
		//	{
		//		Title = "Healthy",
		//		Value = HealthyCount,
		//		Icon = "✓",
		//		IconBackground = "#DCFCE7",
		//		IconColor = "#16A34A",
		//		ValueColor = "#0F172A"
		//	});

		//	SummaryCards.Add(new AdvisorSummary
		//	{
		//		Title = "Monitor",
		//		Value = MonitorCount,
		//		Icon = "👀",
		//		IconBackground = "#E0F2FE",
		//		IconColor = "#0EA5E9",
		//		ValueColor = "#0F172A"
		//	});

		//	SummaryCards.Add(new AdvisorSummary
		//	{
		//		Title = "Attention",
		//		Value = AttentionCount,
		//		Icon = "expired.png",
		//		IconBackground = "#FEF3C7",
		//		IconColor = "#F59E0B",
		//		ValueColor = "#0F172A"
		//	});

		//	SummaryCards.Add(new AdvisorSummary
		//	{
		//		Title = "High Risk",
		//		Value = HighRiskCount,
		//		Icon = "⛔",
		//		IconBackground = "#FEE2E2",
		//		IconColor = "#DC2626",
		//		ValueColor = "#0F172A"
		//	});
		//}

		private void RebuildAgeDistribution()
		{
			int b0_2 = 0, b3_5 = 0, b6_8 = 0, b9_11 = 0, b12p = 0;
			foreach (var a in Appliances)
			{
				var age = a.AgeYears;
				if (age <= 2) b0_2++;
				else if (age <= 5) b3_5++;
				else if (age <= 8) b6_8++;
				else if (age <= 11) b9_11++;
				else b12p++;
			}

			AgeDistribution = new ObservableCollection<AgeDistributionBucket>
			{
				new() { Bucket = "0-2y",  Count = b0_2  },
				new() { Bucket = "3-5y",  Count = b3_5  },
				new() { Bucket = "6-8y",  Count = b6_8  },
				new() { Bucket = "9-11y", Count = b9_11 },
				new() { Bucket = "12y+",  Count = b12p  },
			};
		}

		private void RebuildForecast()
		{
			ForecastSlices = new ObservableCollection<ForecastSlice>
			{
				new() { Category = FilterHealthy,   Count = HealthyCount,   SliceColor = Color.FromArgb("#16A34A") },
				new() { Category = FilterMonitor,   Count = MonitorCount,   SliceColor = Color.FromArgb("#0EA5E9") },
				new() { Category = FilterAttention, Count = AttentionCount, SliceColor = Color.FromArgb("#F59E0B") },
				new() { Category = FilterHighRisk,  Count = HighRiskCount,  SliceColor = Color.FromArgb("#DC2626") },
			};
			// KPI shown in the doughnut center – planned replacements this year.
			ForecastKpiValue = (HighRiskCount + AttentionCount).ToString();
		}

		private void RebuildTopRiskSummary()
		{
			var top = Appliances
				.Where(a => a.RiskCategory is HealthCategory.HighReplacementRisk or HealthCategory.AttentionNeeded)
				.OrderBy(a => a.HealthScore)
				.Take(2)
				.Select(a => a.ProductName)
				.ToList();

			TopRiskSummary = top.Count switch
			{
				0 => "No high-risk appliances detected. Great job keeping everything in shape!",
				1 => $"Based on your inventory data and manufacturer longevity metrics, your {top[0]} is currently operating in its high-risk depreciation window. Proactive replacement planning within the next 12-18 months is advised.",
				_ => $"Based on your inventory data and manufacturer longevity metrics, your {top[0]} and {top[1]} are currently operating in their high-risk depreciation window. Proactive replacement planning within the next 12-18 months is advised to prevent unexpected mechanical failure.",
			};
		}

		/// <summary>
		/// Populate the new display properties on the existing
		/// <see cref="ApplianceHealth"/> objects so the redesigned card
		/// can bind to them. The defaults below are derived from
		/// heuristics on name / category so we always have a sensible
		/// value to render.
		/// </summary>
		private static void EnrichForCardDisplay(IEnumerable<ApplianceHealth> healths)
		{
			var today = DateTime.Today;
			foreach (var h in healths)
			{
				if (h.AgeYears <= 0)
				{
					h.AgeYears = Math.Max(0, (int)Math.Round((today - h.LastServiced).TotalDays / 365.0));
				}
				if (h.AgeYears <= 0) h.AgeYears = 1;

				if (h.ExpectedLifespanYears <= 0)
				{
					h.ExpectedLifespanYears = (h.Category ?? string.Empty).ToLowerInvariant() switch
					{
						"appliances" => 12,
						"electronics" => 8,
						"smart devices" => 5,
						"kitchen" => 10,
						"furniture" => 20,
						_ => 10
					};
				}

				if (h.InstalledYear <= 0)
				{
					h.InstalledYear = today.Year - h.AgeYears;
				}

				if (string.IsNullOrWhiteSpace(h.Location))
				{
					h.Location = (h.Category ?? string.Empty).ToLowerInvariant() switch
					{
						"appliances" => "Kitchen",
						"electronics" => "Living Room",
						"kitchen" => "Kitchen",
						"furniture" => "Living Room",
						"smart devices" => "Home Office",
						_ => "Home"
					};
				}
			}
		}

		[RelayCommand]
		private async Task Refresh() => await RefreshAsync();

		private async Task RecomputeAiInsightTextAsync()
		{
			var requestId = ++_aiInsightRequestId;
			var today = DateTime.Today;
			var soonItems = _inventoryViewModel.AllItems
				.Where(i => i?.WarrantyEndDate is not null)
				.Select(i => new { Item = i!, DaysRemaining = (i!.WarrantyEndDate!.Value.Date - today).Days })
				.Where(x => x.DaysRemaining >= 0 && x.DaysRemaining <= 90)
				.OrderBy(x => x.DaysRemaining)
				.Select(x => x.Item)
				.Take(3)
				.ToList();

			if (soonItems.Count == 0)
			{
				MainThread.BeginInvokeOnMainThread(() =>
					{
						if (requestId != _aiInsightRequestId)
							return;
						AiInsightText = "AI insights will appear when you have warranties expiring in the next 90 days.";
					});
				return;
			}

			var top = soonItems[0];
			var days = (top.WarrantyEndDate!.Value.Date - today).Days;
			var next30Count = _inventoryViewModel.AllItems
				.Where(i => i?.WarrantyEndDate is not null)
				.Select(i => (i!.WarrantyEndDate!.Value.Date - today).Days)
				.Count(d => d >= 0 && d <= 30);
			var recs = await _aiService.GetMaintenanceRecommendationsAsync(top);
			var firstRec = recs.FirstOrDefault() ?? "Schedule the recommended maintenance.";
			var endsOn = top.WarrantyEndDate!.Value.Date.ToString("dd MMM yyyy");

			var insight = next30Count > 0
				? $"AI maintenance guidance: {firstRec}. You have {next30Count} warranty(s) expiring in the next 30 days (next ends in {days} days on {endsOn})."
				: $"AI maintenance guidance: {firstRec} (next warranty ends in {days} days on {endsOn}).";

			MainThread.BeginInvokeOnMainThread(() =>
				{
					if (requestId != _aiInsightRequestId)
						return;
					AiInsightText = insight;
				});
		}

		private void RebuildFromInventory()
		{
			UpcomingExpirations.Clear();
			Warranties.Clear();
			BrandCoverage.Clear();
			YearlyExpirationTrends.Clear();

			var today = DateTime.Today;
			var items = _inventoryViewModel.AllItems;

			foreach (var item in items)
			{
				if (item is null)
					continue;

				var warrantyEnd = item.WarrantyEndDate?.Date;
				if (warrantyEnd is null)
					continue;

				var daysRemaining = (warrantyEnd.Value - today).TotalDays;
				var coverageRemaining = daysRemaining <= 0
					? 0
					: (int)Math.Round(Math.Min(100, (daysRemaining / 180.0) * 100));
				coverageRemaining = Math.Max(0, Math.Min(100, coverageRemaining));

				var hasAmc = coverageRemaining >= 40;

				Warranties.Add(new WarrantyEntry
				{
					InventoryId = item.InventoryId,
					Brand = item.Brand,
					ProductName = item.Name,
					Image = item.Image,
					CoverageRemaining = coverageRemaining,
					HasAmc = hasAmc
				});

				var endDaysInt = (warrantyEnd.Value - today).Days;
				if (endDaysInt >= 0 && endDaysInt <= 90)
				{
					UpcomingExpirations.Add(new UpcomingExpiration
					{
						Bucket = endDaysInt <= 30
							? "NEXT 30 DAYS"
							: endDaysInt <= 60
								? "NEXT 60 DAYS"
								: "NEXT 90 DAYS",
						ProductName = item.Name,
						Brand = item.Brand,
						EndDate = warrantyEnd.Value
					});
				}
			}

			var orderedUpcoming = UpcomingExpirations.OrderBy(u => u.EndDate).ToList();
			UpcomingExpirations.Clear();
			foreach (var u in orderedUpcoming)
				UpcomingExpirations.Add(u);

			foreach (var group in Warranties.GroupBy(w => w.Brand ?? string.Empty))
			{
				if (!group.Any())
					continue;
				BrandCoverage.Add(new BrandCoverage
				{
					Brand = group.Key,
					CoveragePercentage = group.Average(w => w.CoverageRemaining)
				});
			}

			BuildQuarterlyTrends(today);
			TotalExpiringSoon = YearlyExpirationTrends.Sum(x => x.ExpiringSoonCount);
			TotalPeakExpiry = YearlyExpirationTrends.Sum(x => x.PeakExpiryCount);
		}

		private void BuildQuarterlyTrends(DateTime today)
		{
			var start = GetQuarterStart(today);
			for (var i = 0; i < 5; i++)
			{
				var qStart = start.AddMonths(i * 3);
				var qEnd = qStart.AddMonths(3).AddDays(-1);
				var expiringSoon = Warranties.Count(w =>
					{
						var item = _inventoryViewModel.AllItems.FirstOrDefault(ii => ii.InventoryId == w.InventoryId);
						if (item?.WarrantyEndDate is null)
							return false;
						var end = item.WarrantyEndDate.Value.Date;
						if (end < qStart || end > qEnd)
							return false;
						var daysRemaining = (end - today).TotalDays;
						return daysRemaining >= 0 && daysRemaining < 90;
					});

				var peak = Warranties.Count(w =>
					{
						var item = _inventoryViewModel.AllItems.FirstOrDefault(ii => ii.InventoryId == w.InventoryId);
						if (item?.WarrantyEndDate is null)
							return false;
						var end = item.WarrantyEndDate.Value.Date;
						return end >= qStart && end <= qEnd;
					});

				var quarterLabel = $"Q{GetQuarterNumber(qStart)} '{qStart:yy}";
				YearlyExpirationTrends.Add(new WarrantyExpirationYearTrend
				{
					Year = quarterLabel,
					ExpiringSoonCount = expiringSoon,
					PeakExpiryCount = peak
				});
			}
		}

		private static DateTime GetQuarterStart(DateTime date)
		{
			var month = date.Month;
			var quarterStartMonth = ((month - 1) / 3) * 3 + 1;
			return new DateTime(date.Year, quarterStartMonth, 1);
		}

		private static int GetQuarterNumber(DateTime date)
			=> ((date.Month - 1) / 3) + 1;

		private void RecomputeSummary()
		{
			var today = DateTime.Today;
			var items = _inventoryViewModel.AllItems;

			// "Expiring < 30 Days" should be measured by days-remaining on the
			// warranty, not by a coverage-percentage bucket. The previous rule
			// (>=25% and <75% coverage) returned 0 even for seed items whose
			// warranty actually ends in ~25-28 days.
			ExpiringSoonCount = items.Count(i =>
				i.WarrantyEndDate.HasValue &&
				(i.WarrantyEndDate.Value.Date - today).Days is >= 0 and < 30);

			ExpiredCount = items.Count(i =>
				i.WarrantyEndDate.HasValue &&
				(i.WarrantyEndDate.Value.Date - today).Days < 0);

			ActiveCount = items.Count(i =>
				i.WarrantyEndDate.HasValue &&
				(i.WarrantyEndDate.Value.Date - today).Days >= 30);

			AmcCount = Warranties.Count(w => w.HasAmc);

			Summary.Clear();
			Summary.Add(new WarrantyStat
			{
				Icon = "✓",
				IconBackground = "#DCFCE7",
				IconColor = "#16A34A",
				Value = ActiveCount,
				Label = "Active Warranties",
				ValueColor = "#16A34A"
			});
			Summary.Add(new WarrantyStat
			{
				Icon = "⏳",
				IconBackground = "#FEF3C7",
				IconColor = "#F59E0B",
				Value = ExpiringSoonCount,
				Label = "Expiring < 30 Days",
				ValueColor = "#F59E0B"
			});
			Summary.Add(new WarrantyStat
			{
				Icon = "✕",
				IconBackground = "#FEE2E2",
				IconColor = "#DC2626",
				Value = ExpiredCount,
				Label = "Expired",
				ValueColor = "#DC2626"
			});
			Summary.Add(new WarrantyStat
			{
				Icon = "🤝",
				IconBackground = "#E0F2FE",
				IconColor = "#0D33B8",
				Value = AmcCount,
				Label = "AMC Active",
				ValueColor = "#0D33B8"
			});
		}

		private void ApplyFilter()
		{
			FilteredWarranties.Clear();

			IEnumerable<WarrantyEntry> source = SelectedFilter switch
			{
				"Active" => Warranties.Where(w => w.CoverageRemaining >= 75),
				"ExpiringSoon" => Warranties.Where(w => w.CoverageRemaining is >= 25 and < 75),
				"Expired" => Warranties.Where(w => w.CoverageRemaining < 25),
				_ => Warranties
			};

			foreach (var item in source)
				FilteredWarranties.Add(item);
		}

		[RelayCommand]
		private void OpenWarranty(WarrantyEntry? entry)
		{
			if (entry is null)
				return;

			WarrantyTapped?.Invoke(this, entry);
		}

		[RelayCommand]
		private void OpenExpiration(UpcomingExpiration? expiration)
		{
			if (expiration is null)
				return;

			ExpirationTapped?.Invoke(this, expiration);
		}

		[RelayCommand]
		private void ViewAll() => ViewAllRequested?.Invoke(this, EventArgs.Empty);
	}

}
