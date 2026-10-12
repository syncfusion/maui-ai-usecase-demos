using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using Todo.Models.DTOs;
using Todo.Services.Authentication;
using Todo.Services.Infrastructure;
using Todo.Services.Interfaces;
using Todo.Utilities;
using Enums = Todo.Utilities.Enums;

namespace Todo.ViewModels;

/// <summary>
/// Qualitative classification of a candidate password.  Used by the sign-up
/// page to drive the password-strength indicator (and the inline "weak /
/// medium / strong" label).
/// </summary>
public enum PasswordStrength
{
    /// <summary>Empty / not yet entered.</summary>
    None = 0,

    /// <summary>Too short, or otherwise below the policy minimum.</summary>
    Weak = 1,

    /// <summary>Meets the minimum length but missing one or more character
    /// classes required by the strength policy.</summary>
    Medium = 2,

    /// <summary>Meets the minimum length and includes upper-case,
    /// lower-case, digit and symbol.</summary>
    Strong = 3,
}

/// <summary>
/// ViewModel for the sign-up screen (Task 2-04).
///
/// <para>
/// Owns the e-mail / password / confirm-password entries, drives the
/// inline validation (real-time, on every keystroke) and the password
/// strength indicator, and exposes the user-visible sign-up command
/// followed by a navigation to the task list on success.  Navigation
/// back to the sign-in page is exposed through a dedicated
/// <see cref="RelayCommand"/> so the View layer can hook it up via XAML
/// without taking a dependency on the <see cref="INavigationService"/>
/// directly.
/// </para>
///
/// <para>
/// Validation is performed reactively: each time the bound
/// <see cref="Email"/>, <see cref="Password"/> or <see cref="ConfirmPassword"/>
/// property changes the source-generator partial methods re-evaluate the
/// field, refresh <see cref="PasswordStrength"/> and re-raise
/// <see cref="SignUpCommand"/>'s <c>CanExecute</c>.  The View's "Sign up"
/// button is therefore disabled until every field satisfies the
/// application validation policy.
/// </para>
/// </summary>
public sealed partial class SignUpPageViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly INavigationService _navigationService;
    private readonly AppShellViewModel _shellViewModel;

    private bool _emailTouched;
    private bool _passwordTouched;
    private bool _confirmPasswordTouched;

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
    /// Backing store for <see cref="ConfirmPassword"/>.  Empty by default so
    /// the placeholder text is visible immediately when the page is opened.
    /// </summary>
    [ObservableProperty]
    private string confirmPassword = string.Empty;

    /// <summary>
    /// Backing store for <see cref="IsPasswordVisible"/>.  Defaults to
    /// <see langword="false"/> so the password is masked by default; the
    /// eye-toggle on the form binds to this flag so the user can review the
    /// value they have typed before submitting.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordVisibilityIcon))]
    [NotifyPropertyChangedFor(nameof(PasswordAccessibilityLabel))]
    private bool isPasswordVisible;

    /// <summary>
    /// Glyph shown on the eye-toggle button next to the password field.
    /// Pairs with <see cref="PasswordAccessibilityLabel"/>.
    /// </summary>
    public string PasswordVisibilityIcon => IsPasswordVisible ? "🙈" : "👁";

    /// <summary>
    /// Accessibility-friendly label for the eye-toggle button.
    /// Switches between "Show password" and "Hide password" so
    /// screen-readers announce the right action.
    /// </summary>
    public string PasswordAccessibilityLabel => IsPasswordVisible
        ? "Hide password"
        : "Show password";

    /// <summary>
    /// Backing store for <see cref="PasswordStrength"/>.  Updated whenever
    /// <see cref="Password"/> changes so the strength bar and "Weak /
    /// Medium / Strong" label can reactively follow user input.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordStrengthLabel))]
    [NotifyPropertyChangedFor(nameof(HasPasswordStrength))]
    [NotifyPropertyChangedFor(nameof(PasswordStrengthSegment1Fill))]
    [NotifyPropertyChangedFor(nameof(PasswordStrengthSegment2Fill))]
    [NotifyPropertyChangedFor(nameof(PasswordStrengthSegment3Fill))]
    [NotifyPropertyChangedFor(nameof(PasswordStrengthSegment1Color))]
    [NotifyPropertyChangedFor(nameof(PasswordStrengthSegment2Color))]
    [NotifyPropertyChangedFor(nameof(PasswordStrengthSegment3Color))]
    private PasswordStrength passwordStrength = PasswordStrength.None;

    /// <summary>
    /// Human-readable label for the current <see cref="PasswordStrength"/>
    /// (e.g. "Weak password", "Strong password").  Empty while the
    /// strength has not yet been evaluated (empty field) so the label does
    /// not start the page with a "weak" warning.
    /// </summary>
    public string PasswordStrengthLabel => PasswordStrength switch
    {
        PasswordStrength.None => string.Empty,
        PasswordStrength.Weak => "Weak password",
        PasswordStrength.Medium => "Medium password",
        PasswordStrength.Strong => "Strong password",
        _ => string.Empty,
    };

    /// <summary>
    /// Indicates whether the strength indicator should be visible at all.
    /// Hidden when the field is empty / untouched so the page opens with
    /// a clean look.
    /// </summary>
    public bool HasPasswordStrength =>
        _passwordTouched && PasswordStrength != PasswordStrength.None;

    /// <summary>
    /// Fill flag for the first strength segment.  The segment is drawn
    /// when the password is at least weak.
    /// </summary>
    public bool PasswordStrengthSegment1Fill =>
        PasswordStrength >= PasswordStrength.Weak;

    /// <summary>
    /// Fill flag for the second strength segment.  The segment is drawn
    /// when the password is at least medium.
    /// </summary>
    public bool PasswordStrengthSegment2Fill =>
        PasswordStrength >= PasswordStrength.Medium;

    /// <summary>
    /// Fill flag for the third strength segment.  The segment is drawn
    /// when the password is strong.
    /// </summary>
    public bool PasswordStrengthSegment3Fill =>
        PasswordStrength >= PasswordStrength.Strong;

    /// <summary>
    /// Colour of the first strength segment.  Red while weak, gradient up
    /// to green when strong.
    /// </summary>
    public Color PasswordStrengthSegment1Color => PasswordStrength switch
    {
        PasswordStrength.Weak => Color.FromArgb("#D32F2F"),
        PasswordStrength.Medium => Color.FromArgb("#F57C00"),
        PasswordStrength.Strong => Color.FromArgb("#388E3C"),
        _ => Color.FromArgb("#BDBDBD"),
    };

    /// <summary>
    /// Colour of the second strength segment.  Only filled when at least
    /// medium, so the colour only renders meaningful values.
    /// </summary>
    public Color PasswordStrengthSegment2Color => PasswordStrength switch
    {
        PasswordStrength.Medium => Color.FromArgb("#F57C00"),
        PasswordStrength.Strong => Color.FromArgb("#388E3C"),
        _ => Color.FromArgb("#BDBDBD"),
    };

    /// <summary>
    /// Colour of the third strength segment.  Only filled when strong.
    /// </summary>
    public Color PasswordStrengthSegment3Color => PasswordStrength switch
    {
        PasswordStrength.Strong => Color.FromArgb("#388E3C"),
        _ => Color.FromArgb("#BDBDBD"),
    };

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
    /// Inline confirm-password validation error.  Bound by the View to
    /// the label shown immediately under the confirm-password field.
    /// Empty when the field is valid (or has not yet been touched).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConfirmPasswordError))]
    private string? confirmPasswordError;

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
    /// Convenience accessor used by the View to bind the confirm-password
    /// error label's <c>IsVisible</c> property to a single source of
    /// truth.
    /// </summary>
    public bool HasConfirmPasswordError => !string.IsNullOrEmpty(ConfirmPasswordError);

    /// <summary>
    /// Initialises a new instance of the <see cref="SignUpPageViewModel"/>
    /// class.  All dependencies are resolved through the .NET MAUI
    /// dependency-injection container (see <c>MauiProgram</c>).
    /// </summary>
    /// <param name="authenticationService">
    /// Service used to register the new account and obtain the issued
    /// <see cref="AuthToken"/>.
    /// </param>
    /// <param name="navigationService">
    /// Navigation service used to route to the task list on success or
    /// back to the sign-in page when the user already has an account.
    /// </param>
    /// <param name="shellViewModel">
    /// Shell-level ViewModel — used to mark the user as authenticated so
    /// the Shell chrome (back button, profile tab, …) refreshes.
    /// </param>
    /// <param name="logger">Logger injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the injected dependencies is <see langword="null"/>.
    /// </exception>
    public SignUpPageViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService,
        AppShellViewModel shellViewModel,
        ILogger<SignUpPageViewModel> logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(authenticationService);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(shellViewModel);

        _authenticationService = authenticationService;
        _navigationService = navigationService;
        _shellViewModel = shellViewModel;

        // Re-evaluate the sign-up command whenever the base-class
        // IsLoading flag changes.  Subscribe through PropertyChanged
        // rather than the partial-method hook because IsLoading is
        // declared on ViewModelBase, so the CommunityToolkit source
        // generator does not emit a partial method declaration here.
        PropertyChanged += OnViewModelPropertyChanged;

        Title = "Sign up";
    }

    /// <summary>
    /// Listens for <see cref="ViewModelBase.IsLoading"/> changes and
    /// refreshes <see cref="SignUpCommand"/>'s <c>CanExecute</c> state so
    /// the bound button is disabled while a sign-up call is in flight.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsLoading))
        {
            SignUpCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit.Mvvm source
    /// generator whenever <see cref="Email"/> changes (either because
    /// the user typed something or because the binding pushed a value).
    /// Re-validates the field and refreshes the command state.
    /// </summary>
    /// <param name="value">The new e-mail value.</param>
    partial void OnEmailChanged(string value)
    {
        _emailTouched = true;
        ValidateEmail(value);
        SignUpCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit.Mvvm source
    /// generator whenever <see cref="Password"/> changes.  Re-validates
    /// the field, refreshes the password strength indicator and
    /// re-evaluates the command state.  Also re-validates the
    /// confirm-password field so a "passwords must match" error appears
    /// in real time when the user later types the matching value.
    /// </summary>
    /// <param name="value">The new password value.</param>
    partial void OnPasswordChanged(string value)
    {
        _passwordTouched = true;
        ValidatePassword(value);
        UpdatePasswordStrength(value);
        // The match check depends on the new password, so re-run it too.
        if (_confirmPasswordTouched)
        {
            ValidateConfirmPassword(ConfirmPassword);
        }

        SignUpCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Partial method invoked by the CommunityToolkit.Mvvm source
    /// generator whenever <see cref="ConfirmPassword"/> changes.
    /// Re-validates the field against the current password value and
    /// refreshes the command state.
    /// </summary>
    /// <param name="value">The new confirm-password value.</param>
    partial void OnConfirmPasswordChanged(string value)
    {
        _confirmPasswordTouched = true;
        ValidateConfirmPassword(value);
        SignUpCommand.NotifyCanExecuteChanged();
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
    /// Validates <paramref name="value"/> against the application password
    /// rule (non-empty, at least
    /// <see cref="Constants.Validation.PasswordMinLength"/> characters)
    /// and updates <see cref="PasswordError"/>.
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
            PasswordError = "Please enter a password.";
            return;
        }

        if (value.Length < Constants.Validation.PasswordMinLength)
        {
            PasswordError = $"Password must be at least {Constants.Validation.PasswordMinLength} characters.";
            return;
        }

        PasswordError = null;
    }

    /// <summary>
    /// Validates <paramref name="value"/> against the current
    /// <see cref="Password"/> value and updates <see cref="ConfirmPasswordError"/>.
    /// </summary>
    /// <param name="value">The current confirm-password value to validate.</param>
    private void ValidateConfirmPassword(string? value)
    {
        if (!_confirmPasswordTouched)
        {
            ConfirmPasswordError = null;
            return;
        }

        if (string.IsNullOrEmpty(value))
        {
            ConfirmPasswordError = "Please confirm your password.";
            return;
        }

        ConfirmPasswordError = string.Equals(value, Password, StringComparison.Ordinal)
            ? null
            : "Passwords do not match.";
    }

    /// <summary>
    /// Recalculates <see cref="PasswordStrength"/> from the candidate
    /// <paramref name="password"/>.  Mirrors the policy enforced by
    /// <see cref="PasswordValidator.MeetsStrengthPolicy"/> so the visual
    /// indicator stays consistent with the back-end validation.
    /// </summary>
    /// <param name="password">The candidate password.</param>
    private void UpdatePasswordStrength(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            PasswordStrength = PasswordStrength.None;
            return;
        }

        if (password.Length < Constants.Validation.PasswordMinLength)
        {
            PasswordStrength = PasswordStrength.Weak;
            return;
        }

        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSymbol = false;
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else if (!char.IsLetterOrDigit(c)) hasSymbol = true;
        }

        PasswordStrength = (hasUpper && hasLower && hasDigit && hasSymbol)
            ? PasswordStrength.Strong
            : PasswordStrength.Medium;
    }

    /// <summary>
    /// Forces every field into the "touched" state and re-evaluates them
    /// so the user always sees a clear error when tapping the button with
    /// an empty / malformed form (otherwise the inline errors would stay
    /// hidden until the next keystroke).
    /// </summary>
    private void ReevaluateValidation()
    {
        _emailTouched = true;
        _passwordTouched = true;
        _confirmPasswordTouched = true;

        ValidateEmail(Email);
        ValidatePassword(Password);
        ValidateConfirmPassword(ConfirmPassword);
        UpdatePasswordStrength(Password);

        SignUpCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Indicates whether the form is currently valid and the user can
    /// press the sign-up button.  All three fields must satisfy their
    /// application validation rule and the underlying password must
    /// reach <see cref="PasswordStrength.Medium"/> (matches the policy
    /// enforced by <see cref="PasswordValidator.MeetsStrengthPolicy"/>)
    /// so the back-end never rejects the request the View just admitted.
    /// </summary>
    private bool CanSignUp() =>
        !IsLoading
        && !string.IsNullOrWhiteSpace(Email)
        && Helpers.IsValidEmail(Email)
        && PasswordValidator.MeetsStrengthPolicy(Password)
        && string.Equals(Password, ConfirmPassword, StringComparison.Ordinal);

    /// <summary>
    /// Registers the new account and, on success, navigates to the task
    /// list.  Mirrors the manual load / error-management plumbing used
    /// by <see cref="SignInPageViewModel"/> so the resulting
    /// <see cref="Result{T}"/> from
    /// <see cref="IAuthenticationService.SignUpAsync"/> stays typed as
    /// <c>Result&lt;UserDto&gt;</c>.  On duplicate e-mail the message is
    /// surfaced inline under the e-mail field so the user can adjust it
    /// without dismissing a form-level banner.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSignUp))]
    private async Task SignUpAsync()
    {
        Email = Email.Trim();

        ReevaluateValidation();

        if (!CanSignUp())
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
                .SignUpAsync(emailSnapshot, passwordSnapshot, displayName: null)
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Sign-up was cancelled");
            return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during sign-up for {Email}", emailSnapshot);
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
            // The most common sign-up failure is a duplicate e-mail.  Pin
            // the message to the e-mail field when the service's text
            // describes that scenario so the user can react without
            // dismissing a banner; otherwise fall back to the form-level
            // error banner.
            if (!string.IsNullOrEmpty(result.ErrorMessage)
                && result.ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
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

        Logger.LogInformation("Sign-up successful for {UserId}", result.Value!.Id);
        _shellViewModel.MarkAuthenticated();

        // clearStack=true: the back gesture must not return the user to
        // the sign-up screen once they are authenticated.
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.TaskList, clearStack: true)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Toggles <see cref="IsPasswordVisible"/> so the user can review the
    /// value they have typed in the password / confirm-password fields
    /// before submitting the form.
    /// </summary>
    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    /// <summary>
    /// Navigates back to the sign-in page so a user who already has an
    /// account can authenticate instead of registering a new one.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToSignInAsync()
    {
        Logger.LogInformation("Navigating to sign-in page");
        await _navigationService
            .NavigateToAsync(Enums.NavigationTarget.SignIn)
            .ConfigureAwait(true);
    }
}
