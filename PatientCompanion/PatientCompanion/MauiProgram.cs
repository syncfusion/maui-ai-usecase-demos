using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PatientCompanion.Services;
using PatientCompanion.ViewModels;
using PatientCompanion.Views;
using Syncfusion.Maui.Core.Hosting;

namespace PatientCompanion;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("materialicon-regular.ttf", "MaterialIcons");
            });

        RegisterServices(builder);
        RegisterViewModels(builder);
        RegisterPages(builder);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<MockDataService>();

        // Centralized appointment repository
        builder.Services.AddSingleton<Services.AppointmentService>();

        builder.Services.AddSingleton<AppShell>();
    }

    private static void RegisterViewModels(MauiAppBuilder builder)
    {
        builder.Services.AddTransient<HomeViewModel>();

        builder.Services.AddTransient<VisitsViewModel>();

        builder.Services.AddTransient<DoctorsViewModel>();

        builder.Services.AddTransient<AppointmentViewModel>();

        builder.Services.AddTransient<MedicationsViewModel>();

        builder.Services.AddTransient<HealthViewModel>();

        builder.Services.AddTransient<RecordsViewModel>();
    }

    private static void RegisterPages(MauiAppBuilder builder)
    {
        builder.Services.AddTransient<HomePage>();

        builder.Services.AddTransient<VisitsPage>();

        builder.Services.AddTransient<AppointmentDoctorSelectionView>();

        builder.Services.AddTransient<AppointmentPage>();

        builder.Services.AddTransient<MedicationsPage>();

        builder.Services.AddTransient<HealthPage>();

        builder.Services.AddTransient<RecordsPage>();
    }
}