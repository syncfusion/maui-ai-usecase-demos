using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Todo.Models.DTOs;
using Todo.Services.Infrastructure;
using Todo.Services.Interfaces;
using Todo.Utilities;
using Enums = Todo.Utilities.Enums;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the splash screen (Task 2-02).  Runs on app startup,
/// checks the persisted authentication state through
/// <see cref="IAuthenticationService"/> and routes the user to either the
/// task list (already signed-in) or the sign-in page (not signed-in).
///
/// The work is kicked off from <see cref="StartAsync"/> which the page
/// invokes from <c>OnAppearing</c>.  The check is wrapped in a 5-second
/// timeout so the user never sees a permanently spinning indicator, and
/// the outcome is reported through the bindable <see cref="StatusMessage"/>,
/// <see cref="IsLoading"/>, <see cref="HasError"/> and
/// <see cref="ErrorMessage"/> properties.
/// </summary>
public sealed partial class SplashPageViewModel : ViewModelBase
{
    /// <summary>
    /// Maximum amount of time the splash screen will wait for the auth
    /// check to complete before timing out.  Matches the "Timeout after
    /// 5 seconds shows error" acceptance criterion.
    /// </summary>
    public static readonly TimeSpan AuthCheckTimeout =
            TimeSpan.FromSeconds(Constants.Timeouts.SplashAuthCheckSeconds);

    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;

    private CancellationTokenSource? _activeRunCts;
    private bool _hasStarted;

    /// <summary>
    /// Short, user-visible status text shown under the loading indicator
    /// (for example "Checking your session…").
    /// </summary>
    [ObservableProperty]
    private string statusMessage = string.Empty;

    /// <summary>
    /// Controls the visibility of <see cref="StatusMessage"/> independently
    /// of <see cref="IsLoading"/>.  Set to <see langword="true"/> once the
    /// status text has a meaningful value to display.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowStatusMessage))]
    private bool hasStatusMessage;

    /// <summary>
    /// Convenience accessor used by the View to bind the
    /// <c>IsVisible</c> property on the status label.
    /// </summary>
    public bool ShowStatusMessage => HasStatusMessage && !string.IsNullOrEmpty(StatusMessage);

    /// <summary>
    /// Initialises a new instance of the <see cref="SplashPageViewModel"/>
    /// class.  All dependencies are resolved through the .NET MAUI
    /// dependency-injection container (see <c>MauiProgram</c>).
    /// </summary>
    /// <param name="authenticationService">
    /// Service used to inspect / restore the persisted session.
    /// </param>
    /// <param name="navigationService">
    /// Navigation service used to route to the post-splash page.
    /// </param>
    /// <param name="logger">Logger injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the injected dependencies is <see langword="null"/>.
    /// </exception>
    public SplashPageViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService,
        ILogger<SplashPageViewModel> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(authenticationService);
        ArgumentNullException.ThrowIfNull(navigationService);

