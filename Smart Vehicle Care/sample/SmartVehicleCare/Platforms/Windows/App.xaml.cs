using Microsoft.UI.Xaml;
using Syncfusion.Licensing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SmartVehicleCare.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            // Must be registered here (before InitializeComponent) on Windows because
            // MauiXamlInflator=SourceGen causes WinUI to scan Syncfusion assemblies during
            // InitializeComponent, which races against the later MauiProgram registration.
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JAaF5cX2pCd0x1WmFZfVhgc19EZVZSQGYuP1ZhSXxVdk1jXX9ZcnFWQ2BdU0N9XEY=");
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }

}
