using eSIMDataManager.Models;
using eSIMDataManager.Services;
using Xunit;

namespace eSIMDataManager.Tests;

public sealed class EsimStoreTests
{
    [Fact]
    public void SeededProfileHasConsistentUsageAndSevenDayTrend()
    {
        var store = new EsimStore();
        var profile = Assert.IsType<EsimProfile>(store.ActiveProfile);
        Assert.Equal(3.2, profile.RemainingGb, 3);
        Assert.Equal(68, profile.UsagePercent, 3);
        Assert.Equal(7, profile.DailyUsage.Count);
        Assert.Equal(DateTime.Today.AddDays(6), profile.ExpiryDate);
    }

    [Fact]
    public void SearchAndCombinedFiltersAreCaseInsensitive()
    {
        var store = new EsimStore();
        var search = store.SearchPlans("sakura", null, null, NetworkType.FiveG, PlanType.Limited, 15);
        Assert.Single(search);
        Assert.Equal("Japan", search[0].Country);
        Assert.Empty(store.SearchPlans("nomatch", null, null, null, null, null));
        Assert.Equal(8, store.SearchPlans("", null, null, null, null, null).Count);
        Assert.Single(store.SearchPlans("", "Japan", CoverageType.Local, NetworkType.FiveG, PlanType.Limited, 15));
    }

    [Fact]
    public void PurchaseCreatesOneReadyProfileAndDoesNotAffectOtherProfiles()
    {
        var store = new EsimStore();
        var initial = store.Profiles.Count;
        var profile = store.Purchase(store.Plans[0]);
        Assert.Equal(initial + 1, store.Profiles.Count);
        Assert.Equal(ProfileStatus.ReadyToInstall, profile.Status);
        Assert.Equal(5, profile.RemainingGb);
        Assert.Equal(initial + 1, store.Profiles.Count(p => p.Status == ProfileStatus.ReadyToInstall));
    }

    [Fact]
    public void ActivationChangesOnlyEligibleReadyProfile()
    {
        var store = new EsimStore();
        var ready = store.Profiles.Single(p => p.Status == ProfileStatus.ReadyToInstall);
        Assert.True(store.Activate(ready));
        Assert.Equal(ProfileStatus.Active, ready.Status);
        Assert.Equal(DateTime.Today, ready.ActivationDate);
        Assert.False(store.Activate(ready));
        Assert.False(store.Activate(store.Profiles.Single(p => p.Status == ProfileStatus.Expired)));
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(3, 7)]
    [InlineData(5, 10)]
    public void TopUpIncreasesAllocationAndRemainingExactly(double data, decimal price)
    {
        var store = new EsimStore();
        var active = store.ActiveProfile!;
        var beforeRemaining = active.RemainingGb;
        var beforeAllocated = active.AllocatedGb;
        Assert.True(store.TopUp(active, new TopUpPack(data, price)));
        Assert.Equal(beforeAllocated + data, active.AllocatedGb);
        Assert.Equal(beforeRemaining + data, active.RemainingGb);
        Assert.Equal(active.AllocatedGb, active.ConsumedGb + active.RemainingGb, 8);
    }

    [Fact]
    public void ExpiredProfileCannotBeToppedUp()
    {
        var store = new EsimStore();
        var expired = store.Profiles.Single(p => p.Status == ProfileStatus.Expired);
        var allocation = expired.AllocatedGb;
        Assert.False(store.TopUp(expired, EsimStore.TopUpPacks[0]));
        Assert.Equal(allocation, expired.AllocatedGb);
    }
}
