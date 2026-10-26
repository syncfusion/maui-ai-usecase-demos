using System.ComponentModel;

namespace AIChatSample.Controls;

public partial class AIChatNavigationPanel : ContentView, INotifyPropertyChanged
{
    public static readonly BindableProperty IsNewChatVisibleProperty = CreateVisibilityProperty(nameof(IsNewChatVisible));
    public static readonly BindableProperty IsSearchVisibleProperty = CreateVisibilityProperty(nameof(IsSearchVisible));
    public static readonly BindableProperty IsLibraryVisibleProperty = CreateVisibilityProperty(nameof(IsLibraryVisible));
    public static readonly BindableProperty IsAgentVisibleProperty = CreateVisibilityProperty(nameof(IsAgentVisible));
    public static readonly BindableProperty IsRecentChatsVisibleProperty = CreateVisibilityProperty(nameof(IsRecentChatsVisible));
    public static readonly BindableProperty IsProfileVisibleProperty = CreateVisibilityProperty(nameof(IsProfileVisible));

    public static readonly BindableProperty IsCompactProperty =
        BindableProperty.Create(
            nameof(IsCompact),
            typeof(bool),
            typeof(AIChatNavigationPanel),
            false,
            propertyChanged: OnIsCompactChanged);

    public static readonly BindableProperty InitialSelectedItemProperty =
        BindableProperty.Create(
            nameof(InitialSelectedItem),
            typeof(string),
            typeof(AIChatNavigationPanel),
            "NewChat",
            propertyChanged: OnInitialSelectedItemChanged);

    private string selectedItem = "NewChat";

    public AIChatNavigationPanel()
    {
        // Use the shared app shell view-model registered in DI so
        // Recent chats, the user profile and the active session stay
        // in sync with the host page. Fall back to a
        // local instance in design / preview contexts.
        BindingContext = ServiceHelper.GetService<AIMainLayoutViewModel>() ?? new AIMainLayoutViewModel();
        InitializeComponent();
        Application.Current!.RequestedThemeChanged += OnRequestedThemeChanged;
        UpdatePrimarySelectionState();
        UpdateLayoutState();
    }

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public string InitialSelectedItem
    {
        get => (string)GetValue(InitialSelectedItemProperty);
        set => SetValue(InitialSelectedItemProperty, value);
    }

    public bool IsFull => !IsCompact;

    /// <summary>
    /// Width of the drawer column. Desktop keeps the drawer permanently
    /// visible (60px collapsed / 250px expanded). Android uses an
    /// overlay drawer: the column is 0 while collapsed (content fills
    /// the screen) and grows to 250 while expanded.
    /// </summary>

    public GridLength NavigationColumnWidth
    {
        get
        {
#if ANDROID || IOS
            return new GridLength(0);
#else
            return IsCompact ? new GridLength(60) : new GridLength(250);
#endif
        }
    }

    /// <summary>
    /// Grid column the content sits in. Desktop: column 1 (drawer is
    /// always at column 0). Android collapsed: column 0 spanning both
    /// columns, because the drawer is zero-width. Android expanded:
    /// column 1 — the drawer takes column 0, so the content MUST move
    /// right or its (full-width) grid would sit on top of the drawer and
    /// swallow every tap meant for the drawer's list rows.
    /// </summary>
    public int ContentColumn => IsAndroid ? (IsCompact ? 0 : 1) : 1;

    /// <summary>
    /// Number of grid columns the content spans. Two only when the
    /// Android drawer is collapsed and the column has zero width;
    /// otherwise the content occupies exactly one column.
    /// </summary>
    public int ContentColumnSpan => IsAndroid ? (IsCompact ? 2 : 1) : 1;

    public bool IsToolbarNavigationVisible => IsAndroid && IsCompact;

    /// <summary>
    /// True when the toolbar hamburger should be shown so the user can
    /// toggle the side drawer. On non-Android platforms the drawer is
    /// always available, so the hamburger is always visible. On Android
    /// the drawer is overlaid, so the hamburger only shows when the
    /// drawer is collapsed (compact mode).
    /// </summary>
    public bool IsNavigationMenuVisible => !IsAndroid || IsCompact;

    private bool IsAndroid => DeviceInfo.Platform == DevicePlatform.Android;

