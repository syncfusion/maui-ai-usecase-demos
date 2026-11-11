# TASKS — SkyBook .NET MAUI migration

`[x]` done and verified in this repository · `[~]` implemented, needs device/E2E confirmation · `[ ]` open

## Harness
- [x] H1 INTENT.md · [x] H2 PLAN.md · [x] H3 TASKS.md · [x] H4 ARCHITECTURE.md · [x] H5 verify.sh · [x] H6 README

## Scaffold (.NET 10)
- [x] T1 `global.json` pins SDK 10, `Directory.Build.props` (nullable, warnings-as-errors, Syncfusion 35.1.43)
- [x] T2 `SkyBook.Maui` from `dotnet new maui` (net10.0-android/ios/maccatalyst/windows), template sample removed
- [x] T3 `SkyBook.Core` (net10.0) + `SkyBook.Core.Tests` (xUnit)

## Core logic (ports of App.tsx)
- [x] T4 `Stage` table + `StepFromSurfaceId` + `InferStep`
- [x] T5 `ActionSummarizer` (`summarizeAction`)
- [x] T6 `EnvelopeSanitizer` (`sanitizeEnvelope`, `extractExplanation`)
- [x] T7 `BookingSession` (trip memory, history ≤ 8, Book-now instruction, stage-3 intent, noise/busy filter)
- [x] T8 `AgentClient` (A2A `message/send`, response parse)
- [x] T9 `ContextResolver` (`{path}` resolution, ISO dates, `selectedRow` JSON string → object)
- [x] T10 28 unit tests incl. the real agent design JSON and the A2A request shape

## Agent
- [x] T11 `designs_maui/stage1-flight-search.json`
- [x] T12 `designs_maui/stage2-flight-results.json` (row tap + Book now button)
- [x] T13 `designs_maui/stage3-booking-confirmation.json`
- [x] T14 all three load in the real `SurfaceHost` / `syncfusion-maui` catalog without errors
- [x] T15 `FLIGHT_BOOKING_CLIENT=maui` switch + MAUI prompt addendum (default behaviour unchanged)

## App
- [x] T16 `MauiProgram`: `ConfigureSyncfusionCore`, `UseA2UIWithSyncfusionComponents`, DI (HttpClient, AgentClient, BookingSession)
- [x] T17 Theme tokens (`SkyBookTheme.xaml`) from App.css `:root`
- [x] T18 Header (brand mark, title, Home pill)
- [x] T19 Welcome: badge, hero headline, journey dots, stage cards, Ask-SkyBook panel
- [x] T20 Chat: header + presence + Clear + step pill, `SfAIAssistView`, typing indicator
- [x] T21 Surface pane: stepper, loading indicator, `A2uiSurface`, placeholder workspace with sample trip + step buttons
- [x] T22 Responsive: < 980 → Chat / Booking-view tabs
- [x] T23 Agent round-trip, error bubbles, fresh-surface-per-createSurface
- [x] T24 Android builds clean (0 warnings / 0 errors) with .NET 10 SDK
- [ ] T25 Build on Windows (`net10.0-windows10.0.19041.0`) — needs Windows host
- [ ] T26 Build on macOS (`net10.0-ios`, `net10.0-maccatalyst`) — needs macOS host

## Verification / polish
- [~] T27 Run the §6 parity checklist (PLAN.md) against the live agent
- [~] T28 Confirm `SfAIAssistView` typing placeholder ("…" bubble) looks right; swap for a template if not
- [ ] T29 Tune grid/card visuals against React screenshots
- [ ] T30 Add Syncfusion license key via CI secret (never commit)
- [ ] T31 UI automation using the `AutomationId`s (stage-*, welcome-input, welcome-send, home, clear, assist)
- [x] T32 Automated: `tests/SkyBook.Designs.Tests` loads every designs_maui file into the real `SurfaceHost` (6 tests)

### Notes on T25 / T26 (attempted from the Linux sandbox)
Not buildable off-platform: WinUI (`net10.0-windows10.0.19041.0`) needs Windows; iOS / Mac Catalyst need macOS + Xcode.
On a Windows host: `dotnet build src/SkyBook.Maui -f net10.0-windows10.0.19041.0`.
On a Mac: `dotnet workload install maui` then `dotnet build src/SkyBook.Maui -f net10.0-maccatalyst` (and `-f net10.0-ios`).
Project file already declares these targets conditionally (see `SkyBook.Maui.csproj`).

- [x] T33 Font switched from Open Sans to Roboto (Regular/Medium/Bold bundled)
- [x] T34 Friendly 'agent unreachable' chat message; AgentClient sends fixed Content-Length; `harness/mock_agent.py` offline agent
- [x] T35 In-app `LocalAgent` (C#, optional Azure OpenAI) replaces the need for the Python agent; `SKYBOOK_AGENT_URL` still selects the remote agent
- [x] T36 UI parity pass against the React screenshots: custom A2UI renderers (`src/SkyBook.Maui/Ui`) for Column gap/padding, Row as an equal-width grid (stacks when narrow), React type scale, coloured title bar; flight grid styled like React with a real "Book now" button per row; stepper with connector lines; chat bubbles (blue user / white bot with avatar + time); home page journey connectors, gradient card tops, tinted "Ask SkyBook" panel
- [x] T37 Agent designs_maui rebuilt as Title bar + cards (Trip Summary / Available Flights / Booking Details / Passenger Information); offline "fill details on your own" prefill; Book click chat text matches React ("Book JL 001 (SFO → HND) at 3200.")
- [x] T38 `UiRenderTests` builds the real view tree for all three pages (10 design/UI tests, 38 core tests)
