using MediFlowSample.Models;

namespace MediFlowSample.Services;

public sealed class PatientDataService
{
    private readonly List<Patient> patients;

    public PatientDataService()
    {
        patients = CreatePatients();
    }

    public IReadOnlyList<Patient> GetPatients() => patients;

    public void UpdateAppointmentDates(IEnumerable<ScheduleAppointment> appointments)
    {
        foreach (var patient in patients)
        {
            var patientAppointments = appointments
                .Where(appointment => appointment.PatientId == patient.PatientId && !appointment.IsCancelled)
                .ToList();

            patient.LastVisitDate = patientAppointments
                .Where(appointment => appointment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                .Select(appointment => (DateTime?)appointment.EndTime)
                .OrderByDescending(date => date)
                .FirstOrDefault() ?? patient.LastVisitDate;

            patient.NextAppointmentDate = patientAppointments
                .Where(appointment => !appointment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                    && appointment.StartTime >= DateTime.Now)
                .Select(appointment => (DateTime?)appointment.StartTime)
                .OrderBy(date => date)
                .FirstOrDefault();
        }
    }

    public IReadOnlyList<TimelineActivity> GetRecentActivities(string patientId)
    {
        var today = DateTime.Today;

        if (patientId == "PT-1042")
        {
            return
            [
                new TimelineActivity
                {
                    ActivityId = "ACT-1",
                    PatientId = patientId,
                    Date = today.AddHours(9).AddMinutes(30),
                    Title = "Lab results received:",
                    Description = "Comprehensive Metabolic Panel. All values within normal range.",
                    IsLatest = true
                },
                new TimelineActivity
                {
                    ActivityId = "ACT-2",
                    PatientId = patientId,
                    Date = today.AddDays(-1).AddHours(14).AddMinutes(15),
                    Description = "Outpatient visit with Dr. Reynolds (Endocrinology)."
                },
                new TimelineActivity
                {
                    ActivityId = "ACT-3",
                    PatientId = patientId,
                    Date = today.AddDays(-9).AddHours(10),
                    Title = "Prescription refill requested:",
                    Description = "Metformin 500mg."
                }
            ];
        }

        var patient = patients.FirstOrDefault(p => p.PatientId == patientId);
        if (patient is null)
        {
            return [];
        }

        return
        [
            new TimelineActivity
            {
                ActivityId = $"{patientId}-A1",
                PatientId = patientId,
                Date = patient.LastVisitDate ?? today,
                Title = "Visit completed:",
                Description = $"Reviewed {patient.PrimaryCondition} and updated care plan.",
                IsLatest = true
            },
            new TimelineActivity
            {
                ActivityId = $"{patientId}-A2",
                PatientId = patientId,
                Date = (patient.LastVisitDate ?? today).AddDays(-12),
                Description = "Vitals recorded during routine check-in."
            }
        ];
    }

    private static List<Patient> CreatePatients()
    {
        var today = DateTime.Today;

        return
        [
            new Patient
            {
                PatientId = "PT-1042",
                FirstName = "Maya",
                LastName = "Patel",
                Age = 42,
                Gender = "Female",
                DateOfBirth = new DateTime(1981, 10, 14),
                PhoneNumber = "(555) 019-8472",
                Email = "maya.patel@example.com",
                Address = "212 Willow Ave",
                BloodGroup = "B+",
                Language = "English",
                PrimaryCondition = "Asthma",
                ConditionIconGlyph = "\uE3F3",
                Allergies = ["Pollen"],
                RiskLevel = "Low",
                StatusText = "Monitor",
                LastVisitDate = today.AddDays(-18),
                NextAppointmentDate = today.AddDays(-1).AddHours(10).AddMinutes(30),
                CarePlanProgress = 60,
                EmergencyContact = "Raj Patel (Spouse) - (555) 014-7724"
            },
            new Patient
            {
                PatientId = "PT-1045",
                FirstName = "Daniel",
                LastName = "Brooks",
                Age = 52,
                Gender = "Male",
                DateOfBirth = new DateTime(1973, 7, 2),
                PhoneNumber = "(555) 019-2231",
                Email = "daniel.brooks@example.com",
                Address = "48 Cedar Street",
                PrimaryCondition = "Hypertension",
                ConditionIconGlyph = "\uE87D",
                Allergies = [],
                RiskLevel = "Medium",
                StatusText = "Monitor",
                LastVisitDate = today.AddDays(-30),
                NextAppointmentDate = today.AddDays(2).AddHours(14).AddMinutes(30),
                CarePlanProgress = 85,
                EmergencyContact = "Linda Brooks (Spouse) - (555) 019-2232"
            },
            new Patient
            {
                PatientId = "PT-0988",
                FirstName = "Sophia",
                LastName = "Nguyen",
                Age = 61,
                Gender = "Female",
                DateOfBirth = new DateTime(1964, 11, 20),
                PhoneNumber = "(555) 019-8472",
                Email = "sophia.nguyen@example.com",
                Address = "77 Oakridge Blvd",
                PrimaryCondition = "Type 2 DM",
                ConditionIconGlyph = "\uE3F3",
                Allergies = ["Penicillin"],
                RiskLevel = "High",
                StatusText = "High Priority",
                LastVisitDate = today.AddDays(-5),
                NextAppointmentDate = null,
                NeedsReview = true,
                CarePlanProgress = 32,
                EmergencyContact = "Kevin Nguyen (Son) - (555) 019-8473"
            },
            new Patient
            {
                PatientId = "PT-1018",
                FirstName = "James",
                LastName = "Wilson",
                Age = 45,
                Gender = "Male",
                DateOfBirth = new DateTime(1980, 5, 9),
                PhoneNumber = "(555) 022-1187",
                Email = "james.wilson@example.com",
                Address = "9 Harbor Lane",
                PrimaryCondition = "Coronary Artery Disease",
                ConditionIconGlyph = "\uE87D",
                Allergies = ["Sulfa drugs"],
                RiskLevel = "High",
                StatusText = "High Priority",
                LastVisitDate = today.AddDays(-2),
                NextAppointmentDate = today.AddDays(1).AddHours(9).AddMinutes(15),
                CarePlanProgress = 48,
                EmergencyContact = "Anna Wilson (Spouse) - (555) 022-1188"
            },
            new Patient
            {
                PatientId = "PT-1101",
                FirstName = "Olivia",
                LastName = "Martinez",
                Age = 29,
                Gender = "Female",
                DateOfBirth = new DateTime(1996, 2, 27),
                PhoneNumber = "(555) 034-5521",
                Email = "olivia.martinez@example.com",
                Address = "134 Meadow Court",
                PrimaryCondition = "Migraine",
                ConditionIconGlyph = "\uE3F3",
                Allergies = [],
                RiskLevel = "Low",
                StatusText = "Stable",
                LastVisitDate = today.AddDays(-40),
                NextAppointmentDate = today.AddDays(6).AddHours(13),
                CarePlanProgress = 92,
                EmergencyContact = "Carlos Martinez (Brother) - (555) 034-5522"
            },
            new Patient
            {
                PatientId = "PT-1122",
                FirstName = "Ethan",
                LastName = "Clark",
                Age = 67,
                Gender = "Male",
                DateOfBirth = new DateTime(1958, 9, 30),
                PhoneNumber = "(555) 041-9932",
                Email = "ethan.clark@example.com",
                Address = "56 Pinehill Road",
                PrimaryCondition = "COPD",
                ConditionIconGlyph = "\uE3F3",
                Allergies = ["Latex"],
                RiskLevel = "High",
                StatusText = "High Priority",
                LastVisitDate = today.AddDays(-1),
                NextAppointmentDate = null,
                NeedsReview = true,
                CarePlanProgress = 27,
                EmergencyContact = "Susan Clark (Spouse) - (555) 041-9933"
            },
            new Patient
            {
                PatientId = "PT-1156",
                FirstName = "Ava",
                LastName = "Robinson",
                Age = 38,
                Gender = "Female",
                DateOfBirth = new DateTime(1987, 12, 4),
                PhoneNumber = "(555) 051-3387",
                Email = "ava.robinson@example.com",
                Address = "22 Fountain Street",
                PrimaryCondition = "Hypothyroidism",
                ConditionIconGlyph = "\uE87D",
                Allergies = [],
                RiskLevel = "Low",
                StatusText = "Monitor",
                LastVisitDate = today.AddDays(-60),
                NextAppointmentDate = today.AddDays(9).AddHours(10),
                CarePlanProgress = 74,
                EmergencyContact = "Marcus Robinson (Spouse) - (555) 051-3388"
            },
            new Patient
            {
                PatientId = "PT-1189",
                FirstName = "Liam",
                LastName = "Turner",
                Age = 56,
                Gender = "Male",
                DateOfBirth = new DateTime(1969, 4, 18),
                PhoneNumber = "(555) 062-7743",
                Email = "liam.turner@example.com",
                Address = "310 Ridgeline Drive",
                PrimaryCondition = "Chronic Kidney Disease",
                ConditionIconGlyph = "\uE3F3",
                Allergies = ["Contrast dye"],
                RiskLevel = "Medium",
                StatusText = "Monitor",
                LastVisitDate = today.AddDays(-14),
                NextAppointmentDate = today.AddDays(3).AddHours(15).AddMinutes(30),
                CarePlanProgress = 55,
                EmergencyContact = "Grace Turner (Daughter) - (555) 062-7744"
            },
            new Patient
            {
                PatientId = "PT-1204",
                FirstName = "Isabella",
                LastName = "Scott",
                Age = 24,
                Gender = "Female",
                DateOfBirth = new DateTime(2001, 8, 15),
                PhoneNumber = "(555) 071-2245",
                Email = "isabella.scott@example.com",
                Address = "8 Aspen Way",
                PrimaryCondition = "Anxiety Disorder",
                ConditionIconGlyph = "\uE3F3",
                Allergies = [],
                RiskLevel = "Low",
                StatusText = "Stable",
                LastVisitDate = today.AddDays(-22),
                NextAppointmentDate = today.AddDays(11).AddHours(11),
                CarePlanProgress = 88,
                EmergencyContact = "Michael Scott (Father) - (555) 071-2246"
            },
            new Patient
            {
                PatientId = "PT-1237",
                FirstName = "Noah",
                LastName = "Baker",
                Age = 71,
                Gender = "Male",
                DateOfBirth = new DateTime(1954, 6, 6),
                PhoneNumber = "(555) 082-9915",
                Email = "noah.baker@example.com",
                Address = "45 Summit Terrace",
                PrimaryCondition = "Atrial Fibrillation",
                ConditionIconGlyph = "\uE87D",
                Allergies = ["Aspirin"],
                RiskLevel = "High",
                StatusText = "High Priority",
                LastVisitDate = today.AddDays(-3),
                NextAppointmentDate = today.AddDays(1).AddHours(8).AddMinutes(45),
                CarePlanProgress = 41,
                EmergencyContact = "Karen Baker (Spouse) - (555) 082-9916"
            },
            new Patient
            {
                PatientId = "PT-1265",
                FirstName = "Mia",
                LastName = "Foster",
                Age = 33,
                Gender = "Female",
                DateOfBirth = new DateTime(1992, 1, 23),
                PhoneNumber = "(555) 093-4471",
                Email = "mia.foster@example.com",
                Address = "17 Brookside Lane",
                PrimaryCondition = "Gestational Diabetes",
                ConditionIconGlyph = "\uE3F3",
                Allergies = [],
                RiskLevel = "Medium",
                StatusText = "Monitor",
                LastVisitDate = today.AddDays(-7),
                NextAppointmentDate = today.AddDays(4).AddHours(9).AddMinutes(30),
                CarePlanProgress = 66,
                EmergencyContact = "David Foster (Spouse) - (555) 093-4472"
            },
            new Patient
            {
                PatientId = "PT-1291",
                FirstName = "Lucas",
                LastName = "Bennett",
                Age = 48,
                Gender = "Male",
                DateOfBirth = new DateTime(1977, 10, 11),
                PhoneNumber = "(555) 104-2288",
                Email = "lucas.bennett@example.com",
                Address = "63 Lakeshore Drive",
                PrimaryCondition = "Osteoarthritis",
                ConditionIconGlyph = "\uE3F3",
                Allergies = ["Ibuprofen"],
                RiskLevel = "Low",
                StatusText = "Stable",
                LastVisitDate = today.AddDays(-45),
                NextAppointmentDate = today.AddDays(15).AddHours(13).AddMinutes(15),
                CarePlanProgress = 95,
                EmergencyContact = "Rachel Bennett (Spouse) - (555) 104-2289"
            },
            new Patient
            {
                PatientId = "PT-1318",
                FirstName = "Charlotte",
                LastName = "Ramirez",
                Age = 58,
                Gender = "Female",
                DateOfBirth = new DateTime(1967, 3, 3),
                PhoneNumber = "(555) 115-7762",
                Email = "charlotte.ramirez@example.com",
                Address = "29 Maple Grove",
                PrimaryCondition = "Breast Cancer (Remission)",
                ConditionIconGlyph = "\uE3F3",
                Allergies = [],
                RiskLevel = "Medium",
                StatusText = "Monitor",
                LastVisitDate = today.AddDays(-10),
                NextAppointmentDate = today.AddDays(5).AddHours(10).AddMinutes(45),
                CarePlanProgress = 70,
                EmergencyContact = "Jorge Ramirez (Spouse) - (555) 115-7763"
            },
            new Patient
            {
                PatientId = "PT-1345",
                FirstName = "Henry",
                LastName = "Diaz",
                Age = 63,
                Gender = "Male",
                DateOfBirth = new DateTime(1962, 7, 29),
                PhoneNumber = "(555) 126-3391",
                Email = "henry.diaz@example.com",
                Address = "5 Birchwood Court",
                PrimaryCondition = "Stage 3 CKD",
                ConditionIconGlyph = "\uE3F3",
                Allergies = ["Iodine"],
                RiskLevel = "High",
                StatusText = "High Priority",
                LastVisitDate = today.AddDays(-4),
                NextAppointmentDate = null,
                NeedsReview = true,
                CarePlanProgress = 38,
                EmergencyContact = "Elena Diaz (Daughter) - (555) 126-3392"
            },
            new Patient
            {
                PatientId = "PT-1372",
                FirstName = "Amelia",
                LastName = "Coleman",
                Age = 27,
                Gender = "Female",
                DateOfBirth = new DateTime(1998, 5, 17),
                PhoneNumber = "(555) 137-8827",
                Email = "amelia.coleman@example.com",
                Address = "88 Sycamore Street",
                PrimaryCondition = "Seasonal Allergies",
                ConditionIconGlyph = "\uE3F3",
                Allergies = ["Peanuts"],
                RiskLevel = "Low",
                StatusText = "Stable",
                LastVisitDate = today.AddDays(-50),
                NextAppointmentDate = today.AddDays(20).AddHours(14),
                CarePlanProgress = 100,
                EmergencyContact = "Nathan Coleman (Spouse) - (555) 137-8828"
            },
            new Patient
            {
                PatientId = "PT-1399",
                FirstName = "Benjamin",
                LastName = "Ward",
                Age = 55,
                Gender = "Male",
                DateOfBirth = new DateTime(1970, 12, 25),
                PhoneNumber = "(555) 148-4416",
                Email = "benjamin.ward@example.com",
                Address = "41 Chestnut Ave",
                PrimaryCondition = "Type 2 Diabetes",
                ConditionIconGlyph = "\uE3F3",
                Allergies = [],
                RiskLevel = "Medium",
                StatusText = "Monitor",
                LastVisitDate = today.AddDays(-12),
                NextAppointmentDate = today.AddDays(2).AddHours(11).AddMinutes(30),
                CarePlanProgress = 62,
                EmergencyContact = "Diane Ward (Spouse) - (555) 148-4417"
            },
            new Patient
            {
                PatientId = "PT-1426",
                FirstName = "Harper",
                LastName = "Simmons",
                Age = 41,
                Gender = "Female",
                DateOfBirth = new DateTime(1984, 9, 8),
                PhoneNumber = "(555) 159-1152",
                Email = "harper.simmons@example.com",
                Address = "12 Riverside Blvd",
                PrimaryCondition = "Rheumatoid Arthritis",
                ConditionIconGlyph = "\uE3F3",
                Allergies = ["Methotrexate"],
                RiskLevel = "Medium",
                StatusText = "Monitor",
                LastVisitDate = today.AddDays(-9),
                NextAppointmentDate = today.AddDays(7).AddHours(9),
                CarePlanProgress = 58,
                EmergencyContact = "Peter Simmons (Spouse) - (555) 159-1153"
            },
            new Patient
            {
                PatientId = "PT-1453",
                FirstName = "Sebastian",
                LastName = "Reed",
                Age = 66,
                Gender = "Male",
                DateOfBirth = new DateTime(1959, 2, 14),
                PhoneNumber = "(555) 170-6624",
                Email = "sebastian.reed@example.com",
                Address = "70 Hilltop Road",
                PrimaryCondition = "Congestive Heart Failure",
                ConditionIconGlyph = "\uE87D",
                Allergies = ["Beta blockers"],
                RiskLevel = "High",
                StatusText = "High Priority",
                LastVisitDate = today,
                NextAppointmentDate = null,
                NeedsReview = true,
                CarePlanProgress = 22,
                EmergencyContact = "Nora Reed (Spouse) - (555) 170-6625"
            },
            new Patient
            {
                PatientId = "PT-1480",
                FirstName = "Evelyn",
                LastName = "Bell",
                Age = 30,
                Gender = "Female",
                DateOfBirth = new DateTime(1995, 4, 2),
                PhoneNumber = "(555) 181-2298",
                Email = "evelyn.bell@example.com",
                Address = "19 Garden Path",
                PrimaryCondition = "Iron Deficiency Anemia",
                ConditionIconGlyph = "\uE3F3",
                Allergies = [],
                RiskLevel = "Low",
                StatusText = "Stable",
                LastVisitDate = today.AddDays(-35),
                NextAppointmentDate = today.AddDays(12).AddHours(10).AddMinutes(15),
                CarePlanProgress = 90,
                EmergencyContact = "Aaron Bell (Spouse) - (555) 181-2299"
            },
            new Patient
            {
                PatientId = "PT-1507",
                FirstName = "Jack",
                LastName = "Morgan",
                Age = 60,
                Gender = "Male",
                DateOfBirth = new DateTime(1965, 11, 5),
                PhoneNumber = "(555) 192-7761",
                Email = "jack.morgan@example.com",
                Address = "3 Orchard Street",
                PrimaryCondition = "Chronic Back Pain",
                ConditionIconGlyph = "\uE3F3",
                Allergies = [],
                RiskLevel = "Low",
                StatusText = "Monitor",
                LastVisitDate = today.AddDays(-25),
                NextAppointmentDate = today.AddDays(8).AddHours(13).AddMinutes(45),
                CarePlanProgress = 78,
                EmergencyContact = "Patricia Morgan (Spouse) - (555) 192-7762"
            }
        ];
    }
}
