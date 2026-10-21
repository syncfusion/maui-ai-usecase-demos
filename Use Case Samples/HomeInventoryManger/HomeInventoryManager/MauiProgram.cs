using HomeInventoryManager.Services;
using HomeInventoryManager.ViewModel;
using HomeInventoryManager.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;

namespace HomeInventoryManager
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureSyncfusionToolkit()
				.ConfigureSyncfusionCore()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("Roboto-Medium.ttf", "Roboto-Medium");
                    fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
					fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
				});

			// AI service (swap MockAIService for an Azure OpenAI implementation when keys are available)
			builder.Services.AddSingleton<IAIService, MockAIService>();

			// ViewModels (singletons so in-memory data is shared across pages)
			builder.Services.AddSingleton<DashboardViewModel>();
			builder.Services.AddSingleton<InventoryViewModel>();
			builder.Services.AddSingleton<ScanViewModel>();
			builder.Services.AddSingleton<WarrantyViewModel>();

			// Pages
			builder.Services.AddTransient<DashboardPage>();
			builder.Services.AddTransient<InventoryPage>();
			builder.Services.AddTransient<ScanPage>();
			builder.Services.AddTransient<WarrantyPage>();

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
