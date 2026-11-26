# Smart Vehicle Care

![Smart Vehicle Care demo](vehicle-care-demo.gif)

An AI-assisted .NET MAUI vehicle-management sample that combines vehicle records, maintenance schedules, fuel history, dashboard insights, nearby-place discovery, and an Azure OpenAI chat assistant.

## Overview

Users manage vehicles and service/fuel/schedule records. Vehicle context is used by Azure OpenAI to answer maintenance questions and generate dashboard insights. Nearby places are loaded from OpenStreetMap services and fall back through AI or labeled static data.

## AI Capabilities

### Vehicle Care Chat Assistant
- The AI Assist screen uses `SfAIAssistView`. Each question is sent to Azure OpenAI together with the selected vehicle's make, model, year, odometer, health score and label, the last 5 service records, the last 5 fuel entries, and the next 5 reminders (each marked OVERDUE, DUE SOON or UPCOMING).
- The system prompt tells the model to act as a care specialist for that make and model, to refer to the real records, and to keep answers to 2-3 sentences unless a longer breakdown is needed. Answers are about this vehicle and not generic car advice.
- Three suggestion chips start the chat: "When is the next service schedule?", "Analyze fuel efficiency" and "Vehicle health report".
- A busy indicator and the text "Generating response…" show while a request runs, and the command is blocked until it finishes so a question cannot be sent twice. If the call fails, the chat shows a short message with the error reason instead of an empty reply.

### Dashboard Maintenance Insights
- The "Active intelligence" card asks the model for exactly 3 insights. The prompt includes the vehicle, the service count and days since the last service, the fuel entry count and average spend per fill, and the next reminder due in the next 60 days.
- The model must answer in a fixed `TITLE` / `SUBTITLE` / `SEVERITY` format (`low`, `medium` or `high`). The app parses each block, uses the severity to pick the accent color of the card, and shows the title and subtitle.
- If the AI returns nothing usable, the card falls back to rule-based insights built from the same data (service age, fuel activity, oil, tire and brake intervals, and upcoming reminders). If there is no data yet, it shows "Add your first service record" and "Log a fuel entry". If no key is set, it shows "API key not configured".
- Insights refresh when the selected vehicle's data changes and when the API key is saved or removed.

### Vehicle Health Summary
- The health score is calculated in code, not by the AI. It combines maintenance (40%), schedule (15%), fuel (10%) and expense trend (10%) into a label of EXCELLENT, GOOD, FAIR or POOR.
- The AI then writes a two-sentence summary for the "AI insight" box on the Vehicle health card. The prompt includes the score, the items that are going well, the items that need attention, and the next step that would raise the score.
- The prompt changes the tone by score. A score of 85 or more gets praise plus one proactive tip. A score of 70 to 84 gets a short positive note plus the one thing to act on. A lower score leads with the most important problem and a clear next step.
- The model is told to use plain English with no markdown or bullet points. If the call returns nothing, the app shows a rule-based sentence with the condition label and score instead. If no key is set, the box asks the user to add one in Settings.

### Spending Optimization
- On the Vehicle Center Maintenance tab, the "AI financial analysis" card compares the last 90 days with the 90 days before, split into fuel and maintenance spend. The prompt also includes the average fuel price per litre and the average spend per fill-up.
- The model returns exactly three lines: `TREND` (a short headline), `DETAIL` (one sentence using the real rupee amounts and the change) and `ACTION` (one practical fuel-saving step). Each line is shown in its own place on the card.
- If the reply cannot be parsed, the card shows a trend, detail and tip that the app calculates from the same numbers. If no key is set, it shows "AI insights are not configured".

### Nearby Places Fallback
- The Fuel stations and Service centers lists first use live OpenStreetMap data from the Overpass API (three servers are tried in turn).
- Only if that fails and an API key is set does the app ask the model for 8 to 10 places as a JSON array with `Name`, `OpenTime`, `Address`, `Latitude` and `Longitude`, placed within about 0.08° of the user. The model can also return a `Zoom` hint, which the app limits to 12-16 for the map. Invalid items, such as ones without coordinates or named "Error", are removed.
- If AI is also unavailable, the app uses curated sample places. Whenever the results are not live OSM data, the map summary reads "sample data (live OSM unavailable)" so the user can tell them apart.

## Key Features

