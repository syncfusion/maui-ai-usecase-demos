using System;
using System.Collections.Generic;
using System.Text;

namespace HomeInventoryManager.Model
{
    /// <summary>
    /// Categories used to group appliances by their AI-estimated health.
    /// Maps to the labels in the AI Appliance Health Advisor screen.
    /// </summary>
    public enum HealthCategory
    {
        Healthy,
        Monitor,
        AttentionNeeded,
        HighReplacementRisk
    }

    /// <summary>
    /// One appliance entry on the AI Advisor page. Surfaces a 0–100 health
    /// score, the risk category, the estimated remaining useful life, the
    /// last serviced date and the recommended action.
    /// </summary>
    public class ApplianceHealth
    {
        public int InventoryId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Image { get; set; } = "dotnet_bot.png";

        public int HealthScore { get; set; } = 100;

        public HealthCategory RiskCategory { get; set; } = HealthCategory.Healthy;

        public int EstimatedRemainingYears { get; set; }

        public DateTime LastServiced { get; set; }

        public string Recommendation { get; set; } = string.Empty;

        public string RecommendationIcon { get; set; } = string.Empty;

        // === Extended properties used by the redesigned Stitch-inspired card layout ===

        /// <summary>Where the appliance is physically installed (e.g. "Kitchen").</summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>Year the appliance was installed.</summary>
        public int InstalledYear { get; set; } = DateTime.Today.Year;

        /// <summary>Manufacturer-expected lifespan, in years.</summary>
        public int ExpectedLifespanYears { get; set; } = 10;

        /// <summary>Approximate age in years (derived from PurchaseDate or set explicitly).</summary>
        public int AgeYears { get; set; }

        public string CategoryLabel => RiskCategory switch
        {
            HealthCategory.Healthy => "Healthy",
            HealthCategory.Monitor => "Monitor",
            HealthCategory.AttentionNeeded => "Attention Needed",
            HealthCategory.HighReplacementRisk => "High Replacement Risk",
            _ => string.Empty
        };

        public string CategoryColor => RiskCategory switch
        {
            HealthCategory.Healthy => "#16A34A",
            HealthCategory.Monitor => "#0EA5E9",
            HealthCategory.AttentionNeeded => "#F59E0B",
            HealthCategory.HighReplacementRisk => "#DC2626",
            _ => "#64748B"
        };

        public string CategoryBackground => RiskCategory switch
        {
            HealthCategory.Healthy => "#DCFCE7",
            HealthCategory.Monitor => "#E0F2FE",
            HealthCategory.AttentionNeeded => "#FEF3C7",
            HealthCategory.HighReplacementRisk => "#FEE2E2",
            _ => "#F1F5F9"
        };

        // --- Short labels used inside the new compact card ---

        public string ShortStatusLabel => RiskCategory switch
        {
            HealthCategory.HighReplacementRisk => "High Risk",
            HealthCategory.AttentionNeeded => "Attention",
            HealthCategory.Monitor => "Monitor",
            _ => "Healthy"
        };

        public string StatusKey => RiskCategory switch
        {
            HealthCategory.HighReplacementRisk => "High Risk",
            HealthCategory.AttentionNeeded => "Attention",
            HealthCategory.Monitor => "Monitor",
            _ => "Healthy"
        };

        public string LocationAndYear => string.IsNullOrEmpty(Location)
            ? $"Installed {InstalledYear}"
            : $"{Location} \u2022 Installed {InstalledYear}";

        public string AgeDisplay => $"{AgeYears} {(AgeYears == 1 ? "Year" : "Years")}";

        public string ExpectedLifespanDisplay => $"{ExpectedLifespanYears} {(ExpectedLifespanYears == 1 ? "Year" : "Years")}";

        public string RemainingLifeDisplay => $"~{EstimatedRemainingYears} {(EstimatedRemainingYears == 1 ? "Year" : "Years")}";
    }

    /// <summary>
    /// Top-of-page KPI shown above the appliance list on the AI Advisor.
    /// </summary>
    public class AdvisorSummary
    {
        public string Title { get; set; } = string.Empty;

        public int Value { get; set; }

        public string Icon { get; set; } = string.Empty;

        public string IconBackground { get; set; } = "#EEF2FF";

        public string IconColor { get; set; } = "#1D4ED8";

        public string ValueColor { get; set; } = "#0F172A";
    }

    /// <summary>
    /// Action chip on the AI Advisor (Service Appliance, Renew AMC, Replace, Upgrade).
    /// </summary>
    public class AdvisorAction
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string BackgroundColor { get; set; } = "#FFFFFF";

        public string IconColor { get; set; } = "#1D4ED8";
    }
}
