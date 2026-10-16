using System;
using System.Collections.Generic;
using System.Text;

namespace PatientCompanion.Models;

public class Doctor
{
    public string DoctorId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public string Experience { get; set; } = string.Empty;

    public double Rating { get; set; }

    public string HospitalName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Biography { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string Image { get; set; } = string.Empty;

    public List<string> AvailableSlots { get; set; } = new();
}
