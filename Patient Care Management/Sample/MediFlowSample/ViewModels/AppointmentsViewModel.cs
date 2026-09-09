using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using MediFlowSample.Models;
using MediFlowSample.Services;
using Syncfusion.Maui.Scheduler;

namespace MediFlowSample.ViewModels;

public sealed class AppointmentsViewModel : INotifyPropertyChanged
{
    private readonly ScheduleDataService scheduleDataService;
    private bool isBusy;
    private string errorMessage = string.Empty;
    private DateTime selectedDate = DateTime.Today;
    private SchedulerView viewMode = SchedulerView.Day;
    private Clinician? selectedClinician;
    private ScheduleAppointment? selectedAppointment;
    private bool hasNoAppointments;

    public AppointmentsViewModel(ScheduleDataService scheduleDataService)
    {
        this.scheduleDataService = scheduleDataService;

        PreviousDateCommand = new Command(() => SelectedDate = SelectedDate.AddDays(ViewMode == SchedulerView.Week ? -7 : -1));
        NextDateCommand = new Command(() => SelectedDate = SelectedDate.AddDays(ViewMode == SchedulerView.Week ? 7 : 1));
        GoToTodayCommand = new Command(() => SelectedDate = DateTime.Today);
        SelectViewModeCommand = new Command<string>(mode => ViewMode = mode == "Week" ? SchedulerView.Week : SchedulerView.Day);
        SelectClinicianCommand = new Command<Clinician>(SelectClinician);
        CloseAppointmentDetailsCommand = new Command(() => SelectedAppointment = null);
        RetryCommand = new Command(Load);

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

    public DateTime SelectedDate
    {
        get => selectedDate;
        set
        {
            if (SetField(ref selectedDate, value))
            {
                OnPropertyChanged(nameof(SelectedDateDisplay));
                RefreshVisibleAppointments();
            }
        }
    }

    public string SelectedDateDisplay
    {
        get
        {
            if (ViewMode != SchedulerView.Week)
            {
                return SelectedDate.Date == DateTime.Today
                    ? $" {SelectedDate:MMM dd, yyyy}"
                    : SelectedDate.ToString("MMM dd, yyyy");
            }

            var weekEnd = SelectedDate.Date.AddDays(6);
            return SelectedDate.Year == weekEnd.Year
                ? $"{SelectedDate:MMM dd} - {weekEnd:MMM dd, yyyy}"
                : $"{SelectedDate:MMM dd, yyyy} - {weekEnd:MMM dd, yyyy}";
        }
    }

    public SchedulerView ViewMode
    {
        get => viewMode;
        set
        {
            if (SetField(ref viewMode, value))
            {
                OnPropertyChanged(nameof(ViewModeIndex));
                OnPropertyChanged(nameof(SelectedDateDisplay));
                RefreshVisibleAppointments();
            }
        }
    }

    /// <summary>0=Day, 1=Week — mirrors ViewMode so the header's Day/Week toggle stays in sync when the scheduler header template is recreated.</summary>
    public int ViewModeIndex => ViewMode == SchedulerView.Week ? 1 : 0;

    public Clinician? SelectedClinician
    {
        get => selectedClinician;
        private set => SetField(ref selectedClinician, value);
    }

    public ScheduleAppointment? SelectedAppointment
    {
        get => selectedAppointment;
        set
        {
            if (SetField(ref selectedAppointment, value))
            {
                OnPropertyChanged(nameof(IsAppointmentDetailsOpen));
            }
        }
    }

    public bool IsAppointmentDetailsOpen => SelectedAppointment is not null;

    public bool HasNoAppointments
    {
        get => hasNoAppointments;
        private set => SetField(ref hasNoAppointments, value);
    }

    public ObservableCollection<Clinician> Clinicians { get; } = [];

    public ObservableCollection<ScheduleAppointment> VisibleAppointments { get; } = [];

    public Command PreviousDateCommand { get; }
    public Command NextDateCommand { get; }
    public Command GoToTodayCommand { get; }
    public Command<string> SelectViewModeCommand { get; }
    public Command<Clinician> SelectClinicianCommand { get; }
    public Command CloseAppointmentDetailsCommand { get; }
    public Command RetryCommand { get; }

    public void Refresh() => Load();

    public void SynchronizeSchedulerState(SchedulerView view, DateTime visibleDate)
    {
        ViewMode = view;
        SelectedDate = visibleDate.Date;
    }

    public void SetSchedulerView(SchedulerView view)
    {
        if (ViewMode != view)
        {
            ViewMode = view;
        }
    }

    private void Load()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var clinicianList = scheduleDataService.GetClinicians();
            Clinicians.Clear();
            foreach (var clinician in clinicianList)
            {
                Clinicians.Add(clinician);
            }

            SelectClinician(Clinicians.FirstOrDefault());
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

    private void SelectClinician(Clinician? clinician)
    {
        if (clinician is null)
        {
            return;
        }

        SelectedClinician = clinician;

        RefreshVisibleAppointments();
    }

    private void RefreshVisibleAppointments()
    {
        if (SelectedClinician is null)
        {
            return;
        }

        var selectedDay = SelectedDate.Date;
        var weekStart = selectedDay.AddDays(-(int)selectedDay.DayOfWeek);

        var appointments = scheduleDataService.GetAppointments(SelectedClinician.ClinicianId)
            .Where(appointment => ViewMode == SchedulerView.Week
                ? appointment.StartTime.Date >= weekStart.Date && appointment.StartTime.Date < weekStart.Date.AddDays(7)
                : appointment.StartTime.Date == selectedDay)
            .OrderBy(appointment => appointment.StartTime);

        VisibleAppointments.Clear();
        foreach (var appointment in appointments)
        {
            VisibleAppointments.Add(appointment);
        }

        HasNoAppointments = VisibleAppointments.Count == 0;
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
