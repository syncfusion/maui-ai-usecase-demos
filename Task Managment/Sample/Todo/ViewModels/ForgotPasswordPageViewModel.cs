using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Todo.Services.Infrastructure;
using Todo.Services.Interfaces;
using Todo.Utilities;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the forgot-password screen (Task 2-05).
///
/// <para>
/// Owns the e-mail field, drives the inline validation, and exposes the
/// user-visible "send reset link" command.  On success the ViewModel
/// surfaces a non-leaking confirmation message (the local
/// authentication service deliberately returns success whether or not the
/// address matches a real account, so the UI never tells the user
/// whether their e-mail exists on file).  Navigation back to the sign-in
/// page is exposed through a dedicated <see cref="RelayCommand"/> so the
/// View layer can hook it up via XAML without taking a dependency on the
/// <see cref="INavigationService"/> directly.
/// </para>
///
/// <para>
/// Validation is performed reactively: each time the bound
/// <see cref="Email"/> property changes, the
/// <see cref="OnEmailChanged(string?)"/> partial method re-evaluates the
/// field, clears the confirmation banner, and refreshes
/// <see cref="SendResetLinkCommand"/>'s <c>CanExecute</c>.  The View's
/// "Send reset link" button is therefore disabled until the field
/// satisfies the application e-mail validation rule.
/// </para>
/// </summary>
public sealed partial class ForgotPasswordPageViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;

    private bool _emailTouched;

    /// <summary>
    /// Backing store for <see cref="Email"/>.  Empty by default so the
    /// placeholder text is visible immediately when the page is opened.
    /// </summary>
    [ObservableProperty]
    private string email = string.Empty;

    /// <summary>
    /// Inline e-mail validation error.  Bound by the View to the label
    /// shown immediately under the e-mail field.  Empty when the field
    /// is valid (or has not yet been touched).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEmailError))]
    private string? emailError;

    /// <summary>
    /// Convenience accessor used by the View to bind the e-mail error
    /// label's <c>IsVisible</c> property to a single source of truth.
    /// </summary>
    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);

    /// <summary>
    /// Backing store for <see cref="ConfirmationMessage"/>.  Set after a
    /// successful reset request so the user knows what to expect next.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConfirmation))]
    private string? confirmationMessage;

    /// <summary>
    /// Convenience accessor used by the View to bind the confirmation
    /// label's <c>IsVisible</c> property to a single source of truth.
    /// </summary>
    public bool HasConfirmation => !string.IsNullOrEmpty(ConfirmationMessage);

    /// <summary>
    /// Initialises a new instance of the
    /// <see cref="ForgotPasswordPageViewModel"/> class.  All
    /// dependencies are resolved through the .NET MAUI
    /// dependency-injection container (see <c>MauiProgram</c>).
    /// </summary>
    /// <param name="authenticationService">
    /// Service used to send the password-reset e-mail.
    /// </param>
    /// <param name="navigationService">
    /// Navigation service used to route the user back to the sign-in
    /// page.
    /// </param>
    /// <param name="logger">Logger injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the injected dependencies is <see langword="null"/>.
    /// </exception>
    public ForgotPasswordPageViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService,
        ILogger<ForgotPasswordPageViewModel> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(authenticationService);
        ArgumentNullException.ThrowIfNull(navigationService);

        _authenticationService = authenticationService;
        _navigationService = navigationService;

        // Re-evaluate the send-reset-link command whenever the
        // base-class IsLoading flag changes so the bound button is
        // disabled while the request is in flight.
        PropertyChanged += OnViewModelPropertyChanged;

        Title = "Forgot password";
    }

    /// <summary>
    /// Listens for <see cref="ViewModelBase.IsLoading"/> changes and
    /// refreshes <see cref="SendResetLinkCommand"/>'s <c>CanExecute</c>
    /// state so the bound button is disabled while a reset request is in
    /// flight.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsLoading))
        {
            SendResetLinkCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit.Mvvm source
    /// generator whenever <see cref="Email"/> changes (either because
    /// the user typed something or because the binding pushed a value).
    /// Re-validates the field, clears the previous confirmation /
    /// error banners, and refreshes the command state.
    /// </summary>
    /// <param name="value">The new e-mail value.</param>
    partial void OnEmailChanged(string value)
    {
        _emailTouched = true;
        ValidateEmail(value);

        // Reset transient banners every time the user edits the field
        // so the messages stay aligned with the value they were emitted
        // against.
        if (HasError)
        {
            HasError = false;
            ErrorMessage = null;
        }

        if (HasConfirmation)
        {
            ConfirmationMessage = null;
        }

        SendResetLinkCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Validates <paramref name="value"/> against the application e-mail
    /// rule and updates <see cref="EmailError"/>.
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
    /// Forces the field into the "touched" state and re-evaluates it so
    /// the user always sees a clear error when tapping the button with an
    /// empty / malformed form (otherwise the inline error would stay
    /// hidden until the next keystroke).
    /// </summary>
    private void ReevaluateValidation()
    {
        _emailTouched = true;
        ValidateEmail(Email);
        SendResetLinkCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Indicates whether the form is currently valid and the user can
    /// press the "send reset link" button.  The e-mail field must
    /// satisfy the application e-mail validation rule and no reset
    /// request may currently be in flight.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the e-mail passes validation and no
    /// request is in flight; otherwise <see langword="false"/>.
    /// </returns>
    private bool CanSendResetLink() =>
        !IsLoading
        && !string.IsNullOrWhiteSpace(Email)
        && Helpers.IsValidEmail(Email);

    /// <summary>
    /// Sends the password-reset request.  Manages
    /// <see cref="ViewModelBase.IsLoading"/> /
    /// <see cref="ViewModelBase.ErrorMessage"/> / catch-all plumbing by
    /// hand so the resulting <see cref="Result"/> from
    /// <see cref="IAuthenticationService.RequestPasswordResetAsync"/>
    /// stays typed as <see cref="Result"/>.  On success the
    /// confirmation message is populated and the e-mail field is left
    /// intact so the user can verify what they typed.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSendResetLink))]
    private async Task SendResetLinkAsync()
    {
        Email = Email.Trim();

        ReevaluateValidation();

        // After the re-evaluation the form may be invalid (the user
        // tapped the button before the field-level "required" message
        // had a chance to appear).  Bail out without invoking the
        // service.
        if (!CanSendResetLink())
        {
            return;
        }

        string emailSnapshot = Email;

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;
        ConfirmationMessage = null;

        Result result;
        try
        {
            result = await _authenticationService
                .RequestPasswordResetAsync(emailSnapshot)
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Password reset request was cancelled");
            IsLoading = false;
            return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during password reset for {Email}", emailSnapshot);
            HasError = true;
            ErrorMessage = "Something went wrong. Please try again.";
            IsLoading = false;
            return;
        }

        IsLoading = false;

        if (!result.IsSuccess)
        {
            // The local account store has no remote mailer, but we still
            // pin the failure to the e-mail field when it is
            // e-mail-shaped (e.g. malformed value); otherwise surface
            // it through the form-level banner.
            if (!string.IsNullOrEmpty(result.ErrorMessage)
                && (result.ErrorMessage.Contains("e-mail", StringComparison.OrdinalIgnoreCase)
                    || result.ErrorMessage.Contains("email", StringComparison.OrdinalIgnoreCase)))
            {
                EmailError = result.ErrorMessage;
            }
            else
            {
                HasError = true;
                ErrorMessage = result.ErrorMessage;
            }
            return;
        }

        Logger.LogInformation("Password reset requested for {Email}", emailSnapshot);
        ConfirmationMessage = "If an account exists for that e-mail address, we've sent instructions for resetting your password.";
    }

    /// <summary>
    /// Routes the user back to the sign-in page.  No
    /// <c>clearStack</c> flag is set so the navigation history is left
    /// intact and the back gesture continues to work as expected after
    /// returning to sign-in.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToSignInAsync()
    {
        Logger.LogInformation("Navigating back to sign-in page");
        await _navigationService
            .NavigateToAsync(Constants.Routes.SignIn)
            .ConfigureAwait(true);
    }
}