### Onboarding and Vehicle Setup
The app opens on a splash screen and then a welcome page.
- A splash screen with a progress bar loads the saved vehicle data from the device before the welcome page appears.
- A 3-step wizard creates the first vehicle: choose Car or Motorcycle, enter make, model, optional variant and current odometer, then finish. Make, model and a valid odometer are required to continue.
- "Skip & explore with sample data" loads two demo vehicles (Hyundai i20 and Honda Activa) with service history, fuel logs and reminders.
- The dashboard's "Add vehicle" action opens the same form as a side drawer on Windows and Mac Catalyst and as a bottom sheet on Android and iOS. A startup prompt, "Activate AI features", lets the user paste an API key or skip it.

### Dashboard
- A vehicle card shows the car or bike photo, name, variant, registration, odometer, a condition badge, the last service age, the next service due and this month's spend. A header `SfComboBox` switches between vehicles.
- The Vehicle health card shows the score out of 100 and the checklist of items that are fine or need attention.
- The Monthly expense card has a month selector, a total, a comparison with the monthly average, and fuel and maintenance shares drawn with `SfLinearProgressBar`.
- Recent activity lists service, fuel and reminder entries in an `SfDataGrid`. "View all" opens the full list in an `SfPopup`.

### Vehicle Center: Maintenance
- The `SfTabView` has two tabs, Maintenance and Schedule. The Maintenance tab shows cards for total services, last service, total spent and next service due.
- The Service log and Fuel spending `SfDataGrid` tables show the latest 5 entries, with "View all" in a popup. Each row has edit and delete icons, and delete asks for confirmation.
- Add service offers six types (Oil & filter, Tyre & wheel, Brake repair, Battery & electrical, General service, Custom) and the statuses Completed, Pending and Scheduled. Add fuel offers Petrol, Diesel and CNG and shows the total cost as litres × price per litre.
- The Expense history `SfCartesianChart` column chart can be grouped by Day, Week or Month, or by a custom range chosen with `SfDatePicker` dialogs, with a tooltip for each bar.

### Vehicle Center: Schedule
- An `SfScheduler` shows reminders as appointments in Month, Week or Day view, with previous and next buttons and a period title. Appointment colors follow priority (Critical, High, Medium, Low).
- "Add schedule" creates a reminder with a type (Oil & filter, Tyre & wheel, General service, Custom), title, due date and time (`SfDatePicker` and `SfTimePicker`) and priority.
- Upcoming alerts list each reminder with an OVERDUE, DUE SOON (within 14 days) or UPCOMING badge, and a Remove action.
- The "Next service forecast" card estimates the next service date and mileage from the schedule, the service history and the vehicle's service interval. "Add forecast to schedule" adds it as a reminder, or shows "Already on schedule".

### Nearby Fuel Stations and Service Centers
- An `SfMaps` tile layer with OpenStreetMap tiles shows pins with a tooltip (name, details, address). Two `SfButton` toggles switch between Service centers and Fuel stations.
- The app asks for location permission and centers the map on the user. If permission is denied, it uses a fixed default location (latitude 13.0827, longitude 80.2707).
- The list next to the map shows the name, area and opening hours. Tapping an item selects its marker on the map. A loading overlay shows while places are fetched.

### Settings and Storage
- Settings has an app theme choice (Light, System default, Dark) and AI configuration for the API key, endpoint and deployment name, with Save and Remove actions and a status line.
- "Your vehicles" lets the user edit make, model, variant and odometer, or delete a vehicle after a confirmation dialog. "Sign out" removes the saved API key and returns to the welcome page.
- Vehicles, services, fuel entries and reminders are saved as JSON in `Preferences` and restored at startup. The API key is saved in `SecureStorage`.
- The layout adapts to the window width. At 700 or more it shows a side navigation bar. Below 700 it shows a bottom navigation bar with Dashboard, Vehicle, AI Assist and Settings.

## Application Workflow

1. The splash/Welcome flow opens the main vehicle application.
2. The user adds or selects a vehicle.
3. `VehicleDataService` exposes vehicle, service, fuel, and schedule records.
4. Dashboard and AI pages read the selected vehicle and build context.
5. `AzureOpenAIService` sends requests when endpoint/key/deployment are configured.
6. Nearby-place requests query Overpass/OSM before fallback sources.
7. Results are rendered in dashboards, lists, chat, scheduler, and map.

## AI Workflow

```text
Selected Vehicle and Records
    ↓
ViewModel Prompt Builder
    ↓
AzureOpenAIService
    ↓
Azure OpenAI Chat Completions
    ↓
Vehicle Advice or Dashboard Insight
```

