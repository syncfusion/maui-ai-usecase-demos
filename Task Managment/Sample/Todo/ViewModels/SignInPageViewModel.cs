using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Todo.Models.DTOs;
using Todo.Services.Infrastructure;
using Todo.Services.Interfaces;
using Todo.Utilities;
using Enums = Todo.Utilities.Enums;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the sign-in screen (Task 2-03).
///
/// <para>
/// Owns the e-mail / password entries and the validation state of the
/// form, exposes the user-visible sign-in command, and drives navigation
/// to the task list on success.  Navigation to the sign-up and
/// forgot-password pages is exposed through dedicated
/// <see cref="RelayCommand"/>s so the View layer can hook them up via
/// XAML without taking a dependency on the <see cref="INavigationService"/>
/// directly.
/// </para>
///
/// <para>
/// Validation is performed reactively: each time the bound
/// <see cref="Email"/> or <see cref="Password"/> property changes the
/// <see cref="OnEmailChanged(string?)"/> / <see cref="OnPasswordChanged(string?)"/>
/// partial methods re-evaluate the field and re-raise
/// <see cref="SignInCommand"/>'s <c>CanExecute</c>.  The view's "Sign in"
/// button is therefore disabled until both fields satisfy the application
/// validation policy (<see cref="Helpers.IsValidEmail"/> +
/// <see cref="Constants.Validation.PasswordMinLength"/>).
/// </para>
/// </summary>
public sealed partial class SignInPageViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;
    private readonly AppShellViewModel _shellViewModel;

    private bool _emailTouched;
    private bool _passwordTouched;

    /// <summary>
    /// Backing store for <see cref="Email"/>.  Empty by default so the
    /// placeholder text is visible immediately when the page is opened.
    /// </summary>
    [ObservableProperty]
    private string email = string.Empty;

    /// <summary>
    /// Backing store for <see cref="Password"/>.  Empty by default so the
    /// placeholder text is visible immediately when the page is opened.
    /// </summary>
    [ObservableProperty]
    private string password = string.Empty;

    /// <summary>
    /// Backing store for <see cref="RememberMe"/>.  Defaults to
    /// <see langword="false"/> so the user has to opt in to the
    /// "stay signed in" behaviour — the task specification calls out that
    /// remember-me is opt-in.
    /// </summary>
    [ObservableProperty]
    private bool rememberMe;

    /// <summary>
    /// Inline e-mail validation error.  Bound by the View to the label
    /// shown immediately under the e-mail field.  Empty when the field
    /// is valid (or has not yet been touched).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEmailError))]
    private string? emailError;

    /// <summary>
    /// Inline password validation error.  Bound by the View to the label
    /// shown immediately under the password field.  Empty when the field
    /// is valid (or has not yet been touched).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordError))]
    private string? passwordError;

    /// <summary>
    /// Convenience accessor used by the View to bind the e-mail error
    /// label's <c>IsVisible</c> property to a single source of truth.
    /// </summary>
    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);

    /// <summary>
    /// Convenience accessor used by the View to bind the password error
    /// label's <c>IsVisible</c> property to a single source of truth.
    /// </summary>
    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);

    /// <summary>
    /// Initialises a new instance of the <see cref="SignInPageViewModel"/>
    /// class.  All dependencies are resolved through the .NET MAUI
    /// dependency-injection container (see <c>MauiProgram</c>).
    /// </summary>
    /// <param name="authenticationService">
    /// Service used to validate credentials and obtain the issued
    /// <see cref="AuthToken"/>.
    /// </param>
    /// <param name="navigationService">
    /// Navigation service used to route to the next page on success
    /// (task list) or to the sign-up / forgot-password pages.
    /// </param>
    /// <param name="shellViewModel">
    /// Shell-level ViewModel — used to mark the user as authenticated so
    /// the Shell chrome (back button, profile tab, …) refreshes.
    /// </param>
    /// <param name="logger">Logger injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the injected dependencies is <see langword="null"/>.
    /// </exception>
    public SignInPageViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService,
        AppShellViewModel shellViewModel,
        ILogger<SignInPageViewModel> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(authenticationService);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(shellViewModel);

        _authenticationService = authenticationService;
        _navigationService = navigationService;
        _shellViewModel = shellViewModel;

        // Re-evaluate the sign-in command whenever the base-class
        // IsLoading flag changes.  Subscribe through PropertyChanged
        // rather than the partial-method hook because IsLoading is
        // declared on ViewModelBase, so the CommunityToolkit source
        // generator does not emit a partial method declaration here.
        PropertyChanged += OnViewModelPropertyChanged;

        Title = "Sign in";
    }

    /// <summary>
    /// Listens for <see cref="ViewModelBase.IsLoading"/> changes and
    /// refreshes <see cref="SignInCommand"/>'s <c>CanExecute</c> state so
    /// the bound button is disabled while a sign-in call is in flight.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsLoading))
        {
            SignInCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit.Mvvm source
    /// generator whenever <see cref="Email"/> changes (either because the
    /// user typed something or because the binding pushed a value).
    /// Re-validates the field and refreshes the command state.
    /// </summary>
    /// <param name="value">The new e-mail value.</param>
    partial void OnEmailChanged(string value)
    {
        // Mark the field as touched on every keystroke so the inline
        // error starts appearing as soon as the user has typed — but
        // suppress the "required" message before the first keystroke
        // so the form does not start with a red label.
        _emailTouched = true;
        ValidateEmail(value);
        SignInCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit.Mvvm source
    /// generator whenever <see cref="Password"/> changes.  Re-validates
    /// the field and refreshes the command state.
    /// </summary>
    /// <param name="value">The new password value.</param>
    partial void OnPasswordChanged(string value)
    {
        _passwordTouched = true;
        ValidatePassword(value);
        SignInCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Validates <paramref name="value"/> against the application e-mail
    /// rule and updates <see cref="EmailError"/>.  The message is
    /// intentionally kept short so it fits the inline error label
    /// without wrapping on a phone-sized screen.
    /// </summary>
    /// <param name="value">The current e-mail value to validate.</param>
    private void ValidateEmail(string? value)
    {
        if (!_emailTouched)
        {
            EmailError = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            EmailError = "Please enter your e-mail address.";
            return;
        }

        EmailError = Helpers.IsValidEmail(value)
            ? null
            : "Please enter a valid e-mail address.";
    }

    /// <summary>
    /// Validates <paramref name="value"/> against the application password
    /// rule (non-empty, at least
    /// <see cref="Constants.Validation.PasswordMinLength"/> characters)
    /// and updates <see cref="PasswordError"/>.  Full strength policy is
    /// enforced at sign-up time; sign-in only requires the password to be
    /// at least the configured minimum length so it can be hashed and
    /// verified.
    /// </summary>
    /// <param name="value">The current password value to validate.</param>
    private void ValidatePassword(string? value)
    {
        if (!_passwordTouched)
        {
            PasswordError = null;
            return;
        }

        if (string.IsNullOrEmpty(value))
        {
            PasswordError = "Please enter your password.";
            return;
        }

        PasswordError = value.Length >= Constants.Validation.PasswordMinLength
            ? null
            : $"Password must be at least {Constants.Validation.PasswordMinLength} characters.";
    }

    /// <summary>
    /// Forces both fields into the "touched" state and re-evaluates them
    /// so the user always sees a clear error when tapping the button with
    /// an empty / malformed form (otherwise the inline errors would stay
    /// hidden until the next keystroke).
    /// </summary>
    private void ReevaluateValidation()
    {
        _emailTouched = true;
        _passwordTouched = true;
        ValidateEmail(Email);
        ValidatePassword(Password);
        SignInCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Indicates whether the form is currently valid and the user can
    /// press the sign-in button.  Both fields must satisfy the
    /// application validation rule.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the e-mail and password pass
    /// validation and no sign-in call is currently in flight; otherwise
    /// <see langword="false"/>.
    /// </returns>
    private bool CanSignIn() =>
        !IsLoading
        && Helpers.IsValidEmail(Email)
        && !string.IsNullOrEmpty(Password)
        && Password.Length >= Constants.Validation.PasswordMinLength;

    /// <summary>
    /// Signs the user in using the bound e-mail and password.  Manages
    /// <see cref="ViewModelBase.IsLoading"/> /
    /// <see cref="ViewModelBase.ErrorMessage"/> / catch-all plumbing by
    /// hand (rather than going through
    /// <see cref="ViewModelBase.ExecuteAsync{T}"/>) so the resulting
    /// <see cref="Result{T}"/> from
    /// <see cref="IAuthenticationService.SignInAsync"/> stays typed as
    /// <c>Result&lt;UserDto&gt;</c> — the generic
    /// <see cref="ViewModelBase.ExecuteAsync{T}"/> wrapper would otherwise
    /// turn it into <c>Result&lt;Result&lt;UserDto&gt;&gt;</c>.  On
    /// success, navigates to the task list and clears the navigation
    /// stack so the user cannot return to the sign-in page with the back
    /// gesture.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSignIn))]
    private async Task SignInAsync()
    {
        // Trim surrounding whitespace from the e-mail so a value like
        // "  user@example.com  " is accepted by CanSignIn (the underlying
        // IsValidEmail regex does not tolerate whitespace) and is forwarded
        // to the service in its normalised form.
        Email = Email.Trim();

        ReevaluateValidation();

        // After the re-evaluation the form may be invalid (e.g. the user
        // clicked sign-in before the field-level "required" message had a
        // chance to appear).  Bail out without invoking the service.
        if (!CanSignIn())
        {
            return;
        }

        string emailSnapshot = Email;
        string passwordSnapshot = Password;

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        Result<UserDto> result;
        try
        {
            result = await _authenticationService
                .SignInAsync(emailSnapshot, passwordSnapshot)
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Sign-in was cancelled");
            return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during sign-in for {Email}", emailSnapshot);
            HasError = true;
            ErrorMessage = ex.Message;
            return;
        }
        finally
        {
            IsLoading = false;
        }

        if (!result.IsSuccess)
        {
            // Surface a friendly inline message under the password field
            // so the user sees the most likely cause (wrong credentials)
            // without dismissing the form-level error banner.
            PasswordError = result.ErrorMessage;
            return;
        }

        Logger.LogInformation("Sign-in successful for {UserId}", result.Value!.Id);
        _shellViewModel.MarkAuthenticated();

        // clearStack=true: the back gesture must not return the user to
        // the sign-in screen once they are authenticated.
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.TaskList, clearStack: true)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the sign-up page so the user can register a brand
    /// new account.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToSignUpAsync()
    {
        Logger.LogInformation("Navigating to sign-up page");
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.SignUp)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the forgot-password page so the user can request a
    /// password reset e-mail.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToForgotPasswordAsync()
    {
        Logger.LogInformation("Navigating to forgot-password page");
        await _navigationService.NavigateToAsync(Constants.Routes.ForgotPassword)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Re-raises <c>CanExecuteChanged</c> on the
    /// <see cref="SignInCommand"/> whenever the loading state changes so
    /// the button is disabled while a sign-in call is in flight even when
    /// the form is otherwise valid.  Implemented via a
    /// <see cref="PropertyChanged"/> subscription in the constructor
    /// because <see cref="IsLoading"/> is declared on the base class.
    /// </summary>
}