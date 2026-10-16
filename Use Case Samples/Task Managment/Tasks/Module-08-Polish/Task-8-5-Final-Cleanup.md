# Task 8-5 — Final Cleanup

**Module:** 08 — Polish
**Goal:** Remove leftover dead code, dead comments, and dead stubs so the
sample is customer-ready.

---

## Objective

Strip the code base of any leftover that is not reachable from the
runtime path. "Customer-ready" means a customer can run the sample,
read every file, and never encounter a file that is never used.

## Acceptance criteria

- [ ] No `ContentPage` is registered with the DI container that is not
      actually used by `AppShell`.
- [ ] No converter is registered in `App.xaml` that is not consumed by
      at least one XAML file.
- [ ] No public `const` / `static` member on a ViewModel is unused.
- [ ] No file in `Todo/` is unreachable from the runtime path.
- [ ] No commented-out XAML or `// TODO` markers remain in the in-scope
      features.

## Files in

- Removed: `Todo/Features/Notes/Views/NotesPage.xaml`
- Removed: `Todo/Features/Notes/Views/NotesPage.xaml.cs`
- Removed: `Todo/Converters/CountToVisibilityConverter.cs`
- Updated: `Todo/Features/Notes/ViewModels/NotesListViewModel.cs`
  (remove the `SidebarExpandedWidth` const)
- Updated: `Todo/App.xaml` (drop the unused converter registration)

## Files out

- All other source files (cleanup is limited to the unused artifacts
  enumerated above; no structural refactor)

## Gate

- **G-M08-5** `git grep` finds no references to the removed files /
  members anywhere in the repository.
- `dotnet build` succeeds.
- The sample runs end-to-end with the design-matching flow described in
  Task 8-4.

## Test / Verify Locally

- [ ] `dotnet build` succeeds.
- [ ] `git grep "NotesPage"` returns no matches (the dead file is gone).
- [ ] `git grep "CountToVisibilityConverter"` returns no matches (the
      dead converter is gone).
- [ ] `git grep "SidebarExpandedWidth"` returns no matches (the dead
      const is gone).
- [ ] Run the sample end-to-end and confirm no new behavior was lost.
