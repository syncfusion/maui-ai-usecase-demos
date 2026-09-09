using System.Collections.ObjectModel;

namespace MediFlowSample.Models;

public sealed class DashboardAppointment
{
    public string AppointmentId { get; init; } = string.Empty;
    public string PatientId { get; init; } = string.Empty;
    public string PatientName { get; init; } = string.Empty;
    public string PatientPhoto { get; init; } = string.Empty;
    public string ClinicianName { get; init; } = string.Empty;
    public string VisitType { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public string DateDisplay => StartTime.Date == DateTime.Today
        ? "Today"
        : StartTime.ToString("MMM dd");
    public string Status { get; init; } = string.Empty;
    public string DisplayStatus { get; init; } = string.Empty;
    public string TimeText => StartTime.ToString("h:mm");
    public string MeridiemText => StartTime.ToString("tt");
}

public sealed class DashboardChartPoint
{
    public string Label { get; init; } = string.Empty;
    public double Value { get; init; }
}

public sealed class DashboardStatusPoint
{
    public string Label { get; init; } = string.Empty;
    public double Value { get; init; }
    public double Percentage { get; init; }
    public string Color { get; init; } = string.Empty;
}

public sealed class CareAlert
{
    public string AlertId { get; init; } = string.Empty;
    public string PatientId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string IconGlyph { get; init; } = string.Empty;
    public string AccentColor { get; init; } = string.Empty;
    public string IconBackground { get; init; } = string.Empty;
}

public sealed class DashboardData
{
    public int TodayAppointmentsCount { get; init; }
    public int WaitingPatientsCount { get; init; }
    public int HighPriorityCount { get; init; }
    public int CompletedVisitsCount { get; init; }
    public int TodayVisitCount { get; init; }
    public double CompletedPercentage { get; init; }
    public ObservableCollection<DashboardChartPoint> WeeklyAppointments { get; init; } = [];
    public ObservableCollection<DashboardStatusPoint> VisitStatuses { get; init; } = [];
    public ObservableCollection<CareAlert> CareAlerts { get; init; } = [];
    public ObservableCollection<DashboardAppointment> UpcomingAppointments { get; init; } = [];
}
