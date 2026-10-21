using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HomeInventoryManager.Converters
{
	public class TabBackgroundConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value?.ToString() == parameter?.ToString())
				return Color.FromArgb("#7633DA");

			return Colors.Transparent;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> null;
	}
}
