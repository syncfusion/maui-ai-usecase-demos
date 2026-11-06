using System.Collections.Generic;
using System.Threading.Tasks;
using HomeInventoryManager.Model;

namespace HomeInventoryManager.Services
{
    /// <summary>
    /// Abstraction over the AI features described in the SRS:
    /// Invoice Reader (AI-01), Asset Categorization (AI-02),
    /// Warranty Insights (AI-03), Appliance Health Advisor (AI-04)
    /// and the Maintenance Recommendation Engine (AI-05).
    ///
    /// The default implementation is a deterministic mock so the app
    /// remains usable without an Azure OpenAI deployment. The real
    /// implementation can be plugged in by swapping the DI registration
    /// in <see cref="MauiProgram"/>.
    /// </summary>
    public interface IAIService
    {
        Task<string> CategorizeAsync(string productName, string brand);

        Task<int> EstimateLifespanAsync(string category);

        Task<IReadOnlyList<Warranty>> PredictWarrantyRisksAsync(IEnumerable<Warranty> warranties);

        Task<IReadOnlyList<ApplianceHealth>> EvaluateApplianceHealthAsync(IEnumerable<InventoryItem> items);

        Task<IReadOnlyList<string>> GetMaintenanceRecommendationsAsync(InventoryItem item);

        /// <summary>
        /// Computes a single overall "household health score" (0-100) plus
        /// the human-readable band it falls into (e.g. "Excellent").
        /// Used by the dashboard KPI card and the AI health banner.
        /// </summary>
        Task<DashboardHealthInsight> ComputeHouseholdHealthAsync(
            IEnumerable<InventoryItem> items,
            int activeWarranties,
            int expiringSoonCount);

        /// <summary>
        /// Produces the two-line copy shown in the dashboard's health
        /// banner. Both lines are written by the AI based on the current
        /// state of the inventory.
        /// </summary>
        Task<DashboardInsightCopy> GenerateDashboardInsightAsync(
            IEnumerable<InventoryItem> items,
            int activeWarranties,
            int expiringSoonCount,
            int expiredCount);
    }

    /// <summary>Single-number household health metric used in the dashboard KPI card.</summary>
    public class DashboardHealthInsight
    {
        public int HealthScore { get; set; }
        public string HealthBadge { get; set; } = string.Empty;
    }

    /// <summary>AI-generated two-line copy for the dashboard health banner.</summary>
    public class DashboardInsightCopy
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
    }
}
