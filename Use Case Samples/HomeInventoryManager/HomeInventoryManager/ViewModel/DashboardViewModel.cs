using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeInventoryManager.Model;
using HomeInventoryManager.Services;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace HomeInventoryManager.ViewModel
{
    /// <summary>
    /// Backs the Dashboard page. Owns the greeting, KPIs, AI advisor list,
    /// upcoming actions, asset analytics, brand coverage and the
    /// notification badge.
    /// </summary>
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IAIService _aiService;
        private readonly InventoryViewModel _inventoryViewModel;

        [ObservableProperty]
        private string userName = "Annie";

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string assetValue = string.Empty;

        [ObservableProperty]
        private string healthBannerTitle = "Your home inventory is healthy";

        [ObservableProperty]
        private string healthBannerSubtitle = "Loading AI insights…";

		private int _totalCount;
		public int TotalCount
		{
			get => _totalCount;
			set => SetProperty(ref _totalCount, value);
		}

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


		public string Greeting => $"Good {GetTimeOfDay()}, {UserName}";

        public string Today => DateTime.Today.ToString("dddd, MMM d");

		public ObservableCollection<WarrantyStat> Summary { get; } = new();

		public ObservableCollection<AIRecommendation> Recommendations { get; } = new();

        public ObservableCollection<UpcomingAction> UpcomingActions { get; } = new();

        public ObservableCollection<AssetAnalytics> Analytics { get; } = new();

        public ObservableCollection<WarrantyAnalytics> WarrantyAnalytics { get; } = new();

        public ObservableCollection<WarrantyExpirationQuarter> WarrantyExpirations { get; } = new();


        /// <summary>Raised when the user wants to navigate somewhere (handled by the Page).</summary>
        public event EventHandler<string>? NavigateRequested;

        /// <summary>Raised when the user wants to open an AI recommendation's owning item.</summary>
        public event EventHandler<AIRecommendation>? RecommendationTapped;

        public DashboardViewModel() : this(new MockAIService())
        {
        }

        public DashboardViewModel(IAIService aiService)
        {
            _aiService = aiService;
            _inventoryViewModel = new InventoryViewModel(aiService);
            _ = RefreshAsync();
			RecomputeSummary();
		}

        public async Task RefreshAsync()
        {
            try
            {
                IsBusy = true;

                RebuildFromInventory();
                await RebuildAiSectionsAsync();
                await RefreshHealthFromAiAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

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

			TotalCount = items.Count;

			Summary.Clear();
			Summary.Add(new WarrantyStat
			{
				Icon = "total.png",
				IconBackground = "#E0F2FE",
				IconColor = "#0D33B8",
				Value = TotalCount,
				Label = "Total Count",
				ValueColor = "#0D33B8"
			});
			Summary.Add(new WarrantyStat
			{
				Icon = "warrantydashboard.png",
				IconBackground = "#DCFCE7",
				IconColor = "#16A34A",
				Value = ActiveCount,
				Label = "Active Warranties",
				ValueColor = "#16A34A"
			});
			Summary.Add(new WarrantyStat
			{
				Icon = "expiring.png",
				IconBackground = "#FEF3C7",
				IconColor = "#F59E0B",
				Value = ExpiringSoonCount,
				Label = "Expiring < 30 Days",
				ValueColor = "#F59E0B"
			});
			Summary.Add(new WarrantyStat
			{
				Icon = "expired.png",
				IconBackground = "#FEE2E2",
				IconColor = "#DC2626",
				Value = ExpiredCount,
				Label = "Expired",
				ValueColor = "#DC2626"
			});
		}

		private void RebuildFromInventory()
        {
            AssetValue = "₹ 0";

            WarrantyAnalytics.Clear();
            Recommendations.Clear();
            UpcomingActions.Clear();
            Analytics.Clear();

            var items = _inventoryViewModel.AllItems;
            var today = DateTime.Today;


            var warrantyItems = items
                .Where(x => x.WarrantyEndDate.HasValue)
                .ToList();


            var palette = new[]
            {
                Color.FromArgb("#16A34A"),
                Color.FromArgb("#0EA5E9"),
                Color.FromArgb("#F59E0B"),
                Color.FromArgb("#F472B6"),
                Color.FromArgb("#374151")
            };

            var analyticsByCategory = items
                .GroupBy(x => x.Category ?? string.Empty)
                .Select((g, index) => new AssetAnalytics { Category = g.Key, Value = g.Sum(x => (double)x.PurchasePrice) })
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            for (var i = 0; i < analyticsByCategory.Count; i++)
            {
                analyticsByCategory[i].SliceColor = palette[i % palette.Length];
                Analytics.Add(analyticsByCategory[i]);
            }

            var warrantyByBrand = warrantyItems
                .GroupBy(x => x.Brand ?? string.Empty)
                .Select(g => new WarrantyAnalytics
                {
                    Brand = g.Key,
                    CoveragePercentage = g.Count() == 0
                        ? 0
                        : g.Average(x => ComputeCoverageRemaining(x.WarrantyEndDate!.Value, today))
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Brand))
                .OrderByDescending(x => x.CoveragePercentage)
                .ToList();

            foreach (var w in warrantyByBrand)
                WarrantyAnalytics.Add(w);

            BuildUpcomingActions(items);
        }

        /// <summary>
        /// Calls the AI services to refresh the household health KPI and the
        /// banner copy. Replaces what used to be hard-coded strings in the
        /// Dashboard view ("Your home inventory is healthy · 22 active").
        /// </summary>
        private async Task RefreshHealthFromAiAsync()
        {
            var items = _inventoryViewModel.AllItems;
            var today = DateTime.Today;

            var expiredCount = items.Count(i =>
                i?.WarrantyEndDate.HasValue == true &&
                (i.WarrantyEndDate!.Value.Date - today).Days < 0);

        }

        private async Task RebuildAiSectionsAsync()
        {
            var items = _inventoryViewModel.AllItems;
            var healths = await _aiService.EvaluateApplianceHealthAsync(items);

            var topRisks = healths
                .OrderBy(h => h.HealthScore)
                .Take(3)
                .ToList();

            foreach (var h in topRisks)
            {
                Recommendations.Add(new AIRecommendation
                {
                    Icon = h.RecommendationIcon,
                    Message = string.IsNullOrWhiteSpace(h.ProductName)
                        ? h.Recommendation
                        : $"{h.ProductName}: {h.Recommendation}"
                });
            }

            await BuildNotificationsAsync(items, healths);
        }

        private void BuildUpcomingActions(IReadOnlyList<InventoryItem> items)
        {
            var today = DateTime.Today;
            UpcomingActions.Clear();

            var expiring = items
                .Where(x => x.WarrantyEndDate.HasValue)
                .Select(x => new { Item = x, DaysRemaining = (x.WarrantyEndDate!.Value.Date - today).Days })
                .Where(x => x.DaysRemaining >= 0 && x.DaysRemaining <= 60)
                .OrderBy(x => x.DaysRemaining)
                .Take(3)
                .ToList();

            foreach (var x in expiring)
            {
                    var actionText = x.DaysRemaining <= 30 ? "Action Needed" : "Renewal";
                    var description = $"Warranty expires in {x.DaysRemaining} days";
                    UpcomingActions.Add(new UpcomingAction
                    {
                        ProductName = x.Item.Name,
                        Description = description,
                        ActionText = actionText,
                        Image = x.Item.Image
                    });
            }
        }

        private async Task BuildNotificationsAsync(IReadOnlyList<InventoryItem> items, IReadOnlyList<ApplianceHealth> healths)
        {

            var today = DateTime.Today;

            var expiringSoon = items
                .Where(x => x.WarrantyEndDate.HasValue)
                .Select(x => new { Item = x, DaysRemaining = (x.WarrantyEndDate!.Value.Date - today).Days })
                .Where(x => x.DaysRemaining >= 0 && x.DaysRemaining <= 30)
                .OrderBy(x => x.DaysRemaining)
                .FirstOrDefault();


            var appliances = healths
                .Where(h => string.Equals(h.Category, "Appliances", StringComparison.OrdinalIgnoreCase))
                .OrderBy(h => h.HealthScore)
                .ToList();

            var worstAppliance = appliances.FirstOrDefault();
            if (worstAppliance is not null)
            {
                var item = items.FirstOrDefault(i => i.InventoryId == worstAppliance.InventoryId);
                if (item is not null)
                {
                    var recs = await _aiService.GetMaintenanceRecommendationsAsync(item);
                    var msg = recs.Count > 0 ? recs[0] : "Maintenance check recommended.";
                }
            }
        }

        private static int ComputeCoverageRemaining(DateTime warrantyEndDate, DateTime today)
        {
            var daysRemaining = (warrantyEndDate - today).TotalDays;
            var coverageRemaining = daysRemaining <= 0
                ? 0
                : (int)Math.Round(Math.Min(100, (daysRemaining / 180.0) * 100));
            return Math.Max(0, Math.Min(100, coverageRemaining));
        }

        [RelayCommand]
        private void GoToInventory() => NavigateRequested?.Invoke(this, "Inventory");

        [RelayCommand]
        private void GoToScan() => NavigateRequested?.Invoke(this, "Scan");

        [RelayCommand]
        private void GoToWarranty() => NavigateRequested?.Invoke(this, "Warranty");

        [RelayCommand]
        private void GoToAiAdvisor() => NavigateRequested?.Invoke(this, "AIAdvisor");

        [RelayCommand]
        private void GoToReports() => NavigateRequested?.Invoke(this, "Reports");

        [RelayCommand]
        private void GoToSettings() => NavigateRequested?.Invoke(this, "Settings");

        [RelayCommand]
        private void OpenRecommendation(AIRecommendation? recommendation)
        {
            if (recommendation is null)
                return;
            RecommendationTapped?.Invoke(this, recommendation);
        }

        [RelayCommand]
        private void OpenUpcomingAction(UpcomingAction? action)
        {
            if (action is null)
                return;
            NavigateRequested?.Invoke(this, "Warranty");
        }

        [RelayCommand]
        private void OpenAllActions() => NavigateRequested?.Invoke(this, "Warranty");


        private static string GetTimeOfDay()
        {
            var hour = DateTime.Now.Hour;
            if (hour < 12) return "Morning";
            if (hour < 17) return "Afternoon";
            return "Evening";
        }
    }
}
