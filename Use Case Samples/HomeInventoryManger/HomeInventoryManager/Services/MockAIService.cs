using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HomeInventoryManager.Model;

namespace HomeInventoryManager.Services
{
    /// <summary>
    /// Deterministic mock implementation of <see cref="IAIService"/>.
    /// Used in the sample so the app is fully functional without an
    /// Azure OpenAI deployment. All methods are awaitable so swapping
    /// in the real implementation is a one-line DI change.
    /// </summary>
    public class MockAIService : IAIService
    {

        public async Task<string> CategorizeAsync(string productName, string brand)
        {
            await Task.Delay(120);
            var name = (productName ?? string.Empty).ToLowerInvariant();
            if (name.Contains("refrigerator") || name.Contains("washing") ||
                name.Contains("microwave") || name.Contains("conditioner") ||
                name.Contains("vacuum") || name.Contains("dryer"))
                return "Appliances";

            if (name.Contains("phone") || name.Contains("watch") ||
                name.Contains("robot") || name.Contains("smart"))
                return "Smart Devices";

            if (name.Contains("table") || name.Contains("sofa") ||
                name.Contains("chair") || name.Contains("bed"))
                return "Furniture";

            if (name.Contains("coffee") || name.Contains("oven") ||
                name.Contains("grill") || name.Contains("espresso") ||
                name.Contains("kitchen"))
                return "Kitchen";

            return "Electronics";
        }

        public async Task<int> EstimateLifespanAsync(string category)
        {
            await Task.Delay(80);
            return category switch
            {
                "Appliances" => 12,
                "Electronics" => 8,
                "Smart Devices" => 5,
                "Kitchen" => 10,
                "Furniture" => 20,
                _ => 10
            };
        }

        public async Task<IReadOnlyList<Warranty>> PredictWarrantyRisksAsync(IEnumerable<Warranty> warranties)
        {
            await Task.Delay(150);
            return warranties
                .Where(w => w.DaysRemaining >= 0 && w.DaysRemaining <= 90)
                .OrderBy(w => w.DaysRemaining)
                .ToList();
        }

        public async Task<IReadOnlyList<ApplianceHealth>> EvaluateApplianceHealthAsync(IEnumerable<InventoryItem> items)
        {
            await Task.Delay(250);
            var today = DateTime.Today;
            var list = new List<ApplianceHealth>();

            foreach (var item in items)
            {
                var ageYears = Math.Max(0, (today - item.PurchaseDate).TotalDays / 365.0);
                // Use the category-specific expected lifespan so the health
                // score actually spans the full Healthy / Monitor /
                // Attention / High Risk range instead of clustering
                // everything in "Monitor".
                var lifespan = await EstimateLifespanAsync(item.Category);
                var remainingPct = Math.Max(0, 1 - ageYears / lifespan);
                var health = (int)Math.Round(remainingPct * 100);

                HealthCategory category;
                string recommendation;
                string icon;

                if (health >= 75)
                {
                    category = HealthCategory.Healthy;
                    recommendation = "Service Appliance";
                    icon = "amc.png";
                }
                else if (health >= 50)
                {
                    category = HealthCategory.Monitor;
                    recommendation = "Renew AMC";
                    icon = "monitor.png";
                }
                else if (health >= 25)
                {
                    category = HealthCategory.AttentionNeeded;
                    recommendation = "Replace Appliance";
                    icon = "settings.png";
                }
                else
                {
                    category = HealthCategory.HighReplacementRisk;
                    recommendation = "Upgrade Appliance";
                    icon = "attention.png";
                }

                list.Add(new ApplianceHealth
                {
                    InventoryId = item.InventoryId,
                    ProductName = item.Name,
                    Brand = item.Brand,
                    Category = item.Category,
                    Image = item.Image,
                    HealthScore = health,
                    RiskCategory = category,
                    EstimatedRemainingYears = (int)Math.Max(0, Math.Round(lifespan - ageYears)),
                    LastServiced = item.PurchaseDate.AddYears((int)ageYears / 2),
                    Recommendation = recommendation,
                    RecommendationIcon = icon,
                    // Carry over the installed year so the card can render
                    // "Location • Installed YYYY" without re-deriving it.
                    InstalledYear = item.PurchaseDate.Year,
                    AgeYears = (int)Math.Round(ageYears),
                    ExpectedLifespanYears = lifespan
                });
            }

            return list;
        }

