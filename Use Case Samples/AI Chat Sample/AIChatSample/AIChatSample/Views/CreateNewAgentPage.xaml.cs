using AIChatSample.Controls;
namespace AIChatSample;

public partial class CreateNewAgentPage : ContentPage
{
    public CreateNewAgentPage()
    {
        InitializeComponent();
        this.BindingContext = new AIMainLayoutViewModel();
        if (BindingContext is AIMainLayoutViewModel vm)
        {
            vm.CreateAgentVM.DraftReady += OnDraftReady;
        }
    }

    private void OnDraftReady(object? sender, AgentDraft draft)
    {
        // Marshal to the UI thread so we can push pages safely.
        Dispatcher.Dispatch(() =>
        {
            ChatNavigationService.Handle(this, "CreateAgent", draft);
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
       // AIChatToolbar.IsBackVisible = Navigation?.NavigationStack.Count > 1;

        // Refresh the "My Agents" row so freshly created agents appear
        // whenever the user lands back on this page.
        if (BindingContext is AIMainLayoutViewModel vm)
        {
            vm.CreateAgentVM.SyncAgentsFromStore();
        }
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is null)
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
#if WINDOWS
            if (_themeChangedSubscribed && Application.Current is { } app)
                app.RequestedThemeChanged -= OnRequestedThemeChanged;

            _themeChangedSubscribed = false;
            _hoveredCards.Clear();

            foreach (var border in _cards)
            {
                border.Scale = 1.0;
                ApplyCardColors(border, isHovered: false);
            }
#endif
            return;
        }

        AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
        AIChatNavigationPanel.NavigationRequested += OnNavigationRequested;

#if WINDOWS
        if (!_themeChangedSubscribed && Application.Current is { } application)
        {
            application.RequestedThemeChanged += OnRequestedThemeChanged;
            _themeChangedSubscribed = true;
        }

        foreach (var border in _cards)
            ApplyCardColors(border, _hoveredCards.Contains(border));
#endif
    }

    private async void OnBackRequested(object? sender, EventArgs e)
    {
        if (Navigation?.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync(animated: true);
        }
    }

    private void OnNavigationRequested(object? sender, NavigationRequestEventArgs e)
    {
        ChatNavigationService.Handle(this, e.Key, e.Parameter);
    }

    private void AndroidMenuTapped(object? sender, EventArgs e)
    {
#if ANDROID
        AIChatNavigationPanel.ToggleCompact();
#endif
    }

    /// <summary>
    /// Fired when the user taps a card in the Template or My Agents rows.
    /// Pushes the agent-configure page so the user can fill in the
    /// details (name, description, instructions, ...). The tapped
    /// template is forwarded so the form can be pre-filled.
    /// </summary>
    private void CardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not TemplateCardSuggestion card)
        {
            return;
        }

        // Route through the shared service so we get the same "don't
        // push if already on top" behavior as the drawer taps; the
        // card flows through as the navigation parameter.
        ChatNavigationService.Handle(this, "CreateAgent", card);
    }
#if WINDOWS
private readonly HashSet<Border> _cards = new();
private readonly HashSet<Border> _hoveredCards = new();
private bool _themeChangedSubscribed;

private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
{
    foreach (var border in _cards)
        ApplyCardColors(border, _hoveredCards.Contains(border));
}

private static void ApplyCardColors(Border border, bool isHovered)
{
    var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

    border.BackgroundColor = isHovered
        ? (isDark ? Color.FromArgb("#313032") : Color.FromArgb("#F3EDFF"))
        : (isDark ? Color.FromArgb("#1C1B1D") : Colors.White);

    border.Stroke = isHovered
        ? Color.FromArgb("#6B21FF")
        : (isDark ? Color.FromArgb("#474648") : Color.FromArgb("#E5E1E3"));
}
#endif

    // Hover / press feedback for the template / my-agent cards. On Windows
    // we apply a colored border and a small scale animation; on other
    // platforms this is a no-op so the XAML handler reference still
    // resolves cleanly at compile time.
    private async void Item_PointerEntered(object? sender, PointerEventArgs e)
    {
#if WINDOWS
        if (sender is not Border border)
            return;

        _cards.Add(border);
        _hoveredCards.Add(border);
        ApplyCardColors(border, isHovered: true);

        await border.ScaleTo(1.03, 120);
#endif
    }

    private async void Item_PointerExited(object? sender, PointerEventArgs e)
    {
#if WINDOWS
        if (sender is not Border border)
            return;

        _cards.Add(border);
        _hoveredCards.Remove(border);
        ApplyCardColors(border, isHovered: false);

        await border.ScaleTo(1.0, 120);
#endif
    }
}