using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Handlers;
using Syncfusion.Maui.Core.Hosting;
using WardrobeAI.AI;
using WardrobeAI.Data;
using WardrobeAI.Repositories;
using WardrobeAI.Services;

namespace WardrobeAI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Register Syncfusion license key (read from environment / user secrets).
            // Per wardrobe.prompt §1 and the syncfusion-maui-licensing skill.
            string licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY") ?? string.Empty;
            if (!string.IsNullOrEmpty(licenseKey))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
            }

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Roboto-Medium.ttf", "RobotoMedium");
                    fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
                    // MauiMaterialAssets.ttf ships inside the Syncfusion.Maui.Core
                    // NuGet package — it is resolved from the package at build
                    // time, no need to bundle a copy under Resources/Fonts/.
                    fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                    fonts.AddFont("Ui_Kit_Icons.ttf", "UiKitIcons");
                });

            EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
#if IOS
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
#if WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
#endif
            });

            // Data + repositories (SDS §8, wardrobe.prompt §5).
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "wardrobe.db");
            builder.Services.AddDbContext<WardrobeDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPlannedOutfitRepository, PlannedOutfitRepository>();
            builder.Services.AddScoped<IClothingItemRepository, ClothingItemRepository>();

            // NOTE: db.Database.Migrate() used to be called here, inside a BuildServiceProvider
            // block. That is unsafe on Android: it builds an *interim* service provider that
            // is thrown away by the next builder.Build(), and any unhandled exception escapes
            // CreateMauiApp() before the Activity starts — the OS reports a clean exit with
            // no useful stack. The migration is now applied from App.OnStart() instead, which
            // runs after the full DI container + Syncfusion host are built and after the first
            // Activity is created. See wardrobe.prompt §5 + SDS §8.

            // Services (SDS §9, wardrobe.prompt §6).
            // IUserContext replaces the removed IAuthService / IOnboardingService —
            // it auto-seeds the AppConstants.DefaultUserName singleton on first run
            // and resolves the active user id for every VM that needs it.
            builder.Services.AddScoped<IUserContext, UserContext>();

            // AI services (P2.8 — Azure OpenAI daily outfit recommendation).
            // AzureOpenAiOptions defaults are used unless a configuration
            // source is explicitly bound here. Replace the placeholders for
            // local testing or add a secure configuration source when enabling
            // live Azure OpenAI calls.
            builder.Services.AddOptions<AzureOpenAiOptions>();
            builder.Services.AddSingleton<IAIOutfitRecommendationService, AzureOpenAiOutfitRecommendationService>();
            // Smart Shopping AI service (P6.0.1b — randomized + pair-aware
            // missing-essentials selection). Shares the Azure OpenAI
            // AzureOpenAIClient-based service above. The service tolerates a missing Azure config by
            // returning a deterministic randomized fallback so the
            // Shopping page never blocks.
            builder.Services.AddSingleton<IAISmartShoppingService, AzureOpenAiSmartShoppingService>();

            // Weather + geolocation (P2.7 — live weather chip on the dashboard).
            // The platform-specific GeolocationProvider is registered from the
            // matching Platforms/* partial below.
            builder.Services.AddSingleton<IWeatherService, WeatherService>();

            // Shopping intelligence (P6 — wardrobe analysis + curated
            // recommendations + retailer deep links + per-user monthly budget).
            builder.Services.AddSingleton<IWardrobeAnalysisService, WardrobeAnalysisService>();
            builder.Services.AddSingleton<IStoreDeepLinkService, StoreDeepLinkService>();
            builder.Services.AddSingleton<IBudgetService, BudgetService>();
            // Curated 21-item "essential closet" catalog loader (wardrobe.json
            // bundled as a MauiAsset). Diffs the user's real ClothingItem rows
            // against the catalog and returns the items they're still missing
            // — rendered as a card feed on the Shopping page.
            builder.Services.AddSingleton<IWardrobeEssentialsCatalog, WardrobeEssentialsCatalog>();
#if ANDROID || IOS || MACCATALYST || WINDOWS
            builder.Services.AddSingleton<IGeolocationProvider, WardrobeAI.Services.GeolocationProvider>();
#else
            // Headless / unit-test host — no geolocation available; the dashboard
            // simply shows the placeholder until a platform target is added.
            builder.Services.AddSingleton<IGeolocationProvider, NullGeolocationProvider>();
