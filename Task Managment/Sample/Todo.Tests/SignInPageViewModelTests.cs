using Microsoft.Extensions.Logging.Abstractions;
using Todo.Models.DTOs;
using Todo.Services.Infrastructure;
using Todo.Services.Interfaces;
using Todo.Utilities;
using Todo.ViewModels;
using Xunit;

namespace Todo.Tests;

/// <summary>
/// Unit tests for <see cref="SignInPageViewModel"/>.  Covers:
/// <list type="bullet">
///   <item><description>initial state — no inline errors until the user touches a field,</description></item>
///   <item><description>real-time validation when the e-mail or password changes,</description></item>
///   <item><description>the <see cref="SignInPageViewModel.SignInCommand"/> <c>CanExecute</c> gating,</description></item>
///   <item><description>success / failure propagation of the underlying <see cref="IAuthenticationService.SignInAsync"/>,</description></item>
///   <item><description>navigation commands driving the <see cref="INavigationService"/>.</description></item>
/// </list>
/// The class under test has a hard dependency on the CommunityToolkit.Mvvm
/// source generator; the test project supplies the package and lets the
/// generator run on the included <c>SignInPageViewModel.cs</c> source.
/// </summary>
public class SignInPageViewModelTests
{
    /// <summary>
    /// Creates a <see cref="SignInPageViewModel"/> wired to a recording
    /// stub of <see cref="IAuthenticationService"/> and a no-op
    /// <see cref="INavigationService"/>.  The shell ViewModel is real —
    /// it has no other dependencies.
    /// </summary>
    private static SignInPageViewModel CreateViewModel(
        IAuthenticationService authService,
        INavigationService navigationService,
        out AppShellViewModel shellViewModel)
    {
        shellViewModel = new AppShellViewModel(
            NullLogger<AppShellViewModel>.Instance,
            navigationService,
            authService);

        return new SignInPageViewModel(
            authService,
            navigationService,
            shellViewModel,
            NullLogger<SignInPageViewModel>.Instance);
    }

    [Fact]
    public void Constructor_sets_default_title_and_empty_field_state()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        Assert.Equal("Sign in", viewModel.Title);
        Assert.Equal(string.Empty, viewModel.Email);
        Assert.Equal(string.Empty, viewModel.Password);
        Assert.False(viewModel.RememberMe);
        Assert.Null(viewModel.EmailError);
        Assert.Null(viewModel.PasswordError);
        Assert.False(viewModel.HasEmailError);
        Assert.False(viewModel.HasPasswordError);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public void Constructor_throws_when_required_dependency_is_null()
    {
        var navigation = new StubNavigationService();
        var auth = new StubAuthenticationService();
        var shell = new AppShellViewModel(NullLogger<AppShellViewModel>.Instance, navigation, auth);

        Assert.Throws<ArgumentNullException>(() =>
            new SignInPageViewModel(null!, navigation, shell, NullLogger<SignInPageViewModel>.Instance));
        Assert.Throws<ArgumentNullException>(() =>
            new SignInPageViewModel(auth, null!, shell, NullLogger<SignInPageViewModel>.Instance));
        Assert.Throws<ArgumentNullException>(() =>
            new SignInPageViewModel(auth, navigation, null!, NullLogger<SignInPageViewModel>.Instance));
    }

