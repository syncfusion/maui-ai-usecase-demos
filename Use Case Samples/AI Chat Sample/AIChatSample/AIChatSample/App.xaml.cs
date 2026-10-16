namespace AIChatSample;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Initial route: NewChat wrapped in a NavigationPage so Profile,
        // Library, CreateAgent, etc. can be pushed onto the stack.
        return new Window(new AppShell());
    }
}