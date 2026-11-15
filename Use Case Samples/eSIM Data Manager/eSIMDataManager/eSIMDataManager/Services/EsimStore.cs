using System.Collections.ObjectModel;
using eSIMDataManager.Models;

namespace eSIMDataManager.Services;

public sealed class EsimStore
{
    public ObservableCollection<EsimProfile> Profiles { get; } = [];
    public IReadOnlyList<Plan> Plans { get; } =
    [
        new("jp-5-15", "Japan", "Moshi Moshi 5G", "SoftBank Corp.", 5, 15, 8.50m, NetworkType.FiveG, CoverageType.Local, PlanType.Limited),
        new("jp-unl-7", "Japan", "Tokyo Unlimited Pass", "NTT Docomo", 0, 7, 14m, NetworkType.FiveG, CoverageType.Local, PlanType.Unlimited),
        new("us-10-30", "United States", "Coast-to-Coast 5G", "T-Mobile / AT&T Auto", 10, 30, 18m, NetworkType.FiveG, CoverageType.Local, PlanType.Limited),
        new("eu-20-30", "Europe Regional", "EuroConnect 33 Nations", "Orange & Vodafone", 20, 30, 26.50m, NetworkType.FiveG, CoverageType.Regional, PlanType.Limited),
        new("kr-5-10", "South Korea", "Seoul FastPass 5G", "SK Telecom", 5, 10, 9.20m, NetworkType.FiveG, CoverageType.Local, PlanType.Limited),
        new("fr-10-15", "France", "Bonjour High-Speed", "Bouygues Telecom", 10, 15, 12m, NetworkType.FiveG, CoverageType.Local, PlanType.Limited),
    ];
    public event EventHandler? Changed;
    public EsimProfile? SelectedProfile { get; set; }
    public EsimProfile? ActiveProfile => Profiles.FirstOrDefault(p => p.Status == ProfileStatus.Active);

    public EsimStore()
    {
        var active = new EsimProfile
        {
            Id = "demo-jp-active", ProfileName = "Japan 5G Explorer", Country = "Japan", Carrier = "SoftBank Crop", Network = NetworkType.FiveG,
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
        Profiles.Add(new EsimProfile
        {
            Id = "demo-th-expired", ProfileName = "Thailand Unlimited", Country = "Thailand", Carrier = "AIS / TrueMove (demo)", Network = NetworkType.FourG,
            Status = ProfileStatus.Expired, Iccid = "DEMO-8931-0201", ActivationCode = "LPA:1$demo.carrier.example$SAMPLETH000",
            AllocatedGb = 15, ConsumedGb = 15, ActivationDate = DateTime.Today.AddDays(-120), ExpiryDate = DateTime.Today.AddDays(-90)
        });
        Profiles.Add(new EsimProfile
        {
            Id = "demo-it-expired", ProfileName = "Italy City Explorer", Country = "Italy", Carrier = "Roma Mobile (demo)", Network = NetworkType.FourG,
            Status = ProfileStatus.Expired, Iccid = "DEMO-8931-0202", ActivationCode = "LPA:1$demo.carrier.example$SAMPLEIT000",
            AllocatedGb = 3, ConsumedGb = 2.7, ActivationDate = DateTime.Today.AddDays(-75), ExpiryDate = DateTime.Today.AddDays(-60)
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

    public static IReadOnlyList<TopUpPack> TopUpPacks { get; } =
    [
        new(1, 3, 7, "Quick boost for messaging & maps"),
        new(3, 7, 15, "Ideal for social media & navigation", "Most Popular", true),
        new(5, 10, 30, "Generous allowance for streaming & work", "Best Value ($2/GB)")
    ];
}
