using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace HomeInventoryManager.Model
{
	/// <summary>
	/// A single warranty card shown in the "All Warranties" list.
	/// Includes brand, image, a 0-100 health score and the AMC state
	/// (None, Active, Expiring Soon, etc.).
	/// </summary>
	public class WarrantyEntry
	{
		public int InventoryId { get; set; }

		public string Brand { get; set; } = string.Empty;

		public string ProductName { get; set; } = string.Empty;

		public string Image { get; set; } = "dotnet_bot.png";

		/// <summary>Warranty coverage remaining, 0–100.</summary>
		public int CoverageRemaining { get; set; } = 100;

		/// <summary>True when the item currently has an active AMC contract.</summary>
		public bool HasAmc { get; set; }

		public string AmcLabel => HasAmc ? "AMC Active" : "No AMC";

		public string AmcBackground => HasAmc ? "#DBEAFE" : "#F1F5F9";

		public string AmcTextColor => HasAmc ? "#1D4ED8" : "#475569";

		/// <summary>Color of the health dot on the left of the card.</summary>
		public string HealthDotColor => CoverageRemaining switch
		{
			>= 75 => "#16A34A",
			>= 40 => "#F59E0B",
			_ => "#DC2626"
		};
	}

	/// <summary>
	/// A single tile in the 2x2 "Summary" grid.
	/// </summary>
	public class WarrantyStat
	{
		public string Icon { get; set; } = string.Empty;

		public string IconBackground { get; set; } = "#FFFFFF";

		public string IconColor { get; set; } = "#0D33B8";

		public int Value { get; set; }

		public string Label { get; set; } = string.Empty;

		public string ValueColor { get; set; } = "#0D33B8";
	}

	/// <summary>
	/// One entry on the "Upcoming Expirations" timeline (NEXT 30 / NEXT 60 DAYS).
	/// </summary>
	public class UpcomingExpiration
	{
		public string Bucket { get; set; } = string.Empty;

		public string ProductName { get; set; } = string.Empty;

		public string Brand { get; set; } = string.Empty;

		public DateTime EndDate { get; set; }

		public int DaysRemaining => Math.Max(0, (EndDate - DateTime.Today).Days);

		public string DaysLabel => $"{DaysRemaining} Days";

		/// <summary>Background color of the "X Days" pill.</summary>
		public string DaysBackground => DaysRemaining switch
		{
			<= 7 => "#FCA5A5",
			<= 30 => "#FCD9B6",
			_ => "#FDE68A"
		};

		public string DaysTextColor => DaysRemaining switch
		{
			<= 7 => "#7F1D1D",
			<= 30 => "#7C2D12",
			_ => "#78350F"
		};

		/// <summary>Color of the timeline dot.</summary>
		public string DotColor => DaysRemaining switch
		{
			<= 7 => "#DC2626",
			<= 30 => "#EA580C",
			_ => "#92400E"
		};
	}

	/// <summary>
	/// One row of the "Brand Coverage" analytics block.
	/// </summary>
	public class BrandCoverage
	{
		public string Brand { get; set; } = string.Empty;

		public double CoveragePercentage { get; set; }

		public Brush BarColor => CoveragePercentage switch
		{
			>= 40 => new SolidColorBrush(Color.FromArgb("#1D4ED8")),
			>= 25 => new SolidColorBrush(Color.FromArgb("#475569")),
			_ => new SolidColorBrush(Color.FromArgb("#7C2D12"))
		};
	}
}
