using CommunityToolkit.Mvvm.ComponentModel;
using Todo.Features.Notes.Models;

namespace Todo.Features.Settings;

/// <summary>
/// Application-level settings: theme mode and the notification master
/// switch. Registered as a singleton so the Settings screen and any
/// future notification service share one source of truth.
/// </summary>
public partial class AppSettings : ObservableObject
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private AppThemeMode themeMode = AppThemeMode.System;

    /// <summary>
    /// Master switch for OS-level notifications. Bound to the
    /// <c>SfSwitch</c> on the Settings screen.
    /// </summary>
    [ObservableProperty]
    private bool notificationsEnabled = true;

    public AppSettings(IThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>
    /// Applies the current <see cref="ThemeMode"/> to the running app.
    /// </summary>
    public void ApplyTheme() =>
        _themeService.ApplyTheme(ThemeModeToAppTheme(ThemeMode));

    private static AppTheme ThemeModeToAppTheme(AppThemeMode mode) => mode switch
    {
        AppThemeMode.Light => AppTheme.Light,
        AppThemeMode.Dark => AppTheme.Dark,
        _ => AppTheme.System
    };
}

/// <summary>
/// Abstraction over <see cref="App.Current"/> theming so the settings
/// model stays UI-agnostic and testable.
/// </summary>
public interface IThemeService
{
    void ApplyTheme(AppTheme theme);
}

public sealed class MauiThemeService : IThemeService
{
    public void ApplyTheme(AppTheme theme) => App.Current?.ApplyTheme(theme);
}
