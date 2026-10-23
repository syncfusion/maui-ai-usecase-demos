using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace HomeInventoryManager.Converters
{
	public class PercentageConverter : IValueConverter
	{
		public object? Convert(
			object? value,
			Type targetType,
			object? parameter,
			CultureInfo culture)
		{
			if (value is null) return 0d;

			double percentage = value switch
			{
				double d => d,
				float f => f,
				int i => i,
				long l => l,
				decimal m => (double)m,
				_ => 0d
			};

			return Math.Max(0, Math.Min(1, percentage / 100.0));

			// ReSharper disable once HeuristicUnreachableCode
			// This is kept for safety if switch above changes.
			// (Practically unreachable.)
			// return 0d;
		}

		public object? ConvertBack(
			object? value,
			Type targetType,
			object? parameter,
			CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
