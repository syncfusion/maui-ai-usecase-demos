namespace PatientCompanion.Models;

public class FormItem
{
    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ActionText => "View";
}