namespace HomeInventoryManager.Model;

/// <summary>
/// Routes for the shared sidebar. Set on the page-level instance when hosting
/// the sidebar so it knows which entry to render in the active style.
/// </summary>
public enum NavigationTab
{
	Dashboard,
	Inventory,
	Scan,
	Warranty
}

public static class NavigationTabExtensions
{
    public static string ToRoute(this NavigationTab tab) => tab switch
    {
        NavigationTab.Dashboard => "//Dashboard",
        NavigationTab.Inventory   => "//Inventory",
        NavigationTab.Scan      => "//Scan",
        NavigationTab.Warranty     => "//Warranty",
        _ => "//Dashboard"
	};
}