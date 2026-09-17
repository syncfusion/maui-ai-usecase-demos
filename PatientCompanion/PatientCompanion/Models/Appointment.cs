namespace PatientCompanion.Models;

public class Appointment
{
    public string AppointmentId { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public string DoctorImage => DoctorName switch
    {
        "Dr. Robert Miller" => "robert_miller.png",
        "Dr. Sarah Jenkins" => "sarah_jenkins.png",
        "Dr. Emily Wilson" => "emily_wilson.png",
        "Dr. David Brown" => "david_brown.png",
        "Dr. Lisa Taylor" => "lisa_taylor.png",
        "Dr. Michael Scott" => "michael_scott.png",
        "Dr. James Anderson" => "james_anderson.png",
        "Dr. Olivia Martin" => "olivia_martin.png",
        "Dr. William Clark" => "william_clark.png",
        "Dr. Emma Roberts" => "emma_roberts.png",
        _ => "patient_avatar.png"
    };

    public string Specialization { get; set; } = string.Empty;

    public string Hospital { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string Time { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string VisitType { get; set; } = string.Empty;

    public string AvatarText
    {
        get
        {
            var names = DoctorName
                .Replace("Dr.", string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return string.Concat(names.Take(2).Select(x => x[0]));
        }
    }

    public bool IsCompleted => Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);

    public string DisplayDate => Date.ToString("MMM dd, yyyy");

    public string DisplayDateTime => $"{Date:MMM dd, yyyy} • {Time}";

    public bool ShowActions => Status == "Pending" || Status == "Confirmed";

    public Color StatusBackgroundColor =>
    Status switch
    {
        "Confirmed" => Color.FromArgb("#E8F5F2"),
        "Pending" => Color.FromArgb("#ECEEF0"),
        "Completed" => Color.FromArgb("#E8F5E9"),
        _ => Color.FromArgb("#ECEEF0")
    };

    public Color StatusTextColor =>
        Status switch
        {
            "Confirmed" => Color.FromArgb("#00685F"),
            "Pending" => Color.FromArgb("#3D4947"),
            "Completed" => Color.FromArgb("#2E7D32"),
            _ => Color.FromArgb("#3D4947")
        };

    public double CardHeight =>
    ShowActions ? 230 : 150;
}