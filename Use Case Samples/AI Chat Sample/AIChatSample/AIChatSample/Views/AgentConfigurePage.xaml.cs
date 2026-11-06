using AIChatSample.Controls;

namespace AIChatSample;

public partial class AgentConfigurePage : ContentPage
{
    /// <summary>Backing view-model (constructor-injectable for tests).</summary>
    internal AgentConfigurePageViewModel Vm { get; }

    /// <summary>DI/fallback constructor — resolves shared services.</summary>
    public AgentConfigurePage()
        : this(new AgentConfigurePageViewModel(
            ServiceHelper.GetRequiredService<IAgentService>(),
            ServiceHelper.GetRequiredService<IAgentStore>()))
    {
    }

    /// <summary>Preferred constructor — used by DI and unit tests.</summary>
    internal AgentConfigurePage(AgentConfigurePageViewModel viewModel)
    {
        InitializeComponent();
        Vm = viewModel;
        // Composite VM: the XAML binds to a flat shape (AgentName,
        // Instructions, SelectedTab, ...) but the navigation panel
        // still needs the AIMainLayoutViewModel for sidebar state.
        BindingContext = Vm;
        Vm.AgentPopUpVM.RequestClose += OnAgentLogoPopupRequestClose;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is null)
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
        }
        else
        {
            AIChatNavigationPanel.NavigationRequested -= OnNavigationRequested;
            AIChatNavigationPanel.NavigationRequested += OnNavigationRequested;
        }
    }

    protected override void OnDisappearing()
    {
        Vm.AgentPopUpVM.RequestClose -= OnAgentLogoPopupRequestClose;
        base.OnDisappearing();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Vm.AgentPopUpVM.RequestClose -= OnAgentLogoPopupRequestClose;
        Vm.AgentPopUpVM.RequestClose += OnAgentLogoPopupRequestClose;
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

    private void OnConfigureTabTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is AgentConfigurePageViewModel vm)
        {
            vm.SelectedTab = 0;
        }
    }

    private void OnPreviewTabTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is AgentConfigurePageViewModel vm)
        {
            vm.SelectedTab = 1;
        }
    }

    /// <summary>
    /// Bridges <see cref="Syncfusion.Maui.Core.SfChipGroup.SelectionChanged"/>
    /// to the view-model's <c>SelectedTab</c> so the configure/preview
    /// sections swap in step with the chip selection.
    /// </summary>
    private void OnModeChipSelectionChanged(object? sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        if (BindingContext is AgentConfigurePageViewModel vm && e.AddedItem is string label)
        {
            vm.SelectedChipText = label;
        }
    }

    /// <summary>
    /// Opens the agent-logo popup so the user can customize the
    /// avatar (icon + color, or an uploaded image). The popup is
    /// re-bound to the same shared <see cref="AgentLogoPopupViewModel"/>
    /// the page is already pointing at, so Apply writes flow back
    /// to <c>AgentAvatar</c> without an extra round-trip.
    /// </summary>
    private async void OnAgentAvatarTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not AgentConfigurePageViewModel vm)
        {
            return;
        }

        vm.AgentPopUpVM.AgentAvatar = vm.AgentAvatar;
        vm.AgentPopUpVM.ResetForOpen();

        var popup = new AgentLogoPage(vm.AgentPopUpVM);
        await Navigation.PushAsync(popup, false);
    }

    private async void OnAgentLogoPopupRequestClose(object? sender, EventArgs e)
    {
        if (Navigation?.ModalStack.Count > 1)
        {
            await Navigation.PopAsync(animated: true);
        }
    }

    /// <summary>
    /// Promotes the inline name-required error as soon as the user
    /// gives the Name field focus. Without this,
    /// the error would only surface after a failed Create — which
    /// is fine but later than the form-validation feedback the
    /// Figma flow expects.
    /// </summary>
    private void OnAgentNameFocused(object? sender, FocusEventArgs e)
    {
        if (BindingContext is AgentConfigurePageViewModel vm)
        {
            vm.NotifyNameInteracted();
        }
    }

    /// <summary>
    /// Re-evaluates the inline name-required error when the user
    /// leaves the field. If they tabbed through and left it empty,
    /// the error now appears; if they typed something the error
    /// stays hidden.
    /// </summary>
    private void OnAgentNameUnfocused(object? sender, FocusEventArgs e)
    {
        if (BindingContext is AgentConfigurePageViewModel vm)
        {
            vm.NotifyNameInteracted();
        }
    }

    private async void OnCreateAgentClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not AgentConfigurePageViewModel vm || !vm.IsCreateEnabled)
        {
            return;
        }

        // Run the MVVM command so validation, busy state, and the store
        // are handled exactly as in unit tests.
        await vm.AgentVM.CreateAgentCommand.ExecuteAsync(null);

        if (!vm.CreatedSuccessfully)
        {
            // Surface inline error message when creation failed.
            await DisplayAlertAsync("Can't create agent", vm.ErrorMessage ?? "Please try again.", "OK");
            return;
        }

        SemanticScreenReader.Announce($"Agent {vm.AgentVM.AgentName} created");

        // Surface a tiny confirmation then go back to the
        // create-agent page so the user can keep iterating.
        if (Application.Current?.MainPage is Page host)
        {
            await host.DisplayAlertAsync("Agent created",
                $"Specialist agent '{vm.AgentVM.AgentName}' is ready.", "OK");
        }

        if (Navigation?.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync(animated: true);
        }
    }
}