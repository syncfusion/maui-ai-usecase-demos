using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HomeInventoryManager.Converters
{

	public class TabIconConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return null;

			string activeTab = value.ToString()!;
			string tab = parameter.ToString()!;

			bool isSelected = activeTab == tab;

			return tab switch
			{
				"Dashboard" => isSelected
					? "dashboardselect.png"
					: "dashboard.png",

				"Inventory" => isSelected
					? "inventoryselect.png"
					: "inventory.png",

				"Scan" => isSelected
					? "scanselect.png"
					: "scan.png",

				"Warranty" => isSelected
					? "warrantyselect.png"
					: "warranty.png",

				_ => null
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
