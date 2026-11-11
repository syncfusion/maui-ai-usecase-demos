# eSIM Data Manager

A fictional traveler eSIM workflow showcase built with .NET MAUI, MVVM, and Syncfusion controls. All profile, plan, activation, usage, pricing, and order data is demo content held only in memory for the current app session. No network, backend, persistence, carrier provisioning, or payment processing is implemented.

## Features

- Dashboard with active profile, data balance, validity, seven-day usage chart, and quick actions.
- Profile lists for Active, Ready to Install, and Expired sample plans.
- Local plan search, popular destinations, filters, and simulated purchase with illustrative tax breakdown.
- Simulated activation, sample activation details/code with clipboard action, and selectable 1/3/5 GB sample top-ups.
- Compact bottom-tab navigation and wide-window persistent navigation.

## Project structure

- `eSIMDataManager/eSIMDataManager/`: the single .NET MAUI app project, including its view model, sample models, and in-memory demo state.
- `SPEC.md`: product contract; `HARNESS.md`: development constraints; `TASKS.md`: implementation checklist.

## Controls

The UI uses Syncfusion `SfRadialGauge`, `SfCartesianChart`, `SfListView`, and `SfBadgeView`, with native MAUI layouts and controls for navigation, search, dialogs, and plan cards.

## Validation

XAML, project, and solution XML parsing and `git diff --check` passed in the authoring environment. A .NET SDK/compiler is unavailable in that environment, so NuGet restore, compilation, and runtime checks have not been run. Automated test-project validation is not included in this UI-focused sample.
