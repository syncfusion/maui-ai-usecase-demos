using Android.App;
using Android.Runtime;

namespace Todo;

/// <summary>
/// Android <see cref="Application"/> wrapper used to register the MAUI
/// bootstrap and to forward lifecycle callbacks. The class is registered in
/// the generated AndroidManifest and instantiated by the Android runtime.
/// </summary>
[Application]
public class MainApplication : MauiApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainApplication"/> class.
    /// </summary>
    /// <param name="handle">The JNI handle for the underlying Java application instance.</param>
    /// <param name="ownership">How the Java instance is owned by the .NET runtime.</param>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
