using System;
using System.Collections.Generic;
using System.Text;

namespace PatientCompanion.Models;
public class Patient
{
    public string PatientId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string BloodGroup { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<string> MedicalConditions { get; set; } = new();

    public string WelcomeText =>
        $"Good Morning, {Name.Split(' ')[0]}";
}
