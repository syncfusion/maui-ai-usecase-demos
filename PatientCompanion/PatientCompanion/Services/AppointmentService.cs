using PatientCompanion.Models;
using System.Text.Json;

namespace PatientCompanion.Services;

public class AppointmentService
{
    private const string StorageKey = "mediflow.appointments";
    private readonly List<AppointmentBooking> appointments = new();
    private readonly MockDataService mockDataService;
    private AppointmentBooking? pendingReschedule;

    public AppointmentService(MockDataService mockDataService)
    {
        this.mockDataService = mockDataService;

        LoadAppointments();

        if (HasUpcomingAppointment())
            return;

        var doctors = mockDataService.GetDoctors();
        var now = DateTime.Now;

        AddDefaultAppointment(doctors[0], now.Date.AddDays(1).AddHours(10), "Clinic Visit");
        AddDefaultAppointment(doctors[1], now.Date.AddDays(3).AddHours(14), "Video Visit");
        AddDefaultAppointment(doctors[2], now.Date.AddDays(-7).AddHours(11), "Clinic Visit", "Completed");
        AddDefaultAppointment(doctors[3], now.Date.AddDays(-21).AddHours(15), "Video Visit", "Completed");
        Persist();
    }

    private bool HasUpcomingAppointment()
    {
        return appointments.Any(item =>
            item.Status is "Upcoming" or "Confirmed" &&
            item.ScheduledDateTime >= DateTime.Now);
    }

    public event Action? OnAppointmentsChanged;

    public void AddAppointment(AppointmentBooking booking)
    {
        booking.VisitType = booking.VisitType == AppointmentBooking.VideoVisit
            ? AppointmentBooking.VideoVisit
            : AppointmentBooking.ClinicVisit;
        booking.Status = "Confirmed";
        var existing = appointments.FirstOrDefault(item => item.AppointmentId == booking.AppointmentId);
        if (existing == null)
            appointments.Add(booking);

        Persist();
        OnAppointmentsChanged?.Invoke();
    }

    public void RequestReschedule(string appointmentId)
    {
        pendingReschedule = appointments.FirstOrDefault(item => item.AppointmentId == appointmentId);
    }

    public AppointmentBooking? ConsumePendingReschedule()
    {
        var booking = pendingReschedule;
        pendingReschedule = null;
        return booking;
    }

    public IReadOnlyList<AppointmentBooking> GetAppointments()
    {
        return appointments.ToList();
    }

    public void CancelAppointment(string appointmentId)
    {
        var appointment = appointments.FirstOrDefault(item => item.AppointmentId == appointmentId);
        if (appointment == null)
            return;

        appointments.Remove(appointment);
        Persist();
        OnAppointmentsChanged?.Invoke();
    }

    public void RescheduleAppointment(string appointmentId, DateTime date, string timeSlot)
    {
        var appointment = appointments.FirstOrDefault(item => item.AppointmentId == appointmentId);
        if (appointment == null)
            return;

        appointment.SelectedDate = date;
        appointment.SelectedTimeSlot = timeSlot;
        appointment.Status = "Confirmed";
        Persist();
        OnAppointmentsChanged?.Invoke();
    }

    public void Clear()
    {
        appointments.Clear();

        Persist();
        OnAppointmentsChanged?.Invoke();
    }

    private bool LoadAppointments()
    {
        var json = Preferences.Default.Get(StorageKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            var records = JsonSerializer.Deserialize<List<StoredAppointment>>(json);
            if (records == null)
                return false;

            var doctors = mockDataService.GetDoctors();
            foreach (var record in records)
            {
                var doctor = doctors.FirstOrDefault(item => item.DoctorId == record.DoctorId);
                if (doctor == null)
                    continue;

                appointments.Add(new AppointmentBooking
                {
                    AppointmentId = record.AppointmentId,
                    SelectedDoctor = doctor,
                    SelectedSpeciality = new SpecialtyItem(doctor.Specialization, "heart.png"),
                    VisitType = record.VisitType,
                    SelectedDate = record.SelectedDate,
                    SelectedTimeSlot = record.SelectedTimeSlot,
                    Status = record.Status,
                    CreatedOn = record.CreatedOn
                });
            }

            return appointments.Count > 0;
        }
        catch (JsonException)
        {
            Preferences.Default.Remove(StorageKey);
            return false;
        }
    }

    private void Persist()
    {
        var records = appointments.Select(item => new StoredAppointment
        {
            AppointmentId = item.AppointmentId,
            DoctorId = item.SelectedDoctor?.DoctorId ?? string.Empty,
            VisitType = item.VisitType,
            SelectedDate = item.SelectedDate,
            SelectedTimeSlot = item.SelectedTimeSlot,
            Status = item.Status,
            CreatedOn = item.CreatedOn
        });

        Preferences.Default.Set(StorageKey, JsonSerializer.Serialize(records));
    }

    private sealed class StoredAppointment
    {
        public string AppointmentId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string VisitType { get; set; } = string.Empty;
        public DateTime SelectedDate { get; set; }
        public string SelectedTimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    private void AddDefaultAppointment(
        Doctor doctor,
        DateTime dateTime,
        string visitType,
        string status = "Confirmed")
    {
        appointments.Add(new AppointmentBooking
        {
            SelectedDoctor = doctor,
            SelectedSpeciality = new SpecialtyItem(doctor.Specialization, "heart.png"),
            VisitType = visitType,
            SelectedDate = dateTime.Date,
            SelectedTimeSlot = dateTime.ToString("h:mm tt"),
            Status = status
        });
    }
}