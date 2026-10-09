# MAUI To Do List Application - Comprehensive Specification

**Version:** 1.0  
**Status:** Ready for Implementation  
**Last Updated:** 2024  
**Architecture:** MVVM + Layered Services  
**Framework:** .NET MAUI with Syncfusion Controls  

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Overview](#project-overview)
3. [Design Tokens & Visual System](#design-tokens--visual-system)
4. [Screen Specifications](#screen-specifications)
5. [Navigation Flow](#navigation-flow)
6. [Data Models](#data-models)
7. [MVVM Architecture](#mvvm-architecture)
8. [Service Layer](#service-layer)
9. [Syncfusion Control Mapping](#syncfusion-control-mapping)
10. [Resource Dictionary Structure](#resource-dictionary-structure)
11. [Component Reusability](#component-reusability)
12. [Validation Rules](#validation-rules)
13. [State Management](#state-management)
14. [Design System & Tokens](#design-system--tokens)

---

## Executive Summary

The MAUI To Do List Application is a modern, feature-rich task management solution built with .NET MAUI and Syncfusion controls. The application follows MVVM architecture with clear separation of concerns, reusable components, and a responsive, accessible user interface.

**Key Features:**
- Authentication (Sign In, Sign Up, Forgot Password)
- Task Management (Create, Read, Update, Delete)
- Smart Organization (My Notes, Important, Reminders, Bin, Custom Lists)
- Advanced Features (Recurring Tasks, Due Dates, Search, Filters)
- Rich Notifications (Snackbars, Dialogs, Confirmations)
- Local Data Persistence

**Technology Stack:**
- .NET MAUI (Cross-platform UI)
- Syncfusion .NET MAUI controls (Advanced UI components)
- SQLite (Local data persistence)
- MVVM Community Toolkit (ViewModel base classes)
- Dependency Injection (Microsoft.Extensions.DependencyInjection)

---

## Project Overview

### Application Scope

The application provides a complete task management experience with:
- **Authentication Module:** Multi-step sign-up, login, and password recovery
- **Task Module:** Create, read, update, delete tasks with metadata
- **Organization Module:** Categorize tasks into lists and categories
- **Smart Views:** Important, Reminders, Bin with filtering and actions
- **User Experience:** Smooth navigation, responsive design, rich feedback

### Target Platforms

- iOS (minimum 12.0)
- Android (minimum API 27)
- Windows (minimum Windows 10)
- macOS (future consideration)

### Development Approach

**MVVM Pattern:**
```
View (XAML)
    ↓
ViewModel (Logic + State)
    ↓
Model (Data)
    ↓
Services (Business Logic + Persistence)
```

**Layered Architecture:**
```
Presentation Layer (Views + ViewModels)
    ↓
Business Logic Layer (Services)
    ↓
Data Layer (Repository + SQLite)
    ↓
Entities (Data Models)
```

---

## Design Tokens & Visual System

### Color Palette

| Token | Value | Usage |
|-------|-------|-------|
| `ColorPrimary` | `#6366F1` (Indigo) | Primary actions, highlights |
| `ColorSecondary` | `#EC4899` (Pink) | Secondary actions |
| `ColorSuccess` | `#10B981` (Emerald) | Success states, checkmarks |
| `ColorWarning` | `#F59E0B` (Amber) | Warnings, alerts |
| `ColorError` | `#EF4444` (Red) | Errors, destructive actions |
| `ColorInfo` | `#3B82F6` (Blue) | Info messages |
| `ColorBackground` | `#FFFFFF` | Primary background |
| `ColorSurface` | `#F9FAFB` (Gray-50) | Card surfaces, lists |
| `ColorText` | `#1F2937` (Gray-900) | Primary text |
| `ColorTextSecondary` | `#6B7280` (Gray-500) | Secondary text, hints |
| `ColorBorder` | `#E5E7EB` (Gray-200) | Borders, dividers |
| `ColorDisabled` | `#D1D5DB` (Gray-300) | Disabled states |
| `ColorOverlay` | `rgba(0,0,0,0.5)` | Modal overlays |

### Typography

| Token | Font Size | Weight | Line Height | Usage |
|-------|-----------|--------|-------------|-------|
| `HeadingXL` | 32px | Bold (700) | 40px | Page titles |
| `HeadingLG` | 28px | Bold (700) | 36px | Section titles |
| `HeadingMD` | 24px | Semibold (600) | 32px | Card titles |
| `HeadingSM` | 20px | Semibold (600) | 28px | Subsection titles |
| `BodyLG` | 18px | Regular (400) | 28px | Primary body text |
| `BodyMD` | 16px | Regular (400) | 24px | Standard body text |
| `BodySM` | 14px | Regular (400) | 20px | Secondary body text |
| `LabelLG` | 14px | Semibold (600) | 20px | Button labels |
| `LabelMD` | 13px | Semibold (600) | 19px | Input labels |
| `LabelSM` | 12px | Semibold (600) | 18px | Badge labels |
| `CaptionMD` | 12px | Regular (400) | 18px | Captions, hints |
| `CaptionSM` | 11px | Regular (400) | 16px | Timestamps, metadata |

**Font Family:** 
- Default: Platform-specific system font (San Francisco on iOS, Roboto on Android, Segoe UI on Windows)
- Fallback: `sans-serif`

### Spacing System

| Token | Value | Usage |
|-------|-------|-------|
| `Spacing0` | 0px | No spacing |
| `Spacing1` | 4px | Micro spacing |
| `Spacing2` | 8px | Small spacing |
| `Spacing3` | 12px | Compact spacing |
| `Spacing4` | 16px | Standard spacing |
| `Spacing5` | 20px | Medium spacing |
| `Spacing6` | 24px | Large spacing |
| `Spacing7` | 32px | Extra large spacing |
| `Spacing8` | 40px | XXL spacing |

### Border & Corner Radius

| Token | Value | Usage |
|-------|-------|-------|
| `CornerRadiusSM` | 4px | Small elements (badges, icons) |
| `CornerRadiusMD` | 8px | Standard elements (inputs, buttons) |
| `CornerRadiusLG` | 12px | Cards, containers |
| `CornerRadiusXL` | 16px | Large containers |
| `BorderWidthThin` | 1px | Standard borders |
| `BorderWidthMD` | 2px | Emphasis borders |
| `BorderWidthThick` | 4px | Focus states |

### Shadow System

| Token | Value | Usage |
|-------|-------|-------|
| `ShadowElevation1` | 0 2px 4px rgba(0,0,0,0.08) | Subtle elevation |
| `ShadowElevation2` | 0 4px 8px rgba(0,0,0,0.12) | Card elevation |
| `ShadowElevation3` | 0 8px 16px rgba(0,0,0,0.16) | Modal elevation |
| `ShadowElevation4` | 0 16px 32px rgba(0,0,0,0.20) | Floating action button |

---

## Screen Specifications

### 1. Splash Screen

**Purpose:** App initialization, branding display

**Layout Structure:**
```
┌─────────────────────────────┐
│                             │
│        [Logo Image]         │  Centered, 120x120px
│                             │
│      App Name / Slogan      │  Typography: HeadingLG, 40px margin-top
│                             │
│                             │  (Optional: Loading indicator)
│                             │
│  [Loading Bar or Spinner]   │  Syncfusion: SfBusyIndicator
│                             │
└─────────────────────────────┘
```

**Components:**
- Logo Image (120x120px, centered)
- App Title (Syncfusion SparklineText for dynamic effects - optional)
- Loading Indicator (Syncfusion SfBusyIndicator)
- Branding Subtitle

**Behavior:**
- Display for 2-3 seconds
- Auto-navigate to Sign In or Dashboard based on authentication state
- Preload critical resources
- Show loading animation during initialization

**Responsive Behavior:**
- Centered on all screen sizes
- Scales logo proportionally (80-120px based on device)
- Maintains vertical centering

**States:**
- Loading
- Complete

**XAML Considerations:**
- Use VerticalStackLayout for centering
- Absolute positioning for logo
- Grid for layout structure

---

### 2. Sign In Screen

**Purpose:** User authentication into the application

**Layout Structure:**
```
┌─────────────────────────────────────┐
│                                     │
│  ← Back Button (top-left)           │
│                                     │
│  Welcome Back!                      │  Typography: HeadingLG
│  Sign in to your account            │  Typography: BodyMD, ColorTextSecondary
│                                     │
│  ┌───────────────────────────────┐  │
│  │ Email Address                 │  │  Syncfusion: SfTextInputFieldOutline
│  │ [                           ] │  │  Placeholder: "your@email.com"
│  └───────────────────────────────┘  │  Icon: Left (mail icon)
│                                     │
│  ┌───────────────────────────────┐  │
│  │ Password                      │  │  Syncfusion: SfTextInputFieldOutline
│  │ [                           ] │  │  Input type: Password
│  │                            👁 │  │  Icon: Right (eye toggle)
│  └───────────────────────────────┘  │
│                                     │
│                      Forgot Password? →  Typography: LabelSM, ColorPrimary
│                                     │  Right-aligned link
│                                     │
│  ┌───────────────────────────────┐  │
│  │     SIGN IN                   │  │  Syncfusion: SfButton
│  └───────────────────────────────┘  │  Primary style, Full width
│                                     │  CornerRadiusMD
│                                     │
│              OR                     │  Typography: CaptionMD
│                                     │
│  ┌─────────────────────────────┐    │
│  │ [ Google ]  [ Apple ]       │    │  Syncfusion: SfButton (secondary)
│  └─────────────────────────────┘    │  Icons with labels
│                                     │
│  Don't have an account?             │  Typography: BodySM
│  Sign Up →                          │  ColorPrimary link
│                                     │
└─────────────────────────────────────┘
```

**Components:**
- Back Navigation Button
- Title & Subtitle Text
- Email Input Field (Syncfusion SfTextInputFieldOutline)
- Password Input Field (Syncfusion SfTextInputFieldOutline with toggle)
- "Forgot Password?" Link
- Sign In Button (Syncfusion SfButton)
- Social Sign-In Buttons (Google, Apple)
- Sign Up Link

**Behavior:**
- Email validation (format + existence check)
- Password field toggle (show/hide)
- Loading state on button click
- Error message display on failure
- Keyboard handling (email → password → sign in)
- Remember me toggle (optional)

**Validation:**
- Email: Required, valid format (RFC 5322)
- Password: Required, min 6 characters
- Both fields must be non-empty to enable Sign In button

**Responsive Behavior:**
- Stack on mobile (full width inputs)
- Maintain fixed width on tablet (400-500px)
- Add side margins on large screens
- Social buttons: 1 column on mobile, 2+ columns on tablet

**States:**
- Empty (Sign In button disabled)
- Valid (Sign In button enabled)
- Loading (button shows spinner)
- Error (shows error message below field)
- Focus (input highlight)

**Navigation:**
- Back: Return to previous screen or Splash
- Forgot Password: Navigate to Forgot Password screen
- Sign Up: Navigate to Sign Up screen
- Success: Navigate to Dashboard/My Notes

---

### 3. Sign Up Screen

**Purpose:** New user account creation

**Layout Structure:**
```
┌─────────────────────────────────────┐
│                                     │
│  ← Back Button                      │
│                                     │
│  Create Account                     │  Typography: HeadingLG
│  Join us today!                     │  Typography: BodyMD, ColorTextSecondary
│                                     │
│  ┌───────────────────────────────┐  │
│  │ Full Name                     │  │  Syncfusion: SfTextInputFieldOutline
│  │ [                           ] │  │  Placeholder: "John Doe"
│  └───────────────────────────────┘  │
│                                     │
│  ┌───────────────────────────────┐  │
│  │ Email Address                 │  │  Syncfusion: SfTextInputFieldOutline
│  │ [                           ] │  │  Placeholder: "your@email.com"
│  └───────────────────────────────┘  │
│                                     │
│  ┌───────────────────────────────┐  │
│  │ Password                      │  │  Syncfusion: SfTextInputFieldOutline
│  │ [                           ] │  │  Password type, eye toggle
│  │                            👁 │  │
│  └───────────────────────────────┘  │
│  Password strength indicator        │  Syncfusion: SfProgressBar (visual)
│  Weak  ████░░░░░░░░                │  Color-coded feedback
│                                     │
│  ┌───────────────────────────────┐  │
│  │ Confirm Password              │  │  Syncfusion: SfTextInputFieldOutline
│  │ [                           ] │  │  Password type, eye toggle
│  │                            👁 │  │
│  └───────────────────────────────┘  │
│                                     │
│  ☑ I agree to Terms & Conditions   │  Checkbox + Link
│                                     │
│  ┌───────────────────────────────┐  │
│  │     CREATE ACCOUNT            │  │  Syncfusion: SfButton
│  └───────────────────────────────┘  │
│                                     │
│  Already have an account?           │  Typography: BodySM
│  Sign In →                          │
│                                     │
└─────────────────────────────────────┘
```

**Components:**
- Back Navigation Button
- Title & Subtitle Text
- Full Name Input Field (Syncfusion SfTextInputFieldOutline)
- Email Input Field (Syncfusion SfTextInputFieldOutline)
- Password Input Field (Syncfusion SfTextInputFieldOutline)
- Password Strength Indicator (Syncfusion SfProgressBar)
- Confirm Password Input Field (Syncfusion SfTextInputFieldOutline)
- Terms & Conditions Checkbox
- Create Account Button (Syncfusion SfButton)
- Sign In Link

**Behavior:**
- Real-time password strength calculation
- Password confirmation validation
- Email uniqueness check
- Form validation on input
- Loading state during submission
- Success message with redirect
- Error messages for duplicate email or validation failure

**Validation:**
- Full Name: Required, 2-50 characters
- Email: Required, valid format, unique in database
- Password: Required, min 8 characters, must contain uppercase + lowercase + number + symbol
- Confirm Password: Required, must match password field
- Terms: Required (must be checked)

**Responsive Behavior:**
- Full-width inputs on mobile
- Fixed width (400-500px) on tablet
- Centered on large screens
- Password strength indicator adjusts width

**States:**
- Empty
- Partial (some fields filled)
- Invalid (validation errors displayed)
- Valid (button enabled)
- Loading (button shows spinner)
- Success (redirect after brief confirmation)

**Navigation:**
- Back: Return to Sign In
- Sign In Link: Navigate to Sign In
- Success: Auto-navigate to Dashboard or Sign In with confirmation

---

### 4. Forgot Password Screen

**Purpose:** Password recovery flow

**Layout Structure:**
```
┌─────────────────────────────────────┐
│                                     │
│  ← Back Button                      │
│                                     │
│  Forgot Password?                   │  Typography: HeadingLG
│  Enter your email to reset          │  Typography: BodyMD, ColorTextSecondary
│                                     │
│  ┌───────────────────────────────┐  │
│  │ Email Address                 │  │  Syncfusion: SfTextInputFieldOutline
│  │ [                           ] │  │  Placeholder: "your@email.com"
│  └───────────────────────────────┘  │
│                                     │
│  ┌───────────────────────────────┐  │
│  │     SEND RESET LINK           │  │  Syncfusion: SfButton
│  └───────────────────────────────┘  │
│                                     │
│  Remember your password?            │  Typography: BodySM
│  Sign In →                          │
│                                     │
└─────────────────────────────────────┘
```

**Components:**
- Back Navigation Button
- Title & Subtitle Text
- Email Input Field (Syncfusion SfTextInputFieldOutline)
- Send Reset Link Button (Syncfusion SfButton)
- Sign In Link

**Behavior:**
- Email validation
- Send reset link to email (mock or real backend)
- Success message confirmation
- Loading state during submission
- Error handling for non-existent email

**Validation:**
- Email: Required, valid format

**Navigation:**
- Back: Return to Sign In
- Sign In Link: Navigate to Sign In
- Success: Show confirmation message, redirect after 3 seconds

---

### 5. My Notes / Dashboard Screen

**Purpose:** Primary task list view, displays all user tasks

**Layout Structure:**
```
┌─────────────────────────────────────────────────┐
│                                                 │
│  ← Back (if modal)  My Notes          ⋮ Menu   │  Header: Navigation, Menu
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │  [ Search Tasks ]                    🔍   │  │  Search bar (Syncfusion SfTextInputFieldOutline)
│  └───────────────────────────────────────────┘  │
│                                                 │
│  Sort: Recent ▼  |  Filter: All Tasks ▼        │  Sort/Filter controls
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ ☐ Task Title 1                        ⋮  │  │  Task card (Syncfusion SfListView)
│  │   Due: Feb 15, 2024                       │  │  Left: Checkbox, Right: More menu
│  │   📌 Important  🔔 Reminder 1 day left   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ ☐ Task Title 2 - Longer description... │  │  Card: Truncated text
│  │   Due: Today at 2:00 PM                   │  │  Icons for features
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ ☐ Task Title 3                        ⋮  │  │
│  │   No due date                             │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  +  Add New Task (FAB - Syncfusion SfButton)    │  FAB: Floating Action Button
│                                                 │
└─────────────────────────────────────────────────┘
```

**Components:**
- Navigation Header (Back button + Title + Menu button)
- Search Input Field (Syncfusion SfTextInputFieldOutline)
- Sort Dropdown (Syncfusion SfComboBox or custom)
- Filter Dropdown (Syncfusion SfComboBox or custom)
- Task List (Syncfusion SfListView)
  - Each task item: Checkbox, Title, Description, Due Date, Icons, Menu Button
- Empty State Message (when no tasks)
- Floating Action Button - "Add Task" (Syncfusion SfButton)

**Behavior:**
- Display all tasks from "My Notes" list
- Real-time checkbox toggle (task completion)
- Swipe-to-delete or menu actions (Edit, Delete, Move)
- Search filters tasks in real-time
- Sort options: Recent, Oldest, Alphabetical, By Due Date
- Filter options: All Tasks, Completed, Incomplete, Overdue
- Tap task → Task Details modal/page
- Tap ⋮ → Context menu (Edit, Delete, Duplicate, Move to List)
- Tap + → Add New Task

**States:**
- Empty list
- Loading
- Loaded with tasks
- Task completed (visual feedback, strikethrough)
- Task hovered/selected
- Search active
- Filter active

**Responsive Behavior:**
- Mobile: Full-width list, single column
- Tablet: Single or two-column layout
- Desktop: Multiple columns with expanded details (optional)

**Navigation:**
- Tap task → Task Details
- Tap + FAB → Task Creation
- Menu → Navigation to other lists/views
- Back → Previous screen or exit

---

### 6. Important Screen

**Purpose:** Display tasks marked as important

**Layout Structure:**
Similar to My Notes, but:
- Title: "Important"
- Automatically filtered to show only tasks with `isImportant = true`
- Default sort: Most recent important tasks
- Filter options limited to: All Important, Completed, Incomplete

```
┌─────────────────────────────────────────────────┐
│                                                 │
│  ← Back         Important           ⋮ Menu     │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │  [ Search Important Tasks ]          🔍   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  Sort: Recent ▼  |  Filter: All Tasks ▼        │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ ☐ 📌 Important Task 1               ⋮    │  │
│  │   Due: Feb 15, 2024                       │  │
│  │   🔔 Reminder 1 day left                  │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  [More task items...]                          │
│                                                 │
│  +  Add New Task                               │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Components:**
- Same as My Notes, but filtered view

**Behavior:**
- Display only tasks with `isImportant = true`
- All other interactions same as My Notes
- Can unmark as important via context menu

---

### 7. Reminder Screen

**Purpose:** Display tasks with active reminders

**Layout Structure:**
Similar to My Notes, filtered to show only tasks with reminders:
- Title: "Reminder"
- Filter: Tasks with `hasReminder = true`
- Sort options: By reminder time, by due date

```
┌─────────────────────────────────────────────────┐
│                                                 │
│  ← Back         Reminders            ⋮ Menu    │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │  [ Search Tasks with Reminders ]     🔍   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  Sort: By Time ▼  |  Filter: All ▼             │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ ☐ Task with Reminder 1                ⋮  │  │
│  │   Reminder: Tomorrow 9:00 AM              │  │
│  │   Due: Feb 15, 2024                       │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  [More task items...]                          │
│                                                 │
│  +  Add New Task                               │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Components:**
- Same as My Notes, but filtered

**Behavior:**
- Display only tasks with active reminders
- Highlight reminder time prominently
- All other interactions same as My Notes

---

### 8. Bin / Trash Screen

**Purpose:** Display deleted tasks, allow restore or permanent delete

**Layout Structure:**
```
┌─────────────────────────────────────────────────┐
│                                                 │
│  ← Back          Bin / Trash          ⋮ Menu   │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │  [ Search Deleted Tasks ]            🔍   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  Sort: Recently Deleted ▼                      │
│                                                 │
│  Delete All Permanently (if trash not empty)   │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ ☐ Deleted Task 1                     ⋮   │  │
│  │   Deleted 2 days ago                      │  │
│  │   Restore  |  Delete Permanently          │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  [More deleted task items...]                  │
│                                                 │
│  (No FAB on Bin view)                          │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Components:**
- Navigation Header
- Search Input
- Sort Dropdown (default: Recently Deleted)
- Delete All Button (destructive, red)
- Task List (deleted tasks only)
  - Each item shows deletion date
  - Action buttons: Restore, Delete Permanently
- Empty state for empty trash

**Behavior:**
- Display tasks with `isDeleted = true`
- Show deletion date/time for each task
- Restore action: Move task back to original list, set `isDeleted = false`
- Delete Permanently action: Remove from database entirely (with confirmation)
- Delete All action: Permanent delete all tasks in trash (with confirmation)
- Search filters deleted tasks

**States:**
- Empty trash
- With deleted tasks
- Confirmation dialogs for destructive actions

**Navigation:**
- Back: Return to previous screen
- Restore: Move task back to source list
- Delete Permanently: Remove task with confirmation

---

### 9. Custom Lists Screen

**Purpose:** Display all custom lists created by user, manage lists

**Layout Structure:**
```
┌─────────────────────────────────────────────────┐
│                                                 │
│  ← Back        My Lists / Custom Lists ⋮      │
│                                                 │
│  Built-in Lists:                               │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ 📋 My Notes               15 tasks    →   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ ⭐ Important               8 tasks     →   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ 🔔 Reminders              3 tasks     →   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ 🗑️  Bin                   5 tasks     →   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  Custom Lists:                                 │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ 🎓 Projects               2 tasks     ⋮   │  │  Tap → Open list
│  └───────────────────────────────────────────┘  │  Long-press → Edit/Delete
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ 🏥 Health                 5 tasks     ⋮   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ 🎨 Creative               0 tasks     ⋮   │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  +  Create New List                            │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Components:**
- Navigation Header
- Built-in Lists Section (not editable)
  - My Notes list
  - Important list
  - Reminders list
  - Bin list
- Custom Lists Section
  - Each list item with icon, name, task count, menu
- Create New List Button (Syncfusion SfButton)

**Behavior:**
- Display all lists with task counts
- Tap list → Open list view (My Notes template)
- Long-press/menu on custom list → Edit or Delete
- Create New List → Modal dialog (list name, icon, color)
- List icon selection (emoji or icon picker - Syncfusion SfComboBox)
- List color selection (color picker)
- Delete list with confirmation (option to move tasks to another list)

**States:**
- Empty custom lists
- Multiple lists
- List edit mode
- List creation mode
- Confirmation dialogs

**Navigation:**
- Tap list → List view
- Back → Previous screen
- Create New List → Modal dialog
- Edit List → Modal dialog

---

### 10. Task Creation Screen

**Purpose:** Create new task with full details

**Layout Structure:**
```
┌─────────────────────────────────────────────────┐
│                                                 │
│  ← Back    New Task           ✓ Save           │  Header: Back, Title, Save button
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ Task Title                                │  │  Syncfusion: SfTextInputFieldOutline
│  │ [                                       ] │  │  Placeholder: "Enter task title"
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │ Description (Optional)                    │  │  Syncfusion: SfTextInputFieldOutline
│  │ [                                       ] │  │  Multi-line, Placeholder text
│  │ [                                       ] │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  Add to List:                                  │  Dropdown or segmented control
│  ▾ My Notes                                    │  Syncfusion: SfComboBox
│                                                 │
│  Due Date:                                     │  Toggle + Date picker
│  ☐ No Due Date                                 │  Syncfusion: SfDatePicker
│  ☑ 📅 Feb 15, 2024                             │
│                                                 │
│  Due Time:                                     │  Time picker (if date selected)
│  ☑ 🕐 2:00 PM                                  │  Syncfusion: SfTimePicker
│                                                 │
│  Reminder:                                     │  Checkbox + dropdown
│  ☑ 🔔 Remind Me                                │  "1 day before", "1 hour before", etc.
│    ▾ 1 day before                              │
│                                                 │
│  Repeat:                                       │  Checkbox + dropdown for recurring
│  ☑ ♻️ Repeat                                   │  Options: Daily, Weekly, Monthly, Yearly
│    ▾ Weekly                                    │
│                                                 │
│  Important:                                    │  Checkbox for marking important
│  ☐ 📌 Mark as Important                        │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │           SAVE TASK                       │  │  Primary button
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │           CANCEL                          │  │  Secondary button
│  └───────────────────────────────────────────┘  │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Components:**
- Navigation Header (Back, Title, Save button)
- Task Title Input (Syncfusion SfTextInputFieldOutline, required)
- Description Input (Syncfusion SfTextInputFieldOutline, optional, multi-line)
- List Selector (Syncfusion SfComboBox, default: My Notes)
- Due Date Toggle + Date Picker (Syncfusion SfDatePicker)
- Due Time Toggle + Time Picker (Syncfusion SfTimePicker, optional)
- Reminder Toggle + Options (Syncfusion SfComboBox)
- Repeat Toggle + Options (Syncfusion SfComboBox)
- Important Toggle (Checkbox)
- Save Button (Syncfusion SfButton, primary)
- Cancel Button (Syncfusion SfButton, secondary)

**Behavior:**
- Title is required (Save button disabled until filled)
- Date/Time optional
- When due date selected, time picker becomes available
- Reminder/Repeat toggles enable their respective dropdowns
- Save creates new Task entity in database
- Cancel returns to previous screen without saving
- Validation: Title non-empty, date not in past (optional warning)

**Validation:**
- Task Title: Required, 1-200 characters
- Description: Optional, 0-1000 characters
- Due Date: Optional, must be valid date
- Due Time: Optional, must be valid time if date selected
- Reminder: Valid only if due date selected
- Repeat: Valid selection

**Responsive Behavior:**
- Mobile: Full-width form, scrollable
- Tablet: Centered with 400-600px width
- Desktop: Similar to tablet

**States:**
- Empty (Save disabled)
- Partial (some fields filled)
- Valid (Save enabled)
- Loading (Save button shows spinner)
- Success (auto-close after creation)

**Navigation:**
- Back: Return without saving (optional confirmation if fields filled)
- Cancel: Return without saving
- Save: Create task and return to list view

---

### 11. Task Editing Screen

**Purpose:** Modify existing task details

**Layout Structure:**
Same as Task Creation with:
- Title: "Edit Task" instead of "New Task"
- All fields pre-populated with current task data
- Delete button (optional, with confirmation)

```
┌─────────────────────────────────────────────────┐
│                                                 │
│  ← Back    Edit Task          ✓ Save           │
│                                                 │
│  [Same form fields as Task Creation]           │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │           DELETE TASK                     │  │  Delete button (red)
│  └───────────────────────────────────────────┘  │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Components:**
- Same as Task Creation
- Additional: Delete Task Button (Syncfusion SfButton, destructive/red)

**Behavior:**
- All fields pre-filled
- Save updates task in database
- Delete removes task or moves to Bin
- Cancel returns without saving
- Validation same as creation

**Navigation:**
- Save: Update task and return to list
- Delete: Move to Bin with confirmation
- Cancel/Back: Return without saving

---

### 12. Task Details Modal/Page

**Purpose:** View complete task information, quick actions

**Layout Structure:**
```
┌─────────────────────────────────────────────────┐
│                                                 │
│  ← Back / X Close        Edit    ⋮ More       │  Header with close + edit + menu
│                                                 │
│  ☐ Task Title                                  │  Checkbox for completion
│                                                 │  Typography: HeadingMD
│                                                 │
│  Description (if exists):                      │
│  Full description text here...                 │
│                                                 │
│  📋 List: My Notes                             │  List badge
│  📅 Due: Feb 15, 2024 at 2:00 PM               │  Due date/time
│  🔔 Reminder: 1 day before                     │  Reminder info
│  ♻️ Repeat: Weekly                             │  Repeat pattern
│  📌 Important: Yes                             │  Importance status
│                                                 │
│  Created: Feb 1, 2024                          │  Metadata
│  Updated: Feb 5, 2024                          │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │              MARK COMPLETE                │  │  Primary action (if not done)
│  └───────────────────────────────────────────┘  │  OR "UNDO COMPLETE" if done
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │          MOVE TO ANOTHER LIST             │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
│  ┌───────────────────────────────────────────┐  │
│  │              DELETE                       │  │
│  └───────────────────────────────────────────┘  │
│                                                 │
└─────────────────────────────────────────────────┘
```

**Components:**
- Close button (top-left X or back arrow)
- Edit button (pencil icon)
- More menu button (⋮)
- Checkbox to mark complete/incomplete
- Task title display
- Description display
- Metadata display (list, due date, time, reminder, repeat, important, created, updated)
- Action buttons: Mark Complete, Move to List, Delete

**Behavior:**
- Display all task information in read-only mode
- Checkbox toggles completion state
- Edit button → Navigate to Task Editing screen
- More menu → Delete, Duplicate, Move to List
- Close button → Return to previous screen
- Mark Complete button → Toggle completion state

**Responsive Behavior:**
- Mobile: Full-screen modal or slide-up
- Tablet: Modal dialog, centered
- Desktop: Modal dialog, larger

**States:**
- Task incomplete (Mark Complete button visible)
- Task complete (Undo Complete button visible)
- Editing menu open
- Deleting (confirmation)

**Navigation:**
- Close: Return to task list
- Edit: Go to edit screen
- Mark Complete: Update state, stay on details
- Delete: Move to Bin with confirmation
- More menu: Additional actions

---

### 13. Task Context Menu

**Purpose:** Quick actions on task cards without opening details

**Layout Structure:**
```
When ⋮ button tapped on task card:

┌──────────────────┐
│ Edit             │  → Navigate to edit screen
│ Mark Important   │  → Toggle importance (show ✓ if already)
│ Duplicate        │  → Create copy of task
│ Move to List     │  → Submenu or modal to select list
│ Delete           │  → Move to Bin with confirmation
└──────────────────┘
```

**Components:**
- Context Menu (Syncfusion popup or custom)
- Menu items: Edit, Mark Important, Duplicate, Move, Delete

**Behavior:**
- Tap ⋮ on task card → Show context menu
- Tap menu item → Perform action and close menu
- Tap outside menu → Close menu without action
- Visual feedback for toggles (Important: shows/hides 📌 icon)

**States:**
- Menu hidden
- Menu visible
- Menu item hovered/pressed
- Important toggled (visual feedback)

**Navigation:**
- Edit: Open edit screen
- Mark Important: Toggle and close menu
- Duplicate: Create copy and close menu
- Move: Open list selector
- Delete: Show confirmation

---

### 14. Snackbar / Toast Notifications

**Purpose:** Provide brief, non-blocking feedback to user actions

**Layout Structure:**
```
Bottom of screen:

┌─────────────────────────────────────────┐
│  ✓ Task completed successfully           │  Success notification
│                           [UNDO]         │  Optional undo action
└─────────────────────────────────────────┘

OR

┌─────────────────────────────────────────┐
│  ! Task deleted. Moved to Bin.           │  Info notification
└─────────────────────────────────────────┘

OR

┌─────────────────────────────────────────┐
│  ✕ Error: Unable to save task           │  Error notification
└─────────────────────────────────────────┘
```

**Components:**
- Snackbar container (Syncfusion or custom)
- Icon (based on type: ✓, !, ✕)
- Message text
- Optional action button (Undo, Retry, etc.)

**Behavior:**
- Display for 3-5 seconds by default
- Automatically dismiss after timeout
- User can dismiss by swiping or tapping close
- Optional action button (Undo for delete, Retry for error)
- Stack multiple notifications (limited queue)

**Types:**
- Success: Green background, ✓ icon
- Info: Blue background, ℹ icon
- Warning: Amber background, ⚠ icon
- Error: Red background, ✕ icon

**States:**
- Appearing (slide up animation)
- Visible
- Dismissing (fade out or slide down)

---

### 15. Confirmation Dialogs

**Purpose:** Confirm destructive actions before execution

**Layout Structure:**
```
┌──────────────────────────────────┐
│                                  │
│  Delete Task?                    │  Title: Typography HeadingMD
│                                  │
│  This action cannot be undone.   │  Message: Typography BodyMD
│  The task will be moved to Bin.  │
│                                  │
│  ┌──────────────┐  ┌──────────┐  │
│  │   CANCEL     │  │  DELETE  │  │  Buttons: Cancel (gray), Delete (red)
│  └──────────────┘  └──────────┘  │
│                                  │
└──────────────────────────────────┘
```

**Components:**
- Dialog overlay (semi-transparent background)
- Title text
- Message text
- Cancel button (secondary)
- Confirm button (primary, color-coded by action)

**Behavior:**
- Display when destructive action is triggered
- Cancel button → Close dialog without action
- Confirm button → Execute action and close
- Tap outside → Close without action (optional)
- Auto-focus on Cancel button (safe default)

**Types:**
- Delete Task
- Delete List
- Empty Trash
- Permanent Delete
- Discard Changes (optional)

**Responsive Behavior:**
- Mobile: Full-width dialog, bottom-aligned
- Tablet: Centered, fixed width (400-500px)
- Desktop: Centered, fixed width

**States:**
- Hidden
- Visible
- Button focus/hover
- Action in progress (spinner on confirm button)

---

### 16. Popups & Modals

**Purpose:** Display forms, pickers, and content overlays

**Modals Included:**

#### a) Create/Edit List Modal
```
┌──────────────────────────────────┐
│                                  │
│  New List                        │  Title
│                                  │
│  ┌──────────────────────────────┐│
│  │ List Name                    ││  Input field
│  │ [                          ] ││
│  └──────────────────────────────┘│
│                                  │
│  Select Icon:                    │  Icon picker (emoji or icon grid)
│  😀 📋 ✓ ⭐ 🎓 🏥 🎨 ...         │  Syncfusion: SfComboBox or custom grid
│                                  │
│  Select Color:                   │  Color picker
│  [●] [●] [●] [●] [●]             │  Color swatches
│                                  │
│  ┌──────────────┐  ┌──────────┐  │
│  │   CANCEL     │  │  CREATE  │  │
│  └──────────────┘  └──────────┘  │
│                                  │
└──────────────────────────────────┘
```

#### b) Move Task Modal
```
┌──────────────────────────────────┐
│  Move Task to...                 │
│                                  │
│  ☑ My Notes                      │  Checkbox list
│  ☐ Important                     │  Current list checked
│  ☐ Reminders                     │
│  ☐ Projects                      │
│  ☐ Health                        │
│                                  │
│  ┌──────────────┐  ┌──────────┐  │
│  │   CANCEL     │  │  MOVE    │  │
│  └──────────────┘  └──────────┘  │
│                                  │
└──────────────────────────────────┘
```

#### c) Reminder Options Modal
```
┌──────────────────────────────────┐
│  Set Reminder                    │
│                                  │
│  ○ 15 minutes before             │  Radio buttons
│  ○ 1 hour before                 │
│  ● 1 day before                  │  Currently selected
│  ○ 2 days before                 │
│  ○ 1 week before                 │
│  ○ Custom time                   │
│                                  │
│  ┌──────────────┐  ┌──────────┐  │
│  │   CANCEL     │  │  CONFIRM │  │
│  └──────────────┘  └──────────┘  │
│                                  │
└──────────────────────────────────┘
```

**Components:**
- Modal overlay (semi-transparent)
- Title text
- Content (varies by modal)
- Action buttons (Cancel, Confirm/Save/Create)

**Behavior:**
- Display modally (blocks interaction with underlying content)
- Cancel closes without action
- Confirm/Save executes action and closes
- Tap outside (optional) closes without action

---

### 17. Navigation Menu / Sidebar

**Purpose:** Quick access to all main screens

**Layout Structure (Mobile - Slide-Out Menu):**
```
┌──────────────────────────────┐
│ ≡ Menu              ✕ Close  │  Header with icon toggle + close
│                              │
│ [User Avatar] John Doe       │  User section
│ john@example.com             │
│ ─────────────────────────────│
│                              │
│ 📋 My Notes                  │  Built-in lists
│ ⭐ Important                 │
│ 🔔 Reminders                │
│ 🗑️  Bin                     │
│ ─────────────────────────────│
│                              │
│ Custom Lists:                │  Section header
│                              │
│ 🎓 Projects     [>]          │  Custom lists with edit option
│ 🏥 Health       [>]          │
│ 🎨 Creative     [>]          │
│ + Create List                │
│ ─────────────────────────────│
│                              │
│ Settings                     │  App settings
│ Logout                       │  Logout
│                              │
└──────────────────────────────┘
```

**Components:**
- Menu header with close button
- User profile section (avatar, name, email)
- Navigation links (My Notes, Important, Reminders, Bin)
- Custom lists section
- Settings and Logout links

**Behavior:**
- Hamburger menu button (≡) toggles visibility
- Tap item → Navigate to that view, close menu
- Tap overlay → Close menu
- Swipe left/right → Toggle menu (optional)
- Long-press custom list → Edit/Delete options

**States:**
- Hidden
- Visible (slide animation)
- Item hovered/focused
- Item selected (highlight)

---

## Navigation Flow

### Application Navigation Structure

```
Splash Screen
├── (Auto-navigate based on auth state)
│
├─→ Sign In Screen ←─┐
│   ├─→ Sign Up      │
│   ├─→ Forgot Pwd   │
│   └─→ Signup Confirmation
│       └─→ Sign In
│
└─→ Dashboard (My Notes)
    ├── → Important View
    ├── → Reminders View
    ├── → Bin View
    ├── → Custom Lists View
    │   ├── → Custom List Detail
    │   ├── → Create List Modal
    │   └── → Edit List Modal
    ├── → Task Details Modal
    ├── → Task Creation
    ├── → Task Editing
    ├── → Sidebar Navigation
    └── → Settings
```

### Navigation Patterns

1. **Main Navigation:** Bottom tab bar or side menu
2. **Modal Navigation:** Task details, list creation, dialogs
3. **Hierarchical Navigation:** Back button for depth
4. **Direct Navigation:** Search results to task details

---

## Data Models

### Core Entities

#### User Model
```csharp
public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
```

#### TaskList Model
```csharp
public class TaskList
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Color { get; set; }
    public int TaskCount { get; set; }
    public bool IsBuiltIn { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### Task Model
```csharp
public class Task
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TaskListId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsImportant { get; set; }
    public DateTime? DueDate { get; set; }
    public TimeSpan? DueTime { get; set; }
    public bool HasReminder { get; set; }
    public ReminderType? ReminderType { get; set; }
    public RecurringType? RecurringType { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### Enumerations
```csharp
public enum ReminderType
{
    FifteenMinutesBefore,
    OneHourBefore,
    OneDayBefore,
    TwoDaysBefore,
    OneWeekBefore,
    Custom
}

public enum RecurringType
{
    None,
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
    Yearly
}
```

---

## MVVM Architecture

### ViewModel Structure

#### BaseViewModel
- Implements `INotifyPropertyChanged`
- Base for all ViewModels
- Common functionality: Navigation, Loading states, Error handling

#### ViewModels by Screen

1. **SplashViewModel**
   - Properties: LoadingProgress, IsLoading
   - Commands: Initialize()

2. **SignInViewModel**
   - Properties: Email, Password, RememberMe, IsLoading, ErrorMessage
   - Commands: SignIn(), ForgotPassword(), SignUp()
   - Validation: Email format, password required

3. **SignUpViewModel**
   - Properties: FullName, Email, Password, ConfirmPassword, TermsAccepted, PasswordStrength, IsLoading, ErrorMessage
   - Commands: CreateAccount(), SignIn()
   - Validation: All fields, password strength, email uniqueness

4. **ForgotPasswordViewModel**
   - Properties: Email, IsLoading, ErrorMessage, SuccessMessage
   - Commands: SendResetLink(), SignIn()
   - Validation: Email format

5. **MyNotesViewModel**
   - Properties: Tasks (ObservableCollection), SearchText, SortBy, FilterBy, IsLoading
   - Commands: LoadTasks(), SearchTasks(), SortTasks(), FilterTasks(), AddTask(), EditTask(), DeleteTask(), CompleteTask()

6. **TaskDetailsViewModel**
   - Properties: Task (read-only), IsLoading, ErrorMessage
   - Commands: MarkComplete(), MarkImportant(), MoveToList(), Delete(), Edit()

7. **TaskCreationViewModel**
   - Properties: Title, Description, ListId, DueDate, DueTime, HasReminder, ReminderType, HasRecurring, RecurringType, IsImportant, IsLoading, ErrorMessage
   - Commands: SaveTask(), Cancel()
   - Validation: Title required

8. **TaskEditingViewModel**
   - (Extends TaskCreationViewModel)
   - Properties: (same as creation + TaskId)
   - Commands: SaveTask(), DeleteTask(), Cancel()

9. **CustomListsViewModel**
   - Properties: Lists (ObservableCollection), IsLoading
   - Commands: LoadLists(), CreateList(), EditList(), DeleteList()

10. **ImportantViewModel, RemindersViewModel, BinViewModel**
    - (Specialized MyNotesViewModel variants)
    - Filtered task collections

---

## Service Layer

### Service Architecture

```
IAuthService
├── SignIn(email, password)
├── SignUp(name, email, password)
├── ForgotPassword(email)
└── Logout()

ITaskService
├── GetTasks()
├── GetTaskById(id)
├── CreateTask(task)
├── UpdateTask(task)
├── DeleteTask(id)
├── CompleteTask(id)
├── GetTasksByList(listId)
└── SearchTasks(query)

ITaskListService
├── GetLists()
├── GetListById(id)
├── CreateList(name, icon, color)
├── UpdateList(list)
├── DeleteList(id)
└── GetListsByType()

INotificationService
├── ShowSnackbar(message, type, action)
├── ShowDialog(title, message, buttons)
└── ShowToast(message, duration)

INavigationService
├── Navigate(route, parameters)
├── GoBack()
├── GoHome()
└── Replace(route)

IStorageService
├── SaveAsync(key, value)
├── GetAsync(key)
├── DeleteAsync(key)
└── ClearAsync()

ILocalDataService
├── InitializeAsync()
├── GetUserId()
├── SetUserId(id)
└── Synchronize()
```

---

## Syncfusion Control Mapping

### Syncfusion Controls Usage

| Control | Usage | Why Syncfusion |
|---------|-------|---|
| **SfTextInputFieldOutline** | Input fields (email, password, title, description) | Material Design, built-in validation, icon support, outline style |
| **SfButton** | All buttons (Sign In, Save, Delete, Create, etc.) | Consistent styling, ripple effects, loading states, icon support |
| **SfListView** | Task lists, custom lists display | Virtual scrolling, grouping, item templates, swipe gestures |
| **SfDatePicker** | Date selection (due dates) | Picker UI, date validation, culture support |
| **SfTimePicker** | Time selection (due time, reminders) | Time picker UI, 12/24 hour format support |
| **SfComboBox** | Dropdowns (list selector, sort, filter, reminder options) | Autocomplete, filtering, custom items |
| **SfProgressBar** | Password strength indicator | Visual feedback, color coding, smooth animation |
| **SfCheckBox** | Toggles (task completion, reminders, important, etc.) | Customizable appearance, ripple effects |
| **SfSnackBar** | Toast notifications | Built-in animations, auto-dismiss, action buttons |
| **SfBusyIndicator** | Loading spinners | Custom content, animations |
| **SfSegmentedControl** | View toggles (optional, for switching between views) | Segmented button group, icon support |
| **SfChip** | Tags/badges (reminders, categories) | Visual feedback, deletable items |
| **SfCard** | Container for task items | Consistent elevation, styling |

### NuGet Packages Required

```xml
<PackageReference Include="Syncfusion.Maui.Controls" Version="25.1.37" />
<PackageReference Include="Syncfusion.Maui.Core" Version="25.1.37" />
<PackageReference Include="Syncfusion.Maui.Themes" Version="25.1.37" />
```

### Syncfusion Namespaces

```csharp
using Syncfusion.Maui.Controls;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.ListView;
using Syncfusion.Maui.Popup;
using Syncfusion.Maui.Gauges;
using Syncfusion.Maui.Sliders;
```

---

## Resource Dictionary Structure

### Resource Files Organization

```
Resources/
├── Styles/
│   ├── Colors.xaml
│   ├── Fonts.xaml
│   ├── Spacing.xaml
│   ├── ButtonStyles.xaml
│   ├── InputStyles.xaml
│   ├── TextStyles.xaml
│   └── ControlStyles.xaml
├── Themes/
│   ├── LightTheme.xaml
│   └── DarkTheme.xaml
├── Converters/
│   ├── BoolToVisibilityConverter.cs
│   ├── DateTimeToStringConverter.cs
│   ├── ReminderTypeToStringConverter.cs
│   └── RecurringTypeToStringConverter.cs
└── Templates/
    ├── TaskItemTemplate.xaml
    ├── ListItemTemplate.xaml
    └── EmptyStateTemplate.xaml
```

### Resource Dictionary Contents

**Colors.xaml:**
```xml
<ResourceDictionary>
    <Color x:Key="ColorPrimary">#6366F1</Color>
    <Color x:Key="ColorSecondary">#EC4899</Color>
    <!-- ... other colors ... -->
</ResourceDictionary>
```

**Fonts.xaml:**
```xml
<ResourceDictionary>
    <x:Double x:Key="FontSizeHeadingXL">32</x:Double>
    <x:Double x:Key="FontSizeBodyMD">16</x:Double>
    <!-- ... other font sizes ... -->
</ResourceDictionary>
```

---

## Component Reusability

### Reusable Components

1. **TaskCardComponent**
   - Template: Task item with checkbox, title, due date, icons, menu
   - Usage: My Notes, Important, Reminders, Bin, Search results
   - Customization: Hide/show icons, actions menu

2. **ListItemComponent**
   - Template: List with icon, name, task count
   - Usage: Custom Lists, Navigation menu
   - Customization: Icons, colors

3. **InputFieldComponent**
   - Template: Label, input, validation message, icon
   - Usage: All input screens
   - Customization: Type, placeholder, icon, validation

4. **ButtonComponent**
   - Template: Button with consistent styling
   - Usage: All buttons
   - Customization: Variant (primary, secondary, destructive), size

5. **ConfirmationDialogComponent**
   - Template: Title, message, buttons
   - Usage: Destructive actions
   - Customization: Title, message, button labels

6. **SnackbarComponent**
   - Template: Message, icon, optional action
   - Usage: Notifications
   - Customization: Type (success, error, warning, info), duration

7. **EmptyStateComponent**
   - Template: Icon, title, subtitle
   - Usage: Empty lists, no results
   - Customization: Message, icon

---

## Validation Rules

### Field Validations

#### Authentication
- **Email:** Required, valid RFC 5322 format, must be unique (signup)
- **Password:** Required, min 8 chars, uppercase + lowercase + digit + symbol
- **Confirm Password:** Must match password field
- **Full Name:** Required, 2-50 characters
- **Terms & Conditions:** Must be accepted

#### Task
- **Title:** Required, 1-200 characters
- **Description:** Optional, 0-1000 characters
- **Due Date:** Optional, must be valid date, not in past (warning only)
- **Reminder:** Only valid if due date is set
- **Recurring:** Valid selection if enabled

#### List
- **Name:** Required, 1-50 characters
- **Icon:** Required selection
- **Color:** Required selection

### Client-Side Validation
- Real-time validation on input
- Visual feedback (error messages, highlighting)
- Button disabled until form valid

### Server-Side Validation (Mock)
- Email uniqueness check
- Password strength verification
- Data integrity checks

---

## State Management

### Application State

```csharp
public class AppState
{
    // Authentication
    public User CurrentUser { get; set; }
    public bool IsAuthenticated { get; set; }
    public string AuthToken { get; set; }
    
    // Tasks & Lists
    public ObservableCollection<Task> Tasks { get; set; }
    public ObservableCollection<TaskList> Lists { get; set; }
    
    // UI State
    public string SearchQuery { get; set; }
    public SortType CurrentSort { get; set; }
    public FilterType CurrentFilter { get; set; }
    public bool IsMenuOpen { get; set; }
    
    // Notifications
    public ObservableCollection<Notification> Notifications { get; set; }
}
```

### State Management Pattern

- **ViewModel-Based State:** ViewModels maintain their own state
- **Centralized Services:** Services handle data persistence
- **Dependency Injection:** Services injected into ViewModels
- **Observable Collections:** For dynamic UI updates

---

## Design System & Tokens

### Token Categories

1. **Color Tokens:** Primary, secondary, success, warning, error, info, backgrounds, borders
2. **Typography Tokens:** Font sizes, weights, line heights
3. **Spacing Tokens:** Margins, padding, gaps
4. **Border Tokens:** Radius, width, styles
5. **Shadow Tokens:** Elevation levels
6. **Duration Tokens:** Animation timings

All tokens defined in Resource Dictionaries for easy theming and consistency.

---

## Additional Specifications

### Animations & Transitions

- **Page Transitions:** Fade or slide (100-200ms)
- **Button Press:** Ripple effect (200ms)
- **List Item Swipe:** Smooth reveal of actions (150ms)
- **Loading Spinner:** Continuous rotation (1000ms per rotation)
- **Snackbar Appearance:** Slide up (200ms)
- **Dialog Fade:** Fade in/out (150ms)

### Accessibility

- **Contrast:** AAA compliance (WCAG 2.1)
- **Font Sizes:** Minimum 12px for body text
- **Touch Targets:** Minimum 44x44 points
- **Screen Reader Support:** Proper labels and descriptions
- **Color Not Only:** Do not rely on color alone for information
- **Focus Indicators:** Visible focus on all interactive elements

### Responsive Design

- **Mobile (< 600px):** Single column, full-width inputs
- **Tablet (600-1024px):** Adaptive layout, 2-column optional
- **Desktop (> 1024px):** Optimized for larger screens

### Error Handling

- Network errors: Retry button, offline indicator
- Validation errors: Inline error messages
- Server errors: Snackbar with error message
- Graceful degradation: Default values, fallback UI

---

## Summary

This specification provides a complete blueprint for implementing the MAUI To Do List Application. Each screen, component, and service is documented with:

- Visual layout structure
- Interactive behavior
- Validation rules
- Data models
- MVVM architecture
- Reusable components
- Syncfusion control mapping

The specification follows best practices for:
- Clean architecture
- MVVM pattern
- Component reusability
- Accessibility
- Responsive design
- User experience

Implementation can proceed screen-by-screen, module-by-module, with clear acceptance criteria for each feature.

---

**End of Specification Document**

Version: 1.0 | Last Updated: 2024 | Status: Ready for Implementation
