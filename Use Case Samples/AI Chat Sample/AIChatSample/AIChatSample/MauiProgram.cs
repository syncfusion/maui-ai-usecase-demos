using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using Microsoft.Extensions.Logging;

namespace AIChatSample
{
    /// <summary>
    /// MAUI host bootstrap. Wires up DI, Syncfusion themes, the shared
    /// fonts, and picks the appropriate <see cref="IAIService"/>
    /// implementation based on environment configuration.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>Builds and configures the MAUI app. Invoked by the host at startup.</summary>
        /// <returns>The fully-configured <see cref="MauiApp"/> ready to run.</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Roboto-Medium.ttf", "Roboto-Medium");
                    fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
                    fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                    fonts.AddFont("UIFontIcons.ttf", "FontIcons");
                    fonts.AddFont("UIKitIcons.ttf", "UIKitIcons");
                });

            // Agent creation services (swap implementations for a real backend later).
            builder.Services.AddSingleton<IAgentStore, InMemoryAgentStore>();
            builder.Services.AddSingleton<IAgentService, DefaultAgentService>();

            // In-memory chat + user-profile store.
            builder.Services.AddSingleton<IChatDataService, ChatDataService>();

            // Shared app shell view-model. Registered as a singleton so
            // the Recent-chats list, user-profile header, and currently-open
            // session stay in sync across the navigation panel and the chat
            // host page.
            builder.Services.AddSingleton<AIMainLayoutViewModel>();

            // AI service: prefer AzureOpenAIService when the Azure
            // configuration is present, otherwise register the offline
            // MockAIService so the sample is still runnable with zero
            // configuration. AzureOpenAIService's constructor throws if the
            // env vars are missing, so we can't register it unconditionally
            // and let activation fail later.
            if (TryCreateAzureOpenAIService(out var azureService))
            {
                builder.Services.AddSingleton<IAIService>(azureService!);
            }
            else
            {
                builder.Services.AddSingleton<IAIService, MockAIService>();
            }

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            // Give XAML-constructed VMs (which cannot use constructor DI)
            // access to applications services.
            ServiceHelper.Initialize(app.Services);
            return app;
        }

        /// <summary>
        /// Returns <c>true</c> when the environment provides the minimum
        /// Azure OpenAI configuration. All three variables must be set
        /// and non-empty.
        /// </summary>
        private static bool HasAzureOpenAIConfiguration()
        {
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
            var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");

            return !string.IsNullOrWhiteSpace(endpoint)
                && !string.IsNullOrWhiteSpace(apiKey)
                && !string.IsNullOrWhiteSpace(deployment);
        }

        /// <summary>
        /// Attempts to construct an <see cref="AzureOpenAIService"/> from
        /// environment variables. Returns <c>true</c> and sets
        /// <paramref name="service"/> when the env vars are all present
        /// and a valid https endpoint is supplied; otherwise returns
        /// <c>false</c> so the caller can fall back to the mock service.
        /// </summary>
        private static bool TryCreateAzureOpenAIService(out IAIService? service)
        {
            service = null;
            if (!HasAzureOpenAIConfiguration())
            {
                return false;
            }

            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
            var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!;
            var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")!;
            var timeoutRaw = Environment.GetEnvironmentVariable("AZURE_OPENAI_TIMEOUT_SECONDS");
            int timeout = int.TryParse(timeoutRaw, out var t) ? t : 100;

            try
            {
                service = new AzureOpenAIService(endpoint, apiKey, deployment, timeout);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                // Misconfiguration (e.g. http:// endpoint). Surface the
                // problem in the debug log and fall back to the mock so
                // the sample still runs.
                System.Diagnostics.Debug.WriteLine(
                    $"AzureOpenAI configuration invalid, falling back to MockAIService: {ex.Message}");
                service = null;
                return false;
            }
        }
    }
}
