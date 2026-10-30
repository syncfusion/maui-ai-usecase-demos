using CommunityToolkit.Maui.Maps;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
#if ANDROID
using Microsoft.Maui.Handlers;
#endif
using System;
using TravelPlanner.Services;
using TravelPlanner.ViewModels;

namespace TravelPlanner;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if WINDOWS
			// Initialize the .NET MAUI Community Toolkit Maps by passing the Bing Maps Key.
			// The key is read from the environment variable `BING_MAPS_KEY` for security.
			.UseMauiCommunityToolkitMaps(Environment.GetEnvironmentVariable("BING_MAPS_KEY") ?? string.Empty)
#else
			// For all other platforms
			.UseMauiMaps()
#endif
            .ConfigureSyncfusionToolkit()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("ChangaOne-Regular.ttf", "ChangaOneRegular");
				fonts.AddFont("Lobster-Regular.ttf", "LobsterRegular");
				fonts.AddFont("MauiSampleFontIcon.ttf", "MauiSampleFontIcon");
				fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
				fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
				fonts.AddFont("Roboto-Light.ttf", "RobotoLight");
			});

		builder.Services.AddSingleton<OpenAIServices>();
		builder.Services.AddTransient<TravelPlannerViewModel>();
		builder.Services.AddTransient<MainPage>();
#if ANDROID
		EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        });
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
