using System.Collections.ObjectModel;
using eSIMDataManager.Models;

namespace eSIMDataManager.Services;

public sealed class EsimStore
{
    public ObservableCollection<EsimProfile> Profiles { get; } = [];
    public IReadOnlyList<Plan> Plans { get; } =
    [
        new("jp-5-15", "Japan", "Asia", "Sakura Connect (demo)", 5, 15, 8.50m, NetworkType.FiveG, CoverageType.Local, PlanType.Limited),
        new("us-10-30", "USA", "North America", "Liberty Connect (demo)", 10, 30, 18m, NetworkType.FiveG, CoverageType.Local, PlanType.Limited),
        new("fr-5-7", "France", "Europe", "HexaTel (demo)", 5, 7, 7m, NetworkType.FourG, CoverageType.Local, PlanType.Limited),
        new("kr-10-15", "Korea", "Asia", "SeoulWave (demo)", 10, 15, 12m, NetworkType.FiveG, CoverageType.Local, PlanType.Limited),
        new("sg-3-7", "Singapore", "Asia", "LionCity Data (demo)", 3, 7, 5m, NetworkType.FiveG, CoverageType.Local, PlanType.Limited),
        new("th-5-15", "Thailand", "Asia", "Siam Connect (demo)", 5, 15, 6.50m, NetworkType.FourG, CoverageType.Local, PlanType.Limited),
        new("asia-unl-15", "Asia Regional", "Asia", "RoamSphere (demo)", 0, 15, 29m, NetworkType.FiveG, CoverageType.Regional, PlanType.Unlimited),
        new("global-5-30", "Global", "Worldwide", "WorldLink (demo)", 5, 30, 24m, NetworkType.FourG, CoverageType.Global, PlanType.Limited),
    ];
    public event EventHandler? Changed;
    public EsimProfile? SelectedProfile { get; set; }
    public EsimProfile? ActiveProfile => Profiles.FirstOrDefault(p => p.Status == ProfileStatus.Active);

    public EsimStore()
    {
        var active = new EsimProfile
        {
            Id = "demo-jp-active", ProfileName = "Japan Unlimited 5G", Country = "Japan", Carrier = "Sakura Connect (demo)", Network = NetworkType.FiveG,
            Status = ProfileStatus.Active, Iccid = "DEMO-8931-0248", ActivationCode = "LPA:1$demo.carrier.example$SAMPLEJP001",
            AllocatedGb = 10, ConsumedGb = 6.8, ActivationDate = DateTime.Today.AddDays(-4), ExpiryDate = DateTime.Today.AddDays(6),
            DailyUsage = [new("Mon", .8), new("Tue", 1.1), new("Wed", .9), new("Thu", 1.4), new("Fri", 1.0), new("Sat", .9), new("Sun", .7)]
        };
        Profiles.Add(active);
        Profiles.Add(new EsimProfile
        {
            Id = "demo-fr-ready", ProfileName = "France City Break", Country = "France", Carrier = "HexaTel (demo)", Network = NetworkType.FourG,
            Status = ProfileStatus.ReadyToInstall, Iccid = "DEMO-8931-0249", ActivationCode = "LPA:1$demo.carrier.example$SAMPLEFR001",
            AllocatedGb = 5, ConsumedGb = 0, ExpiryDate = DateTime.Today.AddDays(15)
        });
        Profiles.Add(new EsimProfile
        {
            Id = "demo-us-expired", ProfileName = "USA Road Trip", Country = "USA", Carrier = "Liberty Connect (demo)", Network = NetworkType.FiveG,
            Status = ProfileStatus.Expired, Iccid = "DEMO-8931-0200", ActivationCode = "LPA:1$demo.carrier.example$SAMPLEUS000",
            AllocatedGb = 5, ConsumedGb = 5, ActivationDate = DateTime.Today.AddDays(-40), ExpiryDate = DateTime.Today.AddDays(-10)
        });
        SelectedProfile = active;
    }

    public IReadOnlyList<Plan> SearchPlans(string search, string? country, CoverageType? coverage, NetworkType? network, PlanType? type, int? days)
    {
        var query = search.Trim();
        return Plans.Where(p => (string.IsNullOrEmpty(query) || p.Country.Contains(query, StringComparison.OrdinalIgnoreCase) || p.Region.Contains(query, StringComparison.OrdinalIgnoreCase) || p.Provider.Contains(query, StringComparison.OrdinalIgnoreCase))
            && (string.IsNullOrEmpty(country) || p.Country.Equals(country, StringComparison.OrdinalIgnoreCase))
            && (coverage is null || p.Coverage == coverage) && (network is null || p.Network == network)
            && (type is null || p.Type == type) && (days is null || p.ValidityDays == days)).ToList();
    }

    public EsimProfile Purchase(Plan plan)
    {
        var profile = new EsimProfile
        {
            Id = $"demo-{Profiles.Count + 1:D3}", ProfileName = $"{plan.Country} {plan.Network} Traveler", Country = plan.Country, Carrier = plan.Provider,
            Network = plan.Network, Status = ProfileStatus.ReadyToInstall, Iccid = $"DEMO-8931-{Profiles.Count + 250:D4}", ActivationCode = $"LPA:1$demo.carrier.example$SAMPLE{plan.Id.ToUpperInvariant().Replace('-', '0')}",
            AllocatedGb = plan.Type == PlanType.Unlimited ? 0 : plan.DataLimitGb, ConsumedGb = 0, ExpiryDate = DateTime.Today.AddDays(plan.ValidityDays)
        };
        Profiles.Add(profile);
        SelectedProfile = profile;
        Changed?.Invoke(this, EventArgs.Empty);
        return profile;
    }

    public bool Activate(EsimProfile profile)
    {
        if (profile.Status != ProfileStatus.ReadyToInstall) return false;
        profile.Status = ProfileStatus.Active;
        profile.ActivationDate = DateTime.Today;
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool TopUp(EsimProfile profile, TopUpPack pack)
    {
        if (profile.Status != ProfileStatus.Active) return false;
        profile.AllocatedGb += pack.DataGb;
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public static IReadOnlyList<TopUpPack> TopUpPacks { get; } = [new(1, 3), new(3, 7), new(5, 10)];
}
