using System;
using System.Collections.Generic;
using System.Text;

using PatientCompanion.Models;

namespace PatientCompanion.Services;

public class MockDataService
{
    private readonly Random _random = new();

    public Patient GetPatient()
    {
        return new Patient
        {
            PatientId = "MF-2094-11",
            Name = "Natrayan Ramalingam",
            Age = 27,
            Gender = "Male",
            BloodGroup = "O+",
            Phone = "+91 9876543210",
            Email = "natrayan@mediflow.com",
            MedicalConditions = new List<string>
        {
            "Mild Hypertension",
            "Vitamin D Deficiency"
        }
        };
    }

    public List<Doctor> GetDoctors()
    {
        return new List<Doctor>
    {
        // Cardiology

        new Doctor
        {
            DoctorId = "DOC001",
            Name = "Dr. Robert Miller",
            Specialization = "Cardiology",
            Experience = "12 Years",
            Rating = 4.8,
            HospitalName = "Central Medical Clinic",
            Address = "1200 Health Way",
            PhoneNumber = "555-1111",
            Email = "robert.miller@clinic.com",
            Biography = "Experienced Cardiologist",
            Latitude = 0,
            Longitude = 0,
            Image = "robert_miller.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        },

        new Doctor
        {
            DoctorId = "DOC002",
            Name = "Dr. Sarah Jenkins",
            Specialization = "Cardiology",
            Experience = "9 Years",
            Rating = 4.9,
            HospitalName = "Central Medical Clinic",
            Address = "1200 Health Way",
            PhoneNumber = "555-2222",
            Email = "sarah.jenkins@clinic.com",
            Biography = "Cardiology Specialist",
            Latitude = 0,
            Longitude = 0,
            Image = "sarah_jenkins.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        },

        // Dermatology

        new Doctor
        {
            DoctorId = "DOC003",
            Name = "Dr. Emily Wilson",
            Specialization = "Dermatology",
            Experience = "10 Years",
            Rating = 4.7,
            HospitalName = "Skin Care Center",
            Address = "45 Main Street",
            PhoneNumber = "555-3333",
            Email = "emily.wilson@clinic.com",
            Biography = "Dermatology Specialist",
            Latitude = 0,
            Longitude = 0,
            Image = "emily_wilson.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        },

        new Doctor
        {
            DoctorId = "DOC004",
            Name = "Dr. David Brown",
            Specialization = "Dermatology",
            Experience = "8 Years",
            Rating = 4.8,
            HospitalName = "Skin Care Center",
            Address = "45 Main Street",
            PhoneNumber = "555-4444",
            Email = "david.brown@clinic.com",
            Biography = "Skin Care Specialist",
            Latitude = 0,
            Longitude = 0,
            Image = "david_brown.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        },

        // Pediatrics

        new Doctor
        {
            DoctorId = "DOC005",
            Name = "Dr. Lisa Taylor",
            Specialization = "Pediatrics",
            Experience = "11 Years",
            Rating = 4.9,
            HospitalName = "Children's Hospital",
            Address = "Child Care Avenue",
            PhoneNumber = "555-5555",
            Email = "lisa.taylor@clinic.com",
            Biography = "Pediatric Specialist",
            Latitude = 0,
            Longitude = 0,
            Image = "lisa_taylor.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        },

        new Doctor
        {
            DoctorId = "DOC006",
            Name = "Dr. Michael Scott",
            Specialization = "Pediatrics",
            Experience = "7 Years",
            Rating = 4.8,
            HospitalName = "Children's Hospital",
            Address = "Child Care Avenue",
            PhoneNumber = "555-6666",
            Email = "michael.scott@clinic.com",
            Biography = "Child Health Specialist",
            Latitude = 0,
            Longitude = 0,
            Image = "michael_scott.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        },

        // Neurology

        new Doctor
        {
            DoctorId = "DOC007",
            Name = "Dr. James Anderson",
            Specialization = "Neurology",
            Experience = "15 Years",
            Rating = 4.9,
            HospitalName = "Neuro Medical Center",
            Address = "Brain Care Road",
            PhoneNumber = "555-7777",
            Email = "james.anderson@clinic.com",
            Biography = "Neurology Specialist",
            Latitude = 0,
            Longitude = 0,
            Image = "james_anderson.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        },

        new Doctor
        {
            DoctorId = "DOC008",
            Name = "Dr. Olivia Martin",
            Specialization = "Neurology",
            Experience = "8 Years",
            Rating = 4.8,
            HospitalName = "Neuro Medical Center",
            Address = "Brain Care Road",
            PhoneNumber = "555-8888",
            Email = "olivia.martin@clinic.com",
            Biography = "Brain and Nerve Specialist",
            Latitude = 0,
            Longitude = 0,
            Image = "olivia_martin.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        },

        // Ophthalmology

        new Doctor
        {
            DoctorId = "DOC009",
            Name = "Dr. William Clark",
            Specialization = "Ophthalmology",
            Experience = "13 Years",
            Rating = 4.8,
            HospitalName = "Vision Care Center",
            Address = "Eye Care Street",
            PhoneNumber = "555-9999",
            Email = "william.clark@clinic.com",
            Biography = "Eye Care Specialist",
            Latitude = 0,
            Longitude = 0,
            Image = "william_clark.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        },

        new Doctor
        {
            DoctorId = "DOC010",
            Name = "Dr. Emma Roberts",
            Specialization = "Ophthalmology",
            Experience = "10 Years",
            Rating = 4.9,
            HospitalName = "Vision Care Center",
            Address = "Eye Care Street",
            PhoneNumber = "555-1010",
            Email = "emma.roberts@clinic.com",
            Biography = "Ophthalmology Specialist",
            Latitude = 0,
            Longitude = 0,
            Image = "emma_roberts.png",
            AvailableSlots = new List<string>
            {
                "Today, 2:00 PM",
                "Tomorrow, 9:30 AM"
            }
        }
    };
    }

    public List<Appointment> GetAppointments()
    {
        var doctors = GetDoctors();

        return new List<Appointment>
        {
            // Upcoming Appointments

            new Appointment
            {
                AppointmentId = "APT1001",
                DoctorName = doctors[0].Name,
                Specialization = doctors[0].Specialization,
                Hospital = doctors[0].HospitalName,
                Date = DateTime.Today.AddDays(2),
                Time = "10:00 AM",
                Status = "Upcoming",
                Notes = "Routine heart health review."
            },

            new Appointment
            {
                AppointmentId = "APT1002",
                DoctorName = doctors[1].Name,
                Specialization = doctors[1].Specialization,
                Hospital = doctors[1].HospitalName,
                Date = DateTime.Today.AddDays(5),
                Time = "11:00 AM",
                Status = "Upcoming",
                Notes = "Neurology follow-up consultation."
            },

            new Appointment
            {
                AppointmentId = "APT1003",
                DoctorName = doctors[2].Name,
                Specialization = doctors[2].Specialization,
                Hospital = doctors[2].HospitalName,
                Date = DateTime.Today.AddDays(8),
                Time = "02:00 PM",
                Status = "Upcoming",
                Notes = "Skin allergy assessment."
            },

            new Appointment
            {
                AppointmentId = "APT1004",
                DoctorName = doctors[3].Name,
                Specialization = doctors[3].Specialization,
                Hospital = doctors[3].HospitalName,
                Date = DateTime.Today.AddDays(10),
                Time = "03:00 PM",
                Status = "Upcoming",
                Notes = "Knee joint evaluation."
            },

            new Appointment
            {
                AppointmentId = "APT1005",
                DoctorName = doctors[4].Name,
                Specialization = doctors[4].Specialization,
                Hospital = doctors[4].HospitalName,
                Date = DateTime.Today.AddDays(14),
                Time = "04:00 PM",
                Status = "Upcoming",
                Notes = "General health review."
            },

            // Past Appointments

            new Appointment
            {
                AppointmentId = "APT1006",
                DoctorName = doctors[0].Name,
                Specialization = doctors[0].Specialization,
                Hospital = doctors[0].HospitalName,
                Date = DateTime.Today.AddDays(-7),
                Time = "09:00 AM",
                Status = "Completed",
                Notes = "Blood pressure review."
            },

            new Appointment
            {
                AppointmentId = "APT1007",
                DoctorName = doctors[1].Name,
                Specialization = doctors[1].Specialization,
                Hospital = doctors[1].HospitalName,
                Date = DateTime.Today.AddDays(-14),
                Time = "11:00 AM",
                Status = "Completed",
                Notes = "Neurological assessment."
            },

            new Appointment
            {
                AppointmentId = "APT1008",
                DoctorName = doctors[2].Name,
                Specialization = doctors[2].Specialization,
                Hospital = doctors[2].HospitalName,
                Date = DateTime.Today.AddDays(-21),
                Time = "03:00 PM",
                Status = "Completed",
                Notes = "Dermatology consultation."
            },

            new Appointment
            {
                AppointmentId = "APT1009",
                DoctorName = doctors[3].Name,
                Specialization = doctors[3].Specialization,
                Hospital = doctors[3].HospitalName,
                Date = DateTime.Today.AddDays(-28),
                Time = "12:00 PM",
                Status = "Completed",
                Notes = "Orthopedic review."
            },

            new Appointment
            {
                AppointmentId = "APT1010",
                DoctorName = doctors[4].Name,
                Specialization = doctors[4].Specialization,
                Hospital = doctors[4].HospitalName,
                Date = DateTime.Today.AddDays(-35),
                Time = "10:00 AM",
                Status = "Completed",
                Notes = "Physical health assessment."
            },

            new Appointment
            {
                AppointmentId = "APT1011",
                DoctorName = doctors[0].Name,
                Specialization = doctors[0].Specialization,
                Hospital = doctors[0].HospitalName,
                Date = DateTime.Today.AddDays(-42),
                Time = "09:00 AM",
                Status = "Completed",
                Notes = "Follow-up consultation."
            },

            new Appointment
            {
                AppointmentId = "APT1012",
                DoctorName = doctors[1].Name,
                Specialization = doctors[1].Specialization,
                Hospital = doctors[1].HospitalName,
                Date = DateTime.Today.AddDays(-49),
                Time = "11:00 AM",
                Status = "Completed",
                Notes = "Neurology review."
            },

            new Appointment
            {
                AppointmentId = "APT1013",
                DoctorName = doctors[2].Name,
                Specialization = doctors[2].Specialization,
                Hospital = doctors[2].HospitalName,
                Date = DateTime.Today.AddDays(-56),
                Time = "02:00 PM",
                Status = "Completed",
                Notes = "Skin progress evaluation."
            },

            new Appointment
            {
                AppointmentId = "APT1014",
                DoctorName = doctors[3].Name,
                Specialization = doctors[3].Specialization,
                Hospital = doctors[3].HospitalName,
                Date = DateTime.Today.AddDays(-63),
                Time = "03:00 PM",
                Status = "Completed",
                Notes = "Orthopedic follow-up."
            },

            new Appointment
            {
                AppointmentId = "APT1015",
                DoctorName = doctors[4].Name,
                Specialization = doctors[4].Specialization,
                Hospital = doctors[4].HospitalName,
                Date = DateTime.Today.AddDays(-70),
                Time = "04:00 PM",
                Status = "Completed",
                Notes = "Annual wellness consultation."
            }
        };
    }

    public List<Medication> GetMedications()
    {
        return new List<Medication>
        {
new Medication
{
    Name = "Metformin",
    Dosage = "500mg",
    Frequency = "Morning",
    Time = "08:00 AM",
    Instructions = "Take one tablet after breakfast with a full glass of water.",
    StartDate = DateTime.Today.AddMonths(-3),
    EndDate = DateTime.Today.AddMonths(3),
    IsTaken = true
},

new Medication
{
    Name = "Lisinopril",
    Dosage = "10mg",
    Frequency = "Afternoon",
    Time = "02:00 PM",
    Instructions = "Take one tablet after lunch. Avoid taking it on an empty stomach.",
    StartDate = DateTime.Today.AddMonths(-2),
    EndDate = DateTime.Today.AddMonths(2),
    IsTaken = false
},

new Medication
{
    Name = "Vitamin D3",
    Dosage = "2000 IU",
    Frequency = "Afternoon",
    Time = "04:00 PM",
    Instructions = "Take one capsule with food and drink plenty of water throughout the day.",
    StartDate = DateTime.Today.AddMonths(-1),
    EndDate = DateTime.Today.AddMonths(4),
    IsTaken = false
},
new Medication
{
    Name = "Atorvastatin",
    Dosage = "20mg",
    Frequency = "Evening",
    Time = "08:00 PM",
    Instructions = "Take one tablet before bedtime as prescribed by your healthcare provider.",
    StartDate = DateTime.Today.AddMonths(-6),
    EndDate = DateTime.Today.AddMonths(6),
    IsTaken = false
}
        };
    }

    public List<MedicalRecord> GetMedicalRecords()
    {
        return new List<MedicalRecord>
        {
            new MedicalRecord
            {
                RecordId = "REC001",
                Title = "Comprehensive Blood Report",
                Category = "Blood Reports",
                RecordDate = DateTime.Today.AddMonths(-6),
                Description = "Complete blood count, lipid profile and metabolic screening."
            },

            new MedicalRecord
            {
                RecordId = "REC002",
                Title = "ECG Report",
                Category = "Cardiology",
                RecordDate = DateTime.Today.AddMonths(-5),
                Description = "Electrocardiogram findings and clinical summary."
            },

            new MedicalRecord
            {
                RecordId = "REC003",
                Title = "MRI Scan Report",
                Category = "Radiology",
                RecordDate = DateTime.Today.AddMonths(-4),
                Description = "Brain MRI scan evaluation and observations."
            },

            new MedicalRecord
            {
                RecordId = "REC004",
                Title = "Vaccination Report",
                Category = "Vaccination",
                RecordDate = DateTime.Today.AddMonths(-3),
                Description = "Vaccination history and administered doses."
            },

            new MedicalRecord
            {
                RecordId = "REC005",
                Title = "Annual Health Checkup Report",
                Category = "Health Checkup",
                RecordDate = DateTime.Today.AddMonths(-1),
                Description = "Comprehensive annual wellness assessment."
            }
        };
    }

    public List<HealthMetric> GetHealthMetrics()
    {
        var metrics = new List<HealthMetric>();

        for (int i = 89; i >= 0; i--)
        {
            metrics.Add(new HealthMetric
            {
                Date = DateTime.Today.AddDays(-i),
                SystolicPressure = _random.Next(118, 131),
                DiastolicPressure = _random.Next(76, 86),
                HeartRate = _random.Next(68, 85),
                SpO2 = _random.Next(97, 100),
                Weight = Math.Round(75 + _random.NextDouble() * 2, 1)
            });
        }

        return metrics;
    }

    public VitalSummary GetVitalSummary()
    {
        return new VitalSummary
        {
            HealthScore = 88,
            SystolicPressure = 124,
            DiastolicPressure = 82,
            HeartRate = 74,
            SpO2 = 98,
            Weight = 76.2
        };
    }

    private static List<string> GetDefaultSlots()
    {
        return new List<string>
        {
            "09:00 AM",
            "10:00 AM",
            "11:00 AM",
            "12:00 PM",
            "02:00 PM",
            "03:00 PM",
            "04:00 PM",
            "05:00 PM"
        };
    }

    public List<Appointment> GetVisitPageAppointments()
    {
        var now = DateTime.Now;

        return new()
    {
        new Appointment
        {
            AppointmentId = "1",
            DoctorName = "Dr. Sarah Jenkins",
            Specialization = "Cardiology",
            Status = "Confirmed",
            VisitType = "Video Visit",
            Date = now.AddDays(3),
            Time = now.AddDays(3).ToString("hh:mm tt")
        },

        new Appointment
        {
            AppointmentId = "2",
            DoctorName = "Dr. Mark Roberts",
            Specialization = "Dermatology",
            Status = "Pending",
            VisitType = "Clinic Visit",
            Date = now.AddDays(10),
            Time = now.AddDays(10).ToString("hh:mm tt")
        },

        new Appointment
        {
            AppointmentId = "3",
            DoctorName = "Dr. Emily Watson",
            Specialization = "Neurology",
            Status = "Completed",
            VisitType = "Video Visit",
            Date = now.AddDays(-5),
            Time = now.AddDays(-5).ToString("hh:mm tt")
        },

        new Appointment
        {
            AppointmentId = "4",
            DoctorName = "Dr. James Wilson",
            Specialization = "Orthopedics",
            Status = "Completed",
            VisitType = "Clinic Visit",
            Date = now.AddDays(-14),
            Time = now.AddDays(-14).ToString("hh:mm tt")
        }
    };
    }
}
