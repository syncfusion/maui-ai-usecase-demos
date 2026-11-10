# eSIM Data Manager

A fictional traveler eSIM workflow showcase built with .NET MAUI, MVVM, and Syncfusion controls. All profile, plan, activation, usage, pricing, and order data is demo content held only in memory for the current app session. No network, backend, persistence, carrier provisioning, or payment processing is implemented.

## Features

- Dashboard with active profile, data balance, validity, seven-day usage chart, and quick actions.
- Profile lists for Active, Ready to Install, and Expired sample plans.
- Local plan search, popular destinations, filters, and simulated purchase with illustrative tax breakdown.
- Simulated activation, sample activation details/code with clipboard action, and selectable 1/3/5 GB sample top-ups.
- Compact bottom-tab navigation and wide-window persistent navigation.

## Architecture

- `eSIMDataManager/`: .NET MAUI application and view model.
- `ESimDomain.csproj`: session-scoped data models and deterministic in-memory store.
- `eSIMDataManager.Tests/`: domain logic unit tests.
- `SPEC.md`: product contract; `HARNESS.md`: development constraints; `TASKS.md`: acceptance checklist.

## Controls

The UI uses Syncfusion `SfRadialGauge`, `SfCartesianChart`, `SfListView`, and `SfBadgeView`, with native MAUI layouts and controls for navigation, search, dialogs, and plan cards.

## Validation

XAML, project, and solution XML parsing and `git diff --check` passed in the authoring environment. A .NET SDK/compiler is unavailable in that environment, so NuGet restore, compilation, unit tests, and runtime checks have not been run. Do not interpret structural validation as a build or test pass.
