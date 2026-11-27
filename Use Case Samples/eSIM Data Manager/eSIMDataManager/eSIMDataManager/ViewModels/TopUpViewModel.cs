using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eSIMDataManager.Models;
using eSIMDataManager.Services;

namespace eSIMDataManager.ViewModels;

public partial class TopUpViewModel : ObservableObject
{
    private readonly EsimStore store;
    private readonly IDialogService dialogService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TopUpProfileName))]
    [NotifyPropertyChangedFor(nameof(TopUpCarrierName))]
    [NotifyPropertyChangedFor(nameof(TopUpRemainingText))]
    [NotifyPropertyChangedFor(nameof(TopUpDaysLeftText))]
    private EsimProfile? topUpProfile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTopUpPack))]
    [NotifyPropertyChangedFor(nameof(TopUpAllowanceSummary))]
    [NotifyPropertyChangedFor(nameof(TopUpSubmitLabel))]
    private double selectedTopUpGb = 3;

    [ObservableProperty]
    private bool isTopUpPopupVisible;

    public IReadOnlyList<TopUpPack> TopUpPacks => EsimStore.TopUpPacks;
    public TopUpPack SelectedTopUpPack => TopUpPacks.FirstOrDefault(p => Math.Abs(p.DataGb - SelectedTopUpGb) < 0.01) ?? TopUpPacks[1];

    public string TopUpProfileName => TopUpProfile?.ProfileName ?? "No active profile";
    public string TopUpCarrierName => TopUpProfile?.Carrier ?? "—";
    public string TopUpRemainingText => $"{TopUpProfile?.RemainingGb ?? 0:0.#} GB remaining";
    public string TopUpDaysLeftText => $"{TopUpProfile?.DaysRemaining ?? 0} Days Left";
    public string TopUpAllowanceSummary => $"{SelectedTopUpPack.DataLabel} Data Pool ({SelectedTopUpPack.PriceText})";
    public string TopUpSubmitLabel => $"Purchase Top-Up • {SelectedTopUpPack.PriceText}";

    public TopUpViewModel(EsimStore store, IDialogService dialogService)
    {
        this.store = store;
        this.dialogService = dialogService;
    }

    [RelayCommand]
    public void SelectTopUpPack(TopUpPack? pack)
    {
        if (pack is null) return;
        SelectedTopUpGb = pack.DataGb;
    }

    public bool IsTopUpSelected(TopUpPack? pack) =>
        pack is not null && Math.Abs(pack.DataGb - SelectedTopUpGb) < 0.01;

    [RelayCommand]
    public async Task OpenTopUp(EsimProfile? profile)
    {
        var target = profile ?? store.ActiveProfile;
        if (target is null)
        {
            await dialogService.ShowAlertAsync("No Profile", "Please select an active eSIM profile to top up.");
            return;
        }

        if (target.Status != ProfileStatus.Active)
        {
            await dialogService.ShowAlertAsync(
                "Top-up unavailable",
                target.Status == ProfileStatus.Expired ? "Expired demo profiles cannot be topped up." : "Activate this demo profile before topping up.");
            return;
        }

        TopUpProfile = target;
        SelectedTopUpGb = 3;
        IsTopUpPopupVisible = true;
    }

    [RelayCommand]
    public void CloseTopUp()
    {
        IsTopUpPopupVisible = false;
    }

    [RelayCommand]
    public async Task ConfirmTopUp()
    {
        if (TopUpProfile is null || TopUpProfile.Status != ProfileStatus.Active) return;
        var pack = SelectedTopUpPack;
        var before = TopUpProfile.RemainingGb;

        if (store.TopUp(TopUpProfile, pack))
        {
            CloseTopUp();
            await dialogService.ShowAlertAsync(
                "Top-up added",
                $"{pack.DataLabel} was added to {TopUpProfile.ProfileName}. Remaining data is now {TopUpProfile.RemainingGb:0.#} GB (was {before:0.#} GB).\n\nDemo only — no payment was taken and no carrier was contacted.");
        }
    }

    [RelayCommand]
    public async Task ChangePaymentMethod()
    {
        await dialogService.ShowAlertAsync(
            "Demo payment method",
            "The sample top-up always uses the demo card ending in 4242. No real payment details are collected and no card is charged.");
    }
}
