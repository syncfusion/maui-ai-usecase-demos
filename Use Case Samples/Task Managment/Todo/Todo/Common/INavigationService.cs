namespace Todo.Common;

/// <summary>
/// Lightweight abstraction over the shell / modal navigation that
/// ViewModels can use without taking a hard dependency on
/// <c>Application.Current</c>. Registered as a singleton in
/// <c>MauiProgram</c> so all four filter pages share the same instance.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Opens a page modally on top of the current page and awaits
    /// the result (e.g. <see cref="string"/> from an input dialog,
    /// <see cref="bool"/> from a confirmation dialog, or <see cref="null"/>
    /// when cancelled).
    /// </summary>
    Task<object?> PushModalAsync(Page page);

    /// <summary>
    /// Closes the top-most modal page, optionally returning
    /// <paramref name="result"/> to the caller that opened it.
    /// </summary>
    Task PopModalAsync(object? result = null);

    /// <summary>
    /// Shows a native action sheet (e.g. the "More" menu on a note)
    /// and returns the chosen label, or <see cref="null"/> if cancelled.
    /// </summary>
    Task<string?> ShowActionSheetAsync(string title, string? message, string? destruction, params string[] buttons);
}
