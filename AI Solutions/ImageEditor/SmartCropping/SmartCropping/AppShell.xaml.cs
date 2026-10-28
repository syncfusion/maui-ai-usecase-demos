using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace SmartCropping;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        // Pick a desktop or mobile layout based on the device idiom. Both
        // views share the same view model, AI service and overlay pipeline.
        Page mainPage = DeviceInfo.Idiom == DeviceIdiom.Phone
            ? serviceProvider.GetRequiredService<SmartCroppingMobileView>()
            : serviceProvider.GetRequiredService<SmartCropping>();

        MainShellContent.ContentTemplate = new DataTemplate(() => mainPage);
    }
}