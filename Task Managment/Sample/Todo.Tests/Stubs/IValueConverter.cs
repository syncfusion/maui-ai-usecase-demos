// Stubs/IValueConverter.cs
// Minimal stubs of the .NET MAUI types referenced from the foundation
// converter source files.  Kept inside the Todo.Tests project so the
// converter sources compile unchanged in a plain net10.0 console test
// host (the real Microsoft.Maui.Controls assembly requires the MAUI
// workload, which is not available on every CI agent).

using System.Globalization;

namespace Microsoft.Maui.Controls;

public interface IValueConverter
{
    object Convert(object? value, Type targetType, object? parameter, CultureInfo culture);
    object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture);
}
