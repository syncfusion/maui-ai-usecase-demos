using Microsoft.Extensions.Logging;
using SkyBook.Core;
using SkyBook.Maui.Ui;
using Syncfusion.Maui.A2UI.Hosting;
using Syncfusion.Maui.A2UI.SyncfusionComponents;
using Syncfusion.Maui.Core.Hosting;

namespace SkyBook.Maui;

public static class MauiProgram
{
    /// <summary>Catalog id the agent's MAUI designs declare in <c>createSurface</c>.</summary>
    public const string CatalogId = SyncfusionA2uiAppBuilderExtensions.DefaultCatalogId;

    private static async Task<string> LoadDesignAsync(string file)
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync($"designs/{file}");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

  public static MauiApp CreateMauiApp()
  {
    if (AppConfig.SyncfusionLicenseKey.Length > 0)
      Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(AppConfig.SyncfusionLicenseKey);

    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .ConfigureSyncfusionCore()
        .ConfigureFonts(fonts =>
        {
          fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
          fonts.AddFont("Roboto-Medium.ttf", "RobotoMedium");
          fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
        });

    SkyBook.Maui.BorderlessEditor.Register();

    var ai = new SkyBook.Core.AzureOpenAiOptions(
    Endpoint: "", // Include EndPoint 
    ApiKey: "",      // Include ApiKey                          
    Deployment: "");    // Include Deployment Name                                 

    builder.Services.AddSingleton(_ => SkyBook.Maui.Ui.SkyHost.Create(CatalogId));
    builder.Services.AddSingleton(_ => new HttpClient { Timeout = TimeSpan.FromMinutes(3) });

    builder.Services.AddSingleton<SkyBook.Core.IAgentClient>(sp => AppConfig.RemoteAgentUrl is { } remote
        ? new SkyBook.Core.AgentClient(sp.GetRequiredService<HttpClient>(), remote)
        : new SkyBook.Core.LocalAgent(LoadDesignAsync, sp.GetRequiredService<HttpClient>(), ai));

    builder.Services.AddSingleton<SkyBook.Core.BookingSession>();
    builder.Services.AddTransient<MainPage>();

#if DEBUG
    builder.Logging.AddDebug();
#endif
    return builder.Build();
  }
}
