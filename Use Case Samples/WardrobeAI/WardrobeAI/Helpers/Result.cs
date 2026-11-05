namespace WardrobeAI.Helpers;

/// <summary>
/// Error details carried by <see cref="Result{T}"/>. No exceptions leak to ViewModels.
/// </summary>
/// <param name="Code">Stable error code, e.g. "Onboarding.Incomplete".</param>
/// <param name="Message">Human-readable message safe to surface in UI.</param>
public sealed record Error(string Code, string Message);

/// <summary>
/// Discriminated result used by every service per wardrobe.prompt §10
/// ("All AI services return Result&lt;T&gt;"). Success carries the value; Failure carries an Error.
/// </summary>
public readonly record struct Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public Error? Error { get; init; }

    public static Result<T> Ok(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Fail(Error error) => new() { IsSuccess = false, Error = error };
    public static Result<T> Fail(string code, string message) => new() { IsSuccess = false, Error = new Error(code, message) };
}
