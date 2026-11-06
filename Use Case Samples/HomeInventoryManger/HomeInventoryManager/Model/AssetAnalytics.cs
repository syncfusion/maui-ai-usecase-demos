using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeInventoryManager.Model
{
	public class AssetAnalytics
	{
		public string Category { get; set; } = string.Empty;
		public double Value { get; set; }

		public Color SliceColor { get; set; } = Colors.Transparent;
	}
}
