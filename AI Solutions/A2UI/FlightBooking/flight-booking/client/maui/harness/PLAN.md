# PLAN — Migrating SkyBook (React → .NET MAUI + Syncfusion.Maui.A2UI)

Status legend: ✅ done & verified · 🟡 done, needs on-device verification · ⏳ not started

## 1. Discovery (✅)
* Read `App.tsx` (439 lines), `App.css`, `main.tsx`, agent (`flight_booking_agent.py`, `system_prompt.md`, 3 designs).
* Inspected `Syncfusion.Maui.A2UI 35.1.43` and `Syncfusion.A2UI.Core` (net10.0 TFMs present): `SurfaceHost`,
  `A2uiSurface`, catalog id `syncfusion-maui`, 60+ `Syncfusion*` renderers + basic catalog (Row, Column, Text, TextField,
  DateTimeInput …). No `SyncfusionAppBar/Message/Card/TextBox/DropDownList` ⇒ designs must be re-authored.
* `SyncfusionDataGrid` renderer: no command columns; row tap raises `onRowClick`, `selectedRowJson` writes back.

## 2. Approach decisions
| Decision | Rationale |
|---|---|
| Separate `SkyBook.Core` library | All React logic that is not visual becomes unit-testable without a device |
| New `designs_maui/` + env switch instead of editing `designs/` | React client keeps working untouched |
| Keep event names & context keys | The system prompt's booking policy keys off them |
| `SfAIAssistView` for chat | Direct equivalent of EJ2 `AIAssistView` |
| Pin SDK 10 + `TreatWarningsAsErrors` | Requirement: .NET 10; keeps the port clean |

## 3. Phases
| Phase | Output | Status |
|---|---|---|
| P0 Harness | INTENT / PLAN / TASKS / ARCHITECTURE, `verify.sh` | ✅ |
| P1 Scaffold | solution folders, `global.json`, props, MAUI project from .NET 10 template | ✅ |
| P2 Core logic | Stage, ActionSummarizer, EnvelopeSanitizer, BookingSession, AgentClient, ContextResolver + 28 tests | ✅ |
| P3 Agent designs | `designs_maui/*` validated by loading into the real `SurfaceHost` | ✅ |
| P4 App shell | MauiProgram DI, `MainPage`, theme, welcome, chat, stepper, placeholder, responsive tabs | ✅ builds (Android) |
| P5 Parity QA | Run against live agent on Windows/Android; walk the §6 checklist | 🟡 **requires device + OpenRouter key** |
| P6 Polish | Visual tuning of grid/card styling against React screenshots | ⏳ |

## 4. Risks & mitigations
| Risk | Impact | Mitigation |
|---|---|---|
| LLM drifts from `designs_maui` | Broken surface | `EnvelopeSanitizer` + MAUI addendum in system prompt + schema-validated designs |
| `SfDataGrid` row tap vs. sort tap | Wrong flight chosen | `selectedRowJson` written on `CellTapped`; Book button reads the model, not the tap |
| Action context arrives as unresolved `{path}` | Empty booking payload | `ContextResolver` resolves against the surface data model (tested) |
| Stock `DateTimeInput` writes culture-formatted dates | Agent sees non-ISO dates | `ContextResolver` normalises `*Date` keys to `yyyy-MM-dd` (tested) |
| All designs share surface id `workspace` | Stepper stuck at 1 | `Stage.InferStep` falls back to component ids (tested) |
| Android emulator cannot reach `localhost` | Network error bubble | default `10.0.2.2` on Android; override via `SKYBOOK_AGENT_URL` |
| Trial license banner | Cosmetic | `AppConfig.SyncfusionLicenseKey` slot |
| Linux CI cannot build iOS/Win/Mac | Partial verification | `verify.sh` builds Android + runs tests; others on their hosts |

## 5. Verification strategy
1. `harness/verify.sh` — xUnit, `designs_maui` loaded into the real `SurfaceHost` (SkyBook.Designs.Tests), JSON validity, Android build (warnings as errors).
2. Parity unit tests assert React-exact strings.
3. Manual E2E per §6.

## 6. Manual parity checklist (P5)
- [ ] Welcome: badge, headline with blue accent, 3 journey dots, 3 stage cards (copy identical), Ask panel; send disabled until text.
- [ ] Stage card tap → chat view, header shows "SkyBook Assistant", Online, Clear, "Step N · Title" pill, stepper.
- [ ] Typing indicator while waiting; bot reply = explanation sentence.
- [ ] Stage 1 form prefilled by agent; **Search Flights** → bubble "Search flights from … on … for N adults, M child." → stage 2.
- [ ] Stage 2 grid sortable; tap row + **Book now →** → bubble "Book … (A → B) at …" → stage 3 with real user data.
- [ ] Stage 3 success card, outbound/return cards, passenger grid; stepper shows ✓ ✓ 3.
- [ ] Clear empties chat but keeps surface; Home resets everything.
- [ ] Width < 980: Chat / Booking view tabs, surface auto-selected on new surface.
- [ ] Network off / agent down → "Something went wrong: …" bubble, UI stays usable.