        public async Task<IReadOnlyList<string>> GetMaintenanceRecommendationsAsync(InventoryItem item)
        {
            await Task.Delay(100);
            var recs = new List<string>
            {
                "Inspect and clean filters every 6 months.",
                "Run a self-diagnostic cycle once a month."
            };

            if (item.Category.Equals("Appliances", StringComparison.OrdinalIgnoreCase))
            {
                recs.Add("Renew AMC to keep labor costs covered.");
                recs.Add("Check door seals and water lines annually.");
            }
            else if (item.Category.Equals("Electronics", StringComparison.OrdinalIgnoreCase))
            {
                recs.Add("Update firmware when available.");
                recs.Add("Keep ventilation slots clear of dust.");
            }

            return recs;
        }

        public async Task<DashboardHealthInsight> ComputeHouseholdHealthAsync(
            IEnumerable<InventoryItem> items,
            int activeWarranties,
            int expiringSoonCount)
        {
            await Task.Delay(120);

            var itemList = items?.Where(i => i is not null).ToList() ?? new List<InventoryItem>();
            if (itemList.Count == 0)
                return new DashboardHealthInsight { HealthScore = 0, HealthBadge = "No data" };

            // Per-item health comes from the existing EvaluateApplianceHealthAsync
            // so the score stays consistent with the AI Advisor page. Items
            // without a category-specific lifespan (unknown category) fall back
            // to a neutral 70 so they don't drag the score down unfairly.
            var itemHealth = new List<int>(itemList.Count);
            foreach (var item in itemList)
            {
                var lifespan = await EstimateLifespanAsync(item.Category);
                var ageYears = Math.Max(0, (DateTime.Today - item.PurchaseDate).TotalDays / 365.0);
                var remainingPct = Math.Clamp(1 - ageYears / lifespan, 0, 1);
                itemHealth.Add((int)Math.Round(remainingPct * 100));
            }

            var avgHealth = itemHealth.Average();

            // Blend in warranty balance: every "Expiring soon" item drags the
            // score down a touch so households caught off-guard by a wave of
            // expirations aren't reported as "Excellent".
            var expiringPenalty = Math.Min(20, expiringSoonCount * 4);
            var activeBonus = Math.Min(10, activeWarranties * 1);

            var blended = (int)Math.Round(Math.Clamp(avgHealth - expiringPenalty + activeBonus, 0, 100));

            var badge = blended switch
            {
                >= 85 => "Excellent",
                >= 70 => "Good",
                >= 50 => "Monitor",
                _ => "Attention"
            };

            return new DashboardHealthInsight
            {
                HealthScore = blended,
                HealthBadge = badge
            };
        }

        public async Task<DashboardInsightCopy> GenerateDashboardInsightAsync(
            IEnumerable<InventoryItem> items,
            int activeWarranties,
            int expiringSoonCount,
            int expiredCount)
        {
            await Task.Delay(160);

            var itemCount = items?.Count() ?? 0;

            string title;
            string subtitle;

            if (itemCount == 0)
            {
                title = "Your home inventory is empty";
                subtitle = "Add your first item to unlock AI insights.";
            }
            else if (expiredCount > 0 && expiringSoonCount > 0)
            {
                title = "Some items need attention";
                subtitle = $"{activeWarranties} active • {expiringSoonCount} expiring soon • {expiredCount} expired";
            }
            else if (expiredCount == 0 && expiringSoonCount == 0)
            {
                title = "Your home inventory is healthy";
                subtitle = $"All systems protected • {activeWarranties} active warranties";
            }
            else if (expiringSoonCount > 0)
            {
                title = "Warranty renewals coming up";
                subtitle = $"{activeWarranties} active • {expiringSoonCount} expiring within 30 days";
            }
            else // expiredCount > 0, no expiring soon
            {
                title = "Warranty coverage has gaps";
                subtitle = $"{activeWarranties} active • {expiredCount} expired — consider renewing";
            }

            return new DashboardInsightCopy
            {
                Title = title,
                Subtitle = subtitle
            };
        }
    }
}
