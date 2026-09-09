using MediFlowSample.Models;

namespace MediFlowSample.Services;

public sealed class ScheduleDataService
{
    private readonly PatientDataService patientDataService;
    private readonly List<Clinician> clinicians;
    private readonly List<ScheduleAppointment> appointments;

    public ScheduleDataService(PatientDataService patientDataService)
    {
        this.patientDataService = patientDataService;
        clinicians = CreateClinicians();
        appointments = CreateAppointments();
        foreach (var appointment in appointments)
        {
            appointment.PatientName = GetPatientName(appointment.PatientId);
        }
        patientDataService.UpdateAppointmentDates(appointments);
    }

    public IReadOnlyList<Clinician> GetClinicians() => clinicians;

    public IReadOnlyList<ScheduleAppointment> GetAppointments() => appointments;

    public IReadOnlyList<ScheduleAppointment> GetAppointments(string clinicianId) =>
        appointments.Where(appointment => appointment.ClinicianId == clinicianId).ToList();

    public string GetPatientName(string patientId) =>
        patientDataService.GetPatients().FirstOrDefault(patient => patient.PatientId == patientId)?.FullName ?? patientId;

    private static List<Clinician> CreateClinicians() =>
    [
        new() { ClinicianId = "CL-01", Name = "Dr. S. Wilson", Specialty = "Internal Medicine" },
        new() { ClinicianId = "CL-02", Name = "Dr. J. Lee", Specialty = "Cardiology" },
        new() { ClinicianId = "CL-03", Name = "Dr. R. Patel", Specialty = "Endocrinology" },
        new() { ClinicianId = "CL-04", Name = "Dr. A. Nguyen", Specialty = "Pediatrics" },
        new() { ClinicianId = "CL-05", Name = "Dr. M. Davis", Specialty = "Family Medicine" }
    ];

