using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using MediFlowSample.Models;
using MediFlowSample.Services;

namespace MediFlowSample.ViewModels;

public sealed class PatientsViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly PatientDataService patientDataService;
    private readonly ScheduleDataService scheduleDataService;
    private List<Patient> allPatients = [];
    private bool isBusy;
    private string errorMessage = string.Empty;
    private string searchText = string.Empty;
    private string selectedFilter = "All Patients";
    private Patient? selectedPatient;
    private bool hasNoResults;
    private bool isProfileOpen;
    private string returnRoute = string.Empty;

    public PatientsViewModel(PatientDataService patientDataService, ScheduleDataService scheduleDataService)
    {
        this.patientDataService = patientDataService;
        this.scheduleDataService = scheduleDataService;
        FilterOptions = ["All Patients", "High Priority", "Follow-up Due"];
        ClearSearchCommand = new Command(() => SearchText = string.Empty);
        SelectFilterCommand = new Command<string>(filter => SelectedFilter = filter ?? "All Patients");
        RetryCommand = new Command(Load);
        OpenPatientCommand = new Command<Patient>(OpenPatientProfile);
        CloseProfileCommand = new Command(() => IsProfileOpen = false);
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

    public string SearchText
    {
        get => searchText;
        set
        {
            if (SetField(ref searchText, value))
            {
                ApplyFilters();
            }
        }
    }

    public string SelectedFilter
    {
        get => selectedFilter;
        set
        {
            if (SetField(ref selectedFilter, value))
            {
                ApplyFilters();
            }
        }
    }

    public Patient? SelectedPatient
    {
        get => selectedPatient;
        set => SetField(ref selectedPatient, value);
    }

    public bool IsProfileOpen
    {
        get => isProfileOpen;
        set => SetField(ref isProfileOpen, value);
    }

    /// <summary>Route to navigate back to when the profile closes, set when opened from another page.</summary>
    public string ReturnRoute
    {
        get => returnRoute;
        set => SetField(ref returnRoute, value);
    }

    public ObservableCollection<TimelineActivity> RecentActivities { get; } = [];

    public List<string> FilterOptions { get; }

    public List<string> PatientSearchSuggestions { get; private set; } = [];

    public ObservableCollection<Patient> FilteredPatients { get; } = [];

    public bool HasNoResults
    {
        get => hasNoResults;
        private set => SetField(ref hasNoResults, value);
    }

    public Command ClearSearchCommand { get; }
    public Command<string> SelectFilterCommand { get; }
    public Command RetryCommand { get; }
    public Command<Patient> OpenPatientCommand { get; }
    public Command CloseProfileCommand { get; }

    public void Refresh() => Load();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ReturnRoute = query.TryGetValue("returnRoute", out var routeValue) && routeValue is string route
            ? route
            : string.Empty;

        if (query.TryGetValue("patientId", out var value) && value is string patientId)
        {
            var patient = allPatients.FirstOrDefault(p => p.PatientId == patientId);
            if (patient is not null)
            {
                OpenPatientProfile(patient);
            }
        }
    }

    private void OpenPatientProfile(Patient? patient)
    {
        if (patient is null)
        {
            return;
        }

        SelectedPatient = patient;
        RecentActivities.Clear();
        foreach (var activity in patientDataService.GetRecentActivities(patient.PatientId))
        {
            RecentActivities.Add(activity);
        }

        IsProfileOpen = true;
    }

    private void Load()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            patientDataService.UpdateAppointmentDates(scheduleDataService.GetAppointments());
            allPatients = [.. patientDataService.GetPatients()];
            PatientSearchSuggestions = [.. allPatients.Select(patient => $"{patient.FullName} \u2022 {patient.PatientId}")];
            OnPropertyChanged(nameof(PatientSearchSuggestions));
            ApplyFilters();
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

    private void ApplyFilters()
    {
        IEnumerable<Patient> query = allPatients;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Split('\u2022')[0].Trim();
            query = query.Where(patient =>
                patient.PatientId.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                patient.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                patient.LastName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                patient.FullName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        query = SelectedFilter switch
        {
            "High Priority" => query.Where(patient => patient.IsHighPriority),
            "Follow-up Due" => query.Where(patient => patient.IsFollowUpDue),
            _ => query
        };

        FilteredPatients.Clear();
        foreach (var patient in query)
        {
            FilteredPatients.Add(patient);
        }

        HasNoResults = FilteredPatients.Count == 0;
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
