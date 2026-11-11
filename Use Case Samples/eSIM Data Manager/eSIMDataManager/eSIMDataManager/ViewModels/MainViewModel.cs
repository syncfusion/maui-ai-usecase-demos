using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eSIMDataManager.Models;
using eSIMDataManager.Services;

namespace eSIMDataManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly EsimStore store;
    private string? selectedCountry;
    private CoverageType? selectedCoverage;
    private NetworkType? selectedNetwork;
    private PlanType? selectedType;
    private int? selectedValidity;

    public ObservableCollection<Plan> FilteredPlans { get; } = [];
    public ObservableCollection<EsimProfile> ActiveProfiles { get; } = [];
    public ObservableCollection<EsimProfile> ReadyProfiles { get; } = [];
    public ObservableCollection<EsimProfile> ExpiredProfiles { get; } = [];
    public ObservableCollection<DailyUsage> Trend { get; } = [];
    public ObservableCollection<DailyUsage> AverageTrend { get; } = [];
    public IReadOnlyList<UsageSegment> ConsumptionSegments => [new("Consumed", Consumed), new("Remaining", Remaining)];
    public double UsedFraction => UsedPercent / 100d;
    public double ValidityFraction => Math.Clamp(DaysRemaining / 14d, 0, 1);
    public string ActivationDateText => ActiveProfile?.ActivationDate?.ToString("MMM d, yyyy") ?? "—";
    public double SevenDayTotal => Trend.Sum(point => point.UsedGb);
    public IReadOnlyList<string> Destinations { get; } = ["Japan", "USA", "France", "Korea", "Singapore", "Thailand"];
    public IReadOnlyList<string> FilterOptions { get; } = ["Local", "Regional", "Global", "4G", "5G", "Limited", "Unlimited", "7 Days", "15 Days", "30 Days"];
    public IReadOnlyList<TopUpPack> TopUpPacks => EsimStore.TopUpPacks;
    public EsimStore Store => store;
    public EsimProfile? ActiveProfile => store.ActiveProfile;

    [ObservableProperty] private EsimProfile? selectedProfile;
    [ObservableProperty] private Plan? selectedPlan;
    [ObservableProperty] private string selectedFilterLabel = string.Empty;
    [ObservableProperty] private double selectedTopUpGb = 1;
    [ObservableProperty] private string search = string.Empty;
    [ObservableProperty] private bool isPurchaseInProgress;

    public double Remaining => ActiveProfile?.RemainingGb ?? 0;
    public double Consumed => ActiveProfile?.ConsumedGb ?? 0;
    public double Allocation => ActiveProfile?.AllocatedGb ?? 0;
    public double UsedPercent => ActiveProfile?.UsagePercent ?? 0;
    public double AvailablePercent => 100 - UsedPercent;
    public string RemainingText => $"{Remaining:0.#} GB";
    public string ExpiryText => ActiveProfile?.ExpiryDate.ToString("MMM d, yyyy") ?? "—";
    public int DaysRemaining => ActiveProfile?.DaysRemaining ?? 0;
    public bool HasActiveProfile => ActiveProfile is not null;
    public int ProfilesCount => store.Profiles.Count;
    public bool HasSelectedProfile => SelectedProfile is not null;
    public bool IsSelectedProfileReady => SelectedProfile?.Status == ProfileStatus.ReadyToInstall;
    public bool IsSelectedProfileActive => SelectedProfile?.Status == ProfileStatus.Active;
    public bool IsSelectedProfileExpired => SelectedProfile?.Status == ProfileStatus.Expired;
    public string SelectedProfileStatusText => SelectedProfile?.Status switch
    {
        ProfileStatus.Active => "Active · simulated",
        ProfileStatus.ReadyToInstall => "Ready to install · simulated",
        ProfileStatus.Expired => "Expired · sample profile",
        _ => "Select a profile to view details"
    };
    public string SelectedPlanName => SelectedPlan is null ? "" : $"{SelectedPlan.Country} · {SelectedPlan.DataLabel}";
    public decimal SelectedPlanTax => SelectedPlan is null ? 0 : decimal.Round(SelectedPlan.Price * .08m, 2);
    public decimal SelectedPlanTotal => (SelectedPlan?.Price ?? 0) + SelectedPlanTax;
    public TopUpPack SelectedTopUpPack => TopUpPacks.First(p => p.DataGb == SelectedTopUpGb);

    public MainViewModel(EsimStore store)
    {
        this.store = store;
        SelectedProfile = store.SelectedProfile;
        store.Changed += (_, _) => RefreshState();
        RefreshState();
    }

    partial void OnSearchChanged(string value) => RefreshPlans();
    partial void OnSelectedPlanChanged(Plan? value)
    {
        OnPropertyChanged(nameof(SelectedPlanName));
        OnPropertyChanged(nameof(SelectedPlanTax));
        OnPropertyChanged(nameof(SelectedPlanTotal));
    }
    partial void OnSelectedTopUpGbChanged(double value) => OnPropertyChanged(nameof(SelectedTopUpPack));
    partial void OnSelectedProfileChanged(EsimProfile? value)
    {
        store.SelectedProfile = value;
        OnPropertyChanged(nameof(HasSelectedProfile));
        OnPropertyChanged(nameof(IsSelectedProfileReady));
        OnPropertyChanged(nameof(IsSelectedProfileActive));
        OnPropertyChanged(nameof(IsSelectedProfileExpired));
        OnPropertyChanged(nameof(SelectedProfileStatusText));
    }

    private void RefreshState()
    {
        Sync(ActiveProfiles, store.Profiles.Where(p => p.Status == ProfileStatus.Active));
        Sync(ReadyProfiles, store.Profiles.Where(p => p.Status == ProfileStatus.ReadyToInstall));
        Sync(ExpiredProfiles, store.Profiles.Where(p => p.Status == ProfileStatus.Expired));
        if (SelectedProfile is null || !store.Profiles.Contains(SelectedProfile)) SelectedProfile = store.SelectedProfile ?? store.Profiles.FirstOrDefault();
        Trend.Clear();
        AverageTrend.Clear();
        if (ActiveProfile is not null)
        {
            var averageUsage = ActiveProfile.DailyUsage.Average(point => point.UsedGb);
            foreach (var point in ActiveProfile.DailyUsage)
            {
                Trend.Add(point);
                AverageTrend.Add(point with { UsedGb = averageUsage });
            }
        }
        OnPropertyChanged(nameof(ActiveProfile));
        OnPropertyChanged(nameof(Remaining));
        OnPropertyChanged(nameof(Consumed));
        OnPropertyChanged(nameof(Allocation));
        OnPropertyChanged(nameof(UsedPercent));
        OnPropertyChanged(nameof(AvailablePercent));
        OnPropertyChanged(nameof(RemainingText));
        OnPropertyChanged(nameof(ExpiryText));
        OnPropertyChanged(nameof(DaysRemaining));
        OnPropertyChanged(nameof(HasActiveProfile));
        OnPropertyChanged(nameof(ProfilesCount));
        OnPropertyChanged(nameof(ConsumptionSegments));
        OnPropertyChanged(nameof(UsedFraction));
        OnPropertyChanged(nameof(ValidityFraction));
        OnPropertyChanged(nameof(ActivationDateText));
        OnPropertyChanged(nameof(SevenDayTotal));
        RefreshPlans();
    }

    private void RefreshPlans()
    {
        Sync(FilteredPlans, store.SearchPlans(Search, selectedCountry, selectedCoverage, selectedNetwork, selectedType, selectedValidity));
        OnPropertyChanged(nameof(FilteredPlans));
    }

    private static void Sync<T>(ObservableCollection<T> destination, IEnumerable<T> source)
    {
        var items = source.ToList();
        destination.Clear();
        foreach (var item in items) destination.Add(item);
    }

    public void SelectCountry(string country)
    {
        selectedCountry = selectedCountry == country ? null : country;
        RefreshPlans();
    }

    public void ToggleFilter(string label)
    {
        SelectedFilterLabel = SelectedFilterLabel == label ? string.Empty : label;
        selectedCoverage = SelectedFilterLabel switch { "Local" => CoverageType.Local, "Regional" => CoverageType.Regional, "Global" => CoverageType.Global, _ => null };
        selectedNetwork = SelectedFilterLabel switch { "4G" => NetworkType.FourG, "5G" => NetworkType.FiveG, _ => null };
        selectedType = SelectedFilterLabel switch { "Limited" => PlanType.Limited, "Unlimited" => PlanType.Unlimited, _ => null };
        selectedValidity = SelectedFilterLabel switch { "7 Days" => 7, "15 Days" => 15, "30 Days" => 30, _ => null };
        RefreshPlans();
    }

    [RelayCommand]
    private async Task ExportLog()
    {
        var csv = "Event,Details,Timestamp\nPlan Activated,Tokyo Haneda (HND) Terminal 3,Sample data\nData Session Recorded,1.2 GB High-Speed Roaming,Sample data\nTop-Up Purchased,+3 GB Speed Booster Pack,Sample data\nPlan Renewed,Auto-renewal (simulated),Sample data";
        await Clipboard.Default.SetTextAsync(csv);
        await Application.Current!.Windows[0].Page!.DisplayAlert("Demo roaming log", "A fictional sample CSV log has been copied to the clipboard.", "OK");
    }

    [RelayCommand]
    private void ClearFilters()
    {
        selectedCountry = null;
        selectedCoverage = null;
        selectedNetwork = null;
        selectedType = null;
        selectedValidity = null;
        Search = string.Empty;
        SelectedFilterLabel = string.Empty;
        RefreshPlans();
    }

    [RelayCommand]
    private async Task BuyPlan(Plan? plan)
    {
        if (plan is null || IsPurchaseInProgress) return;
        IsPurchaseInProgress = true;
        SelectedPlan = plan;
        var confirm = await Application.Current!.Windows[0].Page!.DisplayAlert("Demo purchase", $"{plan.Country} · {plan.DataLabel}\nPlan: ${plan.Price:0.00}\nIllustrative tax: ${SelectedPlanTax:0.00}\nTotal: ${SelectedPlanTotal:0.00} USD\n\nNo payment is taken. Continue with this simulated purchase?", "Confirm demo purchase", "Cancel");
        if (!confirm) { IsPurchaseInProgress = false; return; }
        SelectedProfile = store.Purchase(plan);
        IsPurchaseInProgress = false;
        await Application.Current.Windows[0].Page!.DisplayAlert("Plan added", "Your sample eSIM is ready to install. This is a simulation only; no eSIM was provisioned.", "View profiles");
        await Shell.Current.GoToAsync("//profiles");
    }

    [RelayCommand]
    private async Task ShowQr(EsimProfile? profile)
    {
        if (profile is null) return;
        SelectedProfile = profile;
        var page = Application.Current!.Windows[0].Page!;
        var copy = await page.DisplayAlert("Sample activation details", $"{profile.Country} · {profile.ProfileName}\n\n{profile.ActivationCode}\n\nDEMO QR ONLY — this code cannot provision an eSIM.", "Copy code", "Close");
        if (copy)
        {
            await Clipboard.Default.SetTextAsync(profile.ActivationCode);
            await page.DisplayAlert("Copied", "Demo activation code copied.", "OK");
        }
    }

    [RelayCommand]
    private async Task InstallProfile(EsimProfile? profile)
    {
        if (profile is null || profile.Status != ProfileStatus.ReadyToInstall) return;
        var confirm = await Application.Current!.Windows[0].Page!.DisplayAlert("Simulate activation", "Change this sample profile to Active? This does not install an eSIM on this device.", "Activate demo profile", "Cancel");
        if (confirm) store.Activate(profile);
    }

    [RelayCommand]
    private async Task TopUp(EsimProfile? profile)
    {
        if (profile is null) return;
        if (profile.Status != ProfileStatus.Active)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Top-up unavailable", profile.Status == ProfileStatus.Expired ? "Expired demo profiles cannot be topped up." : "Activate this demo profile before topping up.", "OK");
            return;
        }
        SelectedProfile = profile;
        var options = TopUpPacks.Select(p => $"{p.DataGb:0} GB · ${p.Price:0.00} demo").ToArray();
        var choice = await Application.Current!.Windows[0].Page!.DisplayActionSheet("Choose a sample top-up", "Cancel", null, options);
        var pack = TopUpPacks.FirstOrDefault(p => choice.StartsWith($"{p.DataGb:0} GB", StringComparison.Ordinal));
        if (pack is null) return;
        var confirm = await Application.Current.Windows[0].Page!.DisplayAlert("Confirm demo top-up", $"Add {pack.DataGb:0} GB for ${pack.Price:0.00} USD? No payment is taken.", "Add data", "Cancel");
        if (confirm) store.TopUp(profile, pack);
    }

    [RelayCommand]
    private async Task Renew(EsimProfile? profile)
    {
        if (profile is null) return;
        await Application.Current!.Windows[0].Page!.DisplayAlert("Renewal demo", "Renewal is a display-only sample action and does not change or purchase a plan.", "Close");
    }
}
