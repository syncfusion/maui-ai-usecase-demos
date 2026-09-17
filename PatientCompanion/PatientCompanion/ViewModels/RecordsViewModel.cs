using CommunityToolkit.Mvvm.ComponentModel;
using PatientCompanion.Models;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class RecordsViewModel : ObservableObject
{
    [ObservableProperty]
    private int selectedTabIndex;

    public ObservableCollection<RecordItem> MedicalRecords { get; }

    public ObservableCollection<FormItem> RequiredForms { get; }

    public bool IsRecordsTabVisible => SelectedTabIndex == 0;

    public bool IsFormsTabVisible => SelectedTabIndex == 1;

    public RecordsViewModel()
    {
        SelectedTabIndex = 0;

        MedicalRecords = new ObservableCollection<RecordItem>
{
    new()
    {
        Title = "Comprehensive Metabolic Panel",
        Description = "Lab Results • Oct 24, 2026 • LabCorp",
        FileName = "CMP_Report.pdf",
        Icon = "labresult.png"
    },

    new()
    {
        Title = "Lisinopril 10mg",
        Description = "Prescription • Oct 12, 2026 • Dr. Smith",
        FileName = "Lisinopril_Prescription.pdf",
        Icon = "prescription.png"
    },

    new()
    {
        Title = "Influenza Vaccine",
        Description = "Immunization • Sep 15, 2026 • CVS Pharmacy",
        FileName = "Influenza_Vaccine_Record.pdf",
        Icon = "vaccine.png"
    }
};

        RequiredForms = new ObservableCollection<FormItem>
        {
            new()
            {
                Title = "Patient Intake Form",
                Status = "Completed Oct 1, 2026",
                IsCompleted = true,
                FileName = "Patient_Intake_Form.pdf"
            },

            new()
            {
                Title = "HIPAA Consent Form",
                Status = "Action required before next visit",
                IsCompleted = false,
                FileName = "HIPAA_Consent_Form.pdf"
            }
        };
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsRecordsTabVisible));
        OnPropertyChanged(nameof(IsFormsTabVisible));
    }
}