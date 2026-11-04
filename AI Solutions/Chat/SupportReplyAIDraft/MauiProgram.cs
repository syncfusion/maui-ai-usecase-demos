using Microsoft.Extensions.Logging;
using Syncfusion.Licensing;
using Syncfusion.Maui.Core.Hosting;

namespace SupportReplyAIDraft;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore();

        var license = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
        if (!string.IsNullOrWhiteSpace(license))
        {
            SyncfusionLicenseProvider.RegisterLicense(license);
        }

        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<Services.ConversationService>();
        builder.Services.AddSingleton<Services.ISupportReplyService, Services.SupportReplyService>();
        builder.Services.AddSingleton<Services.SupportReplyValidator>();
        builder.Services.AddSingleton<ViewModels.SupportChatViewModel>();
        builder.Services.AddSingleton<Views.SupportChatPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
