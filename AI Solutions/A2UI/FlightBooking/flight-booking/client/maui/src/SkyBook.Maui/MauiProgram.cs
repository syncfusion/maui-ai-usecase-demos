using Microsoft.Extensions.Logging;
using SkyBook.Core;
using SkyBook.Maui.Ui;
using Syncfusion.Maui.A2UI.Hosting;
using Syncfusion.Maui.A2UI.SyncfusionComponents;
using Syncfusion.Maui.Core.Hosting;

namespace SkyBook.Maui;

public static class MauiProgram
{
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

    builder.Services.AddSingleton(_ => SkyBook.Maui.Ui.SkyHost.Create(CatalogId));
    builder.Services.AddSingleton(_ => new HttpClient { Timeout = TimeSpan.FromMinutes(3) });

    builder.Services.AddSingleton<SkyBook.Core.IAgentClient>(sp => AppConfig.RemoteAgentUrl is { } remote
        ? new SkyBook.Core.AgentClient(sp.GetRequiredService<HttpClient>(), remote)
        : new SkyBook.Core.LocalAgent(LoadDesignAsync, sp.GetRequiredService<HttpClient>(), ResolveAiOptions()));

    builder.Services.AddSingleton<SkyBook.Core.BookingSession>();
    builder.Services.AddTransient<MainPage>();

#if DEBUG
    builder.Logging.AddDebug();
#endif
    return builder.Build();
  }

  /// <summary>Catalog id the agent's MAUI designs declare in <c>createSurface</c>.</summary>
  public const string CatalogId = SyncfusionA2uiAppBuilderExtensions.DefaultCatalogId;

  private static readonly string AzureEndpoint = "";    // e.g. https://<resource>.openai.azure.com/
  private static readonly string AzureApiKey = "";      // your key
  private static readonly string AzureDeployment = "";  // your deployment name

  private static SkyBook.Core.AzureOpenAiOptions? ResolveAiOptions()
  {
    var fromCode = new SkyBook.Core.AzureOpenAiOptions(AzureEndpoint.Trim(), AzureApiKey.Trim(), AzureDeployment.Trim());
    var options = fromCode.IsConfigured ? fromCode : AppConfig.AzureOpenAi;
    System.Diagnostics.Debug.WriteLine(options is { IsConfigured: true }
        ? "[AI] Azure OpenAI credentials found."
        : "[AI] Azure OpenAI credentials missing (set them in MauiProgram.cs, environment variables or azure-openai.local.json).");
    return options;
  }

  private static async Task<string> LoadDesignAsync(string file)
  {
    await using var stream = await FileSystem.OpenAppPackageFileAsync($"designs/{file}");
    using var reader = new StreamReader(stream);
    return await reader.ReadToEndAsync();
  }

}
