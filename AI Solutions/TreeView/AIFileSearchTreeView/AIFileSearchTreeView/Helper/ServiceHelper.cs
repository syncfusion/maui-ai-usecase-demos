namespace AIFileSearchTreeView;

/// <summary>
/// Holds the application's <see cref="IServiceProvider"/> so that code outside
/// the DI graph (e.g. <see cref="Microsoft.Maui.Controls.Application"/>) can
/// resolve services. Prefer constructor injection in pages and view models.
/// </summary>
public static class ServiceHelper
{
    public static IServiceProvider? CurrentServices { get; set; }

    /// <summary>
    /// Returns the current service provider. Throws when accessed before
    /// <see cref="MauiProgram"/> has finished building the app.
    /// </summary>
    public static IServiceProvider Services =>
        CurrentServices ?? throw new InvalidOperationException("Services not initialized yet.");
}