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

            // In-memory chat + user-profile store (NFR-3, FR-0.4).
            builder.Services.AddSingleton<IChatDataService, ChatDataService>();

            // Shared app shell view-model (FR-0.1, FR-0.4). Registered as
            // a singleton so the Recent-chats list, user-profile header
            // and currently-open session stay in sync across the
            // navigation panel and the chat host page.
            builder.Services.AddSingleton<AIMainLayoutViewModel>();

            // AI service: prefer AzureOpenAIService when the Azure
            // configuration is present, otherwise register the offline
            // MockAIService so the sample is still runnable with zero
            // configuration (NFR-2, NFR-5). AzureOpenAIService's
            // constructor throws if the env vars are missing, so we
            // can't register it unconditionally and let activation fail
            // later.
            if (HasAzureOpenAIConfiguration())
            {
                builder.Services.AddSingleton<IAIService>(_ =>
                    new AzureOpenAIService(
                        Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
                        Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY"),
                        Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
                        int.TryParse(Environment.GetEnvironmentVariable("AZURE_OPENAI_TIMEOUT_SECONDS"), out var t) ? t : 100));
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
        /// True when the environment provides the minimum Azure OpenAI
        /// configuration. All three variables must be set and
        /// non-empty (NFR-5).
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
    }
}
