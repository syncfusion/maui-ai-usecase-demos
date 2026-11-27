# eSIM Data Manager

A local-only .NET MAUI sample that walks through an eSIM lifecycle (browse, buy, install, activate, top up, monitor usage) using Syncfusion controls and an in-memory data store.

## Overview

### What the sample does

The app simulates a traveler eSIM product. It seeds a list of sample plans, an active profile, and a few ready-to-install and expired profiles at startup, then lets the user browse plans, run a fake purchase, install and activate a profile, top it up, view the activation QR code, and inspect a seven-day usage chart on the dashboard. Every action mutates the in-memory `EsimStore`; nothing is sent to a network, carrier, payment processor, or external storage.

### Who it is for

Developers evaluating Syncfusion .NET MAUI controls in a realistic, multi-page MVVM shell, and teams that want a reference for combining gauges, charts, lists, tabs, popups, barcodes, and inputs in a single .NET 10 application.

### What outcome the user gets

A runnable sample that demonstrates how to compose the Syncfusion MAUI control set inside a `Shell`-based app, with a clear separation between pages, view models, models, and services, and a single source of demo data that all surfaces read from.

## Key Features

- Dashboard with the active profile, data balance, validity countdown, and a seven-day usage chart with a daily average overlay.
- Profile tabs (Active, Ready to Install, Expired) with install, activate, top-up, renew, and copy-ICCID actions.
- Store page with a search box, country chips, a segmented coverage filter, combo-box filters for network, quota, and duration, a sort selector, and plan cards that open a purchase popup.
- Top-up popup with selectable 1 GB, 3 GB, and 5 GB packs, a price summary, and a simulated payment confirmation.
- Activation popup that renders the activation QR code with `SfBarcodeGenerator` and exposes copy actions for ICCID, SM-DP+ address, and activation code.

## Application Flow

1. The user opens the app on the Dashboard. `EsimStore` seeds the active profile, the daily usage series, and the top-up packs at startup.
2. The user taps a navigation button or a contextual action (Top Up, QR, Add New eSIM, Buy Plan, Renew).
3. The page forwards the action to the relevant view model, which prepares the bound data (selected plan, selected profile, filter state, pack size).
4. The view model calls a method on `EsimStore`; the store mutates its in-memory state and raises `Changed`.
5. `MainViewModel` re-broadcasts the change to the binding layer, and the affected page or popup re-renders.
6. Confirmation dialogs from `IDialogService` close the loop; no retry logic is needed because no network call is involved.

## Controls Used

- `SfRadialGauge` — displays the active profile usage percentage on the dashboard.
- `SfLinearProgressBar` — shows usage progress for the active profile and each profile card.
- `SfCartesianChart` — renders the seven-day usage trend line and the daily average overlay.
- `SfTabView` with `SfListView` — groups Active, Ready, and Expired profiles on the Profiles page.
- `SfComboBox` — drives the Network, Quota, Duration, and Sort filters on the Store page.
- `SfSegmentedControl` — picks the Coverage filter on the Store page.
- `SfChip` — popular destination shortcuts on the Store page.
- `SfBarcodeGenerator` — renders the activation QR code inside the QR popup.
- `SfButton` — primary, secondary, navigation, and link button styles used across every page.

## Architecture

The sample is a single .NET MAUI app project with a clean MVVM split.

- Views (pages) — `DashboardPage`, `ProfilesPage`, `StorePage`, plus `AppShell`. Each page hosts its own popup overlays (`QrPopupContent`, `TopUpPopupContent`, `PurchasePopupContent`) built in code-behind under `Controls/`.
- ViewModels — `MainViewModel` aggregates `DashboardViewModel`, `ProfilesViewModel`, `StoreViewModel`, and `TopUpViewModel` and re-broadcasts their property changes so legacy single-context bindings keep working.
- Models — `Plan`, `EsimProfile` (observable), `DailyUsage`, `TopUpPack`, plus enums for coverage, network, plan type, and profile status.
- Services — `EsimStore` (in-memory data + `Changed` event), `IDialogService` / `DialogService`, `INavigationService` / `NavigationService`.
- Data flow — pages bind to `MainViewModel`; view models read from and write to `EsimStore`; `EsimStore.Changed` triggers refresh on every screen that shows profile or plan data.