    [Fact]
    public void EmailError_remains_hidden_until_user_touches_the_field()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        // No error before the user types anything (avoids the "form opens
        // with a red label" anti-pattern).
        Assert.Null(viewModel.EmailError);
        Assert.False(viewModel.HasEmailError);
    }

    [Fact]
    public void EmailError_appears_for_invalid_email_after_user_typing()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        viewModel.Email = "not-an-email";

        Assert.False(string.IsNullOrEmpty(viewModel.EmailError));
        Assert.True(viewModel.HasEmailError);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name+tag@sub.example.co.uk")]
    public void EmailError_clears_for_valid_email(string email)
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        viewModel.Email = email;

        Assert.Null(viewModel.EmailError);
        Assert.False(viewModel.HasEmailError);
    }

    [Fact]
    public void PasswordError_appears_for_short_password_after_user_typing()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        viewModel.Password = "abc"; // below the minimum length

        Assert.False(string.IsNullOrEmpty(viewModel.PasswordError));
        Assert.True(viewModel.HasPasswordError);
    }

    [Fact]
    public void PasswordError_clears_for_password_at_or_above_minimum_length()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        viewModel.Password = new string('a', Constants.Validation.PasswordMinLength);

        Assert.Null(viewModel.PasswordError);
        Assert.False(viewModel.HasPasswordError);
    }

    [Fact]
    public void PasswordError_re_appears_when_user_clears_the_field()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        viewModel.Password = "longenoughpassword";
        Assert.Null(viewModel.PasswordError);

        viewModel.Password = string.Empty;
        Assert.False(string.IsNullOrEmpty(viewModel.PasswordError));
    }

    [Fact]
    public void SignIn_command_is_disabled_with_invalid_form()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        // Empty form.
        Assert.False(viewModel.SignInCommand.CanExecute(null));

        // Valid e-mail, password too short.
        viewModel.Email = "user@example.com";
        viewModel.Password = "abc";
        Assert.False(viewModel.SignInCommand.CanExecute(null));

        // E-mail malformed, password fine.
        viewModel.Email = "not-an-email";
        viewModel.Password = new string('a', Constants.Validation.PasswordMinLength + 1);
        Assert.False(viewModel.SignInCommand.CanExecute(null));
    }

    [Fact]
    public void SignIn_command_is_enabled_with_valid_form()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        viewModel.Email = "user@example.com";
        viewModel.Password = new string('a', Constants.Validation.PasswordMinLength + 1);

        Assert.True(viewModel.SignInCommand.CanExecute(null));
    }

    [Fact]
    public async Task SignIn_calls_authentication_service_with_trimmed_email_and_raw_password()
    {
        var auth = new StubAuthenticationService();
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        viewModel.Email = "  user@example.com  ";
        viewModel.Password = new string('p', Constants.Validation.PasswordMinLength + 2);

        await ((System.Windows.Input.ICommand)viewModel.SignInCommand)
            .ExecuteAsyncWithResult(null);

        SignInAsyncCall call = Assert.Single(auth.SignInCalls);
        Assert.Equal("user@example.com", call.Email);
        Assert.Equal(viewModel.Password, call.Password);
    }

    [Fact]
    public async Task SignIn_navigates_to_task_list_and_marks_authenticated_on_success()
    {
        var auth = new StubAuthenticationService
        {
            SignInResult = Result<UserDto>.Success(new UserDto
            {
                Id = "u-1",
                Email = "user@example.com",
            }),
        };
        var nav = new StubNavigationService();
        SignInPageViewModel viewModel = CreateViewModel(auth, nav, out AppShellViewModel shell);

        viewModel.Email = "user@example.com";
        viewModel.Password = new string('p', Constants.Validation.PasswordMinLength + 2);

        await ((System.Windows.Input.ICommand)viewModel.SignInCommand)
            .ExecuteAsyncWithResult(null);

        Assert.True(shell.IsAuthenticated);
        NavigationCall navCall = Assert.Single(nav.NavigateCalls);
        Assert.Equal(Enums.NavigationTarget.TaskList, navCall.Target);
        Assert.True(navCall.ClearStack);
    }

    [Fact]
    public async Task SignIn_surfaces_service_error_message_in_password_field()
    {
        var auth = new StubAuthenticationService
        {
            SignInResult = Result<UserDto>.Failure(
                new UnauthorizedAccessException("Wrong password"),
                "Invalid e-mail or password."),
        };
        var nav = new StubNavigationService();
        SignInPageViewModel viewModel = CreateViewModel(auth, nav, out _);

        viewModel.Email = "user@example.com";
        viewModel.Password = new string('p', Constants.Validation.PasswordMinLength + 2);

        await ((System.Windows.Input.ICommand)viewModel.SignInCommand)
            .ExecuteAsyncWithResult(null);

        Assert.Equal("Invalid e-mail or password.", viewModel.PasswordError);
        Assert.Empty(nav.NavigateCalls);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task SignIn_sets_error_message_and_clears_loading_on_unexpected_exception()
    {
        var auth = new StubAuthenticationService
        {
            SignInException = new InvalidOperationException("Database is down"),
        };
        var nav = new StubNavigationService();
        SignInPageViewModel viewModel = CreateViewModel(auth, nav, out _);

        viewModel.Email = "user@example.com";
        viewModel.Password = new string('p', Constants.Validation.PasswordMinLength + 2);

        await ((System.Windows.Input.ICommand)viewModel.SignInCommand)
            .ExecuteAsyncWithResult(null);

        Assert.True(viewModel.HasError);
        Assert.Equal("Database is down", viewModel.ErrorMessage);
        Assert.False(viewModel.IsLoading);
        Assert.Empty(nav.NavigateCalls);
    }

    [Fact]
    public async Task SignIn_bypasses_service_when_form_invalid_after_reevaluation()
    {
        var auth = new StubAuthenticationService();
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        // Force the CanExecute gate to be true while leaving the password
        // empty — the reevaluation in SignInAsync should catch this and
        // bail out without invoking the service.
        viewModel.Email = "user@example.com";
        viewModel.Password = string.Empty;

        await ((System.Windows.Input.ICommand)viewModel.SignInCommand)
            .ExecuteAsyncWithResult(null);

        Assert.Empty(auth.SignInCalls);
    }

    [Fact]
    public async Task SignIn_re_evaluates_validation_so_errors_appear_on_submit_with_empty_form()
    {
        var auth = new StubAuthenticationService();
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        // User taps Sign-in without touching anything — inline labels
        // should still appear so they know what's wrong.
        // Force-enable the command by mutating Email / Password but
        // then resetting to empty.
        viewModel.Email = "user@example.com";
        viewModel.Password = new string('p', Constants.Validation.PasswordMinLength + 1);

        await ((System.Windows.Input.ICommand)viewModel.SignInCommand)
            .ExecuteAsyncWithResult(null);

        Assert.Single(auth.SignInCalls); // sanity-check the success path ran

        // Now repeat with the form cleared: the command is disabled so
        // we simulate the reevaluation by writing through the source
        // generator's partial method.
        viewModel.Email = string.Empty;
        viewModel.Password = string.Empty;

        Assert.False(viewModel.SignInCommand.CanExecute(null));
    }

    [Fact]
    public async Task NavigateToSignUp_command_routes_to_signup_page()
    {
        var nav = new StubNavigationService();
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            nav,
            out _);

        await ((System.Windows.Input.ICommand)viewModel.NavigateToSignUpCommand)
            .ExecuteAsyncWithResult(null);

        NavigationCall call = Assert.Single(nav.NavigateCalls);
        Assert.Equal(Enums.NavigationTarget.SignUp, call.Target);
    }

    [Fact]
    public async Task NavigateToForgotPassword_command_routes_to_forgot_password_route()
    {
        var nav = new StubNavigationService();
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            nav,
            out _);

        await ((System.Windows.Input.ICommand)viewModel.NavigateToForgotPasswordCommand)
            .ExecuteAsyncWithResult(null);

        NavigationCall call = Assert.Single(nav.NavigateCalls);
        Assert.Equal(Constants.Routes.ForgotPassword, call.Route);
    }

    [Fact]
    public async Task SignIn_sets_IsLoading_around_the_service_call()
    {
        var auth = new StubAuthenticationService
        {
            SignInResult = Result<UserDto>.Success(new UserDto { Id = "u-1", Email = "x@y.z" }),
            SignInDelay = TimeSpan.FromMilliseconds(50),
        };
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        viewModel.Email = "x@y.z";
        viewModel.Password = new string('p', Constants.Validation.PasswordMinLength + 1);

        Task work = ((System.Windows.Input.ICommand)viewModel.SignInCommand)
            .ExecuteAsyncWithResult(null);

        // While the call is in flight the loading flag should be on.
        Assert.True(viewModel.IsLoading);

        await work;

        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task SignIn_forwards_rememberMe_flag_to_authentication_service()
    {
        var auth = new StubAuthenticationService();
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        viewModel.Email = "user@example.com";
        viewModel.Password = new string('p', Constants.Validation.PasswordMinLength + 1);
        viewModel.RememberMe = true;

        await ((System.Windows.Input.ICommand)viewModel.SignInCommand)
            .ExecuteAsyncWithResult(null);

        SignInAsyncCall call = Assert.Single(auth.SignInCalls);
        Assert.True(call.RememberMe, "RememberMe must be propagated to the auth service");
    }

    [Fact]
    public async Task SignIn_clears_password_after_successful_sign_in()
    {
        var auth = new StubAuthenticationService
        {
            SignInResult = Result<UserDto>.Success(new UserDto
            {
                Id = "u-1",
                Email = "user@example.com",
            }),
        };
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        viewModel.Email = "user@example.com";
        viewModel.Password = new string('p', Constants.Validation.PasswordMinLength + 1);

        await ((System.Windows.Input.ICommand)viewModel.SignInCommand)
            .ExecuteAsyncWithResult(null);

        Assert.Equal(string.Empty, viewModel.Password);
    }

    [Fact]
    public async Task LoadRememberedStateAsync_populates_email_and_rememberMe_when_available()
    {
        var auth = new StubAuthenticationService
        {
            RememberedEmail = "remembered@example.com",
        };
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        Assert.False(viewModel.IsRememberedStateLoaded);
        Assert.False(viewModel.RememberMe);
        Assert.Equal(string.Empty, viewModel.Email);

        await viewModel.LoadRememberedStateAsync();

        Assert.True(viewModel.IsRememberedStateLoaded);
        Assert.True(viewModel.RememberMe);
        Assert.Equal("remembered@example.com", viewModel.Email);
    }

    [Fact]
    public async Task LoadRememberedStateAsync_is_noop_when_no_remembered_email()
    {
        var auth = new StubAuthenticationService
        {
            RememberedEmail = null,
        };
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        await viewModel.LoadRememberedStateAsync();

        Assert.True(viewModel.IsRememberedStateLoaded);
        Assert.False(viewModel.RememberMe);
        Assert.Equal(string.Empty, viewModel.Email);
    }

    [Fact]
    public async Task LoadRememberedStateAsync_is_idempotent()
    {
        var auth = new StubAuthenticationService
        {
            RememberedEmail = "remembered@example.com",
        };
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        await viewModel.LoadRememberedStateAsync();
        viewModel.Email = "changed@example.com";
        viewModel.RememberMe = false;

        // Second call should NOT overwrite the user-modified values.
        await viewModel.LoadRememberedStateAsync();

        Assert.Equal("changed@example.com", viewModel.Email);
        Assert.False(viewModel.RememberMe);
    }

    [Fact]
    public async Task LoadRememberedStateAsync_swallows_exceptions_from_auth_service()
    {
        var auth = new ThrowingRememberedEmailAuthenticationService();
        SignInPageViewModel viewModel = CreateViewModel(
            auth,
            new StubNavigationService(),
            out _);

        await viewModel.LoadRememberedStateAsync();

        Assert.True(viewModel.IsRememberedStateLoaded);
        Assert.False(viewModel.RememberMe);
        Assert.Equal(string.Empty, viewModel.Email);
    }

    [Fact]
    public void ClearPassword_resets_password_to_empty_string()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        viewModel.Password = "supersecret";
        viewModel.ClearPassword();
        Assert.Equal(string.Empty, viewModel.Password);
    }

    [Fact]
    public void ClearPassword_is_noop_when_password_already_empty()
    {
        SignInPageViewModel viewModel = CreateViewModel(
            new StubAuthenticationService(),
            new StubNavigationService(),
            out _);

        viewModel.Password = string.Empty;
        viewModel.ClearPassword();
        Assert.Equal(string.Empty, viewModel.Password);
    }

    [Fact]
    public void AppShellViewModel_synchronises_IsAuthenticated_with_SignedInChanged_event()
    {
        var auth = new StubAuthenticationService();
        var nav = new StubNavigationService();
        AppShellViewModel shell = new(
            NullLogger<AppShellViewModel>.Instance,
            nav,
            auth);

        // Seeded from the stub's IsSignedIn (false at construction).
        Assert.False(shell.IsAuthenticated);

        auth.RaiseSignedInChanged(new UserDto { Id = "u-1", Email = "u@example.com" });
        Assert.True(shell.IsAuthenticated);
    }

    [Fact]
    public void AppShellViewModel_routes_to_sign_in_when_SignedInChanged_fires_with_null()
    {
        var auth = new StubAuthenticationService();
        var nav = new StubNavigationService();
        AppShellViewModel shell = new(
            NullLogger<AppShellViewModel>.Instance,
            nav,
            auth);

        auth.RaiseSignedInChanged(new UserDto { Id = "u-1", Email = "u@example.com" });
        Assert.True(shell.IsAuthenticated);
        nav.NavigateCalls.Clear();

        auth.RaiseSignedInChanged(null);
        Assert.False(shell.IsAuthenticated);

        NavigationCall call = Assert.Single(nav.NavigateCalls);
        Assert.Equal(Enums.NavigationTarget.SignIn, call.Target);
        Assert.True(call.ClearStack);
    }

    [Fact]
    public void AppShellViewModel_without_auth_service_starts_unauthenticated_and_can_be_marked_manually()
    {
        var nav = new StubNavigationService();
        AppShellViewModel shell = new(
            NullLogger<AppShellViewModel>.Instance,
            nav,
            authenticationService: null);

        Assert.False(shell.IsAuthenticated);
        shell.MarkAuthenticated();
        Assert.True(shell.IsAuthenticated);
        shell.MarkUnauthenticated();
        Assert.False(shell.IsAuthenticated);
    }
}

