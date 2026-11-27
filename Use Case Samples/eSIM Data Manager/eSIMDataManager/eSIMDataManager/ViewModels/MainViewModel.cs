using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eSIMDataManager.Models;
using eSIMDataManager.Services;

namespace eSIMDataManager.ViewModels;

/// <summary>
/// Backward-compatible unified ViewModel delegating to the focused ViewModels
/// while supporting any legacy bindings and shared state.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly EsimStore store;
    private readonly IDialogService dialogService;
    private readonly INavigationService navigationService;

    public DashboardViewModel DashboardVM { get; }
    public ProfilesViewModel ProfilesVM { get; }
    public StoreViewModel StoreVM { get; }
    public TopUpViewModel TopUpVM { get; }

    public EsimStore Store => store;

    // Forwarded properties for backward compatibility
    public ObservableCollection<Plan> FilteredPlans => StoreVM.FilteredPlans;
    public ObservableCollection<EsimProfile> ActiveProfiles => ProfilesVM.ActiveProfiles;
    public ObservableCollection<EsimProfile> ReadyProfiles => ProfilesVM.ReadyProfiles;
    public ObservableCollection<EsimProfile> ExpiredProfiles => ProfilesVM.ExpiredProfiles;
    public ObservableCollection<DailyUsage> Trend => DashboardVM.Trend;
    public ObservableCollection<DailyUsage> AverageTrend => DashboardVM.AverageTrend;

    public EsimProfile? ActiveProfile => DashboardVM.ActiveProfile;
    public double Remaining => DashboardVM.Remaining;
    public double Consumed => DashboardVM.Consumed;
    public double Allocation => DashboardVM.Allocation;
    public double UsedPercent => DashboardVM.UsedPercent;
    public double AvailablePercent => DashboardVM.AvailablePercent;
    public string RemainingText => DashboardVM.RemainingText;
    public string ExpiryText => DashboardVM.ExpiryText;
    public int DaysRemaining => DashboardVM.DaysRemaining;
    public bool HasActiveProfile => DashboardVM.HasActiveProfile;
    public string ActivationDateText => DashboardVM.ActivationDateText;
    public double SevenDayTotal => DashboardVM.SevenDayTotal;

    public int ProfilesCount => store.Profiles.Count;

    public EsimProfile? SelectedProfile
    {
        get => ProfilesVM.SelectedProfile;
        set => ProfilesVM.SelectedProfile = value;
    }

    public Plan? SelectedPlan
    {
        get => StoreVM.SelectedPlan;
        set => StoreVM.SelectedPlan = value;
    }

    public string SelectedPlanName => StoreVM.SelectedPlanName;
    public decimal SelectedPlanTax => StoreVM.SelectedPlanTax;
    public decimal SelectedPlanTotal => StoreVM.SelectedPlanTotal;

    public EsimProfile? TopUpProfile
    {
        get => TopUpVM.TopUpProfile;
        set => TopUpVM.TopUpProfile = value;
    }

    public double SelectedTopUpGb
    {
        get => TopUpVM.SelectedTopUpGb;
        set => TopUpVM.SelectedTopUpGb = value;
    }

    public TopUpPack SelectedTopUpPack => TopUpVM.SelectedTopUpPack;
    public IReadOnlyList<TopUpPack> TopUpPacks => TopUpVM.TopUpPacks;
    public string TopUpProfileName => TopUpVM.TopUpProfileName;
    public string TopUpCarrierName => TopUpVM.TopUpCarrierName;
    public string TopUpRemainingText => TopUpVM.TopUpRemainingText;
    public string TopUpDaysLeftText => TopUpVM.TopUpDaysLeftText;
    public string TopUpAllowanceSummary => TopUpVM.TopUpAllowanceSummary;
    public string TopUpSubmitLabel => TopUpVM.TopUpSubmitLabel;

    public string Search
    {
        get => StoreVM.Search;
        set => StoreVM.Search = value;
    }

    public IReadOnlyList<string> CoverageOptions => StoreVM.CoverageOptions;
    public IReadOnlyList<string> NetworkFilterOptions => StoreVM.NetworkFilterOptions;
    public IReadOnlyList<string> QuotaFilterOptions => StoreVM.QuotaFilterOptions;
    public IReadOnlyList<string> DurationFilterOptions => StoreVM.DurationFilterOptions;
    public IReadOnlyList<string> SortFilterOptions => StoreVM.SortFilterOptions;

    public int CoverageFilterIndex => StoreVM.CoverageFilterIndex;
    public string SelectedNetworkOption
    {
        get => StoreVM.SelectedNetworkOption;
        set => StoreVM.SelectedNetworkOption = value;
    }
    public string SelectedQuotaOption
    {
        get => StoreVM.SelectedQuotaOption;
        set => StoreVM.SelectedQuotaOption = value;
    }
    public string SelectedDurationOption
    {
        get => StoreVM.SelectedDurationOption;
        set => StoreVM.SelectedDurationOption = value;
    }
    public string SelectedSortOption
    {
        get => StoreVM.SelectedSortOption;
        set => StoreVM.SelectedSortOption = value;
    }

    public bool HasSelectedProfile => ProfilesVM.HasSelectedProfile;
    public bool IsSelectedProfileReady => ProfilesVM.IsSelectedProfileReady;
    public bool IsSelectedProfileActive => ProfilesVM.IsSelectedProfileActive;
    public bool IsSelectedProfileExpired => ProfilesVM.IsSelectedProfileExpired;
    public string SelectedProfileStatusText => ProfilesVM.SelectedProfileStatusText;

    public bool IsTopUpSelected(TopUpPack? pack) => TopUpVM.IsTopUpSelected(pack);

    public MainViewModel(
        EsimStore store,
        IDialogService dialogService,
        INavigationService navigationService,
        DashboardViewModel dashboardVM,
        ProfilesViewModel profilesVM,
        StoreViewModel storeVM,
        TopUpViewModel topUpVM)
    {
        this.store = store;
        this.dialogService = dialogService;
        this.navigationService = navigationService;
        DashboardVM = dashboardVM;
        ProfilesVM = profilesVM;
        StoreVM = storeVM;
        TopUpVM = topUpVM;

        DashboardVM.PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);
        ProfilesVM.PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);
        StoreVM.PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);
        TopUpVM.PropertyChanged += (_, e) => OnPropertyChanged(e.PropertyName);
        store.Changed += (_, _) =>
        {
            OnPropertyChanged(nameof(ProfilesCount));
            OnPropertyChanged(nameof(ActiveProfile));
        };
    }

    [RelayCommand]
    public Task BuyPlan(Plan? plan)
    {
        StoreVM.OpenPurchase(plan);
        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task TopUp(EsimProfile? profile) => TopUpVM.OpenTopUp(profile);

    [RelayCommand]
    public Task ShowQr(EsimProfile? profile)
    {
        ProfilesVM.ShowQr(profile);
        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task InstallProfile(EsimProfile? profile) => ProfilesVM.InstallProfile(profile);

    [RelayCommand]
    public Task Renew(EsimProfile? profile) => ProfilesVM.Renew(profile);

    [RelayCommand]
    public Task ExportLog() => DashboardVM.ExportLog();

    [RelayCommand]
    public void ClearFilters() => StoreVM.ClearFilters();

    [RelayCommand]
    public void SelectCountry(string country) => StoreVM.SelectCountry(country);

    [RelayCommand]
    public void SetNetworkFilter(string value) => StoreVM.SetNetworkFilter(value);

    [RelayCommand]
    public void SetQuotaFilter(string value) => StoreVM.SetQuotaFilter(value);

    [RelayCommand]
    public void SetDurationFilter(string value) => StoreVM.SetDurationFilter(value);

    [RelayCommand]
    public void SetSortFilter(string value) => StoreVM.SetSortFilter(value);

    [RelayCommand]
    public void SelectCoverage(string coverage) => StoreVM.SelectCoverage(coverage);

    [RelayCommand]
    public void SelectTopUpPack(TopUpPack? pack) => TopUpVM.SelectTopUpPack(pack);

    public bool ApplyTopUp()
    {
        if (TopUpProfile is null || TopUpProfile.Status != ProfileStatus.Active) return false;
        return store.TopUp(TopUpProfile, SelectedTopUpPack);
    }
}
