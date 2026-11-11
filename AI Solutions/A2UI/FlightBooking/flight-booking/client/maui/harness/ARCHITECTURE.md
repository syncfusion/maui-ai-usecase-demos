# ARCHITECTURE — SkyBook for .NET MAUI

## 1. System context
```
┌───────────────────────────┐  A2A JSON-RPC `message/send`   ┌─────────────────────────────┐   ┌──────────────┐
│  SkyBook.Maui (.NET 10)   │ ─────────────────────────────► │ Python agent (port 10006)   │──►│ OpenRouter   │
│  Syncfusion.Maui.A2UI     │ ◄───────────────────────────── │ FLIGHT_BOOKING_CLIENT=maui  │   │ LLM          │
└───────────────────────────┘   artifacts[].data.a2uiEnvelope └─────────────────────────────┘   └──────────────┘
        renders A2UI v0.9 surface                                 echoes designs_maui/stage{1,2,3}.json
```
The wire contract is unchanged from React: request = `{jsonrpc,id,method:"message/send",params:{message}}`;
response carries `result.contextId`, `result.status.message.parts[].text` and
`result.artifacts[].parts[].data.a2uiEnvelope` (an array of A2UI v0.9 messages).

## 2. Solution layout
```
client/maui/
├─ global.json, Directory.Build.props      .NET 10 SDK pin, nullable, warnings-as-errors, Syncfusion version
├─ src/SkyBook.Core        (net10.0)       NO UI dependencies — everything testable on any OS
│   ├─ Stage.cs                 STAGES table + stepper inference                (App.tsx: STAGES, currentStep)
│   ├─ ActionSummarizer.cs      chat text for a UI action                       (summarizeAction)
│   ├─ EnvelopeSanitizer.cs     drop dangling children / empty containers,      (sanitizeEnvelope,
│   │                           read /_explanation                               extractExplanation)
│   ├─ BookingSession.cs        trip memory, history, action→prompt, outgoing   (tripRef, userPromptsRef,
│   │                           text assembly                                    contextIdRef, sendToAgent)
│   ├─ ContextResolver.cs       resolve {path} bindings in action context       (MAUI-specific adapter)
│   └─ AgentClient.cs           A2A HTTP client + response parser               (fetch in sendToAgent)
├─ src/SkyBook.Maui        (net10.0-android/ios/maccatalyst/windows)
│   ├─ MauiProgram.cs           ConfigureSyncfusionCore + UseA2UIWithSyncfusionComponents("syncfusion-maui"), DI
│   ├─ MainPage.xaml(.cs)       the whole screen (port of App.tsx)
│   ├─ StageCardView.cs         welcome stage card
│   ├─ ChatViewModel.cs         bindable state for SfAIAssistView
│   ├─ AppConfig.cs             agent URL, license key slot, compact breakpoint
│   └─ Resources/Styles/SkyBookTheme.xaml   design tokens from App.css
└─ tests/SkyBook.Core.Tests (xUnit)    runs the real agent design JSON through the sanitiser
```
Dependency rule: `SkyBook.Maui → SkyBook.Core`; Core never references MAUI or Syncfusion.

## 3. React → MAUI mapping
| React (`App.tsx`) | MAUI |
|---|---|
| `MessageProcessor` + `syncfusionCatalog` | `SurfaceHost` from `UseA2UIWithSyncfusionComponents` (DI singleton) |
| `<SyncfusionA2UIProvider surface=…>` | `<a2ui:A2uiSurface Host= Surface=>` |
| `processor.onSurfaceCreated / Deleted` | `SurfaceHost.SurfaceCreated / SurfaceDeleted` |
| action callback passed to `MessageProcessor` | `SurfaceHost.ActionDispatched` → `OnSurfaceAction` |
| `processor.processMessages(sanitize(env))` | `host.ProcessJson(Sanitize(env).ToJsonString())` |
| delete surfaces on `createSurface` | `deleteSurface` message per `host.Surfaces` key |
| `AIAssistViewComponent` + templates | `SfAIAssistView` bound to `ChatViewModel` |
| `uiStore` (busy/welcome) | `_busy`, `_view` fields + `ApplyState()` |
| CSS grid / media query 980px | `Grid` columns + `AppConfig.CompactWidth` + tab buttons |
| `.env` `VITE_AGENT_URL` | `SKYBOOK_AGENT_URL` env var / `AppConfig.AgentUrl` |

## 4. Runtime flow
1. User taps a stage card / sends text → `SubmitPromptAsync` records the prompt, switches to the chat view.
2. `BookingSession.BuildOutgoingText` appends trip context + up to 8 earlier prompts (identical to React).
3. `AgentClient.SendAsync` POSTs; `Parse` extracts `contextId`, `a2uiEnvelope`, fallback text.
4. If the envelope has `createSurface`, existing surfaces are deleted; then the **sanitised** envelope is processed.
5. `SurfaceCreated` → `A2uiSurface.Surface` is set, stepper/pill update (`Stage.InferStep`).
6. Reply bubble = `/_explanation` → agent text → "Updated the booking view."
7. A button/row action raises `ActionDispatched` → `ContextResolver` resolves `{path}` bindings →
   `BookingSession.BuildActionPrompt` (ignores `select` noise and busy state) → step 1 again with a *silent payload*
   (bubble shows the summary; the agent receives the detailed JSON).

## 5. Agent-side change (additive)
`FLIGHT_BOOKING_CLIENT=maui` makes the agent load `designs_maui/` and append a short MAUI addendum to the system
prompt. Default (`react`) behaviour is unchanged. `designs_maui` mirrors the three stages one-for-one:

| Stage | React components | MAUI components |
|---|---|---|
| 1 Search | `SyncfusionTextBox/DropDownList/DatePicker/NumericTextBox/MaskedTextBox/Button`, `Card` | `TextField`, `SyncfusionComboBox`, `DateTimeInput`, `SyncfusionNumericEntry`, `SyncfusionMaskedEntry`, `SyncfusionButton`, `SyncfusionCardView` |
| 2 Results | `SyncfusionDataGrid` (+ command column) | `SyncfusionDataGrid` (`onRowClick`, `selectedRowJson`) + `SyncfusionButton` "Book now →" |
| 3 Confirmation | `SyncfusionMessage`, cards, grid | `SyncfusionCardView` (green indicator), `Text`, `SyncfusionDataGrid` |
Event names/context keys (`searchFlights`, `selectFlightAndProceed`, …) are preserved so the system prompt's booking
policy applies unchanged.

## 6. Cross-cutting
* **Security**: no secrets in source; license key slot is empty by default; cleartext HTTP only enabled for the
  local dev agent (Android `usesCleartextTraffic`) — use HTTPS for any non-local agent.
* **Resilience**: every agent failure becomes a bot bubble (as in React); a render failure never crashes the page.
* **Threading**: `SurfaceHost` events are marshalled with `MainThread.BeginInvokeOnMainThread`.
* **Accessibility**: `SemanticProperties` on cards/steps, `AutomationId` on interactive controls, heading levels.
* **Platforms**: Android/iOS/MacCatalyst/Windows via `net10.0-*`; the Linux build host can compile Android only.
