# eSIM Data Manager — Product Specification

## 1. Purpose and scope

**eSIM Data Manager** is a cross-platform .NET MAUI showcase sample for travelers to explore simulated eSIM offers, simulate a purchase, view install-ready profiles, simulate activation, monitor data use, and simulate a top-up. It demonstrates Syncfusion MAUI controls and an end-to-end consumer workflow; it is not a production telecom, eSIM provisioning, billing, or payment service.

There is **no AI capability or AI service** in this application. AI coding agents may use `HARNESS.md` and `TASKS.md` to build the sample, but no AI feature, endpoint, prompt, key, or runtime dependency belongs in the app.

## 2. Product goals

- Present a clear traveler-focused workflow: Explore Plans → Purchase Plan → Activate eSIM → Monitor Usage → Top Up Data.
- Demonstrate relevant Syncfusion MAUI controls in functional UI, not decorative placeholders.
- Support phone and tablet experiences on Android, iOS, Windows, and Mac Catalyst.
- Keep the sample compact, deterministic, and usable without a network connection or credentials.
- Clearly identify all carrier, coverage, payment, installation, QR, activation, and usage information as simulated sample content.

## 3. Users

- **Traveler:** needs quick plan discovery, purchase, activation guidance, and usage visibility.
- **Frequent traveler:** needs to manage multiple profiles, compare destinations, and top up an existing active profile.

## 4. Supported platforms and presentation

- .NET MAUI, MVVM.
- Android, iOS, Windows, and Mac Catalyst; responsive layouts for phone, tablet, and desktop window sizes.
- On compact widths, use bottom navigation with Home, My eSIMs, and Explore. On wider layouts, use persistent desktop navigation for Dashboard, My eSIMs, and Explore Plans.
- Keep navigation destinations and user-visible features equivalent across form factors. Dialog presentation may adapt to a centered dialog on wider screens and a sheet/modal on compact screens.

## 5. Navigation and screens

### 5.1 Dashboard (default)

- Active plan card: profile/plan name, destination, provider/carrier, network type, and status.
- Usage widget: allocated, consumed, remaining data, and percentage; visualize the remaining/used amount with `SfCircularGauge`.
- Validity card: days remaining and expiry date.
- Seven-day usage trend rendered with `SfCartesianChart`.
- Quick actions: Buy eSIM (opens Explore), Top Up Data (opens top-up for active profile), Show QR (opens activation details).
- Provide a useful empty state and safe action behavior if no active profile exists.

### 5.2 My eSIMs

- Category tabs: Active, Ready to Install, and Expired, implemented using `SfTabView`.
- Profile list with profile name, destination, carrier, status, remaining data, and days remaining. Use `SfListView` and a status badge (`SfBadgeView`) where appropriate.
- Selecting a profile presents its details: profile identity, country, carrier, simulated ICCID, status; total/used/remaining data; activation/expiry dates and remaining days; activation QR preview and code.
- Profile action area: Install Profile (simulated activation), Top Up, Renew Plan, and Show QR. Explain unavailable actions for statuses where they cannot apply; do not imply OS-level eSIM installation.
- Use `SfExpander` for secondary details where useful.

### 5.3 Explore Plans

- Search plans by country, region, or carrier; search is local and case-insensitive.
- Popular destination chips: Japan, USA, France, Korea, Singapore, Thailand.
- Filters: coverage Local/Regional/Global; network 4G/5G; plan type Limited/Unlimited; validity 7/15/30 days. Multiple filter dimensions can be combined; provide a way to clear filters.
- Plan cards include destination, provider, data allowance/type, validity, price, and network type; actions are View Details and Buy Plan.
- Use `SfChipGroup` for destination/filter chips as appropriate and `SfListView` / `SfCardView` for plan presentation.

## 6. Dialogs and flows

### 6.1 Purchase

- Triggered by Buy Plan.
- Show selected plan and a transparent price breakdown: plan price, sample tax, and total.
- Confirm Purchase and Cancel actions. Confirmation simulates a transaction; no payment details are collected and no payment/network request is made.
- On success, create a profile in Ready to Install, update in-memory state, and present clear confirmation/navigation to My eSIMs.
- Prevent accidental duplicate submissions while confirmation is being processed.

### 6.2 QR activation details