## Prerequisites

- .NET 10 SDK.
- MAUI workload installed (`dotnet workload install maui`).
- Visual Studio 2022 17.13 or later with the .NET MAUI workload, or Visual Studio Code with the MAUI extension.
- Syncfusion MAUI packages referenced in `eSIMDataManager.csproj` (version 34.2.6). A Syncfusion license key is not required for evaluation builds.
- No AI provider account, API key, or external service is needed; the sample runs entirely offline.

## Default Behavior

- Local/offline logic. All plans, profiles, activation codes, QR details, prices, and usage figures are fictional and held only in memory for the current session.
- Purchase, top-up, renew, and "export roaming log" actions mutate `EsimStore` and show a local alert; no payment is taken, no carrier is contacted, and no file is written.
- A `DEMO ONLY` banner is rendered on the Store page footer to make this visible to anyone running the sample.

## Run

Build and run the Android target from the sample root:

```bash
dotnet build eSIMDataManager/eSIMDataManager.slnx -t:Run -f net10.0-android
```

Other supported targets use the same flag with a different `-f` value:

- iOS: `dotnet build eSIMDataManager/eSIMDataManager.slnx -t:Run -f net10.0-ios`
- Mac Catalyst: `dotnet build eSIMDataManager/eSIMDataManager.slnx -t:Run -f net10.0-maccatalyst`
- Windows: `dotnet build eSIMDataManager/eSIMDataManager.slnx -t:Run -f net10.0-windows10.0.19041.0`

## Output

<Img src="eSIMDataManager\eSIMDataManager\Resources\Images\esimdatamanagement.gif"/>

<br/>

- Dashboard with the radial gauge and usage chart.
- Profiles page with the Active tab populated.
- Store page with the segmented coverage filter and a plan card.
- One of the popups (QR, Top-Up, or Purchase) opened over the page.

Place the assets in a `docs/images/` folder next to this README and reference them from the sections above.

## Project Structure

- `Views/` — UI pages and layouts (`DashboardPage`, `ProfilesPage`, `StorePage`, `AppShell`).
- `Controls/` — popup and bottom-sheet content (`QrPopupContent`, `TopUpPopupContent`, `PurchasePopupContent`, `BottomSheetChrome`) plus the shared `DemoPage` that wires popup actions.
- `ViewModels/` — presentation logic (`MainViewModel` plus `Dashboard`, `Profiles`, `Store`, `TopUp` view models).
- `Models/` — data structures (`Plan`, `EsimProfile`, `DailyUsage`, `TopUpPack`, enums).
- `Services/` — helper services (`EsimStore`, `DialogService`, `NavigationService` and their interfaces).
- `Resources/` — styles, images, fonts, and assets (`Resources/Styles/Colors.xaml`, `Resources/Styles/Styles.xaml`, custom fonts, icons).

## Troubleshooting

### Issue 1: Build fails with `workload 'maui-android' not found` (or the equivalent iOS, Windows, or MacCatalyst workload)

Cause: The MAUI workload is not installed for the current .NET SDK.
Fix: Run `dotnet workload install maui` from a developer command prompt, then re-run the build command for the target you want.

### Issue 2: Syncfusion controls render as squares or show a license watermark on first launch

Cause: The Syncfusion MAUI core is not registered for the host, or the package versions in `eSIMDataManager.csproj` do not match a restored set.
Fix: Confirm that `MauiProgram.CreateMauiApp` calls `.ConfigureSyncfusionCore()` and that `dotnet restore` succeeded before the build. Then clean (`dotnet build -t:Clean`) and rebuild the project.

## Supported Platforms

- Android (`net10.0-android`).
- iOS (`net10.0-ios`).
- Windows (`net10.0-windows10.0.19041.0`).
- macOS via Mac Catalyst (`net10.0-maccatalyst`).

## License

This sample is part of the Syncfusion .NET MAUI AI Use Case Demos repository. See the repository `LICENSE` file for the vendor disclaimer and the terms that apply to the sample source code.