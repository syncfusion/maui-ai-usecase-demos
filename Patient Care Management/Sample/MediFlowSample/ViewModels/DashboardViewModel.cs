using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MediFlowSample.Models;
using MediFlowSample.Services;

namespace MediFlowSample.ViewModels;

public sealed class DashboardViewModel : INotifyPropertyChanged
{
    private readonly DashboardDataService dashboardDataService;
    private bool isBusy;
    private string errorMessage = string.Empty;

    public DashboardViewModel(DashboardDataService dashboardDataService)
    {
        this.dashboardDataService = dashboardDataService;
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

    public int TodayAppointmentsCount { get; private set; }
    public int WaitingPatientsCount { get; private set; }
    public int HighPriorityCount { get; private set; }
    public int CompletedVisitsCount { get; private set; }
    public int TodayVisitCount { get; private set; }
    public double CompletedPercentage { get; private set; }
    public ObservableCollection<DashboardChartPoint> WeeklyAppointments { get; } = [];
    public ObservableCollection<DashboardStatusPoint> VisitStatuses { get; } = [];
    public ObservableCollection<CareAlert> CareAlerts { get; } = [];
    public ObservableCollection<DashboardAppointment> UpcomingAppointments { get; } = [];
    public ObservableCollection<DashboardAppointment> UpcomingAppointmentsLimited
    {
        get => new(UpcomingAppointments.Take(3));
    }

    public void Refresh()
    {
        Load();
    }

    private void Load()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            var data = dashboardDataService.GetDashboardData();

            TodayAppointmentsCount = data.TodayAppointmentsCount;
            WaitingPatientsCount = data.WaitingPatientsCount;
            HighPriorityCount = data.HighPriorityCount;
            CompletedVisitsCount = data.CompletedVisitsCount;
            TodayVisitCount = data.TodayVisitCount;
            CompletedPercentage = data.CompletedPercentage;
            Replace(WeeklyAppointments, data.WeeklyAppointments);
            Replace(VisitStatuses, data.VisitStatuses);
            Replace(CareAlerts, data.CareAlerts);
            Replace(UpcomingAppointments, data.UpcomingAppointments);

            OnPropertyChanged(nameof(TodayAppointmentsCount));
            OnPropertyChanged(nameof(WaitingPatientsCount));
            OnPropertyChanged(nameof(HighPriorityCount));
            OnPropertyChanged(nameof(CompletedVisitsCount));
            OnPropertyChanged(nameof(TodayVisitCount));
            OnPropertyChanged(nameof(CompletedPercentage));
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

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }
}
