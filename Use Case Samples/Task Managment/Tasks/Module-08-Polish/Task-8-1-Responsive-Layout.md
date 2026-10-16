# Task 8-1 — Responsive Layout Pass

**Module:** 08 — Polish
**Goal:** Verify the sidebar collapses per `images/collapsed.png` on narrow widths so the design parity is reached across Phone / Tablet / Desktop.

---

## Objective

Make the layout responsive without breaking the existing wide-layout behavior: on
phone-width devices, the sidebar should start collapsed (matching `collapsed.png`);
on tablet and desktop widths, the sidebar should be visible (matching
`mynotes.png`). The hamburger button in the header continues to let the user
toggle the sidebar manually on any device.

## Acceptance criteria

- [ ] `MainLayoutView.xaml` uses an `OnIdiom` column width: `0` on Phone,
      `220` on Tablet / Desktop.
- [ ] `MainLayoutView.xaml.cs` mirrors the XAML initial state into the
      `IsSidebarCollapsed` flag so the code path stays in sync.
- [ ] On a phone-width device, the app launches with the sidebar hidden and
      the main content occupies the full width (per `collapsed.png`).
- [ ] On a tablet- or desktop-width device, the app launches with the sidebar
      visible and the main content shares the remaining width (per
      `mynotes.png`).
- [ ] The hamburger button still toggles the sidebar on any device.
- [ ] No new fonts, colors, or sizes are introduced; the design tokens in
      `Resources/Styles/` remain the single source of truth.

## Files in

- `Todo/Features/Notes/Views/MainLayoutView.xaml`
- `Todo/Features/Notes/Views/MainLayoutView.xaml.cs`
- `Todo/Features/Notes/ViewModels/NotesListViewModel.cs` (remove the now-unused
  `SidebarExpandedWidth` const)

## Files out

- `Resources/Styles/*` (no token changes; we just swap the literal `220` for
  an idiom-aware value)
- `Features/Important/*`, `Features/Reminder/*`, `Features/Bin/*`,
  `Features/EditNote/*`, `Features/CustomLists/*`, `Features/Search/*`

## Gate

- **G-M08-1** Sample launches on the Phone target with the sidebar collapsed
  (per `collapsed.png`) and on a non-Phone target with the sidebar visible
  (per `mynotes.png`).
- `dotnet build` succeeds.

## Test / Verify Locally

- [ ] `dotnet build` succeeds.
- [ ] Run on the **Windows** target with a narrow window (or the
      **Android** target on a phone-sized emulator) and confirm the sidebar
      is hidden at launch.
- [ ] Run on the **Windows** target at desktop width and confirm the sidebar
      is visible.
- [ ] Tap the hamburger button and confirm the sidebar collapses / expands.
- [ ] Verify no other screen regressed (Important, Reminder, Bin, Search,
      Create list, Rename list, Permanent delete).
