# Module 08 — Polish

**Goal:** Sample-wide cleanup, fidelity review, and customer-readiness
pass.

**Module gate (G-M08):** A new customer can clone, build, run, and see
every in-scope screen match its design image. No dead code, no in-scope
TODOs, no out-of-scope plumbing.

---

## Tasks (independent of each other)

| # | Task | Files in | Files out |
|---|------|----------|-----------|
| [8-1](./Task-8-1-Responsive-Layout.md) | Responsive layout pass — sidebar collapses per `collapsed.png` on narrow widths | `Features/Notes/Views/MainLayoutView.xaml(.cs)`, `Features/Notes/ViewModels/NotesListViewModel.cs` | `Resources/Styles/*`, other features |
| [8-2](./Task-8-2-Accessibility.md) | Accessibility pass — touch targets, contrast, semantic labels | `Features/Notes/Views/HeaderView.xaml(.cs)`, `Features/Notes/Views/NotesContentView.xaml` | converters, ViewModels, resources |
| [8-3](./Task-8-3-Empty-Loading-States.md) | Empty / loading states pass | `Features/Notes/ViewModels/NotesListViewModel.cs`, `Features/Notes/Views/SidebarView.xaml` | other features, resources |
| [8-4](./Task-8-4-Customer-Walkthrough.md) | Customer walkthrough — step-by-step click-through matching every in-scope design image | `Tasks/Module-08-Polish/*` (this folder), `README.md` | source code |
| [8-5](./Task-8-5-Final-Cleanup.md) | Final cleanup — remove leftover dead code, dead comments, leftover stubs | `Features/Notes/Views/NotesPage.xaml(.cs)`, `Converters/CountToVisibilityConverter.cs`, `Features/Notes/ViewModels/NotesListViewModel.cs`, `App.xaml` | other source files |

---

## Cross-cutting notes

- The five tasks in this module are independent: each can be reviewed and
  merged without waiting for the others.
- The full Module gate (G-M08) requires G-M08-1 through G-M08-5 to all
  pass; this is the last gate in the sample.
- The walkthrough (8-4) is the customer-facing artifact; a customer can
  follow it without reading the spec, harness, or any other task file.
