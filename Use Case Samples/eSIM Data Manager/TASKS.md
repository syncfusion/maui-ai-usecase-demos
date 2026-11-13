# eSIM Data Manager — Implementation Tasks

Use this checklist with [`HARNESS.md`](HARNESS.md) and [`SPEC.md`](SPEC.md). This implementation follows the user's UI-focused structure preference: one MAUI app project, with no separate domain or test projects. This differs from the `SPEC.md` request for a standalone domain library and automated tests; those checks are intentionally omitted and are not claimed complete.

Statuses: **Not started** · **In progress** · **Complete**

## 1. App project and dependencies

**Status:** In progress — app-only structure is present; Android build verified, other targets pending.

- The MAUI app targets Android, iOS, Mac Catalyst, and Windows according to the committed template's OS conditions.
- Models, in-memory sample state, and view model are included in the MAUI app project.
- No credentials, AI package, external-service setting, separate domain project, or test project is included.

**Checks:** `dotnet build -f net10.0-android` succeeded with the .NET 10 SDK and the `maui-android` workload. iOS, Mac Catalyst, and Windows targets still require their host workloads.

## 2. App data and UI state

**Status:** In progress — seeded data, search/filtering, and simulated in-memory purchase/activation/top-up actions are present in the app project.

**Checks:** No automated test project is included per UI-focused request. Review behavior manually after a successful MAUI build.

## 3. Responsive navigation

**Status:** In progress — compact Shell tabs and wide-window persistent navigation are authored.

**Checks:** XML structural validation passed. Runtime layout across phone, tablet, and desktop sizes is unverified.

## 4. UI views

**Status:** In progress — dashboard, profiles, plan exploration, and illustrative orders content are authored with Syncfusion controls and responsive layouts. The Top-Up Data popup (`Controls/TopUpPopupContent.cs`) reproduces the desktop reference design: gradient accent bar, header, current-profile banner, three selectable data-extension tiers, provisioning/payment summary, and Cancel / Purchase actions.

**Checks:** XAML parses structurally; control API compatibility, bindings, visual quality, and accessibility require MAUI build/runtime verification.

## 5. Simulated interactions

**Status:** In progress — plan purchase, activation, top-up, and sample activation-code copy interactions are authored without external calls. The Top-Up popup selects a tier, updates the allowance summary and the purchase button label, and adds the selected pack to the profile allocation on confirmation.

**Checks:** Runtime interactions remain unverified. Activation code text is demo-only; generated QR visual is not implemented.

## 6. Validation

**Status:** In progress.

- XAML, project, and solution XML parsed successfully with Python `xml.etree.ElementTree`.
- `git diff --check` passed.
- The Android target builds cleanly (`dotnet build -f net10.0-android`) with the .NET 10 SDK, the `maui-android` workload, JDK 17, and the Android SDK 36 platform.
- The popup content tree and tier-selection reactivity were exercised through a headless MAUI layout harness.

**Checks:** Runtime behavior on a device/emulator was not verified in this environment; visual polish and interaction flow should be confirmed on a real device.
