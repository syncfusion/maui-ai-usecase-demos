using System;
using System.Collections.Generic;
using System.Text;

namespace HomeInventoryManager.Model
{
	public class DashboardSummary
	{
		public int TotalAssets { get; set; }

		public int ActiveWarranties { get; set; }

		public int ExpiringSoon { get; set; }

		/// <summary>
		/// Count of warranties whose end date has already passed
		/// (days remaining &lt; 0).
		/// </summary>
		public int Expired { get; set; }

		public decimal TotalAssetValue { get; set; }

		/// <summary>
		/// AI-computed 0-100 household health score, blending each item's
		/// remaining lifespan with warranty-balance penalties/bonuses.
		/// </summary>
		public int HealthScore { get; set; }

		/// <summary>Human-readable band corresponding to <see cref="HealthScore"/>.</summary>
		public string HealthBadge { get; set; } = string.Empty;
	}
}
