using Microsoft.Extensions.DependencyInjection;

namespace SmartAITags
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new TagGeneratorPage());
        }
    }
}