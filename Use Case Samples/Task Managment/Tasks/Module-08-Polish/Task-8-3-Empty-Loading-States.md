# Task 8-3 — Empty / Loading States Pass

**Module:** 08 — Polish
**Goal:** Confirm every screen handles its empty-collection state cleanly and
every notification the user is given is data-driven (no stale copy).

---

## Objective

Walk every in-scope screen and verify the empty-state contract from
Specification.md §5.6 (centered icon, single short message, optional
primary action):

| View | Empty state copy |
|------|------------------|
| My Notes (no notes) | "No list was found. Please create a to-do list." |
| Important (no starred notes) | "No important notes" / "Tap the star on a note to mark it important." |
| Reminder (no reminder dates) | "No reminders" / "Notes with a reminder date will appear here." |
| Bin (no soft-deleted notes) | "Bin is empty" / "Soft-deleted notes can be restored from the Bin." |
| Custom list (no notes) | "No notes in {name}" / "Add a note using the input below." |
| Search (no query) | "You can search the created notes here" (per `search.png`) |
| Search (no matches) | "No notes match your search" / "Try a different keyword." |
| Sidebar "Lists" section header | hidden when there are zero custom lists |

## Acceptance criteria

- [ ] Each filter view's `EmptyStateTitle` / `EmptyStateMessage` match the
      table above.
- [ ] The sidebar "Lists" section header visibility is data-driven and
      refreshes when the user creates or removes a custom list (no stale
      hidden / shown state).
- [ ] The search page shows the initial empty state until the user types,
      then the "no matches" state if the typed query returns zero results.
- [ ] No exceptions are thrown when the user creates the very first note
      from an empty My Notes view.
- [ ] The empty-state visuals sit on the colored card background per the
      design, not on a plain page surface.

## Files in

- `Todo/Features/Notes/ViewModels/NotesListViewModel.cs` (add
  `HasCustomLists` flag and raise it on list add / remove)
- `Todo/Features/Notes/Views/SidebarView.xaml` (bind the "Lists" header to
  `HasCustomLists` instead of `CustomLists.Count`)

## Files out

- `Features/Important/*`, `Features/Reminder/*`, `Features/Bin/*`,
  `Features/EditNote/*`, `Features/CustomLists/*`, `Features/Search/*` (their
  empty states are already correct)
- All converters (no new converter is required)
- `Resources/Styles/*`

## Gate

- **G-M08-3** Every view listed in the table above shows the correct copy
  when its underlying collection is empty, including after the user creates
  or removes custom lists.
- `dotnet build` succeeds.

## Test / Verify Locally

- [ ] `dotnet build` succeeds.
- [ ] Open the app on a freshly seeded state — My Notes shows the empty
      state.
- [ ] Open Important, Reminder, Bin — each shows the seeded items.
- [ ] Switch to My Notes, create a note — the empty state is replaced.
- [ ] Tap the "+" button, create a list, then tap "Delete" in the list's
      more menu — the "Lists" section header disappears.
- [ ] Open Search, verify the initial empty state, type a non-matching
      query, verify the "no matches" state, clear the query, verify the
      initial empty state returns.
