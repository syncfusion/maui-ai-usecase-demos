# Syncfusion To Do List - MAUI Sample Application

**Status:** Documentation aligned to in-scope features and customer-ready sample architecture
**Framework:** .NET MAUI 8.0+
**Target Platforms:** iOS, Android, Windows, macOS
**Syncfusion Version:** 24.1.x

---

## What This Sample Is

A **lightweight, customer-focused MAUI sample application** that demonstrates how to build a real-world To Do List with **Syncfusion controls** following **MVVM strictly**. It is designed to be:

- Easy to clone, run, and explore in minutes
- Free of authentication, user accounts, cloud sync, or production infrastructure
- Driven by local in-memory or local-file data so the app starts immediately
- Built one small, independent task at a time using a **Gate-Based Harness Approach**

The reference designs in the `images/` folder are the **source of truth** for layout, spacing, colors, typography, and visual hierarchy. Screens that are not represented in the design set (sign in, sign up, forgot password, splash, profile, settings) are **out of scope** and are not implemented.

### In-Scope Screens (from `images/`)
| Screen | File | Notes |
|--------|------|-------|
| Task list (My Notes) | `mynotes.png` | Default list, empty state, full visual fidelity target |
| Important | `important.png` | Filtered view of starred tasks |
| Reminder | `remainder.png` | Filtered view of tasks with reminders |
| Bin | `bin.png` | Soft-deleted tasks |
| Permanent delete | `deletepermently.png` | Confirmation flow for hard delete |
| Drag and drop | `draganddrop.png` | Reorder / move behavior |
| Create new list | `createnewlist.png` | Dialog for new list |
| Rename list | `renamelist.png` | Dialog for renaming list |
| Search | `search.png` | Empty / results state |
| Review today | `reviewtodayevents.png` | Today's tasks view |
| Collapsed sidebar | `collapsed.png` | Compressed navigation state |
| Load more icon | `loadmoreicon.png` | Pagination affordance |

### Out of Scope (intentionally excluded)
- Sign in, sign up, forgot password, splash, profile, user accounts, settings
- Any authentication flow, token handling, or user state
- Remote APIs, cloud sync, push notifications
- Navigation Shell infrastructure beyond what the sample screens require

---

## Documentation Map

This sample ships with the following documents in the `Task Managment/` folder:

