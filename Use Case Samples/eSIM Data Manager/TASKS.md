# eSIM Data Manager — Implementation Tasks

Use this file with [`HARNESS.md`](HARNESS.md) and [`SPEC.md`](SPEC.md). Work top to bottom; dependencies are intentional. Mark a task complete only after its checks pass. Update status/evidence as implementation proceeds.

Statuses: **Not started** · **In progress** · **Complete**

## 1. Inspect and bootstrap

**Status:** Not started

- Inspect sibling MAUI samples for project layout, MVVM approach, dependency registration, navigation/responsive layout, tests, and Syncfusion package versions.
- Create the sample solution/project under this directory, following repository naming conventions; target Android, iOS, Windows, and Mac Catalyst as supported by the existing framework setup.
- Configure Syncfusion initialization/license registration consistent with sibling samples; never commit license values.
- Add only dependencies required for the scope.

**Checks:** Project structure and target frameworks are recorded; restore succeeds for available targets; no credentials or AI packages/settings are added.

## 2. Models and in-memory domain state

**Status:** Not started

- Define plan, eSIM profile, usage, and daily usage models needed by the UI; represent profile statuses and coverage/network/type values consistently.
- Implement deterministic fictional seed catalog and profiles, including Active, Ready to Install, and Expired examples.
- Add one session-scoped in-memory store/service shared across views/view models; expose state changes suitable for UI notification.
- Implement/test plan search and composable filters, purchase, simulated activation, top-up eligibility, usage calculations, and expiry/day calculations.
- Enforce data invariants and ensure cancel paths do not mutate state.

**Checks:** Unit tests cover seeded arithmetic, search/filter combinations/clear, purchase creates one install-ready profile, cancel creates none, activation transitions eligible profile, top-up amounts and cancellation, expired top-up rejection, and usage invariants.

## 3. App shell and responsive navigation

**Status:** Not started

- Add the navigation shell with Dashboard, My eSIMs, and Explore destinations.
- Use bottom navigation on compact layouts and persistent desktop navigation on wide layouts, adapting to window size rather than hard-coding platform-only feature gaps.
- Establish shared styling, spacing, typography, and theme appropriate to modern travel connectivity.

**Checks:** All destinations are reachable in compact and wide layouts; content remains navigable at narrow and tablet/desktop sizes; accessible labels are present.

## 4. Dashboard

**Status:** Not started

- Implement active profile summary and clear empty state.
- Add data used/remaining/allocation and percentage display with `SfCircularGauge`.
- Add validity/expiry summary and seven-day usage trend with `SfCartesianChart`.
- Add quick actions for Explore/Buy, Top Up, and Show QR; handle absence of eligible active profile gracefully.
- Use `SfCardView`/`SfButton` where compatible and aligned with the spec.

**Checks:** Dashboard values match the shared profile/usage state; gauge/trend show meaningful values; actions navigate/open the correct flow; no-profile state has safe behavior.

## 5. My eSIMs and profile details

**Status:** Not started

- Implement Active, Ready to Install, and Expired categories with `SfTabView`.
- Render profile lists with `SfListView`, status text/badge using `SfBadgeView`, and clear selected state.
- Show selected profile details, usage arithmetic, dates, simulated ICCID and activation preview; use `SfExpander` for secondary content where appropriate.
- Wire Install Profile, Top Up, Renew Plan, and Show QR; disable or explain inapplicable actions by status.

**Checks:** Profile tabs and list reflect shared state after purchase/activation/top-up; details are correct for all statuses; no real installation is claimed.

## 6. Explore plans

**Status:** Not started

- Build destination search and popular destinations.
- Implement Local/Regional/Global, 4G/5G, Limited/Unlimited, and validity filters; support combined criteria and reset.
- Render responsive plan cards with destination, carrier/provider, data, validity, price, network, details, and buy actions.
- Use `SfChipGroup`/chips, `SfListView`, and `SfCardView` where supported.

**Checks:** Search is case-insensitive across country/region/carrier; combined filters narrow correctly; reset restores catalog; details/buy pass the selected plan to the right flow.

## 7. Purchase, QR, and top-up experiences

**Status:** Not started

- Purchase: show selected plan, sample tax and total; confirm/cancel; add exactly one Ready to Install profile; show clear simulation notice and success feedback.
- QR: show sample activation string, instructions, and QR generated with `SfBarcodeGenerator` if supported; otherwise provide an explicit non-scannable placeholder. Add clipboard copy with user confirmation and close behavior.
- Activation: confirmation followed by simulated status/date transition only; no platform provisioning API.
- Top-up: present 1/3/5 GB packs and sample prices; confirm/cancel; update allocation and remaining data; block expired plans with explanation.
- Adapt presentation for compact modal/bottom-sheet and wider dialog layouts using compatible APIs.

**Checks:** Confirmed actions cause exactly specified state changes; cancellation causes none; copy action works; QR/code and purchase language are clearly demo-only; expired top-up is blocked.

## 8. Accessibility, UX and consistency pass

**Status:** Not started

- Check touch targets, semantics, focus/keyboard behavior where relevant, text scaling, contrast, status text, empty/loading/confirmation feedback, and responsive scrolling.
- Verify currency/date formatting and consistent demo labels.
- Ensure all screens read the same in-memory source of truth and update when state changes.

**Checks:** Manual review against accessibility and UX requirements in `SPEC.md`; no status communicated by color alone; no misleading real-carrier/payment language.

## 9. Verification and delivery documentation

**Status:** Not started

- Run restore, build, and tests for the host-supported targets; run unit tests for business logic.
- Verify target framework setup includes Android, iOS, Windows, and Mac Catalyst where supported; document workload/host constraints rather than misreporting failures as passes.
- Inspect final diff for out-of-scope persistence, databases, network calls, AI, payment, provisioning, secrets, and unnecessary dependencies.
- Add a concise sample README describing scope, features, controls, architecture, platform targets, demo behavior, and verification results.

**Checks:** Tests/build results are reported accurately; sample remains deterministic/offline/session-only; documentation clearly states simulation limitations.
