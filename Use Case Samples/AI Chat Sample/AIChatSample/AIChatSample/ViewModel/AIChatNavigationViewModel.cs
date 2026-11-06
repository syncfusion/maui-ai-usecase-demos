using System.Collections.ObjectModel;


namespace AIChatSample;

/// <summary>
/// Backing data for the primary items in the <c>AIChatNavigationPanel</c>
/// ("New chat", "Search", "Library"). Tapping a row raises a
/// <c>NavigationRequested</c> event on the panel, which routes through
/// <c>ChatNavigationService</c>.
/// </summary>
public sealed class AIChatNavigationViewModel
{
    /// <summary>Primary items rendered at the top of the navigation drawer.</summary>
    public ObservableCollection<AIChatNavigationItem> PrimaryItems { get; } =
    [
        new AIChatNavigationItem { Key = "NewChat", Title = "New chat", Icon = "\ue70d", IconFontFamily = "MaterialAssets" },
        new AIChatNavigationItem { Key = "Search", Title = "Search", Icon = "\ue715", IconFontFamily = "MaterialAssets" },
        new AIChatNavigationItem { Key = "Library", Title = "Library", Icon = "\ue712", IconFontFamily = "MaterialAssets" }
    ];
}
