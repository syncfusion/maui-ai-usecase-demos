using System;
using System.Collections.Generic;
using System.Text;

namespace HomeInventoryManager.Model
{
	public class InventoryItem
	{
		public int InventoryId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string Brand { get; set; } = string.Empty;

		public string Category { get; set; } = string.Empty;

		public string Image { get; set; } = string.Empty;

		public decimal PurchasePrice { get; set; }

		public DateTime PurchaseDate { get; set; }

		public DateTime? WarrantyEndDate { get; set; }

		public string WarrantyStatus { get; set; } = string.Empty;

		public string StatusColor { get; set; } = string.Empty;
	}
}
