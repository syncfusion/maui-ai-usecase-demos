using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ESimDataManager;

namespace ESimDataManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly EsimStore store;
    private string searchText = "";
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
    public IReadOnlyList<string> Destinations { get; } = ["Japan", "USA", "France", "Korea", "Singapore", "Thailand"];
    public IReadOnlyList<string> FilterOptions { get; } = ["Any", "Local", "Regional", "Global", "4G", "5G", "Limited", "Unlimited", "7 Days", "15 Days", "30 Days"];
    public EsimStore Store => store;
    public EsimProfile? ActiveProfile => store.ActiveProfile;
    public EsimProfile? SelectedProfile { get => store.SelectedProfile; set { store.SelectedProfile = value; OnPropertyChanged(); } }
    public double Remaining => ActiveProfile?.RemainingGb ?? 0;
    public double Consumed => ActiveProfile?.ConsumedGb ?? 0;
    public double Allocation => ActiveProfile?.AllocatedGb ?? 0;
    public double UsedPercent => ActiveProfile?.UsagePercent ?? 0;
    public double AvailablePercent => (100 - UsedPercent) / 100d;
    public string RemainingText => $"{Remaining:0.#} GB";
    public string ExpiryText => ActiveProfile?.ExpiryDate.ToString("MMM d") ?? "—";
    public int DaysRemaining => ActiveProfile?.DaysRemaining ?? 0;
    public bool HasActiveProfile => ActiveProfile is not null;
    public string SearchText { get => searchText; set { if (SetProperty(ref searchText, value)) RefreshPlans(); } }
    public string? SelectedCountry { get => selectedCountry; set { if (SetProperty(ref selectedCountry, value)) RefreshPlans(); } }
    public CoverageType? SelectedCoverage { get => selectedCoverage; set { if (SetProperty(ref selectedCoverage, value)) RefreshPlans(); } }
    public NetworkType? SelectedNetwork { get => selectedNetwork; set { if (SetProperty(ref selectedNetwork, value)) RefreshPlans(); } }
    public PlanType? SelectedType { get => selectedType; set { if (SetProperty(ref selectedType, value)) RefreshPlans(); } }
    public int? SelectedValidity { get => selectedValidity; set { if (SetProperty(ref selectedValidity, value)) RefreshPlans(); } }

    public MainViewModel(EsimStore store)
    {
        this.store = store;
        store.Changed += (_, _) => RefreshState();
        RefreshState();
    }

    private void RefreshState()
    {
        Replace(ActiveProfiles, store.Profiles.Where(p => p.Status == ProfileStatus.Active));
        Replace(ReadyProfiles, store.Profiles.Where(p => p.Status == ProfileStatus.ReadyToInstall));
        Replace(ExpiredProfiles, store.Profiles.Where(p => p.Status == ProfileStatus.Expired));
        Trend.Clear();
        if (ActiveProfile is not null) foreach (var point in ActiveProfile.DailyUsage) Trend.Add(point);
        foreach (var name in new[] { nameof(ActiveProfile), nameof(SelectedProfile), nameof(Remaining), nameof(Consumed), nameof(Allocation), nameof(UsedPercent), nameof(AvailablePercent), nameof(RemainingText), nameof(ExpiryText), nameof(DaysRemaining), nameof(HasActiveProfile) }) OnPropertyChanged(name);
        RefreshPlans();
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear(); foreach (var item in source) target.Add(item);
    }

    private void RefreshPlans() => Replace(FilteredPlans, store.SearchPlans(SearchText, SelectedCountry, SelectedCoverage, SelectedNetwork, SelectedType, SelectedValidity));

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = ""; SelectedCountry = null; SelectedCoverage = null; SelectedNetwork = null; SelectedType = null; SelectedValidity = null;
    }

    [RelayCommand]
    private async Task BuyPlan(Plan? plan)
    {
        if (plan is null) return;
        var page = Application.Current!.Windows[0].Page!;
        var tax = Math.Round(plan.Price * 0.06m, 2);
        if (!await page.DisplayAlert("Demo purchase", $"{plan.Country} · {plan.DataLabel} · {plan.ValidityDays} days\nPlan ${plan.Price:0.00} + sample tax ${tax:0.00} = ${plan.Price + tax:0.00}\n\nThis is a simulated purchase; no payment is collected.", "Confirm", "Cancel")) return;
        store.Purchase(plan);
        await page.DisplayAlert("Added to My eSIMs", "The sample profile is ready to install (simulated).", "View My eSIMs");
        SelectedProfile = store.SelectedProfile;
        await Shell.Current.GoToAsync("//main/esims");
    }

    [RelayCommand]
    private async Task ShowQr(EsimProfile? profile)
    {
        profile ??= ActiveProfile ?? SelectedProfile;
        if (profile is null) return;
        var page = Application.Current!.Windows[0].Page!;
        var result = await page.DisplayActionSheet($"Simulated activation · {profile.Country}", "Close", "Copy Code", "How to install", profile.ActivationCode);
        if (result == "Copy Code" || result == profile.ActivationCode)
        {
            await Clipboard.Default.SetTextAsync(profile.ActivationCode);
            await page.DisplayAlert("Code copied", "The demo activation code was copied. It cannot provision a real eSIM.", "OK");
        }
        else if (result == "How to install")
        {
            var install = await page.DisplayAlert("Simulated install", "Review the sample code and instructions. This demo does not install a carrier profile on your device.", "Mark Active", "Cancel");
            if (install && profile.Status == ProfileStatus.ReadyToInstall)
            {
                store.Activate(profile);
                await page.DisplayAlert("Demo profile activated", "The profile status changed in this session only.", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task TopUp(EsimProfile? profile)
    {
        profile ??= SelectedProfile ?? ActiveProfile;
        if (profile is null) return;
        var page = Application.Current!.Windows[0].Page!;
        if (profile.Status != ProfileStatus.Active)
        {
            await page.DisplayAlert("Top-up unavailable", "Only active demo profiles can be topped up.", "OK");
            return;
        }
        var selected = await page.DisplayActionSheet($"Choose a demo top-up for {profile.Country}", "Cancel", null, "$3 · 1 GB", "$7 · 3 GB", "$10 · 5 GB");
        var index = selected switch { "$3 · 1 GB" => 0, "$7 · 3 GB" => 1, "$10 · 5 GB" => 2, _ => -1 };
        if (index < 0) return;
        var pack = EsimStore.TopUpPacks[index];
        if (await page.DisplayAlert("Confirm demo top-up", $"Add {pack.DataGb} GB for ${pack.Price:0.00}? No payment will be collected.", "Purchase", "Cancel")) store.TopUp(profile, pack);
    }

    [RelayCommand]
    private async Task Renew(EsimProfile? profile)
    {
        if (profile is not null) await Application.Current!.Windows[0].Page!.DisplayAlert("Renew plan", "Select a plan in Explore to simulate purchasing a new profile. Renewal of an existing profile is not simulated.", "OK");
    }
}
