using Microsoft.Extensions.DependencyInjection;

namespace AIPoweredWritingAssistant
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a new application window and initializes it with the main application shell.
        /// </summary>
        /// <param name="activationState">The activation state, which may be null.</param>
        /// <returns>A new instance of the application's main window.</returns>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}