    private static List<ScheduleAppointment> CreateAppointments()
    {
        var today = DateTime.Today;

        List<ScheduleAppointment> appointments =
        [
            // Dr. S. Wilson - today
            new()
            {
                AppointmentId = "SA-1001",
                PatientId = "PT-1042",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "General Consultation",
                Status = "Completed",
                StartTime = today.AddHours(9),
                EndTime = today.AddHours(9).AddMinutes(45)
            },
            new()
            {
                AppointmentId = "SA-1002",
                PatientId = "PT-1045",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Lab Review",
                Status = "Scheduled",
                StartTime = today.AddHours(10),
                EndTime = today.AddHours(11).AddMinutes(30),
                Notes = "Bloodwork Results"
            },
            new()
            {
                AppointmentId = "SA-1003",
                PatientId = "PT-0988",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Follow-up (Urgent)",
                Status = "Scheduled",
                IsUrgent = true,
                StartTime = today.AddHours(13),
                EndTime = today.AddHours(13).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1004",
                PatientId = "PT-1018",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Annual Physical",
                Status = "Waiting",
                StartTime = today.AddDays(1).AddHours(9).AddMinutes(30),
                EndTime = today.AddDays(1).AddHours(10)
            },
            new()
            {
                AppointmentId = "SA-1005",
                PatientId = "PT-1101",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Migraine Review",
                Status = "Completed",
                StartTime = today.AddDays(-1).AddHours(11),
                EndTime = today.AddDays(-1).AddHours(11).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1006",
                PatientId = "PT-1156",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Thyroid Check",
                Status = "Scheduled",
                StartTime = today.AddDays(2).AddHours(14),
                EndTime = today.AddDays(2).AddHours(14).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1007",
                PatientId = "PT-1204",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Anxiety Follow-up",
                Status = "Cancelled",
                StartTime = today.AddDays(-2).AddHours(10),
                EndTime = today.AddDays(-2).AddHours(10).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1008",
                PatientId = "PT-1101",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Medication Review",
                Status = "Waiting",
                StartTime = today.AddHours(9).AddMinutes(45),
                EndTime = today.AddHours(10).AddMinutes(15)
            },
            new()
            {
                AppointmentId = "SA-1009",
                PatientId = "PT-1156",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Chronic Care Visit",
                Status = "Scheduled",
                StartTime = today.AddHours(11).AddMinutes(45),
                EndTime = today.AddHours(12).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1010",
                PatientId = "PT-1204",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Anxiety Follow-up",
                Status = "Completed",
                StartTime = today.AddHours(8),
                EndTime = today.AddHours(8).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1011",
                PatientId = "PT-1291",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Preventive Consultation",
                Status = "Scheduled",
                StartTime = today.AddHours(14),
                EndTime = today.AddHours(14).AddMinutes(45)
            },
            new()
            {
                AppointmentId = "SA-1012",
                PatientId = "PT-1345",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Blood Pressure Check",
                Status = "Waiting",
                StartTime = today.AddHours(15),
                EndTime = today.AddHours(15).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1013",
                PatientId = "PT-1372",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Care Plan Review",
                Status = "Scheduled",
                StartTime = today.AddHours(16),
                EndTime = today.AddHours(16).AddMinutes(45)
            },
            new()
            {
                AppointmentId = "SA-1014",
                PatientId = "PT-1453",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Follow-up Consultation",
                Status = "Scheduled",
                StartTime = today.AddHours(17),
                EndTime = today.AddHours(17).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1025",
                PatientId = "PT-1237",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Care Plan Follow-up",
                Status = "Scheduled",
                StartTime = today.AddDays(3).AddHours(10),
                EndTime = today.AddDays(3).AddHours(10).AddMinutes(45),
                Notes = "Review care plan progress"
            },
            new()
            {
                AppointmentId = "SA-1026",
                PatientId = "PT-1372",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Routine Consultation",
                Status = "Scheduled",
                StartTime = today.AddDays(4).AddHours(14),
                EndTime = today.AddDays(4).AddHours(14).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1015",
                PatientId = "PT-1042",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Post Visit Review",
                Status = "Completed",
                StartTime = today.AddDays(-2).AddHours(9),
                EndTime = today.AddDays(-2).AddHours(9).AddMinutes(45)
            },
            new()
            {
                AppointmentId = "SA-1016",
                PatientId = "PT-1045",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Lab Results Review",
                Status = "Completed",
                StartTime = today.AddDays(-3).AddHours(10),
                EndTime = today.AddDays(-3).AddHours(10).AddMinutes(45)
            },
            new()
            {
                AppointmentId = "SA-1017",
                PatientId = "PT-0988",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Urgent Follow-up",
                Status = "Completed",
                IsUrgent = true,
                StartTime = today.AddDays(-4).AddHours(13),
                EndTime = today.AddDays(-4).AddHours(13).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1018",
                PatientId = "PT-1018",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Annual Physical",
                Status = "Completed",
                StartTime = today.AddDays(-5).AddHours(9),
                EndTime = today.AddDays(-5).AddHours(9).AddMinutes(45)
            },
            new()
            {
                AppointmentId = "SA-1019",
                PatientId = "PT-1101",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Migraine Review",
                Status = "Completed",
                StartTime = today.AddDays(-6).AddHours(11),
                EndTime = today.AddDays(-6).AddHours(11).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1020",
                PatientId = "PT-1156",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Thyroid Check",
                Status = "Completed",
                StartTime = today.AddDays(-7).AddHours(14),
                EndTime = today.AddDays(-7).AddHours(14).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1021",
                PatientId = "PT-1204",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Behavioral Health Follow-up",
                Status = "Completed",
                StartTime = today.AddDays(-8).AddHours(10),
                EndTime = today.AddDays(-8).AddHours(10).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1022",
                PatientId = "PT-1291",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Wellness Visit",
                Status = "Completed",
                StartTime = today.AddDays(-9).AddHours(15),
                EndTime = today.AddDays(-9).AddHours(15).AddMinutes(45)
            },
            new()
            {
                AppointmentId = "SA-1023",
                PatientId = "PT-1345",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "ECG Follow-up",
                Status = "Completed",
                StartTime = today.AddDays(-10).AddHours(11),
                EndTime = today.AddDays(-10).AddHours(11).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-1024",
                PatientId = "PT-1372",
                ClinicianId = "CL-01",
                PatientName = "",
                VisitType = "Routine Checkup",
                Status = "Completed",
                StartTime = today.AddDays(-11).AddHours(13),
                EndTime = today.AddDays(-11).AddHours(13).AddMinutes(45)
            },

            // Dr. J. Lee - today and this week
            new()
            {
                AppointmentId = "SA-2001",
                PatientId = "PT-1291",
                ClinicianId = "CL-02",
                PatientName = "",
                VisitType = "Cardiac Consultation",
                Status = "Scheduled",
                StartTime = today.AddHours(9).AddMinutes(30),
                EndTime = today.AddHours(10)
            },
            new()
            {
                AppointmentId = "SA-2002",
                PatientId = "PT-1345",
                ClinicianId = "CL-02",
                PatientName = "",
                VisitType = "ECG Review",
                Status = "Scheduled",
                StartTime = today.AddHours(11),
                EndTime = today.AddHours(11).AddMinutes(45),
                Notes = "Stage 3 CKD monitoring"
            },
            new()
            {
                AppointmentId = "SA-2003",
                PatientId = "PT-1453",
                ClinicianId = "CL-02",
                PatientName = "",
                VisitType = "Heart Failure Follow-up",
                Status = "Scheduled",
                IsUrgent = true,
                StartTime = today.AddHours(13).AddMinutes(30),
                EndTime = today.AddHours(14)
            },
            new()
            {
                AppointmentId = "SA-2004",
                PatientId = "PT-1237",
                ClinicianId = "CL-02",
                PatientName = "",
                VisitType = "Atrial Fibrillation Review",
                Status = "Waiting",
                StartTime = today.AddDays(1).AddHours(9),
                EndTime = today.AddDays(1).AddHours(9).AddMinutes(45)
            },
            new()
            {
                AppointmentId = "SA-2005",
                PatientId = "PT-1372",
                ClinicianId = "CL-02",
                PatientName = "",
                VisitType = "Routine Checkup",
                Status = "Completed",
                StartTime = today.AddDays(-1).AddHours(15),
                EndTime = today.AddDays(-1).AddHours(15).AddMinutes(30)
            },

            // Dr. R. Patel - previous visits and next follow-up
            new()
            {
                AppointmentId = "SA-3001",
                PatientId = "PT-1101",
                ClinicianId = "CL-03",
                PatientName = "",
                VisitType = "Diabetes Review",
                Status = "Completed",
                StartTime = today.AddDays(-7).AddHours(10),
                EndTime = today.AddDays(-7).AddHours(10).AddMinutes(45),
                Notes = "Medication response reviewed"
            },
            new()
            {
                AppointmentId = "SA-3002",
                PatientId = "PT-1156",
                ClinicianId = "CL-03",
                PatientName = "",
                VisitType = "Thyroid Follow-up",
                Status = "Scheduled",
                StartTime = today.AddDays(3).AddHours(14),
                EndTime = today.AddDays(3).AddHours(14).AddMinutes(30)
            },

            // Dr. A. Nguyen - previous visits and next appointment
            new()
            {
                AppointmentId = "SA-4001",
                PatientId = "PT-1204",
                ClinicianId = "CL-04",
                PatientName = "",
                VisitType = "Pediatric Review",
                Status = "Completed",
                StartTime = today.AddDays(-3).AddHours(9),
                EndTime = today.AddDays(-3).AddHours(9).AddMinutes(30)
            },
            new()
            {
                AppointmentId = "SA-4002",
                PatientId = "PT-1237",
                ClinicianId = "CL-04",
                PatientName = "",
                VisitType = "Follow-up Consultation",
                Status = "Waiting",
                StartTime = today.AddDays(4).AddHours(11),
                EndTime = today.AddDays(4).AddHours(11).AddMinutes(45)
            },

            // Dr. M. Davis - previous visits and next appointment
            new()
            {
                AppointmentId = "SA-5001",
                PatientId = "PT-1372",
                ClinicianId = "CL-05",
                PatientName = "",
                VisitType = "Family Medicine Review",
                Status = "Completed",
                StartTime = today.AddDays(-14).AddHours(13),
                EndTime = today.AddDays(-14).AddHours(13).AddMinutes(45)
            },
            new()
            {
                AppointmentId = "SA-5002",
                PatientId = "PT-1453",
                ClinicianId = "CL-05",
                PatientName = "",
                VisitType = "Routine Checkup",
                Status = "Scheduled",
                StartTime = today.AddDays(5).AddHours(10),
                EndTime = today.AddDays(5).AddHours(10).AddMinutes(30)
            }
        ];

        // Add Wilson's completed history so previous-day scheduler views and dashboard charts are meaningful.
        AddHistoricalAppointments(appointments, today, -1, 3, "SA-H1");
        AddHistoricalAppointments(appointments, today, -2, 6, "SA-H2");
        AddHistoricalAppointments(appointments, today, -3, 4, "SA-H3");
        AddHistoricalAppointments(appointments, today, -4, 11, "SA-H4");
        AddHistoricalAppointments(appointments, today, -5, 7, "SA-H5");
        AddFutureAppointments(appointments, today, 1, 4, "SA-F1");
        AddFutureAppointments(appointments, today, 2, 8, "SA-F2");
        AddFutureAppointments(appointments, today, 3, 9, "SA-F3");
        AddFutureAppointments(appointments, today, 4, 12, "SA-F4");

        return appointments;
    }

