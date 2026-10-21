using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HomeInventoryManager.Converters
{
	public class TabTextColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value?.ToString() == parameter?.ToString())
				return Colors.White;

			return Color.FromArgb("#22304B");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> null;
	}
}
