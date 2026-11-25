using Syncfusion.Maui.Themes;

namespace MauiNotes.Services.Theming;

public sealed class SyncfusionThemeService : IThemeService
{
    public void Apply(AppTheme theme)
    {
        if (Application.Current is null)
            return;

        var merged = Application.Current.Resources.MergedDictionaries;
        var current = merged.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
        if (current is not null)
            merged.Remove(current);

        merged.Add(new SyncfusionThemeResourceDictionary
        {
            VisualTheme = theme == AppTheme.Dark
                ? SfVisuals.CupertinoDark
                : SfVisuals.CupertinoLight
        });
    }
}
