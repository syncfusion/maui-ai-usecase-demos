using System;
using System.Collections.Generic;
using System.Text;

namespace PatientCompanion.Models;

public class DoctorAvailability
{
    public string DoctorId { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public List<string> AvailableSlots { get; set; } = new();
}
