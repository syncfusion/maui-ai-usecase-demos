# Travel Planner

![Travel Planner demo](TravelPlanner/Resources/Images/Travel-Planner.gif)

A .NET MAUI travel-planning sample that collects trip details in a conversational interface, requests structured Azure OpenAI recommendations, and displays cards and map pins.

## Overview

The app gathers destination, dates, trip type, and interests, then produces a destination summary and lists of places, hotels, and food spots.

## AI Capabilities

- Structured travel recommendations.
- Destination summaries.
- Place, hotel, and food spot generation.
- Guided conversational trip collection.
- Map-pin coordinates for returned top places.

## Key Features

- Destination, date range, trip type, and interest selection.
- Conversational request and suggestion UI.
- Recommendation cards with ratings.
- Map tile layer and available location pins.
- Explicit local sample itinerary fallback.

## Application Workflow

1. The user enters destination, dates, trip type, and interests.
2. `TravelPlannerViewModel` creates `TripInput`.
3. `OpenAIServices` builds and sends the Azure OpenAI chat request.
4. The response is parsed into `TripRecommendation`.
5. Cards and map pins are updated.
6. Missing or failed AI data is replaced with labeled local sample data.

## AI Workflow

```text
Trip Details
    ↓
TravelPlannerViewModel
    ↓
OpenAIServices
    ↓
Azure OpenAI Chat Completions
    ↓
TripRecommendation
    ↓
Cards and Map
```

## AI Architecture

- **View:** `MainPage.xaml` and `MainPage.xaml.cs`.
- **ViewModel:** `TravelPlannerViewModel`.
- **AI service:** `OpenAIServices`.
- **Models:** `TripInput`, `PlaceCard`, and `TripRecommendation` (in `Models/TripItem.cs`), and `MapMarkerLabel`.
- **UI support:** XAML converters and `DotAnimationBehavior`.

## Syncfusion Controls Used

- `SfAIAssistView` (`Syncfusion.Maui.AIAssistView`)
- `SfBottomSheet` (`Syncfusion.Maui.Toolkit`)
- `SfChip`/`SfChipGroup` (`Syncfusion.Maui.Toolkit`)
- `SfCalendar` (`Syncfusion.Maui.Calendar`)
- `SfListView` (`Syncfusion.Maui.ListView`)
- `SfRating` (`Syncfusion.Maui.Inputs`)
- `SfMaps` with `MapTileLayer` (`Syncfusion.Maui.Maps`)

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workloads for target platforms (the Windows target is built only on Windows).
- Syncfusion MAUI packages referenced by `TravelPlanner.csproj` (Core, AIAssistView, Toolkit, ListView, Inputs, Calendar, Maps).
- Azure OpenAI endpoint, key, and deployment for live recommendations.

## Configuration

### AI Configuration

`OpenAIServices` reads:

| Variable | Required | Default |
| --- | --- | --- |
| `AZURE_OPENAI_ENDPOINT` | Live AI | None |
| `AZURE_OPENAI_KEY` | Live AI | None |
| `AZURE_OPENAI_CHAT_DEPLOYMENT` | No | `gpt-4o-mini` |
| `AZURE_OPENAI_API_VERSION` | No | `2024-08-01-preview` |
| `SYNCFUSION_LICENSE_KEY` | Licensing | Falls back to the placeholder `ENTER_YOUR_LICENSE_KEY` in `App.xaml.cs` |

Requests are sent to `{AZURE_OPENAI_ENDPOINT}/openai/deployments/{deployment}/chat/completions`. Set the variables in the environment that launches the app (for example, before starting the IDE) so they are visible to the process. Without an endpoint and key, the app shows a labeled sample itinerary.

## Getting Started

Run from `Use Case Samples/Travel Planner`:

```sh
dotnet restore TravelPlanner.slnx

dotnet build TravelPlanner/TravelPlanner.csproj -t:Run -f net10.0-android
dotnet build TravelPlanner/TravelPlanner.csproj -t:Run -f net10.0-ios
dotnet build TravelPlanner/TravelPlanner.csproj -t:Run -f net10.0-maccatalyst
dotnet build TravelPlanner/TravelPlanner.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Project Structure

```text
Travel Planner/
├── TravelPlanner.slnx
└── TravelPlanner/
    ├── Converters/
    ├── Models/
    ├── Platforms/
    ├── Properties/
    ├── Resources/
    ├── Services/
    ├── ViewModels/
    ├── App.xaml
    ├── AppShell.xaml
    ├── DotAnimationBehavior.cs
    ├── MainPage.xaml
    ├── MauiProgram.cs
    └── TravelPlanner.csproj
```

## Troubleshooting

- **No live recommendations:** Verify Azure endpoint/key/deployment variables.
- **Empty category:** The app replaces incomplete itinerary data with local sample data.
- **Build target unavailable:** iOS/Mac Catalyst require the appropriate macOS environment.
- **License watermark:** Set a valid `SYNCFUSION_LICENSE_KEY`.

## Responsible AI Considerations

- Recommendation addresses, ratings, and availability may be inaccurate.
- Verify official destination, travel, and booking requirements.
- Destination data is sent to Azure OpenAI in live mode.
- Avoid sensitive personal information in prompts.

## Related Documentation

- [Syncfusion AIAssistView for .NET MAUI](https://help.syncfusion.com/maui/aiassistview/getting-started)
- [Syncfusion Maps for .NET MAUI](https://help.syncfusion.com/maui/maps/getting-started)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)

## Supported Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## Limitations

- Local fallback data is demonstration data, not live destination data.
- Map tiles use OpenStreetMap tile servers.
- Real-time availability is not queried.

## License

Syncfusion controls require a valid Syncfusion license for production use.
