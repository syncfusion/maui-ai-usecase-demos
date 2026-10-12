namespace Todo.Utilities;

/// <summary>
/// Lightweight discriminated result returned by <see cref="ViewModels.ViewModelBase.ExecuteAsync{T}"/>
/// so that ViewModels can communicate success / failure without surfacing raw
/// exceptions to the View layer.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public readonly record struct Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the value returned by the operation when
    /// <see cref="IsSuccess"/> is <see langword="true"/>; otherwise the default
    /// value for <typeparamref name="T"/>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the captured exception when <see cref="IsSuccess"/> is
    /// <see langword="false"/>; otherwise <see langword="null"/>.
    /// </summary>
    public Exception? Error { get; }

    /// <summary>
    /// Gets an optional, human-readable error message suitable for display in
    /// the View layer (pop-ups, banners, validation labels, …).
    /// </summary>
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? value, Exception? error, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> wrapping the supplied
    /// <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Success(T value) => new(true, value, null, null);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> that captures
    /// <paramref name="error"/> and an optional <paramref name="errorMessage"/>
    /// suitable for UI display.
    /// </summary>
    /// <param name="error">The exception that caused the failure.</param>
    /// <param name="errorMessage">An optional UI-friendly error message.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure(Exception error, string? errorMessage = null) =>
        new(false, default, error, errorMessage ?? error.Message);
}
