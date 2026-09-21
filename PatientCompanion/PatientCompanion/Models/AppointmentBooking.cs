using System;
using PatientCompanion.Models;

namespace PatientCompanion.Models;

public class AppointmentBooking
{
    public const string ClinicVisit = "Clinic Visit";
    public const string VideoVisit = "Video Visit";

    public string AppointmentId { get; set; }
        = Guid.NewGuid().ToString();

    // Step 1
    public SpecialtyItem? SelectedSpeciality { get; set; }

    // Step 2
    public Doctor? SelectedDoctor { get; set; }

    // Step 3
    public string VisitType { get; set; } = ClinicVisit;

    public DateTime SelectedDate { get; set; }

    public string SelectedTimeSlot { get; set; } = string.Empty;

    // Appointment Status
    public string Status { get; set; } = "Upcoming";

    public DateTime CreatedOn { get; set; }
        = DateTime.Now;

    // Convenience Properties

    public string DoctorName =>
        SelectedDoctor?.Name ?? string.Empty;

    public string DoctorImage =>
        SelectedDoctor?.Image ?? "patient_avatar.png";

    public string SpecialityName =>
        SelectedSpeciality?.Name ?? string.Empty;

    public string HospitalName =>
        SelectedDoctor?.HospitalName ?? string.Empty;

    public string Location =>
        SelectedDoctor?.Address ?? string.Empty;

    public string AppointmentDateTime =>
        string.IsNullOrWhiteSpace(SelectedTimeSlot)
            ? string.Empty
            : $"{SelectedDate:dddd, MMM dd, yyyy} at {SelectedTimeSlot}";

    public DateTime ScheduledDateTime
    {
        get
        {
            if (DateTime.TryParse(SelectedTimeSlot, out var time))
                return SelectedDate.Date.Add(time.TimeOfDay);

            return SelectedDate.Date;
        }
    }
}