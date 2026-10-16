using System;
using Microsoft.Maui.Controls;
using Todo.Common;
using Todo.Features.Dialogs.ViewModels;
using Todo.Features.Dialogs.Views;
using Todo.Features.Settings.Views;

namespace Todo.Features.Profile.Views;

/// <summary>
/// Floating profile menu shown when the header avatar is tapped. Matches
/// the chrome in <c>profile_pressed.png</c>: header strip with name and
/// email, then three menu items (Manage Accounts, Settings, Sign Out).
/// Sign Out presents the confirmation dialog from
/// <c>signout.png</c>.
/// </summary>
public partial class ProfileMenuView : ContentView
{
    private readonly INavigationService _navigation;
    private readonly Action? _onDismiss;

    public ProfileMenuView(INavigationService navigation, Action? onDismiss = null)
    {
        InitializeComponent();
        _navigation = navigation;
        _onDismiss = onDismiss;
    }

    public event EventHandler? ManageAccountsRequested;
    public event EventHandler? SettingsRequested;

    private void OnManageAccountsClicked(object? sender, EventArgs e)
    {
        _onDismiss?.Invoke();
        ManageAccountsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        _onDismiss?.Invoke();
        SettingsRequested?.Invoke(this, EventArgs.Empty);
        // Push the settings page modally.
        var page = new SettingsPage();
        _ = _navigation.PushModalAsync(page);
    }

    private async void OnSignOutClicked(object? sender, EventArgs e)
    {
        _onDismiss?.Invoke();
        var vm = new ConfirmDialogViewModel(_navigation)
        {
            Title = "Sign out?",
            Message = "You will be signed out of your account.",
            CancelText = "Cancel",
            ConfirmText = "Sign out",
            IsDestructive = true
        };
        var page = new ConfirmDialogPage(vm);
        await _navigation.PushModalAsync(page);
    }
}
