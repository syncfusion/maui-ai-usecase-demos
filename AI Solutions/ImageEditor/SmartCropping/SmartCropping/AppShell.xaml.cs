using Microsoft.Extensions.DependencyInjection;

namespace SmartCropping
{
    public partial class AppShell : Shell
    {
        public AppShell(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            MainShellContent.ContentTemplate =
                new DataTemplate(() => serviceProvider.GetRequiredService<SmartCropping>());
        }
    }
}