        _authenticationService = authenticationService;
        _navigationService = navigationService;
        Title = "Todo";
    }

    /// <summary>
    /// Begins the startup auth check.  Safe to call multiple times —
    /// subsequent invocations are no-ops while a check is already in
    /// flight, so re-appearing the page (for example after the user backs
    /// out of an unauthenticated area) does not retrigger the flow.
    /// </summary>
    public void StartAsync()
    {
        if (_hasStarted)
        {
            Logger.LogDebug("Splash startup already in progress; ignoring duplicate StartAsync");
            return;
        }

        _hasStarted = true;
        // Fire-and-forget — exceptions are surfaced through the bound
        // error properties on the UI thread.
        _ = RunStartupAsync();
    }

    /// <summary>
    /// Executes the startup flow: a non-blocking session restore wrapped in
    /// a 5-second timeout, then navigation to the resolved destination.
    /// </summary>
    private async Task RunStartupAsync()
    {
        CancellationTokenSource cts = new();
        _activeRunCts = cts;
        CancellationToken cancellationToken = cts.Token;

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;
        SetStatus("Checking your session…");

        try
        {
            Result<UserDto> result = await CheckSessionWithTimeoutAsync(cts)
                .ConfigureAwait(true);

            if (cancellationToken.IsCancellationRequested || _activeRunCts != cts)
            {
                // A newer run has started or the page was dismissed; abort
                // silently.
                return;
            }

            if (result.IsSuccess)
            {
                SetStatus("Welcome back!");
                await NavigateAfterSuccessAsync().ConfigureAwait(true);
                return;
            }

            await NavigateAfterNoSessionAsync(result.ErrorMessage).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            if (_activeRunCts == cts)
            {
                Logger.LogWarning("Splash auth check timed out after {Timeout}", AuthCheckTimeout);
                await HandleTimeoutAsync().ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during splash startup");
            if (_activeRunCts == cts)
            {
                await HandleUnexpectedErrorAsync(ex).ConfigureAwait(true);
            }
        }
        finally
        {
            if (_activeRunCts == cts)
            {
                _activeRunCts = null;
                IsLoading = false;
            }

            cts.Dispose();
        }
    }

    /// <summary>
    /// Runs <see cref="IAuthenticationService.RestoreSessionAsync"/> under
    /// a hard timeout.  Returns a failed <see cref="Result{T}"/> when the
    /// timeout elapses so the caller can treat the situation uniformly.
    /// </summary>
    /// <param name="cts">
    /// The owning <see cref="CancellationTokenSource"/> for this run; used
    /// to cancel the in-flight authentication work on timeout.
    /// </param>
    private async Task<Result<UserDto>> CheckSessionWithTimeoutAsync(CancellationTokenSource cts)
    {
        ArgumentNullException.ThrowIfNull(cts);
        CancellationToken cancellationToken = cts.Token;

        Task<Result<UserDto>> authTask = _authenticationService
            .RestoreSessionAsync(cancellationToken);

        Task completed = await Task
            .WhenAny(authTask, Task.Delay(AuthCheckTimeout, cancellationToken))
            .ConfigureAwait(true);

        if (completed != authTask)
        {
            // Timeout: cancel the underlying work so it stops doing
            // work even though we discard its result.
            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Already disposed — nothing to do.
            }
            catch (AggregateException)
            {
                // The cancellation token threw back an aggregate; safe
                // to swallow because we are about to surface the
                // OperationCanceledException below.
            }
            throw new OperationCanceledException("Auth check timed out", cancellationToken);
        }

        return await authTask.ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the task list once we know the user is authenticated.
    /// </summary>
    private async Task NavigateAfterSuccessAsync()
    {
        Logger.LogInformation("User is signed in; navigating to task list");
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.TaskList, clearStack: true)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the sign-in page when no valid session was found.
    /// </summary>
    /// <param name="reason">
    /// Optional reason (typically the <see cref="Result{T}.ErrorMessage"/>
    /// returned by <see cref="IAuthenticationService.RestoreSessionAsync"/>)
    /// recorded for diagnostics.
    /// </param>
    private async Task NavigateAfterNoSessionAsync(string? reason)
    {
        if (!string.IsNullOrEmpty(reason))
        {
            Logger.LogInformation("No active session ({Reason}); routing to sign-in", reason);
        }
        else
        {
            Logger.LogInformation("No active session; routing to sign-in");
        }

        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.SignIn, clearStack: true)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Surface a friendly timeout error to the user and fall back to the
    /// sign-in screen so the user is never stranded on the splash page.
    /// </summary>
    private async Task HandleTimeoutAsync()
    {
        HasError = true;
        ErrorMessage = "We couldn't verify your session in time. Please try again.";
        SetStatus(string.Empty);

        Logger.LogInformation("Falling back to sign-in after timeout");
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.SignIn, clearStack: true)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Surface a generic error to the user and route to sign-in so the
    /// app remains usable.
    /// </summary>
    private async Task HandleUnexpectedErrorAsync(Exception ex)
    {
        HasError = true;
        ErrorMessage = "Something went wrong while starting the app. Please try again.";
        SetStatus(string.Empty);

        Logger.LogDebug("Routing to sign-in after unexpected error: {Message}", ex.Message);
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.SignIn, clearStack: true)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Helper that updates the bound status text.  The companion
    /// <see cref="HasStatusMessage"/> flag is kept in sync through the
    /// <see cref="OnStatusMessageChanged"/> partial method below so the View
    /// can bind the label's <c>IsVisible</c> property to a single source of
    /// truth.
    /// </summary>
    /// <param name="message">The status text to display.</param>
    private void SetStatus(string message)
    {
        StatusMessage = message;
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit.Mvvm source
    /// generator whenever <see cref="StatusMessage"/> changes.  Keeps
    /// <see cref="HasStatusMessage"/> consistent with the underlying text.
    /// </summary>
    partial void OnStatusMessageChanged(string value)
    {
        HasStatusMessage = !string.IsNullOrEmpty(value);
    }
}
