# Start Here - Syncfusion MAUI To Do List Sample

Welcome! This sample is a **lightweight, customer-focused MAUI application** that demonstrates a real-world To Do List built with **Syncfusion controls** and a **strict MVVM** pattern. It is designed to be cloned, run, and understood in minutes — without any authentication, cloud service, or production infrastructure.

---

## 5-Minute Quick Start

### What is this project?
A cross-platform To Do List sample (iOS, Android, Windows, macOS) that uses:
- .NET MAUI
- Syncfusion controls (SfListView, date pickers, etc.)
- CommunityToolkit.MVVM for clean MVVM
- Local, in-memory data — no backend, no internet, no setup

### What is in scope?
- Default **My Notes** list, with the design from `images/mynotes.png`
- **Important**, **Reminder**, **Bin** filter views
- **Custom Lists** with create / rename / delete
- **Search** across note titles
- **Drag and drop** to reorder and move between lists
- A bottom input bar for quick note capture

### What is intentionally excluded?
- Sign in, sign up, forgot password, splash, sign out
- Profile pages, account management, settings
- Backend APIs, cloud sync, remote databases
- Push notifications, scheduled reminders
- User accounts, sessions, tokens
- Global application services, DI containers, shared state

If you are looking for a sample with sign-in or profile, this is not the right sample — and that is by design.

### How do I run it?
1. Clone the repository
2. Open the solution in Visual Studio 2022 (with the .NET MAUI workload) or JetBrains Rider
3. Restore NuGet packages
4. Pick a target (Android emulator, iOS simulator, Windows, or macOS)
5. Run

The sample opens directly on the **My Notes** list with a few seeded notes. No sign-in screen, no splash, no setup wizard.

---

## Which Document Should I Read?

### If you are new to the sample
Read in this order:

1. **README.md** (5 min) – project overview, scope, design fidelity goal
2. **StartHere.md** (this file, 5 min) – orientation
3. **Specification.md** (20 min) – UI + behavior contract for the in-scope screens
4. **Harness.md** (15 min) – the Gate-Based plan and how tasks are organized
5. **QuickReference.md** (bookmark) – keep open while implementing

### If you are implementing a specific task
- Open the corresponding `Tasks/Module-XX-YY/Task-X-XX-*.md`
- The task file lists the **gate** criteria, files in scope, and manual verification steps

### If you are reviewing
- The task file contains the **diff boundary** (files in / files out)
- The **gate** list is your checklist
- The design image in `images/` is the source of truth for visual checks

### If you are a decision maker
- Read **README.md** for scope and goals
- Read **ImplementationSummary.md** for key decisions and success criteria
- Read **Deliverables.md** for what is in the package

### If you are a designer
- The `images/` folder is the source of truth
- **Specification.md → Section 7** documents the screen-by-screen contract
- **QuickReference.md → Design Tokens** lists the visual tokens used in the sample

---

## What Is in Each Document?

### README.md
```
- Project overview and scope
- What is in scope vs out of scope (auth, profile, settings, splash)
- Architecture at a glance
- Quick start
- Design fidelity rules
- Gate-Based Harness summary
```
**Read time:** 5 minutes | **Audience:** Everyone

---

### StartHere.md (this file)
```
- 5-minute quick start
- What is in scope / out of scope
- Reading order by role
- Where to look for a specific topic
```
**Read time:** 5 minutes | **Audience:** Newcomers

---

### Specification.md
```
- In-scope features and screens
- Out-of-scope reminder
- Component specifications
- Design tokens
- Screen-by-screen contract (matched to design images)
- Data models
- Architecture and MVVM rules
- Syncfusion control mapping
- Resource structure
- Accessibility and responsive design
```
**Read time:** 20-25 minutes | **Audience:** Developers, designers, reviewers

---

### Harness.md
```
- Gate-Based Harness Approach
- Task independence rules
- Module map and module gates
- Module-by-module breakdown
- Cross-cutting conventions
- Definition of done
- Validation checklist
- Out of scope (re-emphasized)
```
**Read time:** 15-20 minutes | **Audience:** Implementers, reviewers, leads

---

### QuickReference.md
```
- Module and task checklist
- Design tokens (colors, spacing, typography, radius)
- Project structure
- Code patterns (ViewModel, Service, Resource)
- Common debugging tips
- Syncfusion usage notes
- Out-of-scope reminder
```
**Read time:** Keep bookmarked | **Audience:** Developers (during coding)

---

### ImplementationSummary.md
```
- Executive overview
- Key design decisions and rationale
- Lightweight sample architecture
- Quick start code snippets
- Success criteria
```
**Read time:** 10-15 minutes | **Audience:** Decision makers, leads

---