    public bool IsNewChatVisible { get => (bool)GetValue(IsNewChatVisibleProperty); set => SetValue(IsNewChatVisibleProperty, value); }
    public bool IsSearchVisible { get => (bool)GetValue(IsSearchVisibleProperty); set => SetValue(IsSearchVisibleProperty, value); }
    public bool IsLibraryVisible { get => (bool)GetValue(IsLibraryVisibleProperty); set => SetValue(IsLibraryVisibleProperty, value); }
    public bool IsAgentVisible { get => (bool)GetValue(IsAgentVisibleProperty); set => SetValue(IsAgentVisibleProperty, value); }
    public bool IsRecentChatsVisible { get => (bool)GetValue(IsRecentChatsVisibleProperty); set => SetValue(IsRecentChatsVisibleProperty, value); }
    public bool IsProfileVisible { get => (bool)GetValue(IsProfileVisibleProperty); set => SetValue(IsProfileVisibleProperty, value); }

    public bool IsAgentSectionVisible => IsFull && IsAgentVisible;
    public bool IsRecentChatsSectionVisible => IsFull && IsRecentChatsVisible;
    public bool IsProfileSectionVisible => IsFull && IsProfileVisible;

    public LayoutOptions HorizontalAlignment =>
        IsCompact ? LayoutOptions.Center : LayoutOptions.Start;

    public Thickness ItemPadding => IsCompact ? new Thickness(0) : new Thickness(11, 0);

    public Thickness HeaderItemMargin => IsCompact ? new Thickness(0) : new Thickness(8, 0, 0, 0);

    public double ItemWidth => IsCompact ? 18 : -1;

    public LayoutOptions ItemHorizontalOptions =>
        IsCompact ? LayoutOptions.Center : LayoutOptions.Fill;

    public Color NewChatBackgroundColor => GetItemBackgroundColor("NewChat");
    public Color SearchBackgroundColor => GetItemBackgroundColor("Search");
    public Color LibraryBackgroundColor => GetItemBackgroundColor("Library");
    public Color Agent1BackgroundColor => GetItemBackgroundColor("Agent1");
    public Color Agent2BackgroundColor => GetItemBackgroundColor("Agent2");
    public Color Agent3BackgroundColor => GetItemBackgroundColor("Agent3");
    public Color RecentChat1BackgroundColor => GetItemBackgroundColor("RecentChat1");
    public Color RecentChat2BackgroundColor => GetItemBackgroundColor("RecentChat2");
    public Color RecentChat3BackgroundColor => GetItemBackgroundColor("RecentChat3");

    public Color RecentChat4BackgroundColor => GetItemBackgroundColor("RecentChat4");
    public Color ProfileBackgroundColor => GetItemBackgroundColor("Profile");

    public event EventHandler<bool>? CompactStateChanged;

