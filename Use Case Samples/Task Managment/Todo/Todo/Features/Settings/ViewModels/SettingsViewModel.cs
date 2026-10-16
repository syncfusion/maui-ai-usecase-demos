using CommunityToolkit.Mvvm.ComponentModel;
using Todo.Common;

namespace Todo.Features.Settings.ViewModels;

/// <summary>
/// ViewModel for the Settings page (matches the design intent for a
/// Microsoft To Do style settings sheet): notification toggle and
/// theme picker (Light / Dark / System). Both values are exposed as
/// observable properties so future pages can bind to them.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool notificationsEnabled = true;

    [ObservableProperty]
    private AppThemePreference themePreference = AppThemePreference.System;
}

/// <summary>
/// Available app theme options surfaced in the settings sheet. The
/// <see cref="System"/> option follows the device's current
/// <see cref="Application.Current.RequestedTheme"/>.
/// </summary>
public enum AppThemePreference
{
    Light,
    Dark,
    System
}
