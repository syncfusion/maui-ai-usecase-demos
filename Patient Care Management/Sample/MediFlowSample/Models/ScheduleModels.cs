using Microsoft.Maui.Graphics;

namespace MediFlowSample.Models;

public sealed class Clinician
{
    public string ClinicianId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Specialty { get; init; } = string.Empty;
    public string? Photo { get; init; }

    public string Initials => string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Where(part => part.Length > 0)
        .Select(part => part[0])).ToUpperInvariant();
}

public sealed class ScheduleAppointment
{
    public string AppointmentId { get; init; } = string.Empty;
    public string PatientId { get; init; } = string.Empty;
    public string ClinicianId { get; init; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string VisitType { get; init; } = string.Empty;
    public string Status { get; init; } = "Scheduled";
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public string Notes { get; init; } = string.Empty;
    public bool IsUrgent { get; init; }
    public bool IsCancelled => Status == "Cancelled";

    public string TimeRangeDisplay => $"{StartTime:h:mm} - {EndTime:h:mm tt}";

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

    public string StatusIconGlyph => Status switch
    {
        "Waiting" => "\uE88B",
        "Completed" => "\uE86C",
        "Cancelled" => "\uE645",
        _ => "\uE935"
    };

    public string BackgroundHex => IsUrgent
        ? "#B91C1C"
        : Status switch
        {
            "Waiting" => "#C2410C",
            "Completed" => "#15803D",
            "Cancelled" => "#64748B",
            _ => "#1D4ED8"
        };

    public string TextHex => "White";

    public Brush BackgroundFill => new SolidColorBrush(Color.FromArgb(BackgroundHex));

    public Color TextColorValue => Color.FromArgb(TextHex);
}
