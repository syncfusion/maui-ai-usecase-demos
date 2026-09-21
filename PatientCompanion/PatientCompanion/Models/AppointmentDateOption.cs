using CommunityToolkit.Mvvm.ComponentModel;

namespace PatientCompanion.Models;

public partial class AppointmentDateOption : ObservableObject
{
    public AppointmentDateOption(DateTime date)
    {
        Date = date.Date;
    }

    public DateTime Date { get; }

    public string DayLabel => Date.ToString("ddd").ToUpperInvariant();

    public string DateNumber => Date.Day.ToString();

    [ObservableProperty]
    private bool isSelected;
}
