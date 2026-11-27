using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eSIMDataManager.Models;
using eSIMDataManager.Services;

namespace eSIMDataManager.ViewModels;

public partial class ProfilesViewModel : ObservableObject
{
    private readonly EsimStore store;
    private readonly IDialogService dialogService;
    private readonly INavigationService navigationService;

    public TopUpViewModel TopUp { get; }

    public ObservableCollection<EsimProfile> ActiveProfiles { get; } = [];
    public ObservableCollection<EsimProfile> ReadyProfiles { get; } = [];
    public ObservableCollection<EsimProfile> ExpiredProfiles { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedProfile))]
    [NotifyPropertyChangedFor(nameof(IsSelectedProfileReady))]
    [NotifyPropertyChangedFor(nameof(IsSelectedProfileActive))]
    [NotifyPropertyChangedFor(nameof(IsSelectedProfileExpired))]
    [NotifyPropertyChangedFor(nameof(SelectedProfileStatusText))]
    private EsimProfile? selectedProfile;

    [ObservableProperty]
    private EsimProfile? qrProfile;

    [ObservableProperty]
    private bool isQrPopupVisible;

    public bool HasSelectedProfile => SelectedProfile is not null;
    public bool IsSelectedProfileReady => SelectedProfile?.Status == ProfileStatus.ReadyToInstall;
    public bool IsSelectedProfileActive => SelectedProfile?.Status == ProfileStatus.Active;
    public bool IsSelectedProfileExpired => SelectedProfile?.Status == ProfileStatus.Expired;
    public string SelectedProfileStatusText => SelectedProfile?.Status switch
    {
        ProfileStatus.Active => "Active",
        ProfileStatus.ReadyToInstall => "Ready to install",
        ProfileStatus.Expired => "Expired",
        _ => "—"
    };

    public ProfilesViewModel(
        EsimStore store,
        IDialogService dialogService,
        INavigationService navigationService,
        TopUpViewModel topUp)
    {
        this.store = store;
        this.dialogService = dialogService;
        this.navigationService = navigationService;
        TopUp = topUp;

        store.Changed += (_, _) => RefreshProfiles();
        RefreshProfiles();
    }

    public void RefreshProfiles()
    {
        SyncCollection(ActiveProfiles, store.Profiles.Where(p => p.Status == ProfileStatus.Active));
        SyncCollection(ReadyProfiles, store.Profiles.Where(p => p.Status == ProfileStatus.ReadyToInstall));
        SyncCollection(ExpiredProfiles, store.Profiles.Where(p => p.Status == ProfileStatus.Expired));

        if (SelectedProfile is null || !store.Profiles.Contains(SelectedProfile))
        {
            SelectedProfile = store.ActiveProfile ?? store.Profiles.FirstOrDefault();
        }
    }

    private static void SyncCollection<T>(ObservableCollection<T> target, IEnumerable<T> source) where T : class
    {
        var sourceList = source.ToList();
        for (int i = target.Count - 1; i >= 0; i--)
        {
            if (!sourceList.Contains(target[i]))
                target.RemoveAt(i);
        }
        for (int i = 0; i < sourceList.Count; i++)
        {
            var item = sourceList[i];
            var currentIndex = target.IndexOf(item);
            if (currentIndex == -1)
            {
                target.Insert(Math.Min(i, target.Count), item);
            }
            else if (currentIndex != i)
            {
                target.Move(currentIndex, i);
            }
        }
    }

    [RelayCommand]
    public void SelectProfile(EsimProfile? profile)
    {
        if (profile is not null)
            SelectedProfile = profile;
    }

    [RelayCommand]
    public async Task InstallProfile(EsimProfile? profile)
    {
        var target = profile ?? SelectedProfile;
        if (target is null || target.Status != ProfileStatus.ReadyToInstall) return;
        if (store.Activate(target))
        {
            await dialogService.ShowAlertAsync("Profile Activated", $"{target.ProfileName} is now active.");
        }
    }

    [RelayCommand]
    public async Task TopUpSelected()
    {
        await TopUp.OpenTopUp(SelectedProfile);
    }

    [RelayCommand]
    public async Task TopUpItem(EsimProfile? profile)
    {
        await TopUp.OpenTopUp(profile ?? SelectedProfile);
    }

    [RelayCommand]
    public async Task RenewSelected()
    {
        if (SelectedProfile is { } profile)
            await Renew(profile);
    }

    [RelayCommand]
    public async Task Renew(EsimProfile? profile)
    {
        if (profile is null) return;
        var confirmed = await dialogService.ShowConfirmationAsync(
            "Renew plan (demo)",
            $"Renew {profile.ProfileName} for another billing cycle?\n\nThis will reset consumed data to 0 GB and extend the expiration date by 14 days. Demo only — no payment is taken.",
            "Renew",
            "Cancel");

        if (confirmed)
        {
            store.Renew(profile);
            await dialogService.ShowAlertAsync(
                "Plan renewed",
                $"{profile.ProfileName} was renewed.\nRemaining data: {profile.RemainingGb:0.#} GB.\nExpires: {profile.ExpiryDate:MMM d, yyyy}.");
        }
    }

    [RelayCommand]
    public void ShowQr(EsimProfile? profile)
    {
        var target = profile ?? SelectedProfile;
        if (target is not null)
        {
            QrProfile = target;
            IsQrPopupVisible = true;
        }
    }

    [RelayCommand]
    public void CloseQr()
    {
        IsQrPopupVisible = false;
    }

    [RelayCommand]
    public async Task CopyIccid(EsimProfile? profile)
    {
        var iccid = profile?.Iccid ?? SelectedProfile?.Iccid;
        if (!string.IsNullOrEmpty(iccid))
        {
            await Clipboard.Default.SetTextAsync(iccid);
            await dialogService.ShowAlertAsync("Copied", $"ICCID copied to clipboard:\n{iccid}");
        }
    }

    [RelayCommand]
    public async Task CopySmDp()
    {
        const string smdp = "rsp.global-esim.net";
        await Clipboard.Default.SetTextAsync(smdp);
        await dialogService.ShowAlertAsync("Copied", $"SM-DP+ address copied:\n{smdp}");
    }

    [RelayCommand]
    public async Task CopyActivationCode()
    {
        const string code = "LPA:1$rsp.global-esim.net$JAP-89182390";
        await Clipboard.Default.SetTextAsync(code);
        await dialogService.ShowAlertAsync("Copied", $"Activation code copied:\n{code}");
    }

    [RelayCommand]
    public async Task NavigateToStore()
    {
        await navigationService.NavigateToStoreAsync();
    }

    [RelayCommand]
    public async Task NavigateToDashboard()
    {
        await navigationService.NavigateToDashboardAsync();
    }
}
