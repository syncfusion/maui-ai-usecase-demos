# Task 8-2 — Accessibility Pass

**Module:** 08 — Polish
**Goal:** Confirm every interactive element meets the spec's accessibility
contract: minimum 12sp text, 4.5:1 contrast, 48x48 dp touch targets, and
semantic labels on every actionable control.

---

## Objective

Walk every in-scope screen and verify the spec's accessibility clauses
(Specification.md §13):

- All text uses font sizes from `Resources/Styles/Fonts.xaml` (≥ 12sp).
- Text contrast on the app's surface palette meets 4.5:1 (already validated
  during Module-01; this task re-checks any new text introduced since).
- Primary action surfaces meet the 48x48 dp touch-target minimum.
- Each interactive control has a `SemanticProperties.Description` /
  `SemanticProperties.Hint` so screen-reader users get a clear label.

## Acceptance criteria

- [ ] The hamburger button in the header is at least 48x48 dp.
- [ ] The bottom-input bar's calendar / date / bell / more icons are
      explicitly disabled (they are visual placeholders from the design and
      have no behavior in this sample — the spec puts the date / reminder
      flows behind the note's edit dialog).
- [ ] The header profile avatar is a static, non-interactive element with a
      descriptive `SemanticProperties.Description` (the design shows the
      avatar but the spec keeps profile-management out of scope, so the
      avatar is decorative).
- [ ] The header search entry drops focus immediately after the search page
      opens, so a stuck focus highlight is never left behind and the soft
      keyboard does not appear on phone.
- [ ] No new hardcoded font sizes, colors, or paddings are introduced.

## Files in

- `Todo/Features/Notes/Views/HeaderView.xaml`
- `Todo/Features/Notes/Views/HeaderView.xaml.cs`
- `Todo/Features/Notes/Views/NotesContentView.xaml`

## Files out

- All converters (semantic descriptions live on controls, not in converters)
- All ViewModels (no behavior changes — accessibility is a view concern)
- `Resources/Styles/*` (no token changes)

## Gate

- **G-M08-2** Every primary interactive control (hamburger, search pill,
  save-note tick, sidebar entries, note card star, note card more menu,
  dialog buttons) either meets 48x48 dp or sits inside a parent pill /
  border that does, and every interactive control has a
  `SemanticProperties.Description`.
- `dotnet build` succeeds.

## Test / Verify Locally

- [ ] `dotnet build` succeeds.
- [ ] Run on a phone target and tab through the bottom-input bar — only the
      text entry and the save tick should receive focus.
- [ ] Run on any target and turn on the platform screen reader
      (Narrator / TalkBack / VoiceOver) — every interactive control is
      announced with a meaningful label.
- [ ] Confirm the search entry does not retain focus after the search page
      opens.
