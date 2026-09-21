using CommunityToolkit.Mvvm.ComponentModel;

namespace PatientCompanion.Models;

public partial class AppointmentTimeOption : ObservableObject
{
    public AppointmentTimeOption(string value)
    {
        Value = value;
    }

    public string Value { get; }

    [ObservableProperty]
    private bool isSelected;
}