Nearby places follow:

```text
Overpass/OSM
    ↓
Azure OpenAI Generated Places
    ↓
Curated Static Sample Data
```

## AI Architecture

- **Views:** Dashboard, AI assistant, vehicle center, settings, splash, welcome, and add/edit panels.
- **ViewModels:** Dashboard, AI assistant, vehicle center, settings, welcome, and entry ViewModels.
- **Services:** `AzureOpenAIService`, `VehicleDataService`, and `ServiceCenterDataHelper`.
- **Models:** `Vehicle`, `CustomMarker`, dashboard, and vehicle-center data.
- **Persistence:** Vehicle data is serialized into device `Preferences`.

## Syncfusion Controls Used

- `SfAIAssistView`
- `SfScheduler`
- `SfMaps`
- `SfDataGrid`
- `SfCartesianChart`
- `SfTabView`
- `SfPopup`
- `SfDatePicker`
- `SfTimePicker`
- `SfComboBox`
- `SfTextInputLayout`
- `SfLinearProgressBar`
- `SfBusyIndicator`
- `SfButton`

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workload for the target platform (iOS and Mac Catalyst targets are skipped on Linux; the Windows target is built only on Windows).
- Syncfusion packages referenced by `SmartVehicleCare.csproj` (AIAssistView, Core, Inputs, Buttons, Maps, Picker, Charts, Scheduler, TabView, ProgressBar, DataGrid, Popup).
- Location services for nearby-place discovery.
- Azure OpenAI configuration only for live AI features.
- Internet access for the Overpass/OpenStreetMap nearby-place lookup.

## Configuration

### AI Configuration

AI settings are entered in the app Settings page:

| Setting | Storage | Purpose |
| --- | --- | --- |
| API key | `SecureStorage` | Azure OpenAI API key |
| Base endpoint | `Preferences` | Azure AI Foundry `/openai/v1` endpoint (requests go to `{endpoint}/chat/completions`) |
| Deployment name | `Preferences` | Defaults to `gpt-5-mini` |

No credentials are stored in source code.

### Syncfusion License

The license is read from the `SYNCFUSION_LICENSE_KEY` environment variable at startup in `MauiProgram.cs`. Set it before launching the app.

## Getting Started

Run from `Use Case Samples/Smart Vehicle Care`:

```sh
dotnet restore SmartVehicleCare.slnx

dotnet build SmartVehicleCare/SmartVehicleCare.csproj -t:Run -f net10.0-android
dotnet build SmartVehicleCare/SmartVehicleCare.csproj -t:Run -f net10.0-ios
dotnet build SmartVehicleCare/SmartVehicleCare.csproj -t:Run -f net10.0-maccatalyst
dotnet build SmartVehicleCare/SmartVehicleCare.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Project Structure

```text
Smart Vehicle Care/
├── SmartVehicleCare.slnx
├── vehicle-care-demo.gif
└── SmartVehicleCare/
    ├── Behaviors/
    ├── Converters/
    ├── Helpers/
    ├── Models/
    ├── Platforms/
    ├── Properties/
    ├── Resources/
    ├── Services/
    ├── ViewModels/
    ├── Views/
    ├── App.xaml
    ├── AppShell.xaml
    ├── MauiProgram.cs
    └── SmartVehicleCare.csproj
```

## Troubleshooting

- **AI is unavailable:** Open Settings and enter the key, endpoint, and deployment.
- **Nearby map shows sample data:** Grant location permission and check network access. Static results are labeled as fallback data.
- **Vehicle data missing after reinstall or clearing app data:** Vehicle data is stored as JSON in `Preferences`, which is removed with app data.
- **License watermark:** Set `SYNCFUSION_LICENSE_KEY`.

## Responsible AI Considerations

- AI prompts include vehicle profile and recent record context.
- AI maintenance advice is not authoritative service guidance.
- API keys are not stored in source or committed.
- Review Overpass and Azure requests before adding personal information.

## Related Documentation

- [Syncfusion AIAssistView for .NET MAUI](https://help.syncfusion.com/maui/aiassistview/getting-started)
- [Syncfusion Scheduler for .NET MAUI](https://help.syncfusion.com/maui/scheduler/getting-started)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)

## Supported Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## Limitations

- Live AI features require Azure configuration.
- Vehicle records are demonstration data stored on-device in `Preferences` (no cloud sync or database).
- Nearby static fallbacks are not live inventory.

## License

Syncfusion controls require a valid Syncfusion license for production use.