    private static void OnIsCompactChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AIChatNavigationPanel panel)
        {
            panel.UpdateLayoutState();
            panel.CompactStateChanged?.Invoke(panel, panel.IsCompact);
        }
    }

    private static void OnInitialSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AIChatNavigationPanel panel)
        {
            panel.selectedItem = (string)newValue;
            panel.UpdatePrimarySelectionState();
            panel.UpdateSelectionState();
        }
    }

    private void UpdateLayoutState()
    {
        WidthRequest = IsAndroid ? (IsCompact ? 0 : 250) : (IsCompact ? 60 : 250);
        IsVisible = !IsAndroid || !IsCompact;
        OnPropertyChanged(nameof(NavigationColumnWidth));
        OnPropertyChanged(nameof(ContentColumn));
        OnPropertyChanged(nameof(ContentColumnSpan));
        OnPropertyChanged(nameof(IsToolbarNavigationVisible));
        OnPropertyChanged(nameof(IsNavigationMenuVisible));
        OnPropertyChanged(nameof(IsFull));
        OnPropertyChanged(nameof(HorizontalAlignment));
        OnPropertyChanged(nameof(ItemPadding));
        OnPropertyChanged(nameof(HeaderItemMargin));
        OnPropertyChanged(nameof(ItemWidth));
        OnPropertyChanged(nameof(ItemHorizontalOptions));
        OnPropertyChanged(nameof(IsAgentSectionVisible));
        OnPropertyChanged(nameof(IsRecentChatsSectionVisible));
        OnPropertyChanged(nameof(IsProfileSectionVisible));
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        OnPropertyChanged(nameof(NewChatBackgroundColor));
        OnPropertyChanged(nameof(SearchBackgroundColor));
        OnPropertyChanged(nameof(LibraryBackgroundColor));
        OnPropertyChanged(nameof(Agent1BackgroundColor));
        OnPropertyChanged(nameof(Agent2BackgroundColor));
        OnPropertyChanged(nameof(Agent3BackgroundColor));
        OnPropertyChanged(nameof(RecentChat1BackgroundColor));
        OnPropertyChanged(nameof(RecentChat2BackgroundColor));
        OnPropertyChanged(nameof(RecentChat3BackgroundColor));
        OnPropertyChanged(nameof(RecentChat4BackgroundColor));
        OnPropertyChanged(nameof(ProfileBackgroundColor));
    }

    private void UpdatePrimarySelectionState()
    {
        if (BindingContext is AIChatNavigationViewModel viewModel)
        {
            var selectedColor = GetSelectedItemBackgroundColor();
            foreach (var item in viewModel.PrimaryItems)
            {
                item.BackgroundColor = item.IsHovered
                    ? GetHoverBackgroundColor()
                    : item.Key == selectedItem ? selectedColor : Colors.Transparent;
            }
        }
    }

    private static Color GetHoverBackgroundColor() =>
        Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#35303C")
            : Color.FromArgb("#F3EFF4");

    private Color GetItemBackgroundColor(string itemKey) =>
        selectedItem == itemKey
            ? GetSelectedItemBackgroundColor()
            : Colors.Transparent;

    private static Color GetSelectedItemBackgroundColor()
    {
        var resourceKey = Application.Current?.RequestedTheme == AppTheme.Dark
            ? "UnSelectedDotDark"
            : "UnSelectedDotLight";

        return Application.Current?.Resources[resourceKey] as Color ?? Colors.Transparent;
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        UpdatePrimarySelectionState();
        UpdateSelectionState();
    }

    /// <summary>
    /// Raised when the user taps a navigation destination in the panel
    /// (Profile, Library, NewAgent, Search, New chat, ...). The host page
    /// listens to this and pushes the appropriate page onto the
    /// <see cref="NavigationPage"/> stack.
    /// <para>The optional <c>parameter</c> carries any extra payload
    /// (e.g. the <see cref="RecentChatItem"/> that the user tapped).
    /// Listeners that don't need it can ignore the parameter.
    /// </para>
    /// </summary>
    public event EventHandler<NavigationRequestEventArgs>? NavigationRequested;

    private void RaiseNavigation(string key, object? parameter = null)
    {
        selectedItem = key;
        UpdatePrimarySelectionState();
        UpdateSelectionState();
        NavigationRequested?.Invoke(this, new NavigationRequestEventArgs(key, parameter));
    }

    private void MenuTapped(object sender, TappedEventArgs e)
    {
        ToggleCompact();
    }

    public void ToggleCompact()
    {
        IsCompact = !IsCompact;
    }

    private void NavigationItemTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string itemKey)
        {
            RaiseNavigation(itemKey);
        }
    }

    private void PrimaryItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is AIChatNavigationItem item)
        {
            RaiseNavigation(item.Key);
        }
    }

    private void PrimaryItemPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border { BindingContext: AIChatNavigationItem item })
        {
            item.IsHovered = true;
            UpdatePrimarySelectionState();
        }
    }

    private void PrimaryItemPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border { BindingContext: AIChatNavigationItem item })
        {
            item.IsHovered = false;
            UpdatePrimarySelectionState();
        }
    }

    private void AgentExpanded(object sender, EventArgs e) => SelectItem("Agent");

    private void AgentCollapsed(object sender, EventArgs e) => SelectItem("Agent");

    private void RecentChatsExpanded(object sender, EventArgs e) => SelectItem("RecentChats");

    private void RecentChatsCollapsed(object sender, EventArgs e) => SelectItem("RecentChats");

    private void SelectItem(string itemKey)
    {
        selectedItem = itemKey;
        UpdateSelectionState();
    }

    private void NewAgentButtonClicked(object? sender, EventArgs e)
    {
        RaiseNavigation("NewAgent");
    }

    private void RecentChatItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is RecentChatItem item)
        {
            RaiseNavigation("OpenChat", item);
        }
    }

    private void AgentItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is AgentItem item)
        {
            RaiseNavigation("OpenAgentChat", item);
        }
    }

    private static BindableProperty CreateVisibilityProperty(string propertyName) =>
        BindableProperty.Create(propertyName, typeof(bool), typeof(AIChatNavigationPanel), true);
}

/// <summary>
/// Event args for <see cref="AIChatNavigationPanel.NavigationRequested"/>.
/// Carries the navigation key (e.g. <c>NewChat</c>, <c>OpenChat</c>, <c>Profile</c>)
/// </summary>
public sealed record NavigationRequestEventArgs(string Key, object? Parameter = null);
