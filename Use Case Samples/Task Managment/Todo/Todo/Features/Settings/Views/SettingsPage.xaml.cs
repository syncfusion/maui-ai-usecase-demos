using Todo.Common;
using Todo.Features.Settings.ViewModels;

namespace Todo.Features.Settings.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage()
    {
        InitializeComponent();
        _viewModel = new SettingsViewModel();
        BindingContext = _viewModel;

        // Restore the previously-chosen theme into the segmented control.
        int idx = _viewModel.ThemePreference switch
        {
            AppThemePreference.Light => 0,
            AppThemePreference.Dark => 1,
            _ => 2
        };
        ThemeSegment.SelectedIndex = idx;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        if (BindingContext is not SettingsViewModel vm) return;
        var newValue = ThemeSegment.SelectedIndex switch
        {
            0 => AppThemePreference.Light,
            1 => AppThemePreference.Dark,
            _ => AppThemePreference.System
        };
        vm.ThemePreference = newValue;
        ApplyTheme(newValue);
    }

    private static void ApplyTheme(AppThemePreference pref)
    {
        if (Application.Current is null) return;
        Application.Current.UserAppTheme = pref switch
        {
            AppThemePreference.Light => AppTheme.Light,
            AppThemePreference.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}
