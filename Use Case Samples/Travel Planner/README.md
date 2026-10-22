# .NET MAUI Travel Planner (Syncfusion Showcase)

A .NET MAUI reference app that demonstrates how to build a conversational trip-planning experience with the **Syncfusion® .NET MAUI AIAssistView** control. The app drives the user through a guided multi-step conversation (destination, dates, trip type, interests), calls Azure OpenAI to generate a structured itinerary, and renders the results inside Syncfusion lists, ratings, calendar and maps.

> Targeted at customers evaluating Syncfusion's MAUI suite — every value-bearing UI element in the sample is a Syncfusion control.

---

## Controls demonstrated

| Control                          | Used for                                                                |
|----------------------------------|-------------------------------------------------------------------------|
| `Syncfusion.Maui.AIAssistView`   | Guided conversation, suggestions, request/response items, custom editor |
| `Syncfusion.Maui.Toolkit.BottomSheet` | Phone idiom floating chat surface                                  |
| `Syncfusion.Maui.Toolkit.Chips`  | Interest filter (multi-select) and trip-type picker                     |
| `Syncfusion.Maui.Calendar`       | Date-range picker for the trip                                          |
| `Syncfusion.Maui.ListView`       | Top places / hotels / food-spot lists                                   |
| `Syncfusion.Maui.Inputs.Rating`  | Place rating display                                                    |
| `Syncfusion.Maui.Maps.MapTileLayer` | Main and summary maps with OpenStreetMap tiles and tap-to-pin     |

The AIAssistView showcases a custom **`EditorViewTemplate`**, a custom **`SuggestionTemplate`**, a custom **`ResponseLoaderView` ControlTemplate** and a custom **Syncfusion theme dictionary** (see `App.xaml`).

---

## Prerequisites

- .NET 10 SDK (10.0.401 or newer preview)
- MAUI workload: `dotnet workload install maui-android maui-ios maui-maccatalyst`
- An **Azure OpenAI** resource (chat deployment is sufficient; image generation is optional)
- An **Unsplash** developer key — free at <https://unsplash.com/developers> (used to fetch place photos)

---

## AI / API setup

Configure the following environment variables **before** running the sample. The app reads them on startup; missing values fall back to placeholder strings and the sample generates local mock data instead of calling Azure OpenAI.

| Variable                       | Required | Example                                            | Notes                                            |
|--------------------------------|----------|----------------------------------------------------|--------------------------------------------------|
| `AZURE_OPENAI_ENDPOINT`        | Yes*     | `https://YOUR-RESOURCE.openai.azure.com`           | Trimmed of trailing slash                        |
| `AZURE_OPENAI_KEY`             | Yes*     | `…`                                                | Read by `OpenAIServices`                         |
| `AZURE_OPENAI_CHAT_DEPLOYMENT` | No       | `gpt-4o-mini`                                      | Defaults to `gpt-4o-mini`                        |
| `AZURE_OPENAI_API_VERSION`     | No       | `2024-08-01-preview`                               | Defaults to `2024-08-01-preview`                 |
| `UNSPLASH_ACCESS_KEY`          | No       | `…`                                                | When missing the sample shows a placeholder image |

\* When `AZURE_OPENAI_ENDPOINT` / `AZURE_OPENAI_KEY` are unset (or still `YOUR_AZURE_OPENAI_*`) the sample still builds and runs — the trip plan is generated from local mock data so you can preview the UI without a deployment.

Example (PowerShell):

```powershell
$env:AZURE_OPENAI_ENDPOINT = "https://YOUR-RESOURCE.openai.azure.com"
$env:AZURE_OPENAI_KEY      = "..."
$env:AZURE_OPENAI_CHAT_DEPLOYMENT = "gpt-4o-mini"
$env:UNSPLASH_ACCESS_KEY   = "..."
```

Example (bash):

```bash
export AZURE_OPENAI_ENDPOINT="https://YOUR-RESOURCE.openai.azure.com"
export AZURE_OPENAI_KEY="..."
export AZURE_OPENAI_CHAT_DEPLOYMENT="gpt-4o-mini"
export UNSPLASH_ACCESS_KEY="..."
```

The Syncfusion license key is **not** committed to this repository. Set the `SYNCFUSION_LICENSE_KEY` environment variable before running the sample. As a fallback during development, you can paste the key directly into `TravelPlanner/App.xaml.cs` (see the `// TODO` comment there) but never commit a real key to source.

### Windows-only: Bing Maps key

On Windows, `MauiProgram.cs` reads a `BING_MAPS_KEY` environment variable and passes it to the Community Toolkit Maps initializer. On non-Windows platforms the OS-native maps provider is used and no key is required.

```powershell
$env:BING_MAPS_KEY = "..."
```

---

## Run

```bash
# Restore packages
dotnet restore TravelPlanner.slnx

# Build & run for a specific target
dotnet build TravelPlanner/TravelPlanner.csproj -t:Run -f net10.0-android
dotnet build TravelPlanner/TravelPlanner.csproj -t:Run -f net10.0-ios
dotnet build TravelPlanner/TravelPlanner.csproj -t:Run -f net10.0-maccatalyst
# (Windows — run from a Windows host)
dotnet build TravelPlanner/TravelPlanner.csproj -t:Run -f net10.0-windows10.0.19041.0
```

When the app starts:

1. Tap **Build trip with AI** (phone) or use the right-hand **Trip Assistant** panel (desktop/tablet).
2. Answer the bot's guided questions: **destination → dates → trip type → interests**.
3. The trip is built and the **right drawer** opens to show the consolidated summary (top places, hotels, food spots) with map pins.

---

## Project layout

```
TravelPlanner/
├── App.xaml(.cs)                 # App-level resources, custom Syncfusion theme dictionary, license key
├── AppShell.xaml(.cs)            # Shell host
├── MainPage.xaml(.cs)            # View: phone (BottomSheet) + desktop (right panel) idiom variants
├── MauiProgram.cs                # DI: OpenAIServices (singleton), ViewModel + MainPage (transient)
├── Converters/                   # XAML value converters
├── Models/TripItem.cs            # TripInput, PlaceCard, TripRecommendation
├── Models/MapMarkerLabel.cs      # BindableObject wrapping marker text for tooltips
├── Services/OpenAIServices.cs    # Azure OpenAI + Unsplash clients (env-var driven)
├── Services/IKeyboardService.cs  # Keyboard visibility contract
├── Platforms/Android/KeyboardService_Android.cs
├── Platforms/iOS/KeyboardService_iOS.cs
└── ViewModels/
    └── TravelPlannerViewModel.cs # All conversation/business logic and commands
```

The ViewModel owns the conversation state machine, the `OpenAIServices` call, the suggestions, the calendar pending range, the interests picker state, and exposes `ICommand`s for every user action. `MainPage.xaml.cs` only handles pure view concerns (drawer animation, marker rendering, current-location permissions).

---

## Notes for contributors

- The conversation flow is implemented as a `switch` on `ConversationStep` in `TravelPlannerViewModel.HandleUserInputAsync`. To add a new step, add an enum value, handle the case, and add the corresponding `ICommand`.
- The map uses OpenStreetMap tile servers — no key required for development, but please respect the [tile usage policy](https://operations.osmfoundation.org/policies/tiles/).
- Mock data (`GenerateFallbackPlaces`) is used when the AI call fails or returns empty so the UI is always demonstrable.
