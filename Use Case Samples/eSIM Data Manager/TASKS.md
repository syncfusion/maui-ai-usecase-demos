# eSIM Data Manager — Implementation Tasks

Use this file with [`HARNESS.md`](HARNESS.md) and [`SPEC.md`](SPEC.md). Mark tasks complete only after the listed checks pass.

Statuses: **Not started** · **In progress** · **Complete**

## 1. Inspect and bootstrap

**Status:** In progress — structural implementation present; restore and build not verified.

- MAUI app targets Android, iOS, Mac Catalyst, and Windows according to the committed template's OS conditions.
- Added a plain `net10.0` domain project, Syncfusion/MVVM dependencies, DI registration, and test project.
- No license key, credentials, AI package, or external-service setting is included.

**Checks:** Project structure and target framework conditions inspected. `dotnet` is unavailable, so restore/build checks remain pending.

## 2. Models and in-memory domain state

**Status:** In progress — domain behavior and tests authored; tests not executed.

- Added fictional plan/profile/usage/top-up models and a deterministic session-only store.
- Added local case-insensitive search and composable filters, purchase, activation, top-up, and expiry calculations.
- Added xUnit assertions for seeded arithmetic, search/filter behavior, purchase, activation, exact top-up amounts, and expired-profile restrictions.

**Checks:** XML parsing passed. Run `dotnet test` in a .NET 10-capable environment; no test result is claimed here.

## 3. App shell and responsive navigation

**Status:** In progress — shell destinations authored; runtime behavior not verified.

- Added Dashboard, My eSIMs, Store, and Orders Shell destinations.
- Compact default layouts use Shell tabs; Windows/Mac Catalyst use persistent desktop side navigation.

**Checks:** XML parses; target runtime and window-size behavior require MAUI build/run verification.

## 4. Dashboard

**Status:** In progress — dashboard markup and view model authored.

- Active demo profile, balance, validity, seven-day chart, and quick actions are present.
- Uses `SfRadialGauge` and `SfCartesianChart`; profile/plan status is labeled demo.

**Checks:** Structural XML passed. Gauge/chart binding and empty-state behavior require runtime verification.

## 5. My eSIMs and profile details

**Status:** In progress — active, ready, and expired profile categories are authored with `SfTabView` and `SfListView`, plus `SfBadgeView` and an `SfExpander` details section.

- Simulated activation, QR/code copy dialog, top-up, and renewal display action are wired.
- Explicitly states activation is not OS-level provisioning and expired profiles cannot top up.

**Checks:** Structural XML passed; selected profile details remain basic and the `SfTabView` binding/runtime behavior is unverified.

## 6. Explore plans

**Status:** In progress — local catalog, destination buttons, filters, and responsive plan cards are authored.

- Search uses country, region, provider; filters cover coverage/network/type/validity and clear action.
- Buy action presents illustrative tax/price confirmation and adds a Ready to Install profile.

**Checks:** Domain tests authored but unrun. Verify multi-filter behavior, purchase flow, and app UI at runtime.

## 7. Purchase, QR, and top-up experiences

**Status:** In progress — simulated dialogs and state transitions authored.

- Purchase, activation, top-up confirmation, and copy-code paths do not call external services.
- Activation details are plain sample text. A generated QR is not wired into the activation dialog yet; do not treat the sample code as a scannable QR.

**Checks:** No compilation or runtime verification. Add/verify QR visual and dialog presentation before completion.

## 8. Accessibility, UX and consistency pass

**Status:** In progress — demo labels, touch controls, and scrolling layouts authored.

**Checks:** Manual accessibility, text scaling, keyboard/focus, no-profile state, and responsive checks have not been run.

## 9. Verification and delivery documentation

**Status:** In progress — `README.md` records implementation and limitations.

- XAML, project, and solution XML parsing passed using Python `xml.etree.ElementTree`.
- `git diff --check` passed.
- `dotnet --info` failed with `dotnet: not found`; NuGet restore, compilation, xUnit tests, and runtime checks are not verified.

**Checks:** Run build/tests on a .NET MAUI-equipped host; inspect final diff for scope and correctness before marking complete.
