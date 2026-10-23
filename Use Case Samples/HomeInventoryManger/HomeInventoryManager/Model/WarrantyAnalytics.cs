using System;
using System.Collections.Generic;
using System.Text;

namespace HomeInventoryManager.Model
{
	public class WarrantyAnalytics
	{
		public string Brand { get; set; }

		public double CoveragePercentage { get; set; }
	}

	public class WarrantyExpirationQuarter
	{
		public string Quarter { get; set; }

		public int Count { get; set; }
	}

	/// <summary>
	/// One data point in the "Yearly Expiration Trends" chart.
	/// ExpiringSoonCount is the light series, PeakExpiryCount is the dark series.
	/// </summary>
	public class WarrantyExpirationYearTrend
	{
		public string Year { get; set; } = string.Empty;
		public int ExpiringSoonCount { get; set; }
		public int PeakExpiryCount { get; set; }
	}
}
