using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Todo.Utilities;

namespace Todo.ViewModels;

/// <summary>
/// Base class for every ViewModel in the application. Provides:
/// <list type="bullet">
///   <item><description>INotifyPropertyChanged through <see cref="ObservableObject"/>
///     (CommunityToolkit.Mvvm).</description></item>
///   <item><description>Standard <see cref="IsLoading"/> and <see cref="Title"/>
///     observable properties consumed by the global template.</description></item>
///   <item><description>Centralised, structured <see cref="ILogger{T}"/>
///     diagnostics.</description></item>
///   <item><description>A consistent, exception-safe
///     <see cref="ExecuteAsync{T}"/> wrapper that any
///     <c>[RelayCommand]</c> can build on top of.</description></item>
/// </list>
/// Derived ViewModels should use <c>[ObservableProperty]</c> for their
/// bindable state and <c>[RelayCommand]</c> for user-intent commands.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Logger used by derived ViewModels to record diagnostics.  It is
    /// injected by the .NET MAUI dependency-injection container (see
    /// Task 1-05 — <c>MauiProgram</c> and DI).
    /// </summary>
    protected ILogger<ViewModelBase> Logger { get; }

    /// <summary>
    /// Tracks whether the ViewModel is currently performing a long-running
    /// operation.  Views bind to this property to show/hide a busy indicator.
    /// </summary>
    [ObservableProperty]
    private bool isLoading;

    /// <summary>
    /// Tracks whether the ViewModel has encountered an error during the most
    /// recent operation.  Views can bind to this to show an error banner.
    /// </summary>
    [ObservableProperty]
    private bool hasError;

    /// <summary>
    /// Optional human-readable error message paired with
    /// <see cref="HasError"/>.  Cleared automatically at the start of the
    /// next <see cref="ExecuteAsync{T}(Func{Task{T}?}, string?)"/> call.
    /// </summary>
    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Title shown by the View (typically the page title or a section
    /// header).  Defaults to an empty string so the property always has a
    /// non-null value.
    /// </summary>
    [ObservableProperty]
    private string title = string.Empty;

    /// <summary>
    /// Initialises a new instance of the <see cref="ViewModelBase"/> class.
    /// </summary>
    /// <param name="logger">Logger injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <paramref name="logger"/> is <see langword="null"/>.</exception>
    protected ViewModelBase(ILogger<ViewModelBase> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
    }

    /// <summary>
    /// Executes <paramref name="action"/> inside a try/catch block, sets
    /// <see cref="IsLoading"/> and <see cref="ErrorMessage"/> appropriately and
    /// returns a <see cref="Result{T}"/> describing the outcome.  Intended to be
    /// awaited from a <c>[RelayCommand]</c> method.
    /// </summary>
    /// <typeparam name="T">Return type of <paramref name="action"/>.</typeparam>
    /// <param name="action">The asynchronous operation to perform.</param>
    /// <param name="errorContext">
    /// Optional human-readable context used to make the logged error
    /// easier to find.  When <see langword="null"/> the calling member name is
    /// used.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> describing the outcome.  The result is
    /// always returned (never <see langword="null"/>) so call-sites can be
    /// pattern-matched.
    /// </returns>
    protected async Task<Result<T>> ExecuteAsync<T>(
        Func<Task<T?>> action,
        string? errorContext = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        string context = errorContext ?? nameof(ExecuteAsync);
        Logger.LogDebug("Starting operation {Context}", context);

        try
        {
            T? value = await action().ConfigureAwait(true);
            Logger.LogDebug("Operation {Context} completed successfully", context);
            return value is null
                ? Result<T>.Failure(new InvalidOperationException(
                    $"Operation '{context}' returned no value."))
                : Result<T>.Success(value);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Operation {Context} was cancelled", context);
            return Result<T>.Failure(
                new OperationCanceledException($"Operation '{context}' was cancelled."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Operation {Context} failed", context);
            HasError = true;
            ErrorMessage = ex.Message;
            return Result<T>.Failure(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Non-generic counterpart of <see cref="ExecuteAsync{T}(Func{Task{T}?}, string?)"/>
    /// for commands that do not produce a value.
    /// </summary>
    /// <param name="action">The asynchronous operation to perform.</param>
    /// <param name="errorContext">Optional context used for logging.</param>
    /// <returns>
    /// A <see cref="Result{Object}"/> whose <see cref="Result{T}.IsSuccess"/>
    /// flag indicates whether <paramref name="action"/> ran to completion
    /// without throwing.
    /// </returns>
    protected Task<Result<object>> ExecuteAsync(
        Func<Task> action,
        string? errorContext = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        return ExecuteAsync<object>(async () =>
        {
            await action().ConfigureAwait(true);
            return new object();
        }, errorContext);
    }

    /// <summary>
    /// Raises <see cref="ObservableObject.PropertyChanged"/> for every public
    /// observable property on the derived class.  Useful when the ViewModel
    /// receives an external "refresh" trigger (for example a
    /// <c>MessagingCenter</c> event) and the View needs to re-evaluate its
    /// bindings.
    /// </summary>
    protected void RaiseAllPropertiesChanged()
    {
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
        {
            OnPropertyChanged(property.Name);
        }
    }
}
