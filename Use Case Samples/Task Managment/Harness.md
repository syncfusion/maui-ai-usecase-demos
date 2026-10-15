# Syncfusion To Do List - Gate-Based Development Harness

**Version:** 2.0
**Date:** October 2026
**Framework:** .NET MAUI
**Methodology:** **Gate-Based Harness Approach** with strict task independence

---

## Table of Contents

1. [Purpose](#purpose)
2. [Gate-Based Harness Approach](#gate-based-harness-approach)
3. [Task Independence Rules](#task-independence-rules)
4. [Module Map and Gates](#module-map-and-gates)
5. [Module Details](#module-details)
6. [Cross-Cutting Conventions](#cross-cutting-conventions)
7. [Definition of Done](#definition-of-done)
8. [Validation Checklist](#validation-checklist)
9. [Out of Scope](#out-of-scope)

---

## Purpose

This harness breaks the Syncfusion To Do List sample into **small, self-contained tasks**, each with an **explicit gate** that can be checked, reviewed, and merged without waiting for any other task. It exists to keep the sample:

- **Customer-focused** – a customer must be able to read a single task and understand it in minutes
- **Lightweight** – no authentication, no cloud, no enterprise infrastructure
- **Independent** – a task does not require later tasks to be valid
- **Design-faithful** – tasks reference the images in `images/` as the source of truth
- **Syncfusion-prominent** – tasks use Syncfusion controls naturally, not artificially

A reviewer must never need to know about future tasks to validate the current one. A implementer must never need to touch code outside their task folder to complete a task.

---

## Gate-Based Harness Approach

A **gate** is a binary, observable check that proves a task is **implementable, testable, reviewable, and mergeable** in isolation.

### What a Gate Looks Like
Each task file (in `Tasks/Module-XX-YY/`) contains:

- A clear objective that can be stated in one sentence
- A list of explicit acceptance criteria that can each be checked off
- A list of files it creates or modifies
- A list of files it must **not** touch (boundary)
- A list of explicit dependencies on other tasks (kept minimal)
- A "Gate" section that names the binary outcomes that mean the task is done

### Gate Lifecycle for Every Task
```
G0: Dependencies ready           (other tasks this task needs are merged)
G1: Plan / boundary set          (files-in / files-out / dependencies declared)
G2: Implementation complete      (all files in scope, MVVM clean)
G3: Local build & run            (sample launches, expected screen appears)
G4: Design fidelity check        (matches the referenced image to the documented level)
G5: Self-review                  (no code-behind logic, no out-of-scope data, no hardcoded values)
G6: Ready to merge               (reviewer can approve without reading other tasks)
```

A task is **not** done until **G6** passes. Earlier tasks do not depend on later tasks passing G6.

### Why Gates Matter Here
- A customer reading the sample should be able to read any single task file and see exactly what the task delivers
- A reviewer can spot over-reach quickly (files outside the boundary, scope creep)
- A future maintainer can revert or rewrite a single task with confidence
- A parallel contributor can work on multiple tasks with low collision risk

---

## Task Independence Rules

These rules apply to **every** task in this harness. Any task that violates them must be split or rewritten.

1. **Self-contained**
   - The task lists every file it creates or modifies
   - The task lists the files it must not touch
2. **Independently implementable**
   - All required inputs come from already-merged tasks
   - No need to wait for future tasks
3. **Independently testable**
   - The task has its own acceptance criteria
   - Each criterion can be checked visually or by running the sample
4. **Independently reviewable**
   - A reviewer who has not read any other task can review this one
   - The diff for a task should be small and focused
5. **Independently mergeable**
   - Merging this task does not break the sample
   - The sample runs and shows the expected screen even if no other tasks have been merged
6. **Minimal dependencies**
   - If a dependency on another task is required, it is declared explicitly
   - Dependency chains are kept as short as possible
7. **Parallel-friendly**
   - Tasks in different modules with no shared files can be developed in parallel
8. **No speculative scope**
   - No task adds authentication, profile, settings, splash, sign-in, sign-up, or forgot-password code
   - No task adds cloud sync, push notifications, remote APIs, or DI containers

---

## Module Map and Gates

Modules are organized to maximize task independence. Each module groups tasks around a single feature area, but **every task within a module is also independent** of the others in the same module where possible.

| Module | Focus | Design Image(s) | Out-of-scope notes |
|--------|-------|------------------|---------------------|
| 01 Foundation | Project, resources, sample architecture | – | No auth/splash, just app shell that opens the My Notes list |
| 02 MyNotes | Default list with empty state | `mynotes.png` | Highest fidelity target |
| 03 TaskCRUD | Create, edit, complete, soft delete | `mynotes.png` | Bottom input bar + edit dialog |
| 04 Filtering | Important, Reminder, Bin views | `important.png`, `remainder.png`, `bin.png`, `deletepermently.png` | No reminders firing, no notifications |
| 05 CustomLists | Create, rename, delete lists | `createnewlist.png`, `renamelist.png` | Local only |
| 06 Search | Search notes by title | `search.png` | In-memory filter |
| 07 DragAndDrop | Reorder, move between lists | `draganddrop.png`, `collapsed.png` | Optional but gated |
| 08 Polish | Sample-wide cleanup, fidelity pass | All in-scope | Customer-review pass |

The image references above are the **source of truth** for each module. If a module needs a UI element, it must be matched from the image.

### Module Gate Map
- **G-M01** Sample compiles, opens to the empty `mynotes.png` layout
- **G-M02** My Notes list shows notes from the in-memory store, matches `mynotes.png`
- **G-M03** Create / edit / complete / soft-delete work from the bottom input bar
- **G-M04** Important, Reminder, and Bin filters work and visually match the three images
- **G-M05** Custom lists can be created, renamed, and deleted; dialogs match `createnewlist.png` / `renamelist.png`
- **G-M06** Search filters notes by title and shows `search.png` empty state
- **G-M07** Drag and drop reorders / moves notes per `draganddrop.png`
- **G-M08** Sample runs end-to-end with no dead code, no in-scope TODOs, design parity reached

---

## Module Details

Each module below is described as a **group of independent tasks**. The full per-task instructions live in `Tasks/Module-XX-YY/Task-X-XX-*.md`.

---

### MODULE 01: Foundation

**Goal:** Stand up the lightweight sample architecture that every later task can plug into without change.

**Module gate (G-M01):** A reviewer can run the sample and see a screen that matches the empty-state version of `mynotes.png` (sidebar + main area + bottom input bar), with no note items yet.

**Tasks (independent of each other where possible):**
- **1-01 Project Structure** – Create the lightweight folder layout (`Features/*`, `Resources/*`, `Common/*`). *Files in:* project root. *Files out:* none.
- **1-02 NuGet Packages** – Add `Syncfusion.Maui.ListView`, `Syncfusion.Maui.Calendar` (only if a later task needs it), `Syncfusion.Maui.Core`, `CommunityToolkit.MVVM`. *Files in:* `.csproj`. *Files out:* none.
- **1-03 MVVM Toolkit** – Show the `[ObservableProperty]` / `[RelayCommand]` pattern in a minimal example VM. *Files in:* `Common/ViewModelBase.cs`. *Files out:* none.
- **1-04 Resource Dictionaries** – `Colors.xaml`, `Sizes.xaml`, `Fonts.xaml`, `Styles.xaml` based on the design images. *Files in:* `Resources/Styles/`. *Files out:* none.
- **1-5 MauiProgram Bootstrap** – Register Syncfusion core only; no DI container for sample services. *Files in:* `MauiProgram.cs`, `App.xaml(.cs)`. *Files out:* `Features/*`.
- **1-6 Seed Data** – Provide an in-memory `ObservableCollection<Note>` with a few example notes. *Files in:* `Common/SampleData.cs`. *Files out:* none.
- **1-7 Notes List Page Shell** – Create the page layout that matches the empty `mynotes.png` (sidebar + main + bottom bar) with no items yet. *Files in:* `Features/Notes/Views/NotesPage.xaml(.cs)`, `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* all other feature folders.

> **No task in this module** creates sign-in, sign-up, forgot-password.

---

### MODULE 02: MyNotes

**Goal:** Show notes on the main list using Syncfusion `SfListView` and the item template from `mynotes.png`.

**Module gate (G-M02):** A reviewer can run the sample, see the seeded notes rendered as the cards shown in `mynotes.png`, with the bottom input bar visible and the sidebar showing the **My Notes**, **Important**, **Reminder**, **Bin** entries (these entries are placeholders for now; they activate in Module 04).

**Tasks:**
- **2-1 Notes List ViewModel** – Expose `Notes` collection and the selected-list filter. *Files in:* `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* views, services.
- **2-2 Sidebar Navigation** – Render the sidebar items per `collapsed.png` collapsed state and the expanded state visible in `mynotes.png`. Selecting a built-in item sets the current filter (real filtering comes in Module 04). *Files in:* `Features/Notes/Views/SidebarView.xaml` (or part of `NotesPage.xaml`). *Files out:* other features.
- **2-3 Header Bar** – Title + search icon. *Files in:* `Features/Notes/Views/HeaderView.xaml`. *Files out:* other features.
- **2-4 Note Item Template** – Card with checkbox, title, date metadata, star, more menu. Match `mynotes.png` exactly. *Files in:* `Features/Notes/Views/NoteItemTemplate.xaml`. *Files out:* other features.
- **2-5 SfListView Integration** – Bind the template. *Files in:* `Features/Notes/Views/NotesPage.xaml`. *Files out:* other features.
- **2-6 Bottom Input Bar** – Pill input + checkmark button per `mynotes.png`. *Files in:* `Features/Notes/Views/BottomInputBar.xaml`. *Files out:* other features.
- **2-7 Empty State** – Show empty-state copy when there are no notes. *Files in:* `Features/Notes/Views/EmptyStateView.xaml`. *Files out:* other features.

> **No task in this module** introduces auth, profile, settings, or splash.

---

### MODULE 03: TaskCRUD

**Goal:** Add, edit, complete, and soft-delete notes. The bottom input bar becomes functional; an edit dialog opens for richer edits.

**Module gate (G-M03):** A reviewer can type in the bottom bar, press the checkmark, see a new note appear, tap a note to edit it, mark it complete, and long-press to soft-delete. After delete, the note is hidden from the My Notes list.

**Tasks:**
- **3-1 In-Memory Note Service** – `INoteService` + `InMemoryNoteService` with `Create/Update/Delete/Restore/ToggleComplete`. *Files in:* `Features/Notes/Services/`. *Files out:* views, viewmodels.
- **3-2 Edit Note Page** – Page that matches the dialog patterns in `createnewlist.png` / `renamelist.png` (small centered dialog, title + input + buttons). *Files in:* `Features/EditNote/Views/EditNotePage.xaml(.cs)`. *Files out:* other features.
- **3-3 Edit Note ViewModel** – `Title`, `DueDate`, `ReminderDate`, save / cancel. *Files in:* `Features/EditNote/ViewModels/EditNoteViewModel.cs`. *Files out:* other features.
- **3-4 Create from Bottom Bar** – Submit command in `NotesViewModel`. *Files in:* `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* other features.
- **3-5 Edit Existing Note** – Tap on a note opens `EditNotePage` with current values. *Files in:* `Features/Notes/ViewModels/NotesViewModel.cs`, `Features/EditNote/ViewModels/EditNoteViewModel.cs`. *Files out:* other features.
- **3-6 Complete Toggle** – Checkbox in item template calls the toggle command. *Files in:* `Features/Notes/Views/NoteItemTemplate.xaml`, `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* other features.
- **3-7 Soft Delete** – Long-press / "Delete" in the more menu removes the note from My Notes (Bin is added in Module 04). *Files in:* `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* other features.
- **3-8 More Menu (Context Menu)** – Three-dot menu per `mynotes.png`. *Files in:* `Features/Notes/Views/NoteItemTemplate.xaml`, `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* other features.

> Tasks 3-1 through 3-8 are independent of one another as long as 2-1 (Notes List ViewModel) is merged first. The order above is a suggested order, not a hard dependency.

---

### MODULE 04: Filtering (Important, Reminder, Bin)

**Goal:** Activate the four sidebar entries (My Notes, Important, Reminder, Bin) and render each view as its reference image.

**Module gate (G-M04):** A reviewer can mark a note as important and see it in the **Important** view, set a reminder date and see it in the **Reminder** view, soft-delete a note and see it in the **Bin** view, restore from Bin, and permanently delete from Bin (with the dialog from `deletepermently.png`).

**Tasks:**
- **4-1 Important View** – Filter view matching `important.png`. *Files in:* `Features/Important/`. *Files out:* other features.
- **4-2 Star Toggle** – Wire the star icon in the item template. *Files in:* `Features/Notes/Views/NoteItemTemplate.xaml`, `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* other features.
- **4-3 Reminder View** – Filter view matching `remainder.png`. *Files in:* `Features/Reminder/`. *Files out:* other features.
- **4-4 Reminder Date Picker** – Optional Syncfusion date picker on the edit page. *Files in:* `Features/EditNote/`. *Files out:* other features.
- **4-5 Bin View** – Filter view matching `bin.png`. *Files in:* `Features/Bin/`. *Files out:* other features.
- **4-6 Restore** – "Restore" action returns the note to its source list. *Files in:* `Features/Bin/ViewModels/BinViewModel.cs`. *Files out:* other features.
- **4-7 Permanent Delete** – Confirmation dialog matching `deletepermently.png`; deletes the note from the in-memory store. *Files in:* `Features/Bin/Views/PermanentDeleteDialog.xaml`. *Files out:* other features.

> No reminders fire local notifications or push messages. A "reminder" here is a date flag only.

---

### MODULE 05: CustomLists

**Goal:** Add custom lists to the sidebar with create / rename / delete flows.

**Module gate (G-M05):** A reviewer can tap "+" in the sidebar, create a list (matches `createnewlist.png`), tap the more menu on a custom list to rename it (matches `renamelist.png`), and delete a custom list with confirmation.

**Tasks:**
- **5-1 In-Memory List Service** – `IListService` + `InMemoryListService`. *Files in:* `Features/CustomLists/Services/`. *Files out:* other features.
- **5-2 Create List Dialog** – Matches `createnewlist.png`. *Files in:* `Features/CustomLists/Views/CreateListDialog.xaml`. *Files out:* other features.
- **5-3 Rename List Dialog** – Matches `renamelist.png`. *Files in:* `Features/CustomLists/Views/RenameListDialog.xaml`. *Files out:* other features.
- **5-4 Sidebar Loads Custom Lists** – The sidebar binds to the list service. *Files in:* `Features/Notes/Views/SidebarView.xaml` (or relevant view), `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* other features.
- **5-5 Delete Custom List** – Confirmation + remove. *Files in:* `Features/CustomLists/ViewModels/CustomListsViewModel.cs`. *Files out:* other features.

---

### MODULE 06: Search

**Goal:** Add a simple in-memory search across all notes (title match).

**Module gate (G-M06):** A reviewer can tap the search icon, type a query, see matching notes, clear the query to see the empty state from `search.png`.

**Tasks:**
- **6-1 In-Memory Search Service** – Case-insensitive title match. *Files in:* `Features/Search/Services/`. *Files out:* other features.
- **6-2 Search Results Page** – Page with input + results. *Files in:* `Features/Search/Views/SearchResultsPage.xaml`. *Files out:* other features.
- **6-3 Search Integration** – Search icon in header opens the page. *Files in:* `Features/Notes/Views/HeaderView.xaml`, `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* other features.

---

### MODULE 07: DragAndDrop

**Goal:** Add drag-and-drop reordering and move-between-lists per `draganddrop.png`.

**Module gate (G-M07):** A reviewer can long-press a note in My Notes, drag it to reorder, and drop it on another list in the sidebar to move it there.

**Tasks:**
- **7-1 Drag Reorder in List** – `SfListView` drag reorder. *Files in:* `Features/Notes/Views/NotesPage.xaml`, `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* other features.
- **7-2 Drag to Sidebar (Move)** – Drop on a sidebar entry moves the note. *Files in:* `Features/Notes/Views/SidebarView.xaml`, `Features/Notes/ViewModels/NotesViewModel.cs`. *Files out:* other features.

---

### MODULE 08: Polish

**Goal:** Sample-wide cleanup, fidelity review, and customer-readiness pass.

**Module gate (G-M08):** A new customer can clone, run, and see every in-scope screen match its design image. No dead code, no in-scope TODOs, no out-of-scope plumbing.

**Tasks:**
- **8-1 Responsive Layout Pass** – Verify sidebar collapses per `collapsed.png` on narrow widths.
- **8-2 Accessibility Pass** – Touch targets, contrast, semantic labels.
- **8-3 Empty / Loading States Pass** – All views handle empty collections cleanly.
- **8-4 Customer Walkthrough** – Step-by-step click-through matching the image set.
- **8-5 Final Cleanup** – Remove any leftover dead code, dead comments, leftover stubs.

---

## Cross-Cutting Conventions

These conventions apply to **every** task. They are short on purpose and exist to make every task reviewable on its own.

### Code Style
- One View, one ViewModel, one or more Models, optional Services, per feature
- `partial class` ViewModels with `[ObservableProperty]` and `[RelayCommand]`
- No logic in `*.xaml.cs` (only `BindingContext` assignment and `InitializeComponent`)
- No shared application-wide state
- No DI container; pass services through the constructor as needed

### Resources
- All colors, sizes, fonts, and styles live in `Resources/Styles/`
- No hardcoded hex values in XAML or code-behind
- Reference resources as `{StaticResource TokenName}`

### Data
- Default: in-memory `ObservableCollection<T>` seeded at startup
- Allowed: local JSON file in the app's data directory if a single task needs persistence
- Allowed: local SQLite **only** when a single task truly requires queries that JSON cannot support (none of the current tasks do)
- Never: remote APIs, cloud sync, internet-dependent data

### Syncfusion Usage
- Use the Syncfusion control that maps to the design (e.g. `SfListView` for the main list, date pickers for dates)
- Each task that uses a Syncfusion control must document:
  - **Control used** (e.g. `SfListView`)
  - **Why it was chosen** (e.g. built-in virtualization, item template support)
  - **Customer scenario demonstrated** (e.g. "rendering a long list of notes smoothly on mobile")
- No custom controls where Syncfusion already provides a working control

### Design Fidelity
- The image file is the source of truth for layout, spacing, colors, typography
- The reference background image is part of the design
- Document any deviation with a short justification in the task file

### Test / Verify Locally
Every task must include a short list of manual steps the reviewer can run to confirm the task is done:

```
- [ ] `dotnet build` succeeds
- [ ] Run on chosen target
- [ ] Navigate to the new screen / state
- [ ] Verify the design image is matched
- [ ] Verify no other screens regressed
```

---

## Definition of Done

A task is **done** when, at minimum:

- All gate criteria in the task file are checked
- `dotnet build` succeeds with no new warnings
- The sample launches and the new behavior is visible
- The design image is matched to the documented level
- No code-behind logic, no out-of-scope code, no hardcoded values
- The diff is small and focused on the declared `files-in` / `files-out`
- A reviewer who has not read any other task can approve

A task is **not done** if it:
- Introduces authentication, profile, settings, splash, sign-in, sign-up, or forgot-password code
- Adds a remote API, cloud sync, push notification, or remote database
- Requires a future task to be merged before it works
- Touches files outside its declared boundary
- Has acceptance criteria that depend on a future task

---

## Validation Checklist

For each task, the reviewer checks:

- [ ] Files modified match the declared `files-in`
- [ ] Files outside the task are not touched
- [ ] Acceptance criteria are all met
- [ ] `dotnet build` is clean
- [ ] Sample runs and shows the new behavior
- [ ] Design image is matched to the documented level
- [ ] No code-behind logic
- [ ] No hardcoded values
- [ ] No out-of-scope data or services
- [ ] Task is independently mergeable

---

## Out of Scope

These items are explicitly **out of scope** for this sample. If a task needs them, it is the wrong task.

- Authentication, user accounts, registration, password reset
- Splash screens, onboarding flows, settings pages
- Profile pages, account management, sign-out
- Backend APIs, cloud sync, remote databases, push notifications
- Navigation Shell infrastructure beyond what the sample screens require
- Global application services or shared application state
- Enterprise application architecture or DI containers
- Any production-scale infrastructure

The design images reflect this scope: `images/` contains the in-scope task-management screens and intentionally does not include sign-in, sign-up, forgot-password, splash, profile, or settings screens.

---

**END OF HARNESS**
