using System;
using System.Collections.Generic;
using System.Text;

namespace HomeInventoryManager.Model
{
	/// <summary>
	/// Represents a warranty record tied to an inventory item, including
	/// provider, coverage window, status classification and renewal info.
	/// </summary>
	public class Warranty
	{
		public int WarrantyId { get; set; }

		public int InventoryId { get; set; }

		public string ProductName { get; set; } = string.Empty;

		public string Brand { get; set; } = string.Empty;

		public string Image { get; set; } = string.Empty;

		public string Provider { get; set; } = string.Empty;

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public string CoverageType { get; set; } = string.Empty;

		public int DurationMonths { get; set; }

		public WarrantyStatus Status { get; set; }

		public string StatusText => Status switch
		{
			WarrantyStatus.Active => "Active",
			WarrantyStatus.ExpiringSoon => "Expiring Soon",
			WarrantyStatus.Expired => "Expired",
			_ => string.Empty
		};

		public string StatusColor => Status switch
		{
			WarrantyStatus.Active => "#16A34A",
			WarrantyStatus.ExpiringSoon => "#EA580C",
			WarrantyStatus.Expired => "#DC2626",
			_ => "#64748B"
		};

		/// <summary>Number of days until the warranty expires (negative if expired).</summary>
		public int DaysRemaining => (EndDate - DateTime.Today).Days;
	}

	/// <summary>
	/// Represents an Annual Maintenance Contract (AMC) subscription for an appliance.
	/// </summary>
	public class AmcContract
	{
		public int AmcId { get; set; }

		public int InventoryId { get; set; }

		public string ProductName { get; set; } = string.Empty;

		public string Provider { get; set; } = string.Empty;

		public string PlanName { get; set; } = string.Empty;

		public string Coverage { get; set; } = string.Empty;

		public DateTime RenewalDate { get; set; }

		public decimal AnnualCost { get; set; }

		public int ServiceFrequencyPerYear { get; set; }

		public bool IsActive => RenewalDate >= DateTime.Today;
	}

	public enum WarrantyStatus
	{
		Active,
		ExpiringSoon,
		Expired
	}

	/// <summary>
	 /// Aggregated totals shown at the top of the Warranty Tracker dashboard.
	/// </summary>
	public class WarrantySummary
	{
		public int TotalWarranties { get; set; }

		public int ActiveWarranties { get; set; }

		public int ExpiringSoon { get; set; }

		public int ExpiredWarranties { get; set; }

		public int ActiveAmc { get; set; }

		public decimal TotalAmcAnnualCost { get; set; }

		/// <summary>0-100 value indicating % of assets with active coverage.</summary>
		public double CoveragePercentage { get; set; }
	}

	/// <summary>
	/// A single point on the warranty coverage analytics chart.
	/// </summary>
	public class WarrantyCoveragePoint
	{
		public string Month { get; set; } = string.Empty;

		public int ActiveCount { get; set; }

		public int ExpiringCount { get; set; }

		public int ExpiredCount { get; set; }
	}

	/// <summary>
	/// One renewal alert entry surfaced from the warranty/AMC repository.
	/// </summary>
	public class RenewalAlert
	{
		public int ReferenceId { get; set; }

		public string ProductName { get; set; } = string.Empty;

		public string AlertType { get; set; } = string.Empty;

		public DateTime DueDate { get; set; }

		public int DaysAway => Math.Max(0, (DueDate - DateTime.Today).Days);

		public string AlertText => $"{AlertType} in {DaysAway} days";

		public string UrgencyColor => DaysAway switch
		{
			<= 7 => "#DC2626",
			<= 30 => "#EA580C",
			<= 60 => "#F59E0B",
			_ => "#16A34A"
		};
	}
}