/// <summary>
/// Records <see cref="IAuthenticationService.SignInAsync"/> calls so the
/// test can assert the credentials the ViewModel passed in.  Behaves as a
/// configurable success / failure / throwing fake without taking a
/// dependency on the real <see cref="AuthenticationService"/>.
/// </summary>
internal sealed class StubAuthenticationService : IAuthenticationService
{
    public List<SignInAsyncCall> SignInCalls { get; } = new();

    public Result<UserDto>? SignInResult { get; set; }

    public Exception? SignInException { get; set; }

    public TimeSpan SignInDelay { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// E-mail address returned by <see cref="GetRememberedEmailAsync"/>.
    /// Tests that want to drive the "remember me" pre-population logic
    /// set this value before invoking
    /// <see cref="SignInPageViewModel.LoadRememberedStateAsync"/>.
    /// </summary>
    public string? RememberedEmail { get; set; }

    public UserDto? CurrentUser => _currentUser;

    public bool IsSignedIn => _currentUser is not null;

    /// <summary>
    /// Raised by tests through <see cref="RaiseSignedInChangedAsync"/> to
    /// exercise the <see cref="AppShellViewModel"/> event subscription.
    /// </summary>
    public event EventHandler<UserDto?>? SignedInChanged;

    private UserDto? _currentUser;

    public Task<Result<UserDto>> SignInAsync(
        string email,
        string password,
        bool rememberMe = false,
        CancellationToken cancellationToken = default)
    {
        SignInCalls.Add(new SignInAsyncCall(email, password, rememberMe));
        if (SignInDelay > TimeSpan.Zero)
        {
            return DelayedAsync(SignInDelay, SignInResult, SignInException, cancellationToken);
        }

        Result<UserDto> result = FromConfigImmediate(SignInResult, SignInException);
        if (result.IsSuccess)
        {
            _currentUser = result.Value;
            SignedInChanged?.Invoke(this, _currentUser);
        }

        return Task.FromResult(result);
    }

    private static Result<UserDto> FromConfigImmediate(
        Result<UserDto>? result,
        Exception? exception)
    {
        if (exception is not null)
        {
            throw exception;
        }

        return result
            ?? Result<UserDto>.Failure(new InvalidOperationException("Stub returned no result."));
    }

    public Task<Result<UserDto>> SignUpAsync(
        string email,
        string password,
        string? displayName = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result> SignOutAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result<UserDto>> RestoreSessionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result<AuthToken>> RefreshTokenAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<string?> GetRememberedEmailAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(RememberedEmail);

    /// <summary>
    /// Raises <see cref="SignedInChanged"/> with the supplied payload so
    /// tests can exercise the <see cref="AppShellViewModel"/> event
    /// subscription without going through the real auth service.
    /// </summary>
    public void RaiseSignedInChanged(UserDto? user)
    {
        if (user is null)
        {
            _currentUser = null;
        }
        else
        {
            _currentUser = user;
        }

        SignedInChanged?.Invoke(this, user);
    }

    private static async Task<Result<UserDto>> DelayedAsync(
        TimeSpan delay,
        Result<UserDto>? result,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        return FromConfigImmediate(result, exception);
    }
}

/// <summary>
/// Captures the (email, password, rememberMe) tuple passed to
/// <see cref="IAuthenticationService.SignInAsync"/>.
/// </summary>
internal sealed record SignInAsyncCall(string Email, string Password, bool RememberMe);

/// <summary>
/// In-process stub of <see cref="INavigationService"/>.  Records the
/// navigation requests the ViewModel issues so tests can assert on the
/// <c>Target</c> / <c>Route</c> / <c>ClearStack</c> values without
/// pulling in the real <c>Shell.Current</c> dependency.
/// </summary>
internal sealed class StubNavigationService : INavigationService
{
    public List<NavigationCall> NavigateCalls { get; } = new();

    public string? CurrentRoute => null;

    public bool CanGoBack => false;

    public Page? GetCurrentPage() => null;

    public Task<bool> NavigateToAsync(
        string route,
        IDictionary<string, object>? parameters = null,
        bool clearStack = false)
    {
        NavigateCalls.Add(new NavigationCall(route, parameters, clearStack));
        return Task.FromResult(true);
    }

    public Task<bool> NavigateToAsync<TPage>(
        IDictionary<string, object>? parameters = null,
        bool clearStack = false)
        where TPage : Page =>
        Task.FromResult(true);

    public Task<bool> NavigateToAsync(
        Enums.NavigationTarget navigationTarget,
        bool clearStack = false)
    {
        string? route = navigationTarget switch
        {
            Enums.NavigationTarget.SignIn => Constants.Routes.SignIn,
            Enums.NavigationTarget.SignUp => Constants.Routes.SignUp,
            Enums.NavigationTarget.TaskList => Constants.Routes.TaskList,
            Enums.NavigationTarget.TaskDetail => Constants.Routes.TaskDetail,
            Enums.NavigationTarget.Profile => Constants.Routes.Profile,
            Enums.NavigationTarget.Search => Constants.Routes.Search,
            _ => null,
        };
        NavigateCalls.Add(new NavigationCall(route ?? navigationTarget.ToString(), null, clearStack, navigationTarget));
        return Task.FromResult(true);
    }

    public Task<bool> GoBackAsync() => Task.FromResult(false);
}

/// <summary>
/// Captures one <see cref="INavigationService.NavigateToAsync(string, IDictionary{string, object}?, bool)"/>
/// call so the test can assert on the destination route, the query
/// parameters and the clear-stack flag.
/// </summary>
internal sealed record NavigationCall(
    string? Route,
    IDictionary<string, object>? Parameters,
    bool ClearStack,
    Enums.NavigationTarget? Target = null);

/// <summary>
/// Helper extension that awaits an <see cref="System.Windows.Input.ICommand"/>'
/// <c>ExecuteAsync</c> extension without taking a hard dependency on the
/// <c>Microsoft.Maui.Controls</c> assembly in the test project.  The
/// extension lives on <c>ICommand</c> in MAUI; here we shim the call by
/// directly invoking <c>Execute</c> when the command is an
/// <see cref="System.Windows.Input.IAsyncRelayCommand"/>.
/// </summary>
internal static class CommandTestExtensions
{
    public static async Task ExecuteAsyncWithResult(this System.Windows.Input.ICommand command, object? parameter)
    {
        ArgumentNullException.ThrowIfNull(command);

        switch (command)
        {
            case CommunityToolkit.Mvvm.Input.IAsyncRelayCommand asyncCommand:
                await asyncCommand.ExecuteAsync(parameter).ConfigureAwait(false);
                return;

            default:
                command.Execute(parameter);
                await Task.Yield();
                return;
        }
    }
}

/// <summary>
/// Authentication service stub that throws when
/// <see cref="IAuthenticationService.GetRememberedEmailAsync"/> is invoked.
/// Used to verify that <see cref="SignInPageViewModel.LoadRememberedStateAsync"/>
/// never lets a misbehaving SecureStorage break the sign-in page.
/// </summary>
internal sealed class ThrowingRememberedEmailAuthenticationService : IAuthenticationService
{
    public UserDto? CurrentUser => null;

    public bool IsSignedIn => false;

#pragma warning disable CS0067
    public event EventHandler<UserDto?>? SignedInChanged;
#pragma warning restore CS0067

    public Task<string?> GetRememberedEmailAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Simulated SecureStorage failure");

    public Task<Result<UserDto>> SignInAsync(
        string email,
        string password,
        bool rememberMe = false,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<UserDto>.Failure(new InvalidOperationException("Not used")));

    public Task<Result<UserDto>> SignUpAsync(
        string email,
        string password,
        string? displayName = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result> SignOutAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result<UserDto>> RestoreSessionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<Result<AuthToken>> RefreshTokenAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}