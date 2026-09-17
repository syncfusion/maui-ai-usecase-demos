namespace PatientCompanion;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    public static T Resolve<T>() where T : notnull
    {
        var service = Current?.Handler?.MauiContext?.Services.GetService(typeof(T));
        if (service is T typedService)
            return typedService;

        throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
    }
}