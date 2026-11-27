using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eSIMDataManager.Models;
using eSIMDataManager.Services;

namespace eSIMDataManager.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly EsimStore store;
    private readonly IDialogService dialogService;
    private readonly INavigationService navigationService;

    public TopUpViewModel TopUp { get; }

    public ObservableCollection<DailyUsage> Trend { get; } = [];
    public ObservableCollection<DailyUsage> AverageTrend { get; } = [];

    [ObservableProperty]
    private EsimProfile? qrProfile;

    [ObservableProperty]
    private bool isQrPopupVisible;

    public EsimProfile? ActiveProfile => store.ActiveProfile;
    public bool HasActiveProfile => ActiveProfile is not null;
    public double Remaining => ActiveProfile?.RemainingGb ?? 0;
    public double Consumed => ActiveProfile?.ConsumedGb ?? 0;
    public double Allocation => ActiveProfile?.AllocatedGb ?? 0;
    public double UsedPercent => ActiveProfile?.UsagePercent ?? 0;
    public double AvailablePercent => Math.Max(0, 100 - UsedPercent);
    public string RemainingText => $"{Remaining:0.#} GB";
    public int DaysRemaining => ActiveProfile?.DaysRemaining ?? 0;
    public string ExpiryText => ActiveProfile?.ExpiryDate.ToString("MMM d, yyyy") ?? "—";
    public string ActivationDateText => ActiveProfile?.ActivationDate?.ToString("MMM d, yyyy") ?? "—";
    public double SevenDayTotal => Trend.Sum(d => d.UsedGb);

    public DashboardViewModel(
        EsimStore store,
        IDialogService dialogService,
        INavigationService navigationService,
        TopUpViewModel topUp)
    {
        this.store = store;
        this.dialogService = dialogService;
        this.navigationService = navigationService;
        TopUp = topUp;

        store.Changed += (_, _) => RefreshActiveProfile();
        RefreshActiveProfile();
    }

    public void RefreshActiveProfile()
    {
        OnPropertyChanged(nameof(ActiveProfile));
        OnPropertyChanged(nameof(Remaining));
        OnPropertyChanged(nameof(Consumed));
        OnPropertyChanged(nameof(Allocation));
        OnPropertyChanged(nameof(UsedPercent));
        OnPropertyChanged(nameof(AvailablePercent));
        OnPropertyChanged(nameof(RemainingText));
        OnPropertyChanged(nameof(DaysRemaining));
        OnPropertyChanged(nameof(ExpiryText));
        OnPropertyChanged(nameof(ActivationDateText));

        Trend.Clear();
        AverageTrend.Clear();
        if (ActiveProfile is { } profile)
        {
            foreach (var point in profile.DailyUsage)
                Trend.Add(point);

            double[] averagePoints = [0.88, 0.94, 0.91, 0.97, 1.05, 1.01, 0.96];
            int i = 0;
            foreach (var point in profile.DailyUsage)
            {
                var avg = i < averagePoints.Length ? averagePoints[i] : 0.95;
                AverageTrend.Add(new DailyUsage(point.Day, avg));
                i++;
            }
        }
        OnPropertyChanged(nameof(SevenDayTotal));
    }

    [RelayCommand]
    public async Task TopUpActive()
    {
        await TopUp.OpenTopUp(ActiveProfile);
    }

    [RelayCommand]
    public void ShowActiveQr()
    {
        if (ActiveProfile is { } profile)
        {
            QrProfile = profile;
            IsQrPopupVisible = true;
        }
    }

    [RelayCommand]
    public void CloseQr()
    {
        IsQrPopupVisible = false;
    }

    [RelayCommand]
    public async Task NavigateToStore()
    {
        await navigationService.NavigateToStoreAsync();
    }

    [RelayCommand]
    public async Task NavigateToProfiles()
    {
        await navigationService.NavigateToProfilesAsync();
    }

    [RelayCommand]
    public async Task ExportLog()
    {
        await dialogService.ShowAlertAsync(
            "Export complete",
            "Roaming session log exported to CSV (simulated). 18 data points saved to local demo storage.");
    }
}
