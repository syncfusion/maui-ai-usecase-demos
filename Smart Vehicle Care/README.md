# Build an AI-Powered Vehicle Care App in .NET MAUI with Syncfusion Controls

This repository demonstrates how to build a complete vehicle management app in .NET MAUI by combining Syncfusion's rich UI controls with an Azure OpenAI backend. It covers service tracking, fuel logging, maintenance scheduling, nearby place discovery, and an AI chat assistant — all in a single cross-platform app.

## Key highlights

* Multi-tab dashboard with health scoring and quick-action panels
* AI Assistant powered by Azure OpenAI for vehicle-specific queries
* Real nearby service centers and fuel stations via OpenStreetMap (Overpass API)
* Maintenance scheduler with calendar views and overdue alerts
* Platform-aware UX: two-column layout on Windows/macOS, full-screen on Android/iOS

## Features

* **Dashboard** – Vehicle health score, service history summary, fuel log, and quick actions.
* **AI Assistant** – Conversational AI (SfAIAssistView) answering vehicle maintenance, cost, and service questions.
* **Vehicle Center** – Add and manage multiple vehicles with service and fuel records via SfDataGrid.
* **Maintenance Scheduler** – SfScheduler with Month/Week/Day views, color-coded appointment types (Service, Inspection, Overdue).
* **Nearby Network** – Live map (SfMaps tile layer) showing fuel stations or service centers within 10 km using Overpass OSM data with an AI fallback.
* **Welcome Wizard** – Step-by-step onboarding (vehicle profile) with adaptive layouts.

## Technologies Used

* **.NET MAUI (net10.0)** – Cross-platform framework for Android, iOS, macOS, and Windows.
* **Syncfusion 34.2.3** – SfAIAssistView, SfScheduler, SfMaps, SfDataGrid, SfTabView, SfCharts, SfPopup, SfPicker, SfProgressBar, SfButtons, SfInputs, and more.
* **Azure OpenAI Services** – AI Assistant chat.
* **Overpass API (OpenStreetMap)** – Real-time nearby fuel stations and service centers.

## Prerequisites

* .NET SDK compatible with .NET MAUI (net10.0 or later)
* Visual Studio 2022 with .NET MAUI workload installed
* Azure OpenAI account with an API endpoint, API key, and deployment name (optional — app works offline without it)
* Syncfusion license key

## Quick Install

**Clone:**
```bash
git clone https://github.com/syncfusion/maui-ai-usecase-demos
cd VehicleCareApp
```

**Configure Azure OpenAI credentials** in `Services/AzureOpenAIService.cs`:
```csharp
private const string Endpoint       = "YOUR_AZURE_OPENAI_ENDPOINT";
private const string DeploymentName = "YOUR_DEPLOYMENT_NAME";
private const string ApiKey         = "YOUR_API_KEY";
```

**Register Syncfusion license** in `MauiProgram.cs`:
```csharp
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
```

## How It Works

### Navigation Flow

* Shell hosts a tab bar with Dashboard, AI Assist, Vehicle Center, and Settings tabs.
* Welcome wizard runs on first launch; sample data can be loaded via "Skip & Explore".
* On Windows/macOS the wizard shows a hero image panel alongside the form; on mobile only the form is shown (no card elevation).

### AI Assistant

* User types a vehicle-related question in `SfAIAssistView`.
* `AIAssistViewModel` forwards the message to `AzureOpenAIService` with a vehicle-context system prompt.
* The response streams back and is displayed as an AI bubble.
* If no API key is configured, the assistant shows a graceful offline message.

### Nearby Network

* On tab load, GPS coordinates are obtained and an Overpass OSM query runs for fuel stations or service centers within 10 km.
* If Overpass is unavailable, Azure OpenAI generates plausible nearby places as a fallback.
* If both are unavailable, a curated static sample dataset is shown.
* Results are pinned on an `SfMaps` tile layer and listed in a card below the map.

## Troubleshooting

### No AI Response
* Verify Azure OpenAI credentials (endpoint, key, deployment name).
* Check network connectivity.
* The app runs fully offline — AI features are optional enhancements.

### Nearby Map Shows Sample Data
* Grant location permission when prompted.
* The Overpass API may be temporarily unavailable; the app automatically falls back to static sample data.

### Path Too Long Exception
* Close Visual Studio, rename the cloned folder to a shorter path, and rebuild.

## Screenshot

![Vehicle Care AI — .NET MAUI](VehicleCareApp.gif)

## Documentation

* [Syncfusion .NET MAUI AI AssistView](https://help.syncfusion.com/maui/aiassistview/getting-started)
* [Syncfusion .NET MAUI Scheduler](https://help.syncfusion.com/maui/scheduler/getting-started)
* [Syncfusion .NET MAUI Maps](https://help.syncfusion.com/maui/maps/getting-started)
* [Azure OpenAI Service Documentation](https://learn.microsoft.com/en-us/azure/ai-foundry/?view=foundry-classic)
