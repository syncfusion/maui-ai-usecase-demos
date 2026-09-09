using MediFlowSample.Models;

namespace MediFlowSample.Services;

/// <summary>Local, in-memory care task data. All content is static/templated sample data — no AI or network calls.</summary>
public sealed class CarePlanDataService
{
    public static readonly string[] StageNames = ["Assessed", "Planned", "Active", "Follow-up"];

    public IReadOnlyList<CareTask> GetTasks(Patient patient)
    {
        if (patient.PatientId == "PT-1042")
        {
            return
            [
                new CareTask
                {
                    TaskId = "CT-1042-1",
                    PatientId = patient.PatientId,
                    Title = "Record Blood Pressure",
                    Description = "Daily monitoring required.",
                    Priority = "High",
                    DueDisplay = "Due Today, 5:00 PM",
                    Status = "In Progress"
                },
                new CareTask
                {
                    TaskId = "CT-1042-2",
                    PatientId = patient.PatientId,
                    Title = "Review Lab Results",
                    Description = "Complete Blood Count (CBC) panel.",
                    DueDisplay = "Tomorrow",
                    Status = "Planned"
                },
                new CareTask
                {
                    TaskId = "CT-1042-3",
                    PatientId = patient.PatientId,
                    Title = "Confirm medication adherence",
                    Description = "Metformin 500mg schedule reviewed with patient.",
                    DueDisplay = "Completed",
                    Status = "Completed"
                }
            ];
        }

        // Generic templated tasks for every other patient, derived from their existing record only.
        return
        [
            new CareTask
            {
                TaskId = $"{patient.PatientId}-1",
                PatientId = patient.PatientId,
                Title = $"Monitor {patient.PrimaryCondition}",
                Description = "Daily monitoring required.",
                Priority = patient.IsHighPriority ? "High" : "Normal",
                DueDisplay = "Due Today, 5:00 PM",
                Status = "In Progress"
            },
            new CareTask
            {
                TaskId = $"{patient.PatientId}-2",
                PatientId = patient.PatientId,
                Title = "Review Lab Results",
                Description = "Routine panel due for renewal.",
                DueDisplay = "Tomorrow",
                Status = "Planned"
            },
            new CareTask
            {
                TaskId = $"{patient.PatientId}-3",
                PatientId = patient.PatientId,
                Title = "Confirm medication adherence",
                Description = "Reviewed prescribed medications with patient.",
                DueDisplay = "Completed",
                Status = "Completed"
            }
        ];
    }

    /// <summary>0=Assessed, 1=Planned, 2=Active, 3=Follow-up — derived from the patient's existing CarePlanProgress value.</summary>
    public int GetStageIndex(Patient patient) => patient.CarePlanProgress switch
    {
        >= 90 => 3,
        >= 55 => 2,
        >= 25 => 1,
        _ => 0
    };

    /// <summary>Builds a templated, non-AI summary sentence from the patient's own record and current tasks.</summary>
    public (string Intro, string Action) GetAssistantSummary(Patient patient, IReadOnlyList<CareTask> tasks)
    {
        var pronoun = patient.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase) ? "her" : "his";
        var intro = $"{patient.FirstName} is progressing well with {pronoun} {patient.PrimaryCondition.ToLowerInvariant()} management plan. Medication adherence is confirmed.";

        var inProgress = tasks.FirstOrDefault(task => task.Status == "In Progress");
        var action = inProgress is null
            ? "No outstanding action items."
            : $"{inProgress.Title} {inProgress.DueDisplay.ToLowerInvariant()}.";

        return (intro, action);
    }
}