    private static void AddHistoricalAppointments(
        List<ScheduleAppointment> appointments, DateTime today, int dayOffset, int count, string idPrefix)
    {
        var patientIds = new[]
        {
            "PT-1042", "PT-1045", "PT-0988", "PT-1018", "PT-1101",
            "PT-1156", "PT-1204", "PT-1291", "PT-1345", "PT-1372",
            "PT-1453", "PT-1237"
        };

        for (var index = 0; index < count; index++)
        {
            var startTime = today.AddDays(dayOffset).AddHours(8 + (index % 9));
            appointments.Add(new ScheduleAppointment
            {
                AppointmentId = $"{idPrefix}-{index + 1:00}",
                PatientId = patientIds[index % patientIds.Length],
                ClinicianId = "CL-01",
                VisitType = index % 2 == 0 ? "Follow-up Visit" : "Routine Consultation",
                Status = "Completed",
                StartTime = startTime,
                EndTime = startTime.AddMinutes(30)
            });
        }
    }

    private static void AddFutureAppointments(
        List<ScheduleAppointment> appointments, DateTime today, int dayOffset, int targetCount, string idPrefix)
    {
        var patientIds = new[]
        {
            "PT-1042", "PT-1045", "PT-0988", "PT-1018", "PT-1101",
            "PT-1156", "PT-1204", "PT-1291", "PT-1345", "PT-1372",
            "PT-1453", "PT-1237"
        };
        var appointmentDate = today.AddDays(dayOffset).Date;
        var existingCount = appointments.Count(appointment => appointment.StartTime.Date == appointmentDate && appointment.ClinicianId == "CL-01");

        for (var index = existingCount; index < targetCount; index++)
        {
            var startTime = appointmentDate.AddHours(8).AddMinutes(index * 45);
            while (appointments.Any(appointment => appointment.ClinicianId == "CL-01" && appointment.StartTime == startTime))
            {
                startTime = startTime.AddMinutes(15);
            }

            appointments.Add(new ScheduleAppointment
            {
                AppointmentId = $"{idPrefix}-{index + 1:00}",
                PatientId = patientIds[index % patientIds.Length],
                ClinicianId = "CL-01",
                VisitType = index % 2 == 0 ? "Follow-up Visit" : "Routine Consultation",
                Status = "Scheduled",
                StartTime = startTime,
                EndTime = startTime.AddMinutes(30)
            });
        }
    }
}
