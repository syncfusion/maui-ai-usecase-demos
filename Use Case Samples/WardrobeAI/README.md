# WardrobeAI

WardrobeAI is a .NET MAUI sample app that demonstrates an AI-assisted wardrobe experience using Syncfusion .NET MAUI controls. Users can catalog clothing, get outfit suggestions for an occasion and weather, plan outfits, and explore wardrobe essentials to shop for. Azure OpenAI powers outfit and shopping recommendations when configured; deterministic fallback behavior is available without an Azure model setup.

## Features

- Five main areas: Dashboard, Closet, Plan, Shop, and Profile.
- Dashboard with a daily outfit suggestion, weather summary, quick actions, and upcoming plans.
- Closet gallery with category filters and an Add Clothing popup that supports camera/gallery capture and clothing attributes.
- Outfit planning by occasion and date, with weather-aware recommendations, match scores, alternatives, and an explanation of the recommendation.
- Shopping recommendations for wardrobe essentials, a completion score, budget information, and retailer deep links.
- Profile controls for style preferences, palette, achievements, and monthly budget.
- Mobile tab navigation and desktop split-view layouts for Windows and Mac Catalyst.
- Live AI recommendations through Azure OpenAI when configured, with deterministic outfit and shopping fallbacks otherwise.
- Weather and geolocation data from Open-Meteo; no API key is required for these services.

## Syncfusion Controls Demonstrated

| Control | Example use |
| --- | --- |
| `SfTabView` | Main mobile tab shell |
| `SfAvatarView` | Dashboard greeting, desktop side rail, and Profile header |
| `SfRotator` | Outfit and shopping product cards |
| `SfRadialGauge` | Outfit match and planner scores |
| `SfChipGroup` | Clothing filters, attributes, occasion, and style selection |
| `SfComboBox` | Seven clothing attribute selectors in the Add Clothing popup |
| `SfButton` | Quick actions, save actions, and shopping calls to action |
| `SfPopup` | Add Clothing, Plan an Outfit, and Add Style dialogs |
| `SfDatePicker` | Outfit planning date selection |
| `SfAccordion` | Expanding the shopping recommendation methodology |


## Project Structure

```text
WardrobeAI/
├── AI/                  Azure OpenAI services, prompt builders, and scoring
├── Constants/           Clothing attribute options (`WardrobeAI.Constants` namespace)
├── Constants.cs         Project-wide `AppConstants` (`WardrobeAI` root namespace)
├── Contracts/           Shared data contracts, including weather
├── Converters/          XAML value converters
├── Data/                EF Core database context
├── Helpers/             Result type and utility extensions
├── Models/              User, clothing, outfit, and shopping models
├── Platforms/           Android, iOS, Windows, and Mac Catalyst code
├── Repositories/        SQLite-backed data access
├── Resources/           Images, fonts, styles, and bundled raw assets
├── Services/            Weather, user, wardrobe, budget, and navigation services
├── ViewModels/          MVVM viewmodels for the app's pages and hosts
└── Views/               Dashboard, Closet, Planner, Shopping, Profile, and host pages
```

The solution file is `WardrobeAI.slnx`, and the app project is `WardrobeAI/WardrobeAI.csproj`.

## Quick Start

### Prerequisites

- .NET 10 SDK.
- .NET MAUI workload. Install it if needed:

  ```bash
  dotnet workload install maui
  ```

- An Azure OpenAI resource is optional. Without configured Azure credentials, the app uses deterministic fallback recommendations.
- A Syncfusion license key is optional for trial use. Without a key, Syncfusion controls run in trial mode and may display a watermark.

### Restore

Run these commands from the `Use Case Samples/WardrobeAI` directory:

```bash
dotnet workload restore
dotnet restore WardrobeAI/WardrobeAI.csproj
```

### Configure Syncfusion licensing (optional)

Set `SYNCFUSION_LICENSE_KEY` in the environment before launching. The app reads this variable at startup; the key is not stored in the repository.

```powershell
# Windows PowerShell
$env:SYNCFUSION_LICENSE_KEY = "your-key-here"
```

