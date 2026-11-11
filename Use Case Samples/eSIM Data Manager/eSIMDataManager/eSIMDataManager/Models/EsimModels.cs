namespace eSIMDataManager.Models;

public enum CoverageType { Local, Regional, Global }
public enum PlanType { Limited, Unlimited }
public enum NetworkType { FourG, FiveG }
public enum ProfileStatus { Active, ReadyToInstall, Expired }

public sealed record Plan(string Id, string Country, string Region, string Provider, double DataLimitGb, int ValidityDays, decimal Price, NetworkType Network, CoverageType Coverage, PlanType Type)
{
    public string DataLabel => Type == PlanType.Unlimited ? "Unlimited" : $"{DataLimitGb:0.#} GB";
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
    public List<DailyUsage> DailyUsage { get; init; } = [];
}

public sealed record DailyUsage(string Day, double UsedGb);
public sealed record UsageSegment(string Label, double Value);
public sealed record TopUpPack(double DataGb, decimal Price);