### Deliverables.md
```
- What is delivered
- Coverage matrix
- Reading order
- Quality checks
- Out-of-scope reminder
```
**Read time:** 10 minutes | **Audience:** Reviewers, leads

---

### Tasks/ folder
```
- Per-task instructions grouped by module
- Each task file: objective, gate, files in/out, acceptance criteria, manual verification
```
**Read time:** Per task | **Audience:** Implementers

---

## Reading Order (recommended)

```
StartHere.md (you are here)
   ↓
README.md (5 min) ← Stop here if just reviewing
   ↓
Specification.md (20 min) ← The contract
   ↓
Harness.md (15 min) ← How the work is organized
   ↓
QuickReference.md (bookmark) ← During implementation
   ↓
Tasks/Module-01-Foundation/... ← Begin work
```

**Total time to be ready:** ~45 minutes

---

## Super Quick Version

If you only have 2 minutes:

1. This is a **MAUI To Do List sample** that showcases Syncfusion controls and MVVM
2. It is **lightweight** — no auth, no cloud, no enterprise plumbing
3. The `images/` folder is the **source of truth** for every in-scope screen
4. The work is organized as small, **gate-based, independent tasks**
5. Read `README.md` for the scope, `Harness.md` for the plan

---

## Common Questions

### "Where is the sign-in screen?"
There isn't one. The sample does not implement authentication, sign-up, forgot-password, or sign-out. The app opens directly on the My Notes list with seeded data.

### "Where is the profile page?"
There isn't one. The sample does not implement profile, account management, or settings.

### "Where does the data come from?"
From an in-memory `ObservableCollection<Note>` seeded at startup. The sample does not require a backend, cloud service, or local SQLite. If a single task needs persistence, a local JSON file in the app's data directory is allowed; SQLite is allowed only if truly necessary.

### "How is the UI built?"
With Syncfusion controls (e.g. `SfListView` for the main list) inside MAUI XAML, with all styling via resource dictionaries. No code-behind logic.

### "What if a future contributor wants to add sign-in?"
That's a sign that the task is **out of scope** for this sample. This sample is intentionally a customer-focused reference, not a production-ready app with full identity management. A future sample can pick that up.

### "How is the work organized?"
Into small, **independent tasks** grouped by module. Each task has a gate and can be implemented, tested, reviewed, and merged on its own. See `Harness.md` for the rules.

### "Where do I find the design?"
In the `images/` folder. The README, Specification, and each task file reference the exact image that is the source of truth for that work.

---

## Where to Look for a Specific Topic

| Topic | Document |
|-------|----------|
| In-scope vs out-of-scope | README.md, Specification.md §1.3, Harness.md (final section) |
| Design fidelity rules | README.md, Specification.md §2 |
| Gate-Based methodology | Harness.md |
| Task independence rules | Harness.md |
| Module / task breakdown | Harness.md, Tasks/ folder |
| Design tokens (colors, fonts, sizes) | QuickReference.md, Specification.md §6 |
| Project layout | Specification.md §11, QuickReference.md |
| Code patterns | QuickReference.md |
| Per-screen contract | Specification.md §7 |
| Syncfusion controls and why | Specification.md §10, QuickReference.md |
| Accessibility | Specification.md §13, QuickReference.md |
| Responsive design | Specification.md §14, QuickReference.md |
| Out of scope (canonical list) | README.md, Specification.md Appendix A, Harness.md (final section) |

---

## Your First Task

1. **Read** `README.md` (5 minutes)
2. **Skim** `Specification.md` §1-3 to understand scope (5 minutes)
3. **Read** `Harness.md` §1-3 to understand the gate model (10 minutes)
4. **Pick** a task from `Tasks/Module-01-Foundation/` and follow its instructions
5. **Bookmark** `QuickReference.md` for the patterns you will reuse

---

## Document Locations

All documents are in this folder:

```
Task Managment/
├── StartHere.md              ← You are here
├── README.md                 ← Project overview
├── Specification.md          ← UI + behavior contract
├── Harness.md                ← Gate-Based development plan
├── QuickReference.md         ← Lookup guide (bookmark)
├── ImplementationSummary.md  ← Executive overview
├── Deliverables.md           ← What is delivered
├── images/                   ← Design reference (in-scope screens)
└── Tasks/
    ├── TASKS_INDEX.md
    ├── Module-01-Foundation/
    ├── Module-02-MyNotes/
    ├── Module-03-TaskCRUD/
    ├── Module-04-Filtering/
    ├── Module-05-CustomLists/
    ├── Module-06-Search/
    ├── Module-07-DragAndDrop/
    └── Module-08-Polish/
```

---

## Let's Get Started

### Next step: open `README.md` and confirm the in-scope screens match what you want to build.

---

**Version:** 2.0
**Updated:** October 2026
**Status:** Documentation aligned to in-scope features and Gate-Based Harness Approach
