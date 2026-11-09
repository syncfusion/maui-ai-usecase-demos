# eSIM Data Manager

A .NET MAUI showcase sample for browsing simulated eSIM data plans and managing demo profiles. It demonstrates the traveler workflow **Explore → Purchase → Activate → Monitor → Top Up** using Syncfusion MAUI UI controls.

> **Simulation only:** No AI, backend, database, persistence, carrier integration, real payment, or eSIM provisioning is included. Plan names, carrier labels, prices, QR codes, activation codes, ICCIDs, and usage are fictional sample data. The generated QR cannot provision an eSIM. Changes last only for the current app process.

## Screens and workflows

- **Dashboard:** active profile, data gauge, seven-day usage chart, validity, and quick actions.
- **My eSIMs:** Active, Ready to Install, and Expired profile lists; profile detail, simulated activation, QR/code, and top-up actions.
- **Explore Plans:** local search, popular destinations, coverage/network/type/validity filters, details, and simulated purchase.
- **Purchase:** review plan, illustrative tax and total; confirm to add one install-ready profile, or cancel without a change.
- **Activation:** show/copy the sample activation code, view sample QR and instructions, and optionally simulate a Ready-to-Install → Active status change.
- **Top-up:** simulate adding 1, 3, or 5 GB to an active profile. Expired/ready profiles cannot be topped up.

## Platforms and design

Targets Android, iOS, Mac Catalyst, and Windows when built on Windows. Responsive navigation switches from compact bottom tabs to wide-screen persistent navigation. Visual direction follows the reference HTML designs under `design/Mobile` and `design/Desktop`, with adjustments necessary for .NET MAUI native controls and the sample-only scope.

## Architecture and controls

- `ESimDomain`: deterministic, session-only domain state, plan filtering, purchase, activation, and top-up rules.
- `eSIMDataManager`: MAUI shell, responsive views, and MVVM presentation state.
- `eSIMDataManager.Tests`: domain unit tests.
- Syncfusion controls: `SfRadialGauge`, `SfCartesianChart`, `SfTabView`, `SfListView`, `SfBadgeView`, `SfExpander`, and `SfBarcodeGenerator` (QR). MAUI buttons/cards and responsive layout complete the UI.
- Register a valid Syncfusion key with `SYNCFUSION_LICENSE_KEY` at runtime/build environment as appropriate; no key is included in the source.

## Verification

The implementation targets .NET 10 and Syncfusion MAUI 34.2.9 package versions. Automated domain tests cover seed arithmetic, search/filter combinations, purchase, activation, top-up packs, and expired-profile restrictions. In this environment, XML parsing and `git diff --check` passed. A .NET SDK/compiler is not installed, so restore, build, and xUnit tests could not be executed here.
