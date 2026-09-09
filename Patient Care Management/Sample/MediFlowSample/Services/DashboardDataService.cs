using System.Collections.ObjectModel;
using MediFlowSample.Models;

namespace MediFlowSample.Services;

public sealed class DashboardDataService
{
    private const string DashboardClinicianId = "CL-01";
    private static readonly string[] MalePatientPhotos = ["patinet_1.jpg", "patinet_2.jpg", "patient_3.jpg", "patient_4.jpg"];
    private static readonly string[] FemalePatientPhotos = ["patinet_5.jpg", "patinet_6.jpg"];
    private readonly ScheduleDataService scheduleDataService;
    private readonly NotificationService notificationService;
    private readonly PatientDataService patientDataService;

    public DashboardDataService(
        ScheduleDataService scheduleDataService,
        NotificationService notificationService,
        PatientDataService patientDataService)
    {
        this.scheduleDataService = scheduleDataService;
        this.notificationService = notificationService;
        this.patientDataService = patientDataService;
    }

    public DashboardData GetDashboardData()
    {
        var allAppointments = scheduleDataService.GetAppointments();
        var clinicians = scheduleDataService.GetClinicians()
            .ToDictionary(clinician => clinician.ClinicianId, clinician => clinician.Name);
        var patients = patientDataService.GetPatients()
            .ToDictionary(patient => patient.PatientId);
        var today = DateTime.Today;
        var dashboardAppointments = allAppointments
            .Where(appointment => appointment.ClinicianId == DashboardClinicianId)
            .ToList();
        var todayAppointments = dashboardAppointments
            .Where(appointment => appointment.StartTime.Date == today && !appointment.IsCancelled)
            .OrderBy(appointment => appointment.StartTime)
            .ToList();
        var completed = todayAppointments.Count(IsCompleted);
        var remaining = todayAppointments.Count - completed;
        var completionPercentage = todayAppointments.Count == 0
            ? 0
            : completed * 100d / todayAppointments.Count;

        return new DashboardData
        {
            TodayAppointmentsCount = todayAppointments.Count,
            WaitingPatientsCount = todayAppointments.Count(appointment => IsStatus(appointment, "Waiting")),
            HighPriorityCount = todayAppointments.Count(appointment => appointment.IsUrgent),
            CompletedVisitsCount = completed,
            TodayVisitCount = todayAppointments.Count,
            CompletedPercentage = completionPercentage,
            WeeklyAppointments = CreateWeeklyAppointments(dashboardAppointments, today),
            VisitStatuses =
            [
                new() { Label = "Done", Value = completed, Percentage = completionPercentage, Color = "#087F73" },
                new() { Label = "Remaining", Value = remaining, Percentage = todayAppointments.Count == 0 ? 0 : remaining * 100d / todayAppointments.Count, Color = "#2563EB" }
            ],
            CareAlerts = new ObservableCollection<CareAlert>(notificationService.CareAlerts),
            UpcomingAppointments = new ObservableCollection<DashboardAppointment>(allAppointments
                .Where(appointment => appointment.ClinicianId == DashboardClinicianId
                    && appointment.StartTime.Date == today
                    && !appointment.IsCancelled)
                .OrderBy(GetStatusOrder)
                .ThenBy(appointment => appointment.StartTime)
                .Select(appointment =>
                ToDashboardAppointment(appointment, clinicians, patients)))
        };
    }

    private static ObservableCollection<DashboardChartPoint> CreateWeeklyAppointments(
        IReadOnlyList<ScheduleAppointment> appointments, DateTime today)
    {
        var weekStart = today.Date.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (today.DayOfWeek == DayOfWeek.Sunday)
        {
            weekStart = today.Date.AddDays(-6);
        }

        return new ObservableCollection<DashboardChartPoint>(Enumerable.Range(0, 5).Select(offset =>
        {
            var date = weekStart.AddDays(offset);
            var count = appointments.Count(appointment =>
                !appointment.IsCancelled && appointment.StartTime.Date == date.Date);
            return new DashboardChartPoint { Label = date.ToString("ddd")[..1], Value = count };
        }));
    }

    private static DashboardAppointment ToDashboardAppointment(
        ScheduleAppointment appointment,
        IReadOnlyDictionary<string, string> clinicians,
        IReadOnlyDictionary<string, Patient> patients)
    {
        var patient = patients.GetValueOrDefault(appointment.PatientId);
        var photos = string.Equals(patient?.Gender, "Female", StringComparison.OrdinalIgnoreCase)
            ? FemalePatientPhotos
            : MalePatientPhotos;
        var photoIndex = (int)(unchecked((uint)StringComparer.Ordinal.GetHashCode(appointment.AppointmentId)) % (uint)photos.Length);

        return new()
        {
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            PatientName = appointment.PatientName,
            PatientPhoto = photos[photoIndex],
            ClinicianName = clinicians.GetValueOrDefault(appointment.ClinicianId, appointment.ClinicianId),
            VisitType = appointment.VisitType,
            StartTime = appointment.StartTime,
            Status = appointment.Status,
            DisplayStatus = appointment.Status == "Completed" ? "Done" : appointment.Status
        };
    }

    private static bool IsCompleted(ScheduleAppointment appointment) => IsStatus(appointment, "Completed");

    private static bool IsStatus(ScheduleAppointment appointment, string status) =>
        appointment.Status.Equals(status, StringComparison.OrdinalIgnoreCase);

    private static int GetStatusOrder(ScheduleAppointment appointment) => appointment.Status.ToLowerInvariant() switch
    {
        "scheduled" => 0,
        "waiting" => 1,
        "completed" => 2,
        _ => 3
    };

}
