using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Todo.Services.Infrastructure;
using Enums = Todo.Utilities.Enums;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the application-level <see cref="AppShell"/>.  Exposes the
/// navigation commands the Shell itself needs to evaluate (for example, the
/// back-button and tab-bar items) and forwards them to the injected
/// <see cref="INavigationService"/>.
/// </summary>
/// <remarks>
/// This ViewModel is intentionally kept small — it does not own page-level
/// state.  Per-page logic lives in dedicated ViewModels (e.g.
/// <c>SplashViewModel</c>, <c>SignInViewModel</c>) which are introduced by
/// later tasks (Module 2 — Authentication).  The Shell ViewModel is created
/// once at startup and re-used for the lifetime of the application.
/// </remarks>
public partial class AppShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Indicates whether the back gesture / button should be available.
    /// Drives the visibility of the back-arrow in the Shell chrome.
    /// </summary>
    [ObservableProperty]
    private bool canGoBack;

    /// <summary>
    /// Indicates whether the user is currently authenticated.  Bound by
    /// future tabs / flyout items to show or hide authenticated-only entries.
    /// </summary>
    [ObservableProperty]
    private bool isAuthenticated;

    /// <summary>
    /// Initialises a new instance of the <see cref="AppShellViewModel"/>
    /// class.
    /// </summary>
    /// <param name="logger">Logger injected by the DI container.</param>
    /// <param name="navigationService">
    /// Navigation service injected by the DI container.
    /// </param>
    public AppShellViewModel(
        ILogger<AppShellViewModel> logger,
        INavigationService navigationService)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        _navigationService = navigationService;
    }

    /// <summary>
    /// Initialises the Shell-level state.  Should be called once after the
    /// Shell has been created and rendered; subsequent calls are no-ops.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when initialisation is
    /// done.</returns>
    public Task InitializeAsync()
    {
        Logger.LogInformation("Initialising AppShellViewModel");
        CanGoBack = _navigationService.CanGoBack;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the cached <see cref="CanGoBack"/> state.  Wired to
    /// <see cref="Shell.Navigated"/> in <c>AppShell.xaml.cs</c>.
    /// </summary>
    public void RefreshNavigationState()
    {
        CanGoBack = _navigationService.CanGoBack;
    }

    /// <summary>
    /// Navigates back to the previous page, when one exists.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoBackInternal))]
    private async Task GoBackAsync()
    {
        Logger.LogDebug("Back command invoked");
        bool navigated = await _navigationService.GoBackAsync().ConfigureAwait(true);
        if (navigated)
        {
            RefreshNavigationState();
        }
    }

    /// <summary>
    /// Internal accessor used by the <see cref="GoBackCommand"/> to evaluate
    /// its <c>CanExecute</c> state.  Kept separate from the public
    /// <see cref="CanGoBack"/> property so the source generator can resolve
    /// it unambiguously.
    /// </summary>
    private bool CanGoBackInternal() => CanGoBack;

    /// <summary>
    /// Navigates to the sign-in page and clears the navigation stack so the
    /// user cannot return to the previous authenticated screen.
    /// </summary>
    [RelayCommand]
    private async Task SignOutAsync()
    {
        Logger.LogInformation("Signing out and navigating to sign-in");
        IsAuthenticated = false;
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.SignIn, clearStack: true)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Marks the current user as authenticated.  Called by the sign-in
    /// command on successful authentication (Module 2).
    /// </summary>
    public void MarkAuthenticated()
    {
        IsAuthenticated = true;
    }

    /// <inheritdoc />
    partial void OnCanGoBackChanged(bool value)
    {
        GoBackCommand.NotifyCanExecuteChanged();
    }
}
