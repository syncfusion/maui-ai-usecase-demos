using Syncfusion.Maui.Themes;

namespace MauiNotes.Services.Theming;

/// <summary>
/// Switches the Syncfusion visual theme when the OS theme changes so
/// AppThemeBinding colors and Syncfusion controls stay in sync.
/// </summary>
public sealed class SyncfusionThemeService : IThemeService
{
    public void Apply(AppTheme theme)
    {
        if (Application.Current?.Resources is not ResourceDictionary resources)
            return;

        ResourceDictionary? existing = resources.MergedDictionaries
            .OfType<SyncfusionThemeResourceDictionary>()
            .FirstOrDefault();
        if (existing is not null)
            resources.MergedDictionaries.Remove(existing);

        resources.MergedDictionaries.Add(new SyncfusionThemeResourceDictionary
        {
            VisualTheme = theme == AppTheme.Dark
                ? SfVisuals.CupertinoDark
                : SfVisuals.CupertinoLight
        });
    }
}
