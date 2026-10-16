# Task 8-4 — Customer Walkthrough

**Module:** 08 — Polish
**Goal:** A new customer can clone, build, run, and see every in-scope
screen match its design image. The walkthrough is a step-by-step script
that exercises the seven in-scope screens in the order the design images
present them.

---

## Objective

Document a single end-to-end script that a customer can follow to
verify the sample against every in-scope design image. The script lives
in this task file so a customer can read it without having to scan
`Specification.md` or `Harness.md` end-to-end.

## Acceptance criteria

- [ ] The walkthrough is a numbered list of "tap X, then expect Y" steps
      that mirrors the in-scope reference images in `images/`.
- [ ] Each step names the design image the reviewer should compare against.
- [ ] The walkthrough covers: My Notes, Important, Reminder, Bin, Create
      list, Rename list, Search, Permanent delete, and Drag-and-drop.
- [ ] Each step is verifiable on at least one of the four supported
      platforms (Windows, macOS, iOS, Android).

## Files in

- `Tasks/Module-08-Polish/Task-8-4-Customer-Walkthrough.md` (this file)
- `README.md` (cross-link to the walkthrough so the script is discoverable)

## Files out

- The Todo source (no code changes — this task is documentation only)

## Walkthrough script

1. **My Notes (default landing) — `mynotes.png`**
   - Launch the app.
   - Expect: a sidebar on the left (collapsed on phone, expanded on
     tablet / desktop), a header at the top, an empty main area with the
     "No list was found. Please create a to-do list." empty state, and a
     pill-shaped bottom input bar.

2. **Important filter — `important.png`**
   - Tap **Important** in the sidebar.
   - Expect: the main area shows the seeded starred notes (no reminder
     date), the header background switches to the Important accent, the
     sidebar highlights **Important**.

3. **Reminder filter — `remainder.png`**
   - Tap **Reminder** in the sidebar.
   - Expect: the main area shows the seeded notes that have a reminder
     date set, the header background switches to the Reminder accent, the
     sidebar highlights **Reminder**.

4. **Bin filter — `bin.png`**
   - Tap **Bin** in the sidebar.
   - Expect: the main area shows the seeded soft-deleted notes, the
     header background switches to the Bin accent, the sidebar highlights
     **Bin**. Each item's "more" menu exposes **Restore note** and
     **Delete permanently**.

5. **Permanent delete — `deletepermently.png`**
   - With the Bin view active, tap the "more" menu on a note and pick
     **Delete permanently**.
   - Expect: a centered confirmation dialog ("Delete permanently? This
     note will be permanently deleted and cannot be recovered.") with
     **Cancel** and **Delete** buttons. Tapping **Delete** removes the
     note from the in-memory store.

6. **Create a new list — `createnewlist.png`**
   - Switch back to My Notes. Tap the **+** floating button at the
     bottom of the sidebar.
   - Expect: a centered create-list dialog ("Create new list" title, a
     single text input, **Cancel** and **Create** buttons). Enter a name
     and tap **Create** — the new list appears in the sidebar under a
     **Lists** section header.

7. **Rename a list — `renamelist.png`**
   - Tap the "more" menu on the new list and pick **Rename list**.
   - Expect: a centered rename-list dialog ("Rename list" title, the
     text input pre-populated with the current name, **Cancel** and
     **Save** buttons). Save the new name — the sidebar label updates
     immediately.

8. **Search — `search.png`**
   - Tap the search pill in the header.
   - Expect: the search page opens with a back arrow, a single search
     input pre-focused, and the initial empty state ("You can search
     the created notes here"). Type a query that matches a seeded
     note's title — the matching notes appear. Type a non-matching
     query — the "No notes match your search" state appears.

9. **Drag and drop — `draganddrop.png`**
   - On My Notes, long-press a note card and drag it to a different
     position in the list — release to reorder.
   - Then long-press a note card and drag it onto **Important** in the
     sidebar — release to move the note to the Important filter.

10. **Collapsed sidebar — `collapsed.png`**
    - On a phone-width device, expect the sidebar to be hidden at
      launch.
    - Tap the hamburger button to expand the sidebar; tap again to
      collapse it.

## Gate

- **G-M08-4** The walkthrough above passes end-to-end on at least one
  supported target without any unhandled exceptions, hardcoded values,
  out-of-scope screens appearing, or design regressions.
- `dotnet build` succeeds.

## Test / Verify Locally

- [ ] `dotnet build` succeeds.
- [ ] Walk through every step on a Windows desktop and on an Android
      phone-sized emulator.
- [ ] For each step, compare the live screen against the named design
      image; document any deviation in the task file (none expected for
      the in-scope screens).
