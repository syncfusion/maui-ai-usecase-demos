# Syncfusion To Do List - Sample Specification

**Version:** 2.0
**Date:** October 2026
**Application:** MAUI To Do List sample with Syncfusion Controls
**Target Platform:** .NET MAUI (iOS, Android, Windows, macOS)

> This specification is intentionally scoped to the **in-scope** task-management screens shown in the `images/` folder. Authentication, sign-in, sign-up, forgot-password, splash, profile, settings, and account management are **out of scope** and are not specified here.

---

## Table of Contents

1. [Application Overview](#1-application-overview)
2. [Design Source of Truth](#2-design-source-of-truth)
3. [In-Scope Screens](#3-in-scope-screens)
4. [Non-Functional Requirements](#4-non-functional-requirements)
5. [Component Specifications](#5-component-specifications)
6. [Design Tokens](#6-design-tokens)
7. [Screen-by-Screen Specifications](#7-screen-by-screen-specifications)
8. [Data Models](#8-data-models)
9. [Architecture and MVVM](#9-architecture-and-mvvm)
10. [Syncfusion Control Mapping](#10-syncfusion-control-mapping)
11. [Resource Structure](#11-resource-structure)
12. [Validation and Error Handling](#12-validation-and-error-handling)
13. [Accessibility](#13-accessibility)
14. [Responsive Design](#14-responsive-design)

---

## 1. Application Overview

### 1.1 Purpose
A lightweight, customer-facing MAUI sample application that demonstrates a real-world To Do List built with Syncfusion controls and a strict MVVM pattern. The sample runs immediately after cloning the repository, without any external service.

### 1.2 In-Scope Features
- A default **My Notes** list with create / edit / complete / soft-delete
- Sidebar navigation with **My Notes**, **Important**, **Reminder**, **Bin**, and user-defined **Custom Lists**
- **Important** view (starred notes)
- **Reminder** view (notes with a reminder date)
- **Bin** view with restore and permanent delete
- **Custom Lists** with create, rename, delete
- **Search** across note titles
- **Drag and drop** to reorder and move between lists
- A bottom input bar for quick note capture
- A consistent design language across all in-scope screens

### 1.3 Out of Scope
- Authentication, sign-in, sign-up, forgot-password, sign-out
- Splash screens, onboarding flows
- Profile pages, account management, settings pages
- Backend APIs, cloud sync, remote databases
- Push notifications, scheduled reminders
- User accounts, sessions, tokens
- Global application services, shared application state, DI containers
- Production-scale infrastructure

### 1.4 Target Customers
- Developers learning Syncfusion in MAUI
- Teams that want a starting point for a task-management UI
- Customers who want to read a sample in minutes and extend it

---

## 2. Design Source of Truth

The `images/` folder is the **primary UI design reference**. Every in-scope screen must be implemented to match its reference image as closely as possible.

### 2.1 In-Scope Reference Images
| File | Screen | Fidelity target |
|------|--------|-----------------|
| `mynotes.png` | Default list (My Notes), empty state | **Near 100%** |
| `important.png` | Important filter | High |
| `remainder.png` | Reminder filter | High |
| `bin.png` | Bin filter | High |
| `deletepermently.png` | Permanent delete confirmation | High |
| `draganddrop.png` | Drag and drop affordance | High |
| `createnewlist.png` | Create list dialog | High |
| `renamelist.png` | Rename list dialog | High |
| `search.png` | Search empty state | High |
| `reviewtodayevents.png` | Today's notes | High |
| `collapsed.png` | Collapsed sidebar | High |
| `loadmoreicon.png` | Pagination affordance | High |

### 2.2 Excluded Reference Images
The following images are intentionally **excluded** from implementation:
- `splash.png` – splash screens are out of scope
- `signout.png` – sign-out is out of scope
- `profile_pressed.png` – profile pages are out of scope

If the documentation refers to a screen that is not in the in-scope list, treat it as out of scope.

### 2.3 Deviation Policy
A deviation from a design image is allowed only when there is a documented technical limitation. The deviation is recorded in the task file with:
- The image that was the source of truth
- The exact element that was adjusted
- The technical reason
- The chosen alternative

---

## 3. In-Scope Screens

| # | Screen | Reference | Behavior summary |
|---|--------|-----------|------------------|
| 1 | My Notes (default) | `mynotes.png` | Shows the active list's notes, sidebar, header, bottom input bar |
| 2 | Important | `important.png` | Shows only notes flagged as important |
| 3 | Reminder | `remainder.png` | Shows only notes with a reminder date set |
| 4 | Bin | `bin.png` | Shows soft-deleted notes, with restore and permanent delete |
| 5 | Create List dialog | `createnewlist.png` | Enter a new list name, save |
| 6 | Rename List dialog | `renamelist.png` | Edit an existing list name, save |
| 7 | Search | `search.png` | Filter notes by title, empty state when no results |
| 8 | Permanent Delete confirm | `deletepermently.png` | Confirm dialog before hard delete |
| 9 | Drag and drop | `draganddrop.png` | Reorder notes; move to a different list |
| 10 | Today's notes | `reviewtodayevents.png` | Today-focused view of notes |
| 11 | Collapsed sidebar | `collapsed.png` | Compressed navigation state on narrow widths |

---

## 4. Non-Functional Requirements

### 4.1 Performance
- App launch on the default My Notes list should be near-instant in a debug build
- List scrolling must remain smooth with the in-memory seeded data
- Search filtering should be near-instant for the seeded set

### 4.2 Accessibility
- Minimum 12sp font size
- 4.5:1 text contrast
- 48x48 dp minimum touch targets
- Visible focus indicators for keyboard navigation
- Semantic labels on interactive elements

### 4.3 Responsive Design
- Phone (< 600 px) – sidebar collapses per `collapsed.png`
- Tablet (600-1024 px) – sidebar visible, condensed
- Desktop (>= 1025 px) – sidebar visible, full

### 4.4 Reliability
- All data is in-memory at startup; no remote calls
- Soft-delete is reversible from the Bin view
- Permanent delete is irreversible and requires confirmation

### 4.5 Maintainability
- One View, one ViewModel per screen
- All styling via resource dictionaries
- No code-behind logic
- No shared application-wide state
- Each feature folder is independently understandable

---

## 5. Component Specifications

These are the recurring visual components used across the in-scope screens. They are matched against the design images.

### 5.1 Header Bar (per `mynotes.png` and others)
- Title (current list name) on the left
- Search icon on the right
- Optional menu icon to collapse the sidebar on narrow widths

### 5.2 Sidebar (per `mynotes.png` expanded, `collapsed.png` collapsed)
- Built-in entries: My Notes, Important, Reminder, Bin
- Custom lists appear below
- Active item is visually highlighted
- Collapses to a compact state on narrow widths

### 5.3 Note Item Card (per `mynotes.png`)
- Checkbox (toggle complete)
- Title (single or two lines, ellipsized)
- Optional date metadata row
- Star icon (toggle important)
- Three-dot more menu (edit, delete, etc.)
- Card padding and radius per the design

### 5.4 Bottom Input Bar (per `mynotes.png`)
- Pill-shaped input
- Calendar / bell icons (used to attach a date or reminder)
- Checkmark button (primary, enabled when input is non-empty)

### 5.5 Dialogs (per `createnewlist.png`, `renamelist.png`, `deletepermently.png`)
- Centered card
- Title, single input (or message), two buttons
- Backdrop dims the underlying page

### 5.6 Empty State (per `mynotes.png` and `search.png`)
- Centered icon
- Single short message
- Optional primary action

---

## 6. Design Tokens

All visual values are kept in `Resources/Styles/`. No hardcoded values in XAML or code-behind.

### 6.1 Colors
Categories follow the design images:
- **Primary** – used for primary actions, highlights, and the brand accent
- **Important** – used for the Important category
- **Reminder** – used for the Reminder category
- **Bin** – used for the Bin category and destructive actions
- **My Notes** – used for the default list, neutral
- **Text** – primary, secondary, tertiary text colors
- **Surface** – background, card, divider, border colors

Concrete hex values are captured in `Resources/Styles/Colors.xaml` and matched against the design images.

### 6.2 Spacing
Standard scale: 4, 8, 12, 16, 24, 32 dp.

### 6.3 Typography
Standard scale:
- Caption: 11sp
- Small: 12sp
- Label: 14sp
- Body: 16sp
- Title: 18sp
- Heading: 20sp
- Large: 24sp

### 6.4 Border Radius
- Small: 4 dp
- Medium: 8 dp
- Large: 16 dp
- Pill: 24 dp
- Full: 50% (avatars, round buttons)

### 6.5 Shadows
- Elevation 1 – small, resting cards
- Elevation 2 – raised cards, dialogs
- Elevation 3 – popups, menus

---

## 7. Screen-by-Screen Specifications

Each screen below is a one-page contract. The reference image is the source of truth for layout, spacing, colors, and typography. A deviation requires justification recorded in the task file.

### 7.1 My Notes (default)
**Reference:** `mynotes.png`

- Sidebar on the left, with **My Notes** active
- Header at the top: title "My Notes", search icon on the right
- Main content area:
  - Full-bleed background image (per the design)
  - List of note cards using `SfListView`
  - Empty state when there are no notes (per the empty state in `mynotes.png`)
- Bottom input bar fixed at the bottom

### 7.2 Important
**Reference:** `important.png`

- Sidebar with **Important** active
- Same shell as My Notes
- Main content shows only notes where `IsImportant == true`
- Empty state: "No important notes" (or per image text)

### 7.3 Reminder
**Reference:** `remainder.png`

- Sidebar with **Reminder** active
- Same shell as My Notes
- Main content shows only notes where a reminder date is set
- Empty state: "No reminders" (or per image text)
- **No local notifications fire** in this sample; the reminder is a date flag only

### 7.4 Bin
**Reference:** `bin.png`

- Sidebar with **Bin** active
- Same shell as My Notes
- Main content shows only soft-deleted notes
- Each item exposes a "Restore" action and a "Delete permanently" action
- Empty state: "Bin is empty" (or per image text)

### 7.5 Create List Dialog
**Reference:** `createnewlist.png`

- Title: "Create new list" (or per image text)
- Single text input
- Two buttons: cancel, create
- New list appears in the sidebar immediately after creation

### 7.6 Rename List Dialog
**Reference:** `renamelist.png`

- Title: "Rename list" (or per image text)
- Single text input, pre-populated with the current name
- Two buttons: cancel, save
- Sidebar updates immediately after save

### 7.7 Search
**Reference:** `search.png`

- Page with a single search input at the top
- Results list below (uses the same note card as the main list)
- Empty state: "You can search the created notes here" (or per image text)
- Case-insensitive title match

### 7.8 Permanent Delete Confirmation
**Reference:** `deletepermently.png`

- Title and message explaining that the action cannot be undone
- Two buttons: cancel, delete
- Delete removes the note from the in-memory store

### 7.9 Drag and Drop
**Reference:** `draganddrop.png`

- Long-press a note in any list to begin dragging
- Drop on the same list to reorder
- Drop on a different sidebar entry to move the note to that list

### 7.10 Today's Notes
**Reference:** `reviewtodayevents.png`

- Filtered view of notes whose due date is today (or as defined by the filter)
- Reuses the My Notes shell

### 7.11 Collapsed Sidebar
**Reference:** `collapsed.png`

- On narrow widths, the sidebar collapses to a compact representation
- Active state is still visible
- Tapping expands the sidebar

---

## 8. Data Models

Models are kept small and feature-scoped. The default sample uses an in-memory `ObservableCollection<Note>` seeded at startup.

### 8.1 Note
```csharp
public class Note
{
    public string Id { get; set; }            // GUID
    public string Title { get; set; }         // Required, max 500
    public DateTime? DueDate { get; set; }    // Optional
    public DateTime? ReminderDate { get; set; } // Optional, in-memory only
    public string? ListId { get; set; }       // Null = My Notes
    public bool IsImportant { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DeletedAt { get; set; }  // Soft delete
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 8.2 NoteList
```csharp
public class NoteList
{
    public string Id { get; set; }            // GUID
    public string Name { get; set; }          // Required, unique
    public bool IsSystem { get; set; }        // True for built-in lists
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 8.3 Built-In Lists (seeded)
- My Notes
- Important
- Reminder
- Bin

These are seeded at startup and cannot be deleted. They are the same set visible in the sidebar of every design image.

---

## 9. Architecture and MVVM

### 9.1 MVVM Strictly
- **Views (XAML)** contain only layout, bindings, and styles
- **ViewModels** own all UI state and commands
- **Models** are plain data types
- **Services** hold reusable logic for a single feature
- No logic in `*.xaml.cs`
- No shared application-wide state

### 9.2 Feature Scoping
```
Features/
├── Notes/         Views, ViewModels, Services, Models
├── Important/     Views, ViewModels
├── Reminder/      Views, ViewModels
├── Bin/           Views, ViewModels
├── CustomLists/   Views, ViewModels, Services
├── Search/        Views, ViewModels, Services
├── DragAndDrop/   (no dedicated folder; lives in Notes)
└── EditNote/      Views, ViewModels
```

### 9.3 Sample-Level Conventions
- A common `ViewModelBase` (with `INotifyPropertyChanged` only) lives in `Common/`
- No DI container; pass services through the constructor
- No global navigation service; pages navigate directly

### 9.4 Data Layer
- Default: in-memory `ObservableCollection<T>` seeded at startup
- Allowed: local JSON file in the app's data directory if a single task needs persistence
- Allowed: local SQLite **only** when a single task truly requires queries that JSON cannot support (none of the current tasks do)
- Never: remote APIs, cloud sync, internet-dependent data

---

## 10. Syncfusion Control Mapping

Each in-scope screen uses Syncfusion controls naturally. No custom controls are introduced where Syncfusion already provides a working control.

| UI need | Syncfusion control | Why | Customer scenario |
|---------|---------------------|-----|-------------------|
| Main notes list | `SfListView` | Built-in virtualization, item template support, drag and drop | Render a long list of notes smoothly on mobile |
| Date input on edit page | Syncfusion date picker (e.g. `SfDatePicker`) | Native-feel date selection | Customer picks a due date or reminder date |
| Note card | `SfListView` item template | Customizable card layout with bindings | Show title, metadata, actions per note |
| Sidebar | Standard MAUI layout | Simple, no Syncfusion equivalent needed | Navigate between lists |

Each task that introduces a Syncfusion control must document:
- The exact control used (with namespace)
- Why it was chosen over the alternative
- The customer scenario it demonstrates

---

## 11. Resource Structure

### 11.1 Project Layout
```
src/
├── App.xaml(.cs)
├── MauiProgram.cs
├── Common/
│   ├── ViewModelBase.cs
│   └── SampleData.cs
├── Features/
│   ├── Notes/
│   ├── Important/
│   ├── Reminder/
│   ├── Bin/
│   ├── CustomLists/
│   ├── Search/
│   └── EditNote/
└── Resources/
    ├── Styles/
    │   ├── Colors.xaml
    │   ├── Sizes.xaml
    │   ├── Fonts.xaml
    │   └── Styles.xaml
    └── Images/
```

### 11.2 Resource Dictionary Rules
- `Colors.xaml` – all color tokens, organized by category
- `Sizes.xaml` – spacing, radius, icon sizes
- `Fonts.xaml` – font family and size tokens
- `Styles.xaml` – typed styles (e.g. `PrimaryButtonStyle`, `CardStyle`, `SidebarItemStyle`)

No hardcoded values are allowed in XAML or code-behind. Every visual value resolves through `{StaticResource ...}`.

---

## 12. Validation and Error Handling

### 12.1 Validation Rules
- **Note title** – required, max 500 characters, trim whitespace
- **List name** – required, max 50 characters, unique
- **Date** – optional, not in the past (configurable per task)

### 12.2 Error Handling
- ViewModels handle user-recoverable errors with friendly messages
- No global error service; the relevant ViewModel surfaces its own errors
- No silent failures; every error path is visible to the user

### 12.3 Test / Verify Locally
Every task includes a short list of manual verification steps the reviewer can run.

---

## 13. Accessibility

- Minimum 12sp font size on all text
- 4.5:1 contrast for text on background
- 48x48 dp minimum touch target
- Visible focus indicators for keyboard users
- Semantic labels on every interactive element
- No information conveyed by color alone

---

## 14. Responsive Design

| Width | Sidebar | Notes |
|-------|---------|-------|
| < 600 px | Collapsed per `collapsed.png` | Single column, sidebar expands on tap |
| 600-1024 px | Visible, condensed | Two-column layout |
| >= 1025 px | Visible, full | Wide layout, content centered with max width |

The reference image set shows the wide layout; the collapsed state is its own reference (`collapsed.png`).

---

## Appendix A – Out-of-Scope Reminder

The following items are **not** part of this sample. They must not be implemented, even if a future contributor is tempted to add them:

- Sign-in, sign-up, forgot-password, sign-out
- Splash screens, onboarding, settings
- Profile pages, account management
- Backend APIs, cloud sync, remote databases
- Push notifications, scheduled reminders, OS notifications
- User accounts, sessions, tokens, SecureStorage
- Global application services, DI containers, shared application state
- Production-scale infrastructure

The sample is intentionally **lighter** than a real product so a customer can read it end-to-end in minutes.

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| 2.0 | Oct 2026 | Removed auth, profile, splash, settings, account, and sign-out screens. Aligned to the in-scope design image set. Added Gate-Based Harness language. |
| 1.0 | Oct 2026 | Initial specification (deprecated by 2.0) |

---

**END OF SPECIFICATION**
