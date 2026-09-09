using System.Collections.ObjectModel;
using MediFlowSample.Models;

namespace MediFlowSample.Services;

public sealed class NotificationService
{
    public ObservableCollection<CareAlert> CareAlerts { get; } =
    [
        new()
        {
            AlertId = "AL-1001",
            PatientId = "PT-1042",
            Title = "Elevated BP Alert",
            Description = "Maya Patel - Room 3 - Requires immediate review",
            Severity = "High",
            IconGlyph = "\uE645",
            AccentColor = "#DC2626",
            IconBackground = "#FEE2E2"
        },
        new()
        {
            AlertId = "AL-1002",
            PatientId = "PT-1045",
            Title = "Allergy Review",
            Description = "Daniel Brooks - Pending penicillin confirmation",
            Severity = "Medium",
            IconGlyph = "\uE3F3",
            AccentColor = "#2563EB",
            IconBackground = "#DBEAFE"
        },
        new()
        {
            AlertId = "AL-1003",
            PatientId = "PT-0988",
            Title = "Follow-up Overdue",
            Description = "Sarah Jenkins - Post-visit review is overdue",
            Severity = "Medium",
            IconGlyph = "\uE88B",
            AccentColor = "#2563EB",
            IconBackground = "#DBEAFE"
        }
    ];

    public int CareAlertCount => CareAlerts.Count;
}
