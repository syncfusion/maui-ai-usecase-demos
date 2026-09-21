using System.Collections.ObjectModel;

namespace PatientCompanion.Models;

public class MedicationGroup
{
    public string Title { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public ObservableCollection<Medication> Medications { get; set; } = new();
}