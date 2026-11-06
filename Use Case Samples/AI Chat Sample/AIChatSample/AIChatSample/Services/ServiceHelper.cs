namespace AIChatSample;

/// <summary>
/// Minimal service locator giving non-DI-constructed view-models (XAML
/// BindingContexts, Activator-created pages) access to app services.
/// Initialized once in <c>MauiProgram</c>; kept deliberately small so DI
/// constructors remain the preferred pattern.
/// </summary>
public static class ServiceHelper
{
    /// <summary>Root service provider of the running MAUI app.</summary>
    public static IServiceProvider? Services { get; private set; }

    /// <summary>Stores <paramref name="services"/> as the root provider for the app.</summary>
    public static void Initialize(IServiceProvider services) => Services = services;

    /// <summary>Resolves <typeparamref name="T"/> from <see cref="Services"/>, or <c>null</c> when unavailable.</summary>
    public static T? GetService<T>() where T : class =>
        Services?.GetService(typeof(T)) is T service ? service : null;

    /// <summary>Resolves <typeparamref name="T"/> from <see cref="Services"/>, throwing if the type is unregistered.</summary>
    public static T GetRequiredService<T>() where T : class
    {
        var service = Services?.GetService(typeof(T)) as T;
        return service ?? throw new InvalidOperationException(
            $"{typeof(T).Name} is not registered. Call ServiceHelper.Initialize(app.Services) at startup.");
    }
}