| Document | Purpose | Audience |
|----------|---------|----------|
| **README.md** (this file) | Project overview, quick start, scope | Everyone |
| **StartHere.md** | Customer onboarding, what to read first | Newcomers |
| **Specification.md** | UI + behavior contract for the in-scope screens | Developers, designers |
| **Harness.md** | Gate-Based development plan with task independence rules | Implementers, reviewers |
| **QuickReference.md** | Lookups: tokens, structure, patterns, debugging | Developers (during coding) |
| **ImplementationSummary.md** | Executive overview, key decisions, success criteria | Decision makers |
| **Deliverables.md** | What is delivered, coverage matrix, reading order | Reviewers, leads |
| **Tasks/** | Per-task instructions grouped by module | Implementers |

### Recommended Reading Order
1. **README.md** (5 min) – scope, design fidelity goal, sample rules
2. **StartHere.md** (5 min) – orientation and reading paths
3. **Specification.md** (20 min) – UI + behavior contract
4. **Harness.md** (15 min) – gate-based plan, what to build first
5. **QuickReference.md** (bookmark) – keep open while implementing
6. Individual `Tasks/Module-XX-*/Task-X-XX-*.md` files for the specific task you are working on

---

## Goals and Non-Goals

### Goals
- Showcase Syncfusion controls in realistic, customer-style UI patterns
- Keep every code path understandable to a customer within a few minutes
- Match the in-scope reference screens as closely as possible (My Notes and similar screens are near-100% fidelity targets)
- Demonstrate a strict MVVM pattern with no logic in code-behind
- Allow the sample to run immediately after cloning — no external services required

### Non-Goals (out of scope by design)
- Authentication, user accounts, registration, password reset
- Splash screens, onboarding flows, settings pages
- Profile pages, account management, sign-out
- Backend APIs, cloud sync, remote databases, push notifications
- Production-scale infrastructure, DI containers beyond what is required
- Enterprise application architecture or shared application-wide state

---

## Architecture at a Glance

### MVVM Strictly
- **Views (XAML)** contain only layout, bindings, and styles
- **ViewModels** own all UI state, commands, and business logic
- **Models** are plain data types scoped to the feature they belong to
- **Services** hold reusable logic for a single feature; not shared application-wide

### Scoped by Feature
Each feature folder (e.g. `Features/Notes`, `Features/Important`, `Features/Bin`) contains its own `Views/`, `ViewModels/`, `Models/`, and `Services/`. There is no global state container and no enterprise event bus.

### Data
- Default: **in-memory `ObservableCollection<T>`** seeded at startup
- If persistence is required for a single task: **local JSON file** under the app's data directory
- SQLite is allowed **only** when a single task truly needs it; it is **not** required for the sample to run
- No backend, no remote API, no internet dependency

### Syncfusion
- Used **prominently and naturally** for the screens that call for it (e.g. `SfListView` for the main task list, calendar/date pickers for due dates)
- No custom controls where Syncfusion already provides a working control
- Each task documents the Syncfusion control it uses, why it was chosen, and the customer scenario it demonstrates

---

## Quick Start

### Prerequisites
- Visual Studio 2022 (17.8+) with the **.NET MAUI** workload, or JetBrains Rider
- .NET 8 SDK
- Git

### Run the Sample
1. Clone the repository
2. Open the solution in your IDE
3. Restore NuGet packages
4. Set the desired target (Android emulator, iOS simulator, Windows, macOS)
5. Run

The sample should start directly on the **My Notes** list screen with no sign-in, no splash, and no setup.

### Project Layout (lightweight)
```
src/
├── App.xaml(.cs)
├── MauiProgram.cs
├── Features/
│   ├── Notes/                # Default list (My Notes)
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── Models/
│   │   └── Services/         # In-memory note store, seed data
│   ├── Important/            # Filter view
│   ├── Reminder/             # Filter view
│   ├── Bin/                  # Soft delete + restore + permanent delete
│   ├── CustomLists/          # Create / rename / delete lists
│   ├── Search/               # Search across notes
│   ├── DragAndDrop/          # Reorder / move
│   └── EditNote/             # Create / edit a note
├── Resources/
│   ├── Styles/               # Colors, Sizes, Fonts, Styles
│   └── Images/               # Icons + background images
└── Common/                   # Small shared helpers only (no app state)
```

---

## Design Fidelity Rules

- The `images/` folder is the **primary UI design reference**
- Layout, spacing, colors, typography, cards, lists, sections, visual hierarchy, and interactions are taken from the images
- Deviation from the design is allowed **only** for a documented technical limitation, which is recorded in the task's notes
- Screens such as `mynotes.png` are near-100% fidelity targets
- For the list view, the reference background image (full-bleed) is part of the design and should be replicated

---

## Gate-Based Harness Approach

This sample uses a **Gate-Based Harness**: every task is a small, self-contained unit that is:

- **Independently implementable** – does not require changes outside the task
- **Independently testable** – has its own acceptance criteria
- **Independently reviewable** – reviewers do not need to know future tasks
- **Independently mergeable** – a small, single concern, low risk to merge

Tasks expose **explicit gates** (`G1`, `G2`, …) so progress is visible without needing later tasks to validate earlier ones. A task is considered complete only when its own gate criteria are met.

The full set of gates, ordering rules, and the dependency graph live in `Harness.md`.

### Task Independence Rules
- Keep tasks self-contained; do not assume future tasks will be merged first
- Document any unavoidable dependency explicitly in the task file
- Prefer adding or changing one feature/folder per task
- A reviewer can understand a task in isolation
- Tasks can be developed in parallel where they have no shared dependencies

---

## What Customers Will Learn

- How to wire Syncfusion controls in a real MAUI layout with a near-pixel match to a design
- How to follow strict MVVM with `CommunityToolkit.MVVM` (`[ObservableProperty]`, `[RelayCommand]`)
- How to keep a sample lightweight: no auth, no cloud, no enterprise plumbing
- How to design tasks so they are reviewable, mergeable, and testable on their own
- How to use the design images as the source of truth for UI

---

## Status

| Area | Status |
|------|--------|
| Documentation (in-scope) | Complete and aligned to design images |
| Documentation (out-of-scope sections) | Removed (auth, profile, splash, settings) |
| Design reference | Available in `images/` |
| Architecture | Lightweight, MVVM-strict, customer-focused |
| Implementation | Ready to begin via `Tasks/` |

---

## License

Add license information here as appropriate for your repository.
