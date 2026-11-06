using System;
using System.Collections;
using System.Globalization;

namespace HomeInventoryManager.Converters
{
    /// <summary>
    /// Returns true when an integer is greater than zero. Used to show
    /// the unread-count badge on the dashboard notification icon.
    /// </summary>
    public class HasUnreadConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int i) return i > 0;
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Returns true when the bound value is non-null and, for strings,
    /// non-empty. Used to toggle visibility of preview elements like the
    /// extracted scan image when no value is present yet.
    /// </summary>
    public class NotNullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null) return false;
            if (value is string s) return !string.IsNullOrWhiteSpace(s);
            if (value is ICollection c) return c.Count > 0;
            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Inverse boolean → bool. Used to swap visible/empty state UI.</summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : true;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : false;
    }
}