- Triggered by Show QR or Install Profile.
- Show a large, scannable-looking sample QR, activation code (e.g. `LPA:1$carrier.example$SAMPLE12345`), and concise installation instructions.
- The QR/code must be explicitly labeled simulated and must not provision a real profile. Use `SfBarcodeGenerator` if supported for encoding the sample string; otherwise render a clearly labeled non-scannable placeholder and do not claim it can be scanned.
- Copy Code copies the sample code to clipboard and gives confirmation. Close dismisses the view.
- Install Profile performs only a simulated state transition after user confirmation; never call platform eSIM provisioning APIs.

### 6.3 Top-up

- Opened for a selected eligible profile.
- Offer 1 GB / $3, 3 GB / $7, and 5 GB / $10 sample packs.
- Purchase Top-Up and Cancel actions. Confirming adds the selected data allowance to that profile's allocation/remaining balance and updates displayed usage. No real payment or network request.
- Do not allow top-up for expired profiles; explain why.

## 7. Data and state requirements

- All app data is local in-memory sample data held for the current process/session only. No SQL/NoSQL database, filesystem persistence, preferences, secure storage, cloud backend, carrier integration, real payment, or network API.
- Seed deterministic, clearly fictional sample data on startup, including an active Japan profile (10 GB allocation, 6.8 GB consumed, 3.2 GB remaining; SoftBank-style fictionalized demo presentation; six days remaining), a ready-to-install profile, an expired profile, and a varied plan catalog. Do not use real ICCIDs or credentials.
- Define models for user (optional), eSIM profile, plan, usage, and daily usage points as needed. Keep domain data and UI state separated according to existing project MVVM conventions.
- Maintain consistent invariants: allocation = consumed + remaining; remaining and consumed cannot become negative; top-up increases allocation and remaining by the same selected pack; purchase creates exactly one Ready to Install profile; simulated activation changes its status and dates consistently.
- Use a single in-memory source of truth shared by screens/view models for the running session. No app restart persistence is required.
- Seed values and all generated codes are demo data and must be identified as such in relevant UI/help text.

## 8. Syncfusion controls

Use the specified controls where supported by the selected package/version; check the relevant official Syncfusion MAUI API and project conventions before choosing exact XAML/API names.

- Dashboard: `SfCircularGauge`, `SfCartesianChart`, `SfCardView` (where applicable), `SfButton`.
- My eSIMs: `SfTabView`, `SfListView`, `SfBadgeView`, `SfExpander`.
- Explore: `SfChipGroup` / chips, `SfListView`, `SfCardView`.
- QR: `SfBarcodeGenerator` when supported and suitable for a demo activation string.
- Use native MAUI controls for supporting UI when no listed Syncfusion control applies. Do not add Syncfusion controls solely to increase count.

## 9. Accessibility and UX

- Ensure navigation, buttons, chips, tabs, dialogs, and copy actions have accessible labels and usable touch targets.
- Do not communicate status only through color; include text labels.
- Ensure content scrolls at small window sizes and scales on tablets/desktop.
- Provide empty, selected, confirmation, and invalid-action states; prevent silent data changes.
- Display prices consistently in USD and dates in a user-readable format; demo tax/price content is illustrative.

## 10. Explicit non-goals

No APN configuration, network diagnostics, VoLTE, connectivity verification, real carrier integration or profile provisioning, real QR activation, real payments, identity verification, family/enterprise plans, detailed activity history, complex auto-renew rules, account system, database, persistence, backend, or AI feature.

## 11. Acceptance criteria

1. The solution builds for Android, iOS, Windows, and Mac Catalyst using the project’s chosen MAUI/.NET target and platform workload constraints.
2. All three primary destinations are reachable through responsive phone and wide-screen navigation.
3. Dashboard metrics agree with the seeded active profile; gauge and seven-day trend render meaningful sample data.
4. Search and combined filters update the local plan list, and clearing filters restores the catalog.
5. Purchase confirmation creates one Ready to Install profile in shared session state; cancel creates none.
6. Simulated install moves an eligible profile to Active and updates activation/expiry state; UI never claims the carrier profile was installed on the device.
7. Copy code copies the sample activation string and confirms the action; QR content is clearly identified as simulated.
8. Top-up adds the exact selected pack to allocation and remaining data; cancel and expired-profile actions do not alter data.
9. Usage values obey the stated invariants and are reflected consistently across dashboard and profile details.
10. No database, persistence, payment, carrier, backend, AI, or other external-service integration is introduced.
11. User-facing labels clearly distinguish simulated purchase, activation, provider, coverage, prices, and sample data from real telecom services.
12. Relevant automated tests cover filtering, purchase/cancel, activation, top-up/cancel, expired-profile restrictions, and usage arithmetic; build/test limitations by host or workload are reported accurately.
