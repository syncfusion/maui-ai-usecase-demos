namespace MediFlowSample.Models;

using Microsoft.Maui.Graphics;

public sealed class Patient
{
    public string PatientId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Gender { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string BloodGroup { get; init; } = string.Empty;
    public string Language { get; init; } = "English";
    public string PrimaryCondition { get; init; } = string.Empty;
    public string ConditionIconGlyph { get; init; } = "\uE3F3";
    public List<string> Allergies { get; init; } = [];
    public string RiskLevel { get; init; } = "Low";
    public string StatusText { get; init; } = "Monitor";
    public DateTime? LastVisitDate { get; set; }
    public DateTime? NextAppointmentDate { get; set; }
    public bool NeedsReview { get; init; }
    public double CarePlanProgress { get; init; }
    public string? Photo { get; init; }
    public string EmergencyContact { get; init; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    public string Initials => string.Concat(
        FirstName.Length > 0 ? FirstName[..1] : string.Empty,
        LastName.Length > 0 ? LastName[..1] : string.Empty).ToUpperInvariant();

    public bool IsHighPriority => RiskLevel == "High";

    public string NextAppointmentDisplay => NeedsReview
        ? "Needs Review"
        : NextAppointmentDate.HasValue
            ? NextAppointmentDate.Value.ToString("MMM dd, hh:mm tt")
            : "No appointment";

    public string LastVisitDisplay => LastVisitDate.HasValue
        ? LastVisitDate.Value.ToString("MMM dd, yyyy")
        : "Not provided";

    public string ProgressColor => IsHighPriority
        ? "#DC2626"
        : CarePlanProgress >= 80
            ? "#2563EB"
            : "#087F73";

    // Binding a Color/string to a Brush-typed property (e.g. ProgressFill, Ellipse.Fill) does not auto-convert; expose a Brush explicitly.
    public Brush ProgressFill => new SolidColorBrush(Color.FromArgb(ProgressColor));

    public string RiskColor => RiskLevel switch
    {
        "High" => "#DC2626",
        "Medium" => "#F59E0B",
        _ => "#16A34A"
    };

    public Brush RiskFill => new SolidColorBrush(Color.FromArgb(RiskColor));

    public string StatusColor => StatusText switch
    {
        "High Priority" => "#DC2626",
        "Stable" => "#16A34A",
        _ => "#F59E0B"
    };

    public string NextAppointmentTextColor => IsHighPriority ? "#DC2626" : "#0F172A";

    public bool IsFollowUpDue => NeedsReview ||
        (NextAppointmentDate.HasValue && NextAppointmentDate.Value.Date <= DateTime.Today.AddDays(3));

    public string LeftColumnLabel => IsHighPriority ? "CONDITION" : "STATUS";

    public string LeftColumnValue => IsHighPriority ? PrimaryCondition : StatusText;

    public string NextApptIconGlyph => NeedsReview ? "\uE915" : "\uE935";

    public string PriorityBadgeText => IsHighPriority ? "High Priority" : PrimaryCondition;

    public string PriorityBadgeIconGlyph => IsHighPriority ? "\uE645" : ConditionIconGlyph;

    public string PriorityBadgeBackground => IsHighPriority ? "#FEE2E2" : "#DBEAFE";

    public string PriorityBadgeTextColor => IsHighPriority ? "#DC2626" : "#1D4ED8";

    public string AllergiesDisplay => Allergies.Count == 0 ? "No known allergies" : string.Join(", ", Allergies);

    public string IdentitySummary => $"{PatientId} \u2022 {Age}y / {(Gender.Length > 0 ? Gender[..1] : string.Empty)}" +
        (string.IsNullOrWhiteSpace(BloodGroup) ? string.Empty : $" \u2022 {BloodGroup}");

    public string DateOfBirthDisplay => DateOfBirth.ToString("MMM dd, yyyy");

    public string PrimaryAllergyBadge => Allergies.Count > 0 ? Allergies[0] : string.Empty;

    public bool HasAllergies => Allergies.Count > 0;
}

public sealed class TimelineActivity
{
    public string ActivityId { get; init; } = string.Empty;
    public string PatientId { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsLatest { get; init; }

    public string TimestampDisplay
    {
        get
        {
            var today = DateTime.Today;
            if (Date.Date == today)
            {
                return $"Today, {Date:hh:mm tt}";
            }

            if (Date.Date == today.AddDays(-1))
            {
                return $"Yesterday, {Date:hh:mm tt}";
            }

            return Date.ToString("MMM dd, hh:mm tt");
        }
    }

    public string MarkerColor => IsLatest ? "#087F73" : "#94A3B8";

    public Brush MarkerFill => new SolidColorBrush(Color.FromArgb(MarkerColor));

    public bool HasTitle => !string.IsNullOrEmpty(Title);
}