#endif

            // Shell + App
            builder.Services.AddSingleton<AppShell>();

            // ViewModels + Pages.
            // Auth/login pages and the onboarding wizard were removed in P7 —
            // the app now boots straight into the SfTabView host with the
            // seeded default user (AppConstants.DefaultUserName).
            // Main-tab navigation service. Wraps MainTabHostViewModel so view
            // models (Dashboard, …) can request a tab switch without taking a
            // reverse constructor-injection dependency on the host (which
            // already depends on the dashboard VM for refresh dispatch).
            builder.Services.AddSingleton<Services.INavigationService, Services.MainTabNavigationService>();

            builder.Services.AddTransient<ViewModels.DashboardViewModel>();
            builder.Services.AddTransient<Views.Dashboard.DashboardPage>();
            builder.Services.AddSingleton<ViewModels.ClosetViewModel>();
            // ClosetPage is transient (not singleton) so the desktop split
            // view (ClosetPlannerSplitPage) and the SfTabView host
            // (MainTabHostPage) each get a fresh page instance with a fresh
            // root visual tree. As a singleton, MainTabHostPage.BuildTabs()
            // steals page.Content during startup and rehosts it inside a
            // tab ContentView; the desktop split page would then resolve the
            // same singleton and find page.Content == null, leaving its
            // Closet column blank. The shared state — the ClosetViewModel —
            // is still a singleton, so the data the user sees is identical
            // across both surfaces.
            builder.Services.AddTransient<Views.Closet.ClosetPage>();
            builder.Services.AddSingleton<ViewModels.OutfitPlannerViewModel>();
            // OutfitPlannerPage: same transient lifetime rationale as
            // ClosetPage above. MainTabHostPage.BuildTabs() reparents the
            // page's root view into the SfTabView, so the desktop
            // ClosetPlannerSplitPage would otherwise resolve the same
            // singleton with an empty Content tree and render nothing in
            // the planner column. OutfitPlannerViewModel stays a singleton
            // so the AI / outfit state is shared between the two hosts.
            builder.Services.AddTransient<Views.OutfitPlanner.OutfitPlannerPage>();
            builder.Services.AddSingleton<ViewModels.ShoppingViewModel>();
            // ShoppingPage is registered transient (not singleton like the
            // other tab pages). The page owns a fresh, disposable root
            // visual tree that gets reparented into either the SfTabView
            // (phone/tablet via MainTabHostPage) OR the desktop
            // DashboardShoppingSplitPage. Registering it as a singleton
            // would mean both surfaces share a single ContentPage whose
            // root view can only live in one visual tree at a time —
            // whichever one steals it first (the MainTabHost at startup)
            // wins, and the second consumer (the SplitPage on desktop)
            // ends up with an empty column. Transient lifetime gives each
            // surface its own page instance while they still share the
            // singleton ShoppingViewModel through BindingContext so the
            // AI refresh pipeline runs exactly once per data change.
            builder.Services.AddTransient<Views.Shopping.ShoppingPage>();
            builder.Services.AddSingleton<ViewModels.ProfileViewModel>();
            builder.Services.AddSingleton<Views.Profile.ProfilePage>();

            // Split-view pane: Dashboard (col 1) + Smart Shopping (col 2)
            // rendered side-by-side in a single 2-column Grid. Resolves
            // DashboardPage / ShoppingPage from DI so each child keeps its
            // existing VM, navigation, and OnAppearing refresh behaviour.
            // Transient (not singleton) so a fresh root view tree is
            // available for each consumer — the desktop host owns one,
            // and a fallback routing consumer can own another. Each
            // transient instance still reparents the singleton
            // DashboardViewModel / ShoppingViewModel via BindingContext.
            builder.Services.AddTransient<Views.DashboardShoppingSplitPage>();

            // Split-view pane: Closet (col 1) + Outfit Planner (col 2)
            // rendered side-by-side in a single 2-column Grid. Transient
            // for the same reason as DashboardShoppingSplitPage above.
            builder.Services.AddTransient<Views.ClosetPlannerSplitPage>();

            // Persistent desktop split-view host. Owns the side rail +
            // the profile popup, and toggles between the two
            // split-view panes via `IsVisible` — side-rail taps do NOT
            // trigger any Shell navigation, which avoids the
            // "whole UI sliding in from the right + full refresh"
            // behaviour the previous GoToAsync-based split-view pages
            // had. Singleton so the rail + pane state survive across
            // any external Shell navigation back into //main/desktop.
            builder.Services.AddSingleton<ViewModels.DesktopSplitHostViewModel>();
            builder.Services.AddSingleton<Views.DesktopSplitHostPage>();

            // Main tab host (SfTabView). Owns the active-tab dispatch and
            // syncs with Shell navigation for //main/* sub-routes. The host
            // page is a single-instance because its SfTabView hosts the five
            // child pages by reference; the child VMs can stay singletons
            // (they already were) so the host can dispatch refresh calls to
            // the same instance Shell previously created per page.
            builder.Services.AddSingleton<ViewModels.MainTabHostViewModel>();
            // MS DI does NOT auto-wire Lazy<T> for a registered service
            // (validated 2026-09-04: throws
            // "Unable to resolve service for type
            // 'System.Lazy`1[WardrobeAI.ViewModels.MainTabHostViewModel]'
            // while attempting to activate
            // 'WardrobeAI.Services.MainTabNavigationService'" on Android at
            // App.CreateWindow). Register the Lazy<MainTabHostViewModel>
            // wrapper explicitly so MainTabNavigationService can receive a
            // lazy reference and break the constructor cycle
            //   AppShell → MainTabHostViewModel → DashboardViewModel
            //           → INavigationService → MainTabNavigationService
            //           → MainTabHostViewModel
            // without forcing the host VM to be constructed twice.
            builder.Services.AddSingleton(
                sp => new Lazy<ViewModels.MainTabHostViewModel>(
                    () => sp.GetRequiredService<ViewModels.MainTabHostViewModel>()));
            builder.Services.AddSingleton<Views.Main.MainTabHostPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
