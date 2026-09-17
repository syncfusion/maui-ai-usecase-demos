namespace PatientCompanion.Models;

public class MedicalRecord
{
    public string RecordId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime RecordDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public string DisplayDate =>
        RecordDate.ToString("MMM dd, yyyy");
}