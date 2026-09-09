using System.Globalization;
using MediFlowSample.Models;

namespace MediFlowSample.Converters;

/// <summary>Returns the card background or stroke color for a clinician list item based on whether it is the active selection.</summary>
public sealed class ClinicianHighlightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var isSelected = values.Length == 2 &&
            values[0] is Clinician clinician &&
            values[1] is Clinician selected &&
            clinician.ClinicianId == selected.ClinicianId;

        return parameter as string == "Stroke"
            ? (isSelected ? Color.FromArgb("#087F73") : Color.FromArgb("#DCE4F2"))
            : (isSelected ? Color.FromArgb("#D1E7E4") : Color.FromArgb("#F4F7FC"));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
