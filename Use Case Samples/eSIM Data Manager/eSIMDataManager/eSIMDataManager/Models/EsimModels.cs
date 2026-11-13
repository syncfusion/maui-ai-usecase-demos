namespace eSIMDataManager.Models;

public enum CoverageType { Local, Regional, Global }
public enum PlanType { Limited, Unlimited }
public enum NetworkType { FourG, FiveG }
public enum ProfileStatus { Active, ReadyToInstall, Expired }

public sealed record Plan(string Id, string Country, string Region, string Provider, double DataLimitGb, int ValidityDays, decimal Price, NetworkType Network, CoverageType Coverage, PlanType Type)
{
    public string DataLabel => Type == PlanType.Unlimited ? "Unlimited" : $"{DataLimitGb:0.#} GB";
    public string CountryCode => Id switch
    {
        "jp-unl-7" => "JP", "us-10-30" => "US", "eu-20-30" => "EU", "kr-5-10" => "KR", "fr-10-15" => "FR", _ => "JP"
    };
    public string Badge => Id switch
    {
        "jp-5-15" => "★ Best Value", "jp-unl-7" => "◉ 5G Fast", "us-10-30" => "◈ 5G Ultra", "eu-20-30" => "♧ 33 Countries", "kr-5-10" => "♆ Ultra Latency", "fr-10-15" => "▥ 5G Ready", _ => "5G Fast"
    };
    public string NetworkLabel => Id switch
    {
        "jp-5-15" => "◢ 5G mmWave", "jp-unl-7" => "5G Fast", "us-10-30" => "5G Ultra", "eu-20-30" => "33 Countries", "kr-5-10" => "Ultra Latency", "fr-10-15" => "5G Ready", _ => "5G"
    };
    public string AllocationNote => Id switch
    {
        "jp-unl-7" => "3GB/day full-speed", "us-10-30" => "Nationwide 5G", "eu-20-30" => "Regional Data", _ => "High-Speed 5G"
    };
    public string ValidityNote => Id switch
    {
        "jp-unl-7" => "From scan", "us-10-30" => "Monthly flexible", "eu-20-30" => "Seamless cross-border", "kr-5-10" => "City & Provincial", "fr-10-15" => "France & Monaco", _ => "From activation"
    };
    public string FeatureList => Id switch
    {
        "jp-5-15" => "⇅  Instant QR Code delivery via email & app\n◉  Personal Hotspot & Tethering allowed\n▤  Local Japanese IP routing (Low ping)",
        "jp-unl-7" => "∞  Uncapped browsing after fair-use\n◉  Direct Docomo Tier-1 backbone\n▦  Instant 1-click activation",
        "us-10-30" => "➤  Dual network auto-switching\n◉  High-speed Hotspot enabled\n▦  Encrypted enterprise APN",
        "eu-20-30" => "⇄  Zero reconnection crossing borders\n◉  EU GDPR Compliant privacy routing\n▤  Full tethering support in all 33 states",
        "kr-5-10" => "⇅  Korean 5G Ultra-wideband\n▦  Instant e-delivery in < 30 seconds\n▤  Bilingual live chat setup guidance",
        "fr-10-15" => "◉  Unrestricted hotspot for laptops\n◉  Zero ID or passport verification\n☷  Pre-configured APN profile", _ => string.Empty
    };
}

public sealed class EsimProfile
{
    public required string Id { get; init; }
    public required string ProfileName { get; init; }
    public required string Country { get; init; }
    public required string Carrier { get; init; }
    public required NetworkType Network { get; init; }
    public required ProfileStatus Status { get; set; }
    public required string Iccid { get; init; }
    public required string ActivationCode { get; init; }
    public required double AllocatedGb { get; set; }
    public required double ConsumedGb { get; set; }
    public double RemainingGb => Math.Max(0, AllocatedGb - ConsumedGb);
    public DateTime? ActivationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int DaysRemaining => Math.Max(0, (ExpiryDate.Date - DateTime.Today).Days);
    public double UsagePercent => AllocatedGb <= 0 ? 0 : Math.Clamp(ConsumedGb / AllocatedGb * 100, 0, 100);
    public double UsageFraction => UsagePercent / 100;
    public List<DailyUsage> DailyUsage { get; init; } = [];
}

public sealed record DailyUsage(string Day, double UsedGb);

public sealed record TopUpPack(double DataGb, decimal Price, int ExtensionDays, string Description, string? Tag = null, bool IsPopular = false)
{
    public string DataLabel => $"{DataGb:0} GB";
    public string ExtensionLabel => $"+{ExtensionDays} Days extension";
    public string PriceText => $"${Price:0.00}";
    public string PricePerGbText => $"${(decimal)Price / (decimal)DataGb:0.00}/GB";
}