```bash
# macOS / Linux
export SYNCFUSION_LICENSE_KEY="your-key-here"
```

### Configure Azure OpenAI (optional)

The required fields are `Endpoint`, `ApiKey`, `Deployment`, and `ApiVersion`. The values in `WardrobeAI/AI/AzureOpenAiOptions.cs` are placeholders, and `MauiProgram.cs` currently registers the options defaults. To enable live Azure recommendations, replace those placeholder values for local testing or wire a suitable configuration source in `MauiProgram.cs`. Keep credentials out of source control. When the options are not configured, deterministic fallback recommendations are used.

### Build and run

Use a platform supported by the host operating system and installed MAUI workload. iOS and Mac Catalyst require macOS. The project targets Android, iOS, Mac Catalyst, and Windows where supported by the host.

```bash
# Windows
dotnet build WardrobeAI/WardrobeAI.csproj -f net10.0-windows10.0.19041.0
dotnet run --project WardrobeAI/WardrobeAI.csproj -f net10.0-windows10.0.19041.0

# Android
dotnet build WardrobeAI/WardrobeAI.csproj -f net10.0-android
dotnet run --project WardrobeAI/WardrobeAI.csproj -f net10.0-android

# Mac Catalyst (macOS host required)
dotnet build WardrobeAI/WardrobeAI.csproj -f net10.0-maccatalyst
dotnet run --project WardrobeAI/WardrobeAI.csproj -f net10.0-maccatalyst
```

The first launch creates/updates the local SQLite database at `FileSystem.AppDataDirectory/wardrobe.db`, seeds a default user, and opens the Dashboard.

## How to Use

1. Open Dashboard to review the suggested outfit, weather, and upcoming plans.
2. Add clothing in Closet and use the filters to browse your wardrobe.
3. Open Plan, choose an occasion and date, and review the suggested outfit, scores, alternatives, and explanation.
4. Browse Shop for missing wardrobe essentials and review budget information or retailer links.
5. Adjust style preferences and budget from Profile.

## Notes

- This sample demonstrates Syncfusion controls, MVVM with `CommunityToolkit.Mvvm`, local SQLite persistence through EF Core, and Azure OpenAI structured-output recommendations.
- The default Azure OpenAI settings are placeholders. Configure actual endpoint, key, deployment, and API version values to enable live model calls.
- Open-Meteo provides weather/geolocation data without an API key.
- `WardrobeAI.csproj` forces runtime XAML inflation for the tab host and outfit planner pages to work around MAUIG1001 source-generator parsing issues; the remaining XAML continues to use source generation.
- `MauiMaterialAssets.ttf` is provided by the `Syncfusion.Maui.Core` NuGet package and does not need to be copied into the project.
- Project Type and Priority are not applicable to this sample.

## Key Highlights

- Responsive wardrobe experience across mobile and desktop layouts.
- AI-assisted daily outfit planning and shopping recommendations, with offline fallback behavior.
- Persistent clothing and outfit data stored locally in SQLite.
- Weather-aware outfit suggestions and a curated missing-essentials catalog.
- Practical examples of Syncfusion MAUI controls and shared MVVM patterns.

## Troubleshooting

- **The app does not build for a target:** Confirm that .NET 10 SDK and the required MAUI workloads are installed, and that the target is supported on the current host.
- **AI recommendations use fallback results:** Check that `AzureOpenAiOptions` has a valid endpoint, API key, deployment name, and API version in the runtime configuration, then restart the app.
- **Syncfusion controls show a trial watermark:** Set `SYNCFUSION_LICENSE_KEY` before launching and verify that the key is valid for the installed Syncfusion packages.
- **Weather information is unavailable:** Check network connectivity and whether the Open-Meteo service is reachable; it does not require an API key.
- **Location is unavailable:** Verify that location permission is granted to the app and that the device or emulator provides location data.

## License

Syncfusion controls are subject to the Syncfusion license. A free community license may be available to qualifying individuals and small businesses; see Syncfusion's licensing terms for eligibility. No separate project license is specified in the sample directory.
