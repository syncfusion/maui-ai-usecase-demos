using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Todo.Models.DTOs;
using Todo.Services.Infrastructure;
using Todo.Services.Interfaces;
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
    private readonly IAuthenticationService? _authenticationService;

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
    /// <remarks>
    /// The property is kept in sync with the
    /// <see cref="IAuthenticationService.SignedInChanged"/> event so the
    /// Shell reflects token refresh / expiry / sign-out transitions
    /// without the page-level code having to nudge it manually.
    /// </remarks>
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
    /// <param name="authenticationService">
    /// Authentication service used to subscribe to the
    /// <see cref="IAuthenticationService.SignedInChanged"/> event so the
    /// Shell stays in sync with the auth state.  When <see langword="null"/>
    /// (for example in unit tests that do not need an auth service) the
    /// subscription is skipped and <see cref="IsAuthenticated"/> must be
    /// driven manually through <see cref="MarkAuthenticated"/> /
    /// <see cref="MarkUnauthenticated"/>.
    /// </param>
    public AppShellViewModel(
        ILogger<AppShellViewModel> logger,
        INavigationService navigationService,
        IAuthenticationService? authenticationService = null)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        _navigationService = navigationService;
        _authenticationService = authenticationService!;

        // Seed the cached flag with the current auth state so the Shell
        // does not flash the "unauthenticated" chrome between launch and
        // the first call to <see cref="InitializeAsync"/>.
        if (_authenticationService is not null)
        {
            IsAuthenticated = _authenticationService.IsSignedIn;

            // Keep the flag in sync with every auth state transition
            // (sign-in, sign-out, transparent token refresh, refresh
            // failure → forced sign-out, etc.).  Task 2-06 made this
            // subscription mandatory: the splash page no longer calls
            // <see cref="MarkAuthenticated"/> manually because it cannot
            // know whether a silent token refresh succeeded.
            _authenticationService.SignedInChanged += OnAuthenticationServiceSignedInChanged;
        }
    }

    /// <summary>
    /// Handler for <see cref="IAuthenticationService.SignedInChanged"/>.
    /// Updates <see cref="IsAuthenticated"/> and routes the user back to
    /// the sign-in page when the transition is a sign-out (or a forced
    /// sign-out triggered by a failed refresh).
    /// </summary>
    private void OnAuthenticationServiceSignedInChanged(object? sender, UserDto? user)
    {
        bool nowAuthenticated = user is not null;
        if (nowAuthenticated == IsAuthenticated)
        {
            // No-op: the only thing that can change in this branch is the
            // UserDto identity (after a sign-in of a *different* user
            // through the same auth service instance), and we do not
            // expose that detail at the Shell level today.
            return;
        }

        IsAuthenticated = nowAuthenticated;

        if (!nowAuthenticated)
        {
            Logger.LogInformation("SignedInChanged fired with null user; routing to sign-in");
            // Best-effort navigation.  We deliberately swallow any
            // navigation failure because the page-level code that handled
            // the original sign-out is responsible for the redirect; we
            // only handle the case where the auth service alone initiated
            // the sign-out (e.g. a failed token refresh from a background
            // page).
            _ = _navigationService.NavigateToAsync(Enums.NavigationTarget.SignIn, clearStack: true);
        }
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
    /// command on successful authentication (Module 2).  The flag is also
    /// driven automatically through the <see cref="IAuthenticationService.SignedInChanged"/>
    /// subscription — the manual call is kept for callers that want to
    /// short-circuit the wait for the event to be raised.
    /// </summary>
    public void MarkAuthenticated()
    {
        IsAuthenticated = true;
    }

    /// <summary>
    /// Marks the current user as not authenticated.  Inverse of
    /// <see cref="MarkAuthenticated"/>; primarily useful in unit tests
    /// where the auth service subscription has not been wired up.
    /// </summary>
    public void MarkUnauthenticated()
    {
        IsAuthenticated = false;
    }

    /// <inheritdoc />
    partial void OnCanGoBackChanged(bool value)
    {
        GoBackCommand.NotifyCanExecuteChanged();
    }
}
