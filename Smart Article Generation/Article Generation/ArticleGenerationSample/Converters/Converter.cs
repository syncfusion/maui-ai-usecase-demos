using System;
using System.Globalization;

namespace ArticleGenerationSample
{
    /// <summary>
    /// Converts a boolean into a GridLength, expanding to a fixed width when true and Star otherwise.
    /// </summary>
    public sealed class BoolToGridLengthConverter : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b && b ? new GridLength(300) : new GridLength(1, GridUnitType.Star);
        }

        /// <inheritdoc />
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean into a GridLength, returning Star when true and 0 when false.
    /// </summary>
    public sealed class BoolToGridLengthConverterInverse : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b && b ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        }

        /// <inheritdoc />
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Inverts a boolean value.
    /// </summary>
    public sealed class InverseBoolConverter : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b ? !b : value;
        }

        /// <inheritdoc />
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b ? !b : value;
        }
    }

    /// <summary>
    /// Returns true when the bound string is not null or empty; otherwise false.
    /// </summary>
    public sealed class IsNotNullOrEmptyConverter : IValueConverter
    {
        /// <inheritdoc />
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is string s && !string.IsNullOrEmpty(s);
        }

        /// <inheritdoc />
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
