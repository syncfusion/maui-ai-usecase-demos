using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using MediFlowSample.Models;
using MediFlowSample.Services;
using Syncfusion.Maui.ProgressBar;

namespace MediFlowSample.ViewModels;

public sealed class CarePlansViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly CarePlanDataService carePlanDataService;
    private readonly PatientDataService patientDataService;
    private bool isBusy;
    private string errorMessage = string.Empty;
    private Patient? selectedPatient;
    private int activeStepIndex;
    private string careStatusText = string.Empty;
    private string assistantIntro = string.Empty;
    private string assistantAction = string.Empty;
    private bool hasNoTasks;
    private bool hasNoInProgressTasks;
    private bool hasNoPlannedTasks;
    private bool hasNoCompletedTasks;

    public CarePlansViewModel(CarePlanDataService carePlanDataService, PatientDataService patientDataService)
    {
        this.carePlanDataService = carePlanDataService;
        this.patientDataService = patientDataService;

        CareStages = [.. CarePlanDataService.StageNames.Select(name => new StepProgressBarItem { PrimaryText = name })];
        DesktopCareStages = [.. CarePlanDataService.StageNames.Select(name => new StepProgressBarItem { PrimaryText = name, ProgressTrackSize = 140 })];

        SelectPatientCommand = new Command<Patient>(SelectPatient);
        RetryCommand = new Command(() => SelectPatient(SelectedPatient ?? Patients.FirstOrDefault()));

        Load();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsBusy
    {
        get => isBusy;
        private set => SetField(ref isBusy, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetField(ref errorMessage, value);
    }

    public Patient? SelectedPatient
    {
        get => selectedPatient;
        private set => SetField(ref selectedPatient, value);
    }

    public int ActiveStepIndex
    {
        get => activeStepIndex;
        private set => SetField(ref activeStepIndex, value);
    }

    public string CareStatusText
    {
        get => careStatusText;
        private set => SetField(ref careStatusText, value);
    }

    public string AssistantIntro
    {
        get => assistantIntro;
        private set => SetField(ref assistantIntro, value);
    }

    public string AssistantAction
    {
        get => assistantAction;
        private set => SetField(ref assistantAction, value);
    }

    public bool HasNoTasks
    {
        get => hasNoTasks;
        private set => SetField(ref hasNoTasks, value);
    }

    public bool HasNoInProgressTasks
    {
        get => hasNoInProgressTasks;
        private set => SetField(ref hasNoInProgressTasks, value);
    }

    public bool HasNoPlannedTasks
    {
        get => hasNoPlannedTasks;
        private set => SetField(ref hasNoPlannedTasks, value);
    }

    public bool HasNoCompletedTasks
    {
        get => hasNoCompletedTasks;
        private set => SetField(ref hasNoCompletedTasks, value);
    }

    public ObservableCollection<Patient> Patients { get; } = [];

    public ObservableCollection<StepProgressBarItem> CareStages { get; }

    public ObservableCollection<StepProgressBarItem> DesktopCareStages { get; }

    public ObservableCollection<CareTask> InProgressTasks { get; } = [];

    public ObservableCollection<CareTask> PlannedTasks { get; } = [];

    public ObservableCollection<CareTask> CompletedTasks { get; } = [];

    public Command<Patient> SelectPatientCommand { get; }

    public Command RetryCommand { get; }

    public void Refresh() => SelectPatient(SelectedPatient ?? Patients.FirstOrDefault());

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("patientId", out var value) && value is string patientId)
        {
            var patient = Patients.FirstOrDefault(item => item.PatientId == patientId);
            if (patient is not null)
            {
                SelectPatient(patient);
            }
        }
    }

    private void Load()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            Patients.Clear();
            foreach (var patient in patientDataService.GetPatients())
            {
                Patients.Add(patient);
            }

            var initialPatient = Patients.FirstOrDefault(patient => patient.PatientId == "PT-1042") ?? Patients.FirstOrDefault();
            SelectPatient(initialPatient);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SelectPatient(Patient? patient)
    {
        if (patient is null)
        {
            return;
        }

        SelectedPatient = patient;

        var tasks = carePlanDataService.GetTasks(patient);

        InProgressTasks.Clear();
        PlannedTasks.Clear();
        CompletedTasks.Clear();

        foreach (var task in tasks)
        {
            switch (task.Status)
            {
                case "In Progress":
                    InProgressTasks.Add(task);
                    break;
                case "Completed":
                    CompletedTasks.Add(task);
                    break;
                default:
                    PlannedTasks.Add(task);
                    break;
            }
        }

        HasNoTasks = tasks.Count == 0;
        HasNoInProgressTasks = InProgressTasks.Count == 0;
        HasNoPlannedTasks = PlannedTasks.Count == 0;
        HasNoCompletedTasks = CompletedTasks.Count == 0;

        ActiveStepIndex = carePlanDataService.GetStageIndex(patient);
        CareStatusText = CarePlanDataService.StageNames[ActiveStepIndex];

        var (intro, action) = carePlanDataService.GetAssistantSummary(patient, tasks);
        AssistantIntro = intro;
        AssistantAction = action;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
