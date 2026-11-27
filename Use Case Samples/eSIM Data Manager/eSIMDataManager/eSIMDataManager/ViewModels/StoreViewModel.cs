using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eSIMDataManager.Models;
using eSIMDataManager.Services;

namespace eSIMDataManager.ViewModels;

public partial class StoreViewModel : ObservableObject
{
    private readonly EsimStore store;
    private readonly IDialogService dialogService;
    private readonly INavigationService navigationService;
    private CancellationTokenSource? searchCts;

    public ObservableCollection<Plan> FilteredPlans { get; } = [];

    [ObservableProperty]
    private string search = string.Empty;

    [ObservableProperty]
    private string? selectedCountry;

    [ObservableProperty]
    private CoverageType? selectedCoverage;

    [ObservableProperty]
    private NetworkType? selectedNetwork;

    [ObservableProperty]
    private PlanType? selectedPlanType;

    [ObservableProperty]
    private int? selectedValidity;

    [ObservableProperty]
    private int coverageFilterIndex;

    [ObservableProperty]
    private string selectedNetworkOption = "4G / LTE & 5G";

    [ObservableProperty]
    private string selectedQuotaOption = "All Quotas";

    [ObservableProperty]
    private string selectedDurationOption = "Any Duration";

    [ObservableProperty]
    private string selectedSortOption = "Duration: Longest";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPlanName))]
    [NotifyPropertyChangedFor(nameof(SelectedPlanTax))]
    [NotifyPropertyChangedFor(nameof(SelectedPlanTotal))]
    private Plan? selectedPlan;

    [ObservableProperty]
    private bool isPurchasePopupVisible;

    public IReadOnlyList<string> CoverageOptions { get; } = ["All Coverage", "Local (160+)", "Regional (6)", "Global (130+)"];
    public IReadOnlyList<string> NetworkFilterOptions { get; } = ["4G / LTE & 5G", "5G Ultra-Wideband", "LTE / 4G Only"];
    public IReadOnlyList<string> QuotaFilterOptions { get; } = ["All Quotas", "Standard (1–10 GB)", "Heavy (20+ GB)", "Unlimited Daily"];
    public IReadOnlyList<string> DurationFilterOptions { get; } = ["Any Duration", "Short (1–7 Days)", "Medium (10–15 Days)", "Monthly (30 Days)"];
    public IReadOnlyList<string> SortFilterOptions { get; } = ["Price: Lowest first", "Price: Highest first", "Duration: Longest", "Duration: Shortest", "Data: Largest pool"];

    public string SelectedPlanName => SelectedPlan?.Country ?? string.Empty;
    public decimal SelectedPlanTax => SelectedPlan is null ? 0m : Math.Round(SelectedPlan.Price * 0.10m, 2);
    public decimal SelectedPlanTotal => SelectedPlan is null ? 0m : Math.Round(SelectedPlan.Price + SelectedPlanTax, 2);

    public StoreViewModel(
        EsimStore store,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        this.store = store;
        this.dialogService = dialogService;
        this.navigationService = navigationService;

        ApplyFilters();
    }

    partial void OnSearchChanged(string value)
    {
        searchCts?.Cancel();
        searchCts = new CancellationTokenSource();
        var token = searchCts.Token;

        Task.Delay(250, token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
            {
                MainThread.BeginInvokeOnMainThread(ApplyFilters);
            }
        }, TaskScheduler.Default);
    }

    partial void OnSelectedCountryChanged(string? value) => ApplyFilters();
    partial void OnSelectedCoverageChanged(CoverageType? value) => ApplyFilters();
    partial void OnSelectedNetworkChanged(NetworkType? value) => ApplyFilters();
    partial void OnSelectedPlanTypeChanged(PlanType? value) => ApplyFilters();
    partial void OnSelectedValidityChanged(int? value) => ApplyFilters();

    partial void OnCoverageFilterIndexChanged(int value)
    {
        SelectedCoverage = value switch
        {
            1 => CoverageType.Local,
            2 => CoverageType.Regional,
            3 => CoverageType.Global,
            _ => null
        };
    }

    partial void OnSelectedNetworkOptionChanged(string value)
    {
        SelectedNetwork = value switch
        {
            "5G Ultra-Wideband" => NetworkType.FiveG,
            "LTE / 4G Only" => NetworkType.FourG,
            _ => null
        };
    }

    partial void OnSelectedQuotaOptionChanged(string value)
    {
        SelectedPlanType = value switch
        {
            "Unlimited Daily" => PlanType.Unlimited,
            "Standard (1–10 GB)" => PlanType.Limited,
            _ => null
        };
    }

    partial void OnSelectedDurationOptionChanged(string value)
    {
        SelectedValidity = value switch
        {
            "Short (1–7 Days)" => 7,
            "Medium (10–15 Days)" => 15,
            "Monthly (30 Days)" => 30,
            _ => null
        };
    }

    partial void OnSelectedSortOptionChanged(string value) => ApplyFilters();

    public void ApplyFilters()
    {
        var raw = store.SearchPlans(
            search: Search,
            country: SelectedCountry,
            coverage: SelectedCoverage,
            network: SelectedNetwork,
            type: SelectedPlanType,
            days: SelectedValidity);

        IEnumerable<Plan> sorted = SelectedSortOption switch
        {
            "Price: Lowest first" => raw.OrderBy(p => p.Price),
            "Price: Highest first" => raw.OrderByDescending(p => p.Price),
            "Duration: Shortest" => raw.OrderBy(p => p.ValidityDays),
            "Data: Largest pool" => raw.OrderByDescending(p => p.DataLimitGb),
            _ => raw.OrderByDescending(p => p.ValidityDays)
        };

        var results = sorted.ToList();

        for (int i = FilteredPlans.Count - 1; i >= 0; i--)
        {
            if (!results.Contains(FilteredPlans[i]))
                FilteredPlans.RemoveAt(i);
        }
        for (int i = 0; i < results.Count; i++)
        {
            var item = results[i];
            var currentIndex = FilteredPlans.IndexOf(item);
            if (currentIndex == -1)
            {
                FilteredPlans.Insert(Math.Min(i, FilteredPlans.Count), item);
            }
            else if (currentIndex != i)
            {
                FilteredPlans.Move(currentIndex, i);
            }
        }
    }

    [RelayCommand]
    public void ClearFilters()
    {
        Search = string.Empty;
        SelectedCountry = null;
        SelectedCoverage = null;
        SelectedNetwork = null;
        SelectedPlanType = null;
        SelectedValidity = null;
        CoverageFilterIndex = 0;
        SelectedNetworkOption = "4G / LTE & 5G";
        SelectedQuotaOption = "All Quotas";
        SelectedDurationOption = "Any Duration";
        SelectedSortOption = "Duration: Longest";
        ApplyFilters();
    }

    public void SetNetworkFilter(string value) => SelectedNetworkOption = value;
    public void SetQuotaFilter(string value) => SelectedQuotaOption = value;
    public void SetDurationFilter(string value) => SelectedDurationOption = value;
    public void SetSortFilter(string value) => SelectedSortOption = value;

    [RelayCommand]
    public void SelectCountry(string? country)
    {
        SelectedCountry = country;
    }

    [RelayCommand]
    public void SelectCoverage(string? coverage)
    {
        if (Enum.TryParse<CoverageType>(coverage, true, out var type))
            SelectedCoverage = type;
        else
            SelectedCoverage = null;
    }

    [RelayCommand]
    public void OpenPurchase(Plan? plan)
    {
        if (plan is null) return;
        SelectedPlan = plan;
        IsPurchasePopupVisible = true;
    }

    [RelayCommand]
    public void ClosePurchase()
    {
        IsPurchasePopupVisible = false;
    }

    [RelayCommand]
    public async Task ConfirmPurchase()
    {
        if (SelectedPlan is null) return;
        var plan = SelectedPlan;
        var beforeCount = store.Profiles.Count;
        var newProfile = store.Purchase(plan);
        ClosePurchase();

        await dialogService.ShowAlertAsync(
            "Plan added",
            $"{plan.Country} · {plan.DataLabel} was added to your dashboard.\n\nDemo only — no payment was taken and no eSIM was provisioned.",
            "View profiles");

        if (store.Profiles.Count != beforeCount)
        {
            await navigationService.NavigateToProfilesAsync();
        }
    }

    [RelayCommand]
    public async Task NavigateToDashboard()
    {
        await navigationService.NavigateToDashboardAsync();
    }

    [RelayCommand]
    public async Task NavigateToProfiles()
    {
        await navigationService.NavigateToProfilesAsync();
    }
}
