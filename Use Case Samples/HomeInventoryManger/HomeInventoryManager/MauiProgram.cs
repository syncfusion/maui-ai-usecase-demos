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

			builder.Services.AddSingleton<IAIService, MockAIService>();
			builder.Services.AddTransient<DashboardViewModel>();
			builder.Services.AddTransient<InventoryViewModel>();
			builder.Services.AddTransient<ScanViewModel>();
			builder.Services.AddTransient<WarrantyViewModel>();
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
