using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AIChatSample;

/// <summary>
/// View-model for the Profile page. Owns the user-identity block, the
/// plan card, and the connected-services list shown on the page, plus
/// the connect / disconnect / sign-out commands.
/// </summary>
public partial class ProfileSettingsViewModel : ObservableObject
{
    /// <summary>Profile header — display name.</summary>
    [ObservableProperty] private string? displayName;

    /// <summary>Profile header — contact email.</summary>
    [ObservableProperty] private string? emailAddress;

    /// <summary>Profile header — account type (e.g. "Google Account").</summary>
    [ObservableProperty] private string? accountType;

    /// <summary>Profile header — avatar image file name.</summary>
    [ObservableProperty] private string? avatarSource;

    /// <summary>Plan name (e.g. "Enterprise Pro").</summary>
    [ObservableProperty] private string? planName;

    /// <summary>Plan billing cadence (e.g. "Billed annually").</summary>
    [ObservableProperty] private string? billingCycle;

    /// <summary>Free-form feature bullets under the plan card.</summary>
    public ObservableCollection<string> PlanFeatures { get; }

    /// <summary>Connected services list (Google Drive, OneDrive, …).</summary>
    public ObservableCollection<ConnectedService> ConnectedServices { get; }

    /// <summary>Drawer / responsive layout state (kept here for parity with the XAML template).</summary>
    [ObservableProperty] private bool isDrawerOpen;

    /// <summary>Seeds the profile with demo data and the default connected-services rows.</summary>
    public ProfileSettingsViewModel()
    {
        // --- Profile header ---
        DisplayName = "Alexa Developer";
        EmailAddress = "alex.dev@example.com";
        AccountType = "Google Account";
        AvatarSource = "userprofile.png";

        // --- Plan ---
        PlanName = "Enterprise Pro";
        BillingCycle = "Billed annually";
        PlanFeatures = new ObservableCollection<string>
        {
            "Unlimited AI Chats",
            "Priority Processing",
        };

        // --- Connected services ---
        ConnectedServices = new ObservableCollection<ConnectedService>
        {
            new() { Name = "Google Drive", IconSource = "googledrive.png", IsConnected = true, ActionText = "" },
            new() { Name = "OneDrive", IconSource = "onedrive.png", IsConnected = false, ActionText = "Connect" },
        };
    }

    /// <summary>No-op stub for the "Edit profile" command.</summary>
    [RelayCommand] private void EditProfile() { }

    /// <summary>No-op stub for the "Manage billing" command.</summary>
    [RelayCommand] private void ManageBilling() { }

    /// <summary>Toggles the supplied service to the "disconnected" state.</summary>
    [RelayCommand] private void DisconnectService(ConnectedService svc)
    {
        // Toggle connection off in place
        var idx = ConnectedServices.IndexOf(svc);
        if (idx >= 0)
        {
            ConnectedServices[idx] = new ConnectedService
            {
                Name = svc.Name,
                IconSource = svc.IconSource,
                IsConnected = false,
                ActionText = "Connect",
            };
        }
    }

    /// <summary>Toggles the supplied service to the "connected" state.</summary>
    [RelayCommand] private void ConnectService(ConnectedService svc)
    {
        var idx = ConnectedServices.IndexOf(svc);
        if (idx >= 0)
        {
            ConnectedServices[idx] = new ConnectedService
            {
                Name = svc.Name,
                IconSource = svc.IconSource,
                IsConnected = true,
                ActionText = "",
            };
        }
    }

    /// <summary>No-op stub for the "Request data export" command.</summary>
    [RelayCommand] private async Task RequestExportAsync() => await Task.CompletedTask;

    /// <summary>No-op stub for the "Sign out" command.</summary>
    [RelayCommand] private async Task SignOutAsync() => await Task.CompletedTask;

    /// <summary>No-op stub for the "Delete account" command.</summary>
    [RelayCommand] private async Task DeleteAccountAsync() => await Task.CompletedTask;
}
