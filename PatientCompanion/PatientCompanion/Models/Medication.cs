using CommunityToolkit.Mvvm.ComponentModel;

namespace PatientCompanion.Models;

public partial class Medication : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string dosage = string.Empty;

    [ObservableProperty]
    private string frequency = string.Empty;

    [ObservableProperty]
    private string instructions = string.Empty;

    [ObservableProperty]
    private string notificationText = string.Empty;

    [ObservableProperty]
    private string instructionIconGlyph = MaterialIcons.LocalPharmacy;

    [ObservableProperty]
    private string iconGlyph = "💊";

    [ObservableProperty]
    private string avatarText = "💊";

    [ObservableProperty]
    private string avatarBackgroundColor = "#DDE2FF";

    [ObservableProperty]
    private DateTime medicationTime;

    [ObservableProperty]
    private DateTime startDate;

    [ObservableProperty]
    private DateTime endDate;

    [ObservableProperty]
    private MedicationStatus status;

    [ObservableProperty]
    private bool isTaken;

    public DateTime NextOccurrence
    {
        get
        {
            DateTime next = MedicationTime;

            if (next <= DateTime.Now)
            {
                next = next.AddDays(1);
            }

            return next;
        }
    }

    public string Time
    {
        get => MedicationTime == default
            ? string.Empty
            : MedicationTime.ToString("hh:mm tt");

        set
        {
            if (DateTime.TryParse(value, out var parsedTime))
            {
                MedicationTime = DateTime.Today
                    .AddHours(parsedTime.Hour)
                    .AddMinutes(parsedTime.Minute);
            }
        }
    }

    public string ScheduleDisplay =>
        $"{Dosage} • {Time}";

    public bool ShowActions =>
        Status == MedicationStatus.DueNow && !IsTaken;

    public double CardHeight =>
        ShowActions ? 172 : 130;

    [ObservableProperty]
    private string period = string.Empty;

    public string StatusText =>
        Status switch
        {
            MedicationStatus.Taken => "TAKEN",
            MedicationStatus.DueNow => "DUE NOW",
            _ => "UPCOMING"
        };

    public string StatusColor =>
        Status switch
        {
            MedicationStatus.Taken => "#64748B",
            MedicationStatus.DueNow => "#DC2626",
            _ => "#64748B"
        };

    public string StatusBackgroundColor =>
        Status switch
        {
            MedicationStatus.Taken => "#F1F5F9",
            MedicationStatus.DueNow => "#FEE2E2",
            _ => "#F1F5F9"
        };

    public string NotificationContent =>
        string.IsNullOrWhiteSpace(NotificationText)
            ? Instructions
            : NotificationText;

    public void RefreshStatusProperties()
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(StatusBackgroundColor));
        OnPropertyChanged(nameof(ShowActions));
        OnPropertyChanged(nameof(CardHeight));
    }
}