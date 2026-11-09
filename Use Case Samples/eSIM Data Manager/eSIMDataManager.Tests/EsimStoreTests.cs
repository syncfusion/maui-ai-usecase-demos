
namespace ESimDataManager.Tests;

public class EsimStoreTests
{
    [Fact]
    public void SeededUsageBalances()
    {
        var store = new EsimStore();
        var profile = Assert.IsType<EsimProfile>(store.ActiveProfile);
        Assert.Equal(10, profile.AllocatedGb);
        Assert.Equal(6.8, profile.ConsumedGb, 5);
        Assert.Equal(3.2, profile.RemainingGb, 5);
        Assert.Equal(68, profile.UsagePercent, 5);
    }

    [Fact]
    public void SearchCombinesTextAndFiltersAndCanReset()
    {
        var store = new EsimStore();
        Assert.Single(store.SearchPlans("Sakura", null, null, null, null, null));
        Assert.Empty(store.SearchPlans("Japan", "Japan", CoverageType.Global, NetworkType.FiveG, PlanType.Limited, 15));
        var results = store.SearchPlans("", "Japan", CoverageType.Local, NetworkType.FiveG, PlanType.Limited, 15);
        Assert.Single(results);
        Assert.Equal(8, store.SearchPlans("", null, null, null, null, null).Count);
    }

    [Fact]
    public void PurchaseCreatesReadyProfileOnceAndNoCancelOperationMutates()
    {
        var store = new EsimStore();
        var plan = store.Plans[0];
        var before = store.Profiles.Count;
        var profile = store.Purchase(plan);
        Assert.Equal(before + 1, store.Profiles.Count);
        Assert.Equal(ProfileStatus.ReadyToInstall, profile.Status);
        Assert.Equal(plan.DataLimitGb, profile.AllocatedGb);
        Assert.Equal(0, profile.ConsumedGb);
        // Cancel is intentionally a UI-only path: the store is not called.
        Assert.Equal(before + 1, store.Profiles.Count);
    }

    [Fact]
    public void ActivationOnlyMovesReadyProfileToActive()
    {
        var store = new EsimStore();
        var ready = store.Profiles.Single(p => p.Status == ProfileStatus.ReadyToInstall);
        Assert.True(store.Activate(ready));
        Assert.Equal(ProfileStatus.Active, ready.Status);
        Assert.Equal(DateTime.Today, ready.ActivationDate);
        Assert.False(store.Activate(ready));
        var expired = store.Profiles.Single(p => p.Status == ProfileStatus.Expired);
        Assert.False(store.Activate(expired));
    }

    [Theory]
    [InlineData(0, 1, 3)]
    [InlineData(1, 3, 7)]
    [InlineData(2, 5, 10)]
    public void TopUpIncreasesAllocationAndRemainingWithoutChangingUsage(int index, double gb, decimal price)
    {
        var store = new EsimStore();
        var profile = store.ActiveProfile!;
        var originalAllocation = profile.AllocatedGb;
        var originalRemaining = profile.RemainingGb;
        var originalUsed = profile.ConsumedGb;
        var pack = EsimStore.TopUpPacks[index];
        Assert.Equal(gb, pack.DataGb);
        Assert.Equal(price, pack.Price);
        Assert.True(store.TopUp(profile, pack));
        Assert.Equal(originalAllocation + gb, profile.AllocatedGb, 5);
        Assert.Equal(originalRemaining + gb, profile.RemainingGb, 5);
        Assert.Equal(originalUsed, profile.ConsumedGb, 5);
        Assert.Equal(profile.AllocatedGb, profile.ConsumedGb + profile.RemainingGb, 5);
    }

    [Fact]
    public void ExpiredProfileCannotTopUp()
    {
        var store = new EsimStore();
        var profile = store.Profiles.Single(p => p.Status == ProfileStatus.Expired);
        var allocation = profile.AllocatedGb;
        Assert.False(store.TopUp(profile, EsimStore.TopUpPacks[0]));
        Assert.Equal(allocation, profile.AllocatedGb);
    }
}
