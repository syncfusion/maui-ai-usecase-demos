using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion;
using PatientCompanion.Models;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class MedicationsViewModel : BaseViewModel
{
    [ObservableProperty]
    private double weeklyAdherence = 92;

    [ObservableProperty]
    private string selectedChip = "Today";

    public ObservableCollection<Medication> Medications { get; } = new();

    public ObservableCollection<MedicationGroup> MedicationGroups { get; } = new();

    public ObservableCollection<string> Filters { get; } =
    [
        "Today",
        "Upcoming",
        "All"
    ];

    public MedicationsViewModel()
    {
        Title = "Medications";

        LoadMedications();
        ApplyFilter();
    }

    public string NextDoseDateText
    {
        get
        {
            if (NextDose == null)
                return string.Empty;

            return NextDose.NextOccurrence.Date > DateTime.Today
                ? "Tomorrow"
                : "Today";
        }
    }

    public Medication? NextDose =>
        Medications
            .OrderBy(x => x.NextOccurrence)
            .FirstOrDefault();

    partial void OnSelectedChipChanged(string value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    private void SkipMedication(Medication medication)
    {
        medication.IsTaken = true;
        medication.Status = MedicationStatus.Taken;
        RefreshMedicationState(medication);
    }

    [RelayCommand]
    private void MarkMedicationAsTaken(Medication medication)
    {
        medication.IsTaken = true;
        medication.Status = MedicationStatus.Taken;
        RefreshMedicationState(medication);
    }

    private void RefreshMedicationState(Medication medication)
    {
        medication.RefreshStatusProperties();
    }

    private void LoadMedications()
    {
        Medications.Clear();

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        Medications.Add(new Medication
        {
            Name = "Metformin",
            Dosage = "500mg",
            MedicationTime = today.AddHours(8),
            Period = "Morning",
            IconGlyph = MaterialIcons.CheckCircle,
            AvatarText = "✓",
            AvatarBackgroundColor = "#DDF3EF",
            InstructionIconGlyph = MaterialIcons.LocalPharmacy,
            Instructions = "Take with breakfast.",
            NotificationText = "Take with breakfast."
        });

        Medications.Add(new Medication
        {
            Name = "Lisinopril",
            Dosage = "10mg",
            MedicationTime = today.AddHours(14),
            Period = "Afternoon",
            IconGlyph = MaterialIcons.MedicalServices,
            AvatarText = "+",
            AvatarBackgroundColor = "#4F7CF3",
            InstructionIconGlyph = MaterialIcons.LocalPharmacy,
            Instructions = "Take with food. Do not take on an empty stomach.",
            NotificationText = "Take with food. Do not take on an empty stomach."
        });

        Medications.Add(new Medication
        {
            Name = "Vitamin D3",
            Dosage = "2000 IU",
            MedicationTime = today.AddHours(16),
            Period = "Afternoon",
            IconGlyph = MaterialIcons.Favorite,
            AvatarText = "D",
            AvatarBackgroundColor = "#9CA3AF",
            InstructionIconGlyph = MaterialIcons.LocalPharmacy,
            Instructions = "Take with plenty of water.",
            NotificationText = "Take with plenty of water."
        });

        Medications.Add(new Medication
        {
            Name = "Atorvastatin",
            Dosage = "20mg",
            MedicationTime = today.AddHours(20),
            Period = "Evening",
            IconGlyph = MaterialIcons.MedicalServices,
            AvatarText = "A",
            AvatarBackgroundColor = "#4F7CF3",
            InstructionIconGlyph = MaterialIcons.LocalPharmacy,
            Instructions = "Take before bedtime.",
            NotificationText = "Take before bedtime."
        });

        Medications.Add(CreateUpcomingMedication(
            "Losartan", "50mg", tomorrow.AddHours(9), "Morning", MaterialIcons.MedicalServices));
        Medications.Add(CreateUpcomingMedication(
            "Amlodipine", "5mg", tomorrow.AddHours(13), "Afternoon", MaterialIcons.MedicalServices));
        Medications.Add(CreateUpcomingMedication(
            "Vitamin B12", "1000mcg", tomorrow.AddHours(16), "Afternoon", MaterialIcons.Favorite));
        Medications.Add(CreateUpcomingMedication(
            "Levothyroxine", "75mcg", tomorrow.AddHours(21), "Evening", MaterialIcons.MedicalServices));

        UpdateMedicationStatus();

        OnPropertyChanged(nameof(NextDose));
    }

    private static Medication CreateUpcomingMedication(
        string name,
        string dosage,
        DateTime medicationTime,
        string period,
        string iconGlyph)
    {
        return new Medication
        {
            Name = name,
            Dosage = dosage,
            MedicationTime = medicationTime,
            Period = period,
            IconGlyph = iconGlyph,
            AvatarText = name[..1],
            AvatarBackgroundColor = "#DDE8E6",
            InstructionIconGlyph = MaterialIcons.LocalPharmacy,
            Instructions = "Take as prescribed.",
            NotificationText = "Take as prescribed."
        };
    }

    private void UpdateMedicationStatus()
    {
        DateTime now = DateTime.Now;

        foreach (var medication in Medications)
        {
            if (now >= medication.MedicationTime &&
                now <= medication.MedicationTime.AddMinutes(30))
            {
                medication.Status = MedicationStatus.DueNow;
            }
            else if (now > medication.MedicationTime.AddMinutes(30))
            {
                medication.Status = MedicationStatus.Taken;
            }
            else
            {
                medication.Status = MedicationStatus.Upcoming;
            }
        }

        OnPropertyChanged(nameof(NextDose));
    }

    private void ApplyFilter()
    {
        MedicationGroups.Clear();

        IEnumerable<Medication> source = Medications;

        switch (SelectedChip)
        {
            case "Upcoming":
                source = Medications.Where(x => x.MedicationTime.Date == DateTime.Today.AddDays(1));
                break;

            case "All":
                source = Medications;
                break;

            case "Today":
            default:
                source = Medications.Where(x => x.MedicationTime.Date == DateTime.Today);
                break;
        }

        foreach (var group in source.GroupBy(x => x.Period))
        {
            MedicationGroups.Add(new MedicationGroup
            {
                Title = group.Key,
                Icon = group.Key switch
                {
                    "Morning" => MaterialIcons.LightMode,
                    "Afternoon" => MaterialIcons.WbSunny,
                    "Evening" => MaterialIcons.DarkMode,
                    _ => MaterialIcons.MoreHoriz
                },
                Medications = new ObservableCollection<Medication>(group)
            });
        }
    }
}