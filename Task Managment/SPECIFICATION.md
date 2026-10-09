# Syncfusion To Do List Application - Detailed Specification

**Version:** 1.0  
**Date:** October 2026  
**Application:** MAUI To Do List with Syncfusion Controls  
**Target Platform:** .NET MAUI (iOS, Android, Windows, macOS)

---

## TABLE OF CONTENTS

1. [Application Overview](#application-overview)
2. [Design Analysis](#design-analysis)
3. [Functional Requirements](#functional-requirements)
4. [Non-Functional Requirements](#non-functional-requirements)
5. [UI Requirements](#ui-requirements)
6. [Design Tokens & Theme](#design-tokens--theme)
7. [Navigation Structure](#navigation-structure)
8. [Screen-by-Screen Specifications](#screen-by-screen-specifications)
9. [Data Models](#data-models)
10. [Architecture & MVVM](#architecture--mvvm)
11. [Syncfusion Control Mapping](#syncfusion-control-mapping)
12. [Resource Structure](#resource-structure)
13. [Validation & Error Handling](#validation--error-handling)
14. [Accessibility Requirements](#accessibility-requirements)

---

## 1. APPLICATION OVERVIEW

### 1.1 Purpose
A modern, feature-rich To Do List application built with .NET MAUI and Syncfusion controls for task management, organization, reminders, and productivity tracking.

### 1.2 Core Features
- **Authentication**: Sign in, Sign up, Forgot password, Sign out
- **Task Management**: Create, edit, complete, delete, restore tasks
- **Organization**: Custom lists, predefined categories (My notes, Important, Reminder, Bin)
- **Task Features**: Due dates, reminders, recurring tasks, color categorization
- **Search & Filter**: Full-text search, filter by category
- **Drag & Drop**: Reorder tasks, move between lists
- **Responsive Design**: Works on phone, tablet, and desktop layouts

### 1.3 Target Users
- Productivity-focused professionals
- Students managing assignments
- Project teams coordinating tasks
- Personal productivity users

---

## 2. DESIGN ANALYSIS

### 2.1 Key Design Observations

#### Layout Structure
- **Split Navigation**: Left sidebar navigation panel (230px estimated width)
- **Content Area**: Full-width main content area with dynamic background
- **Header Bar**: Fixed top bar with app title, search, and profile
- **Bottom Input**: Fixed "Add a note" input bar at bottom

#### Visual Hierarchy
1. **Primary**: Task items with checkbox, title, date info
2. **Secondary**: Category/list name (header with icon)
3. **Tertiary**: Metadata (dates, labels, reminders)
4. **Interactive**: Context menus, dialogs, buttons

#### Navigation Sidebar Items
```
My notes (icon: person outline)
Important (icon: star outline)
Reminder (icon: bell outline)
Bin (icon: delete/trash)
[Custom Lists follow]
```

#### Task Item Structure
```
[Checkbox] [Title] [Star-important] [MoreMenu]
           [Date info with icons]
```

#### List-Level Structure
Each list/category shows:
- Header: Icon + Title + More options menu
- Background image (decorative, full-bleed)
- Task items (white cards on semi-transparent background)
- "Add a note" input bar

### 2.2 Identified Screens

| Screen | Image File | Purpose |
|--------|-----------|---------|
| Splash | splash.png | Loading/initialization screen |
| Sign In | signin.png | User authentication |
| Sign Up | signup.png | New account creation |
| Forgot Password | forgot.png | Password recovery |
| Main List | reviewtodayevents.png | Display tasks in a list |
| Important | important.png | Filter starred tasks |
| Reminder | remainder.png | Filter tasks with reminders |
| Bin | bin.png | Deleted items with restore option |
| My Notes | mynotes.png | Empty state/custom list |
| Search | search.png | Search empty state |
| Rename List | renamelist.png | Dialog to rename custom list |
| Profile Menu | profile_pressed.png | User account menu |
| Sign Out | signout.png | Sign out confirmation |

---

## 3. FUNCTIONAL REQUIREMENTS

### 3.1 Authentication Module

#### FR-AUTH-001: Sign In
- Input: Email, Password
- Validation: Email format, password required
- Actions: Authenticate user, navigate to main list
- Error handling: Invalid credentials, network errors

#### FR-AUTH-002: Sign Up
- Input: Username, Email, Password
- Validation: Unique email, password strength, username format
- Actions: Create account, auto-login, navigate to main list
- Error handling: Account exists, validation errors

#### FR-AUTH-003: Forgot Password
- Input: Email address
- Action: Send password reset email
- Confirmation: Message displayed

#### FR-AUTH-004: Sign Out
- Action: Clear session, confirm before logout
- Navigation: Return to sign-in screen

#### FR-AUTH-005: Remember Me
- Feature: Optional checkbox on sign-in
- Behavior: Auto-login on app restart (when enabled)

### 3.2 Task Management Module

#### FR-TASK-001: Create Task
- Input: Title, due date (optional), reminder (optional)
- Action: Add task to current list
- Validation: Title required, max 500 chars
- Success: Display in list, show success notification

#### FR-TASK-002: Edit Task
- Input: All task fields
- Action: Update task properties
- Validation: Same as create
- Success: Update displayed, show notification

#### FR-TASK-003: Complete Task
- Action: Toggle task completion checkbox
- Behavior: Visual change (strikethrough, opacity change)
- Effect: May stay in list or hide (configurable)

#### FR-TASK-004: Delete Task
- Action: Move to Bin (soft delete)
- Behavior: Task disappears from main list

#### FR-TASK-005: Restore Task
- Location: Bin screen
- Action: Return task to original list
- Validation: Original list still exists

#### FR-TASK-006: Permanently Delete Task
- Location: Bin screen
- Action: Irreversible removal from database
- Confirmation: Confirmation dialog required

#### FR-TASK-007: Mark as Important
- Action: Toggle star icon
- Effect: Task appears in "Important" list
- Visual: Star filled when marked

#### FR-TASK-008: Set Due Date
- Input: Date picker
- Validation: Date must be today or future
- Display: Show date in task item

#### FR-TASK-009: Set Reminder
- Input: Reminder time (before task due date)
- Options: 15 min, 1 hour, 1 day, custom
- Behavior: Trigger notification at specified time

#### FR-TASK-010: Recurring Tasks (Optional)
- Input: Recurrence pattern (daily, weekly, monthly)
- Behavior: Auto-create new task after completion
- Display: Show recurrence indicator

### 3.3 List Management Module

#### FR-LIST-001: Create Custom List
- Input: List name, color (optional)
- Action: Create new category
- Validation: Name required, unique name
- Navigation: New list appears in sidebar

#### FR-LIST-002: Rename List
- Action: Edit existing list name
- Validation: Unique name
- UI: Modal dialog with text input

#### FR-LIST-003: Delete List
- Action: Remove custom list (built-in lists cannot be deleted)
- Behavior: Option to move tasks to another list first
- Confirmation: Confirmation dialog

#### FR-LIST-004: Move Task Between Lists
- Action: Select destination list from context menu
- Behavior: Task moves to new list
- Display: Update current view

### 3.4 Search & Filter Module

#### FR-SEARCH-001: Search Tasks
- Input: Text search box (top header)
- Scope: All lists, all completed/incomplete tasks
- Result: Display matching tasks
- Empty State: Message "You can search the created notes here"

#### FR-SEARCH-002: Filter by Category
- Categories: My notes, Important, Reminder, Bin, Custom lists
- Action: Click sidebar item
- Effect: Display only tasks in category
- Default: First list on app start

### 3.5 User Profile Module

#### FR-PROFILE-001: View Account Info
- Display: Username, email
- Location: Profile menu (top-right)

#### FR-PROFILE-002: Manage Account
- Links: Settings, Manage accounts (placeholder for future)

#### FR-PROFILE-003: Sign Out
- Action: Clear session
- Confirmation: Dialog "Are you sure you want to sign out?"
- Navigation: Return to sign-in

---

## 4. NON-FUNCTIONAL REQUIREMENTS

### 4.1 Performance
- **Load Time**: Splash screen → Main screen: < 2 seconds
- **List Rendering**: Smooth scrolling at 60 FPS with 100+ tasks
- **Search Response**: < 500ms for search results
- **Data Sync**: Local-first, background sync with server

### 4.2 Accessibility (WCAG 2.1 AA)
- Minimum font size: 12sp
- Color contrast ratio: 4.5:1 for text
- Touch target size: 48x48 dp minimum
- Keyboard navigation: Full support
- Screen reader support: All interactive elements labeled
- Focus indicators: Clear and visible

### 4.3 Responsive Design
- **Phone**: 320px - 480px width (portrait)
- **Tablet**: 481px - 1024px width
- **Desktop**: 1025px+ width
- **Breakpoints**:
  - Small: < 600px (single column, full-width content)
  - Medium: 600px - 1200px (sidebar + content)
  - Large: > 1200px (expanded layout)

### 4.4 Data Security
- Credentials: Encrypted storage using SecureStorage
- Authentication: Token-based (JWT recommended)
- Data: End-to-end encryption for sensitive fields
- Privacy: No analytics on task content

### 4.5 Reliability
- **Offline Support**: Local SQLite storage with sync when online
- **Error Recovery**: Graceful degradation, retry logic
- **Data Persistence**: All changes persisted locally
- **Crash Handling**: Session recovery, no data loss

---

## 5. UI REQUIREMENTS

### 5.1 Component Specifications

#### Header Bar
```
[Menu Icon] [App Title] [Search Box] [Profile Avatar]
Height: 56dp (estimated)
Background: White or app color
Spacing: 16dp padding
```

#### Sidebar Navigation
```
Width: 230px (estimated)
Background: #F5F5F5 or light gray
Items:
  - Icon (24x24) + Label (14sp)
  - Padding: 16dp horizontal, 12dp vertical
  - Active: Background highlight color
  - Hover: Subtle background change
```

#### Task Item Card
```
Padding: 12dp
Margin: 8dp horizontal, 6dp vertical
Border: None (elevation shadow)
Radius: 8dp (estimated)
Components:
  [Checkbox] [Title Text] [Star] [MoreMenu]
  [Metadata Row: Date + Icons]
```

#### Dialogs
```
Width: 320px (estimated) or 80% screen width
Corner Radius: 16dp (estimated)
Padding: 24dp
Backdrop: Scrim 40% opacity
Buttons: 44dp height, full width or side-by-side
```

#### Bottom Input Bar
```
Height: 64dp (estimated)
Padding: 8dp
Content: [TextField] [Icons] [Checkmark Button]
Position: Fixed at bottom of screen
```

### 5.2 Visual States

#### Task Item States
1. **Normal**: Opaque, shadow visible
2. **Completed**: Strikethrough text, reduced opacity (50%)
3. **Hovered**: Background subtle highlight
4. **Important**: Star filled, visual emphasis
5. **Selected**: Checkbox checked, visual change

#### Button States
1. **Enabled**: Full color, interactive cursor
2. **Disabled**: Reduced opacity (40%), no interaction
3. **Pressed/Active**: Darker shade, slight elevation change
4. **Hover**: Subtle background change

#### Input States
1. **Empty**: Placeholder text visible
2. **Focused**: Border color change, cursor visible
3. **Filled**: Text color, label optional
4. **Error**: Border red, error message below

---

## 6. DESIGN TOKENS & THEME

### 6.1 Color Palette

#### Primary Colors
| Token | Light Mode | Dark Mode | Usage |
|-------|-----------|----------|-------|
| Primary | #5C4EAE (Purple) | #8A7FD9 | Buttons, accents, headers |
| Surface | #FFFFFF | #1E1E1E | Backgrounds |
| Background | #FAFAFA | #121212 | Screen backgrounds |
| Error | #B3261E | #F28482 | Error states |
| Success | #188A3D | #81C995 | Success states |

#### Secondary Colors (Category Highlights)
| List | Color | Icon |
|------|-------|------|
| My notes | #E8E8E8 | 👤 |
| Important | #9C27B0 | ⭐ |
| Reminder | #2196F3 | 🔔 |
| Bin | #F44336 | 🗑️ |

#### List Background Images (Full-Bleed)
Each list has a decorative background image that covers the main content area with semi-transparent overlay (30-40% opacity).

### 6.2 Typography

#### Font Family
- **Primary**: Segoe UI (Windows), System Font (iOS/Android)
- **Fallback**: Roboto (Android), Helvetica Neue (iOS)

#### Font Sizes & Styles

| Component | Size | Weight | Line Height |
|-----------|------|--------|-------------|
| App Title | 18sp | 600 (SemiBold) | 24sp |
| List Title | 20sp | 400 (Regular) | 28sp |
| Task Title | 16sp | 400 (Regular) | 24sp |
| Label | 14sp | 500 (Medium) | 20sp |
| Metadata | 12sp | 400 (Regular) | 18sp |
| Button | 14sp | 500 (Medium) | 20sp |
| Caption | 11sp | 400 (Regular) | 16sp |

### 6.3 Spacing & Layout

#### Standard Margins & Padding
- **Micro**: 4dp (smallest gaps)
- **Small**: 8dp (between related items)
- **Medium**: 12dp (item padding)
- **Standard**: 16dp (common padding, spacing)
- **Large**: 24dp (section separators)
- **Extra Large**: 32dp (major sections)

#### Border Radius
- **Buttons & Cards**: 8dp
- **Dialogs**: 16dp
- **Input Fields**: 8dp
- **Full Round**: 50% (avatars, icons)

#### Shadows (Material Design 3)
- **Elevation 1**: 0dp 1dp 3dp rgba(0,0,0,0.12)
- **Elevation 2**: 0dp 3dp 6dp rgba(0,0,0,0.16)
- **Elevation 3**: 0dp 6dp 12dp rgba(0,0,0,0.20)

### 6.4 Motion & Animation

#### Duration
- **Short**: 150ms (quick feedback)
- **Medium**: 300ms (standard transitions)
- **Long**: 500ms (complex animations)

#### Easing
- **Ease In**: For elements leaving/hiding
- **Ease Out**: For elements entering/showing
- **Ease In-Out**: For property changes

---

## 7. NAVIGATION STRUCTURE

### 7.1 Navigation Flow Diagram

```
Splash Screen
    ↓
    ├─→ [Authenticated] → Main Screen (Default List)
    └─→ [Not Authenticated] → Sign In Screen
                    ↓
                Sign In Screen
                    ├─→ [Success] → Main Screen
                    ├─→ Forgot Password Screen → Sign In
                    └─→ [New User] → Sign Up Screen
                    
                Sign Up Screen
                    ├─→ [Success] → Main Screen
                    └─→ [Existing Account] → Sign In
                    
                Forgot Password Screen
                    └─→ [Reset Email Sent] → Sign In Screen

Main Screen (Authenticated)
    ├─→ Navigate Sidebar
    │   ├─→ My notes
    │   ├─→ Important
    │   ├─→ Reminder
    │   ├─→ Bin
    │   └─→ Custom Lists [n]
    ├─→ Search
    ├─→ Create Task (+ button)
    ├─→ Edit Task (long press / context menu)
    ├─→ Delete Task (move to Bin)
    ├─→ Toggle Important (star icon)
    ├─→ Profile Menu
    │   ├─→ Manage Accounts
    │   ├─→ Settings
    │   └─→ Sign Out → Sign In Screen
    └─→ Dialogs
        ├─→ Create/Rename List
        ├─→ Confirm Delete
        ├─→ Confirm Sign Out
        └─→ Edit Task Details
```

### 7.2 Navigation Implementation

**Pattern**: NavigationPage with tab-like sidebar  
**Framework**: MVVM with Shell or custom NavigationPage  
**Parameters**: Pass list ID to filter tasks  

---

## 8. SCREEN-BY-SCREEN SPECIFICATIONS

### 8.1 SPLASH SCREEN

**File**: `splash.png`

#### Layout
```
Vertical centered stack:
- Logo (120x120dp estimated)
- App Name: "SYNCFUSION"
- Subtitle: "TO DO LIST"
```

#### Specifications
- **Background**: White (#FFFFFF)
- **Logo**: Asset from project
- **App Name**: Font 28sp, SemiBold, Color #000000
- **Subtitle**: Font 14sp, Regular, Color #666666
- **Duration**: 2-3 seconds before navigation

#### Behavior
- Auto-navigate to Sign In (not authenticated) or Main Screen (authenticated)
- No user interaction
- Fully center content both horizontally and vertically

---

### 8.2 SIGN IN SCREEN

**File**: `signin.png`

#### Layout
```
Vertical scrollable:
- Title: "Sign in"
- Subtitle: "Enter your details to sign in"
- Email Input Field
- Password Input Field
- [Checkbox] Remember me
- "Forgot password?" Link
- Sign In Button (full width)
- Divider: "Or"
- Sign in with Google Button
- Sign in with Microsoft Button
- "Don't have an account? Sign up" Link
```

#### Specifications

##### Text Fields
- **Width**: 100% - 32dp padding
- **Height**: 48dp
- **Corner Radius**: 8dp
- **Padding**: 12dp horizontal
- **Border**: 1dp, Color #CCCCCC (default), #5C4EAE (focused)
- **Font**: 14sp, Regular
- **Placeholder**: "Email" / "Password"

##### Remember Me
- **Checkbox**: 20x20dp
- **Label**: 14sp, Regular
- **Spacing**: 8dp between checkbox and label
- **Default**: Unchecked

##### Forgot Password Link
- **Text**: "Forgot password?"
- **Color**: #5C4EAE (purple)
- **Alignment**: Right side, next to Remember Me
- **Style**: Underline on hover

##### Sign In Button
- **Width**: 100% - 32dp padding
- **Height**: 48dp
- **Corner Radius**: 24dp (pill shape)
- **Background**: #5C4EAE (purple)
- **Text**: "Sign in", 16sp, SemiBold, White
- **Elevation**: 2dp
- **Disabled State**: Opacity 50%

##### Social Login Buttons
- **Layout**: Two-column grid
- **Width**: (100% - 40dp padding - 16dp gap) / 2
- **Height**: 48dp
- **Corner Radius**: 8dp
- **Border**: 1dp, #CCCCCC
- **Background**: White
- **Icons**: Google, Microsoft (24x24dp)
- **Text**: "Sign in with Google", "Sign in with Microsoft"
- **Font**: 12sp, Medium

##### Sign Up Link
- **Text**: "Don't have an account? Sign up"
- **Color**: "Don't have an account?" = Black, "Sign up" = #5C4EAE
- **Style**: "Sign up" is clickable link
- **Alignment**: Center bottom

#### Validations
- Email: Valid format (user@domain.com)
- Password: At least 6 characters
- Show error messages below fields
- Disable Sign In button if invalid

---

### 8.3 SIGN UP SCREEN

**File**: `signup.png`

#### Layout
```
Vertical scrollable:
- Title: "Sign up"
- Subtitle: "Enter your details to sign up"
- Username Input Field
- Email Input Field
- Password Input Field
- Sign Up Button (full width)
- Divider: "Or"
- Sign up with Google Button
- Sign up with Microsoft Button
- "Already have an account? Sign in" Link
```

#### Specifications
- **Similar to Sign In screen**
- **Additional Field**: Username (20-50 characters, alphanumeric)
- **Password Requirements**: Show strength indicator (optional)
- **All styling**: Same as Sign In screen

#### Validations
- Username: 3-50 chars, alphanumeric + underscore
- Email: Valid format, unique check
- Password: Min 6 chars, recommend uppercase + number + special char
- Terms of Service: Optional checkbox (no visible in mockup)

---

### 8.4 FORGOT PASSWORD SCREEN

**File**: `forgot.png`

#### Layout
```
Vertical centered stack:
- Title: "Forgot password?"
- Subtitle: "Enter your email and we'll send a link to reset password"
- Email Input Field
- Send Reset Link Button (full width)
- "Return to Sign in" Link
```

#### Specifications

##### Text Elements
- **Title**: 24sp, SemiBold, Color #000000
- **Subtitle**: 14sp, Regular, Color #666666
- **Max Width**: 80% screen width

##### Email Field
- **Same as Sign In screen**

##### Send Reset Link Button
- **Same styling as Sign In button**
- **Background**: #5C4EAE
- **Height**: 48dp, 24dp border-radius
- **Text**: "Send reset link"

##### Return Link
- **Text**: "Return to Sign in"
- **Color**: #5C4EAE
- **Position**: Center, below button
- **Alignment**: Centered

#### Behavior
- Validate email format
- Show success message after submission
- Navigate back to Sign In after 2-3 seconds or on manual action

---

### 8.5 MAIN LIST SCREEN (Task Display)

**File**: `reviewtodayevents.png` (and variations)

#### Layout - Left Sidebar (Fixed)
```
Top:
[Search Box with Icon]

Navigation Items:
[Icon] My notes
[Icon] Important
[Icon] Reminder
[Icon] Bin
[Icon] Review today's emails (example custom list)
[Collapsed Items with + count indicator]
...
[+ Icon] Create New List
```

#### Layout - Right Main Content
```
Header Bar:
[Menu] [App Title] [Search Icon] [Profile Avatar]

Content Area:
[List Header Icon + Title] [... more menu]
[Background Image - Full Bleed with overlay]
[Task Item Cards Overlay - Semi-transparent white]
  Task Item 1
  Task Item 2
  ...
  Task Item N

Bottom:
[Add Note Input Bar - Fixed]
  [Add a note placeholder] [Icons] [Checkmark]

Bottom Toast:
[1 task moved] [X icon]
```

#### Specifications - Sidebar

##### Width & Styling
- **Width**: 230px (estimated)
- **Background**: #F5F5F5
- **Scroll**: Vertical, hidden scrollbar
- **Separator**: 1dp border right, #E0E0E0

##### Search Box
- **Width**: 100% - 16dp padding
- **Height**: 40dp
- **Margin**: 8dp vertical, 8dp horizontal
- **Border Radius**: 20dp
- **Background**: #FFFFFF
- **Border**: 1dp, #E0E0E0
- **Icon**: Magnifying glass, 20x20dp, #666666
- **Placeholder**: "Search"
- **Font**: 14sp, Regular

##### Navigation Items
- **Height**: 44dp
- **Padding**: 12dp vertical, 16dp horizontal
- **Margin**: 4dp top/bottom
- **Icon**: 24x24dp, color varies by category
  - My notes: #757575
  - Important: #9C27B0
  - Reminder: #2196F3
  - Bin: #F44336
  - Custom: Assigned color
- **Label**: 14sp, Regular, color #333333
- **Count Badge** (if tasks > 0): 
  - Background: Category color
  - Text: White, 12sp, SemiBold
  - Padding: 2dp horizontal
  - Border Radius: 10dp
  - Position: Right aligned

##### Active State
- **Background**: #E8E8E8
- **Font Weight**: SemiBold
- **Border Left**: 3dp, accent color

##### Create New List
- **Icon**: + (plus)
- **Position**: Bottom of scrollable list or fixed
- **Color**: #5C4EAE
- **Size**: 44dp square minimum touch target

#### Specifications - Header Bar

##### Height & Styling
- **Height**: 56dp
- **Background**: White, elevation 1dp
- **Padding**: 8dp horizontal, 8dp vertical
- **Alignment**: Horizontal space-between

##### Menu Icon (Left)
- **Size**: 24x24dp
- **Color**: #333333
- **Action**: Toggle sidebar (mobile) or no-op (desktop)

##### App Title
- **Text**: "To Do List"
- **Font**: 18sp, SemiBold
- **Color**: #333333
- **Alignment**: Center

##### Search Icon (Right Section)
- **Size**: 24x24dp
- **Color**: #666666
- **Action**: Open search view

##### Profile Avatar
- **Size**: 40x40dp
- **Border Radius**: 50% (circular)
- **Image**: User profile photo or initials
- **Action**: Open profile menu

#### Specifications - Main Content Area

##### List Header
- **Layout**: Horizontal flex, space-between
- **Icon**: 24x24dp, color category-specific
- **Title**: 20sp, SemiBold, color #333333
- **More Menu Button**: 24x24dp, three-dot vertical
- **Padding**: 12dp vertical, 16dp horizontal
- **Background**: Overlay the background image

##### Background Image
- **Size**: Full screen width and height (minus header)
- **Position**: Absolute, z-index lower than content
- **Opacity**: 100% (image clearly visible)
- **Parallax**: Optional scroll effect

##### Task Item Card
```
Horizontal Layout:
[Checkbox] [Title + Metadata] [Star] [MoreMenu]
```

###### Checkbox
- **Size**: 20x20dp
- **Type**: Unselected (empty circle) / Selected (filled + checkmark)
- **Color**: #5C4EAE (selected)
- **Margin**: 12dp left

###### Title + Metadata
- **Width**: Flex (remaining space)
- **Padding**: 0dp (inherits from card)
- **Title**:
  - Font: 16sp, Regular
  - Color: #333333 (normal) or #999999 (completed, strikethrough)
  - Max lines: 2
  - Ellipsis: ... if overflow
- **Metadata Row**:
  - Font: 12sp, Regular
  - Color: #999999
  - Icons: 14x14dp
  - Format: [Calendar Icon] Date [Clock Icon] Time [Repeat Icon] (if recurring)
  - Spacing: 8dp between items

###### Star Icon
- **Size**: 24x24dp
- **Color**: #FFC107 (important) or #CCCCCC (normal)
- **Margin**: 8dp right

###### More Menu
- **Icon**: Three-dot vertical
- **Size**: 24x24dp
- **Color**: #666666
- **Margin**: 12dp right

##### Card Styling
- **Padding**: 12dp
- **Margin**: 8dp horizontal, 6dp vertical
- **Background**: #FFFFFF
- **Border Radius**: 8dp
- **Shadow**: Elevation 1dp (0dp 1dp 3dp rgba(0,0,0,0.12))
- **Border**: 1px #E8E8E8 (optional, subtle)

##### Completed Task Styling
- **Title**: Strikethrough, #999999
- **Opacity**: 50%
- **Card Background**: #F9F9F9

##### Task Item Container
- **Width**: 100% - 32dp padding
- **Max Width**: 600px (estimated)
- **Margin**: Auto (center on wide screens)

##### Empty State
- **Icon**: Clipboard/notepad outline, 64x64dp, #CCCCCC
- **Title**: "No list was found. Please create a to-do list."
- **Font**: 16sp, Regular
- **Color**: #999999
- **Alignment**: Center, vertical middle

#### Specifications - Bottom Input Bar

##### Layout
- **Height**: 64dp
- **Position**: Fixed bottom
- **Background**: White
- **Padding**: 8dp all sides
- **Border Top**: 1dp, #E0E0E0
- **Elevation**: 2dp

##### Components
```
Horizontal Layout:
[Input Field] [Icons Spacer] [Checkmark Button]
```

###### Input Field
- **Width**: Flex (100% - 80dp)
- **Height**: 48dp
- **Border Radius**: 24dp (pill shape)
- **Background**: #F5F5F5
- **Padding**: 12dp horizontal
- **Font**: 14sp, Regular
- **Placeholder**: "Add a note"
- **Color**: #333333
- **Placeholder Color**: #999999
- **Border**: 1dp, #E0E0E0 (optional)

###### Icon Section
- **Width**: 48dp
- **Icons**: Calendar, Bell, Tag (3x 24x24dp)
- **Spacing**: 8dp between icons
- **Color**: #666666
- **Action**: Open date picker, reminder picker, tag selector (optional)

###### Checkmark Button
- **Size**: 48x48dp
- **Background**: #5C4EAE (enabled) or #CCCCCC (disabled)
- **Icon**: Checkmark, 24x24dp, White
- **Border Radius**: 50%
- **Action**: Submit task

#### Specifications - Toast Notification
- **Text**: "[n] task moved"
- **Background**: #323232 (dark)
- **Text Color**: White
- **Font**: 14sp, Regular
- **Padding**: 12dp
- **Margin Bottom**: 12dp
- **Close Icon**: X, 24x24dp, white
- **Duration**: 4 seconds auto-dismiss
- **Position**: Bottom center, above input bar

---

### 8.6 IMPORTANT LIST SCREEN

**File**: `important.png`

#### Differences from Main List
- **List Title**: "⭐ Important"
- **Background**: Dark teal/cyan
- **Task Items**: Same layout, but all have star filled
- **Filter**: Only tasks with important flag = true
- **Empty State**: "No important tasks"

---

### 8.7 REMINDER LIST SCREEN

**File**: `remainder.png`

#### Differences from Main List
- **List Title**: "🔔 Reminder"
- **Background**: Ocean/water theme
- **Task Items**: Same layout, tasks with reminders
- **Filter**: Only tasks with reminder set
- **Empty State**: "No reminders set"

---

### 8.8 BIN SCREEN

**File**: `bin.png`

#### Layout
```
Main Content Area (same as other lists, but pink tinted):
[Bin Header] 🗑️ Bin
[Task Items]

Task Item Context Menu:
- ✓ Mark as completed
- ✎ Edit note
- → Move notes to [submenu]
- ↺ Restore note
- 🗑️ Delete permanently (red color)
```

#### Specifications
- **Background**: Light pink/coral (#F3E5F5)
- **Header Icon**: Trash/delete
- **Header Color**: #F44336 (red)
- **Task Items**: Same layout as main list
- **Context Menu**: Different options (restore, permanent delete)
- **Metadata**: Date deleted (optional)

#### Behavior
- Soft-deleted tasks appear here
- Long-press or three-dot menu shows context menu
- "Restore note" returns task to original list
- "Delete permanently" removes irreversibly
- Optional: Auto-purge deleted tasks after 30 days

---

### 8.9 MY NOTES / EMPTY STATE SCREEN

**File**: `mynotes.png`

#### Layout
```
Sidebar (same as main)

Content Area:
[Header: 👤 My notes]
[Background Image]
[Centered Empty State]:
  [Icon]
  "No list was found. Please create
   a to-do list."
  [+ Button]
```

#### Specifications
- **Empty State Icon**: Clipboard/notepad, 80x80dp, #CCCCCC
- **Message**: 16sp, Regular, #999999
- **Message Alignment**: Center, both horizontally and vertically
- **Background**: Dark with teal/blue wave pattern
- **Button**: + icon, 56x56dp, #5C4EAE, positioned bottom-right
- **Button Action**: Create new task

---

### 8.10 SEARCH SCREEN (EMPTY STATE)

**File**: `search.png`

#### Layout
```
Header: [Search Input]
Content:
[Icon] "You can search the created notes here"
[Task List Below (empty)]
```

#### Specifications
- **Empty State Icon**: Clipboard/notepad, 80x80dp, #CCCCCC
- **Message**: 16sp, Regular, #999999
- **Position**: Center, vertical middle
- **With Results**: Show matching tasks below, same card layout as main list

---

### 8.11 RENAME LIST DIALOG

**File**: `renamelist.png`

#### Layout
```
Dialog Box:
- Title: "Rename list"
- Text Input: "Review requirements"
- Button: "Rename" (blue, right-aligned)
```

#### Specifications
- **Dialog Width**: 80% screen width (max 400dp)
- **Padding**: 24dp
- **Title**: 20sp, SemiBold, #333333
- **Input Field**:
  - Width: 100%
  - Height: 48dp
  - Padding: 12dp
  - Border: 1dp, #CCCCCC → #5C4EAE (focused)
  - Font: 14sp, Regular
  - Initial Value: Current list name
- **Buttons**:
  - Layout: Right-aligned, horizontal
  - Spacing: 16dp between buttons
  - Cancel Button: Text only, "Cancel"
  - Rename Button: "Rename" (purple text or button)
  - Height: 44dp
- **Backdrop**: Scrim 40% opacity, click to dismiss
- **Validation**: Unique name, not empty

---

### 8.12 PROFILE MENU

**File**: `profile_pressed.png`

#### Layout
```
Dropdown Menu:
- Title: "Hi, John Britto"
- Email: "johnbritto@gmail.com"
- ─────────────────
- Manage accounts
- Settings
- ─────────────────
- Sign out
```

#### Specifications
- **Position**: Top-right, anchored to avatar button
- **Width**: 280dp (estimated)
- **Shadow**: Elevation 3dp
- **Background**: White
- **Border Radius**: 8dp

##### Header Section
- **Background**: #F5F5F5
- **Padding**: 12dp
- **Name**:
  - Font: 16sp, SemiBold
  - Color: #333333
- **Email**:
  - Font: 12sp, Regular
  - Color: #999999

##### Menu Items
- **Height**: 44dp
- **Padding**: 12dp horizontal, 12dp vertical
- **Font**: 14sp, Regular
- **Color**: #333333
- **Hover**: Background #F5F5F5
- **Icons** (optional): 20x20dp, left side
- **Items**:
  1. Manage accounts (link to settings)
  2. Settings (link to settings page)
  3. Sign out (logout action)

##### Dividers
- **Style**: 1dp line
- **Color**: #E0E0E0
- **Margin**: 4dp vertical (no horizontal margin)

---

### 8.13 SIGN OUT CONFIRMATION DIALOG

**File**: `signout.png`

#### Layout
```
Dialog:
- Title: "Are you sure you want to sign out?"
- Message: "You will be signed out of your account."
- [Buttons: Cancel | Sign out]
```

#### Specifications
- **Title**: 18sp, SemiBold, #333333
- **Message**: 14sp, Regular, #666666
- **Dialog Width**: 80% screen (max 400dp)
- **Padding**: 24dp
- **Buttons**:
  - Layout: Horizontal, centered, 16dp spacing
  - Width**: 100% or auto (buttons centered)
  - Cancel Button:
    - Background: Transparent
    - Text: "Cancel", 14sp, Medium, #666666
    - Height: 44dp
  - Sign Out Button:
    - Background: #5C4EAE
    - Text: "Sign out", 14sp, Medium, White
    - Height: 44dp
    - Border Radius: 4dp or 8dp

---

## 9. DATA MODELS

### 9.1 Task Model

```csharp
public class Task
{
    public string Id { get; set; }                    // Unique identifier (GUID)
    public string UserId { get; set; }                // Owner/creator
    public string ListId { get; set; }                // Parent list
    public string Title { get; set; }                 // Task description (max 500)
    public string Description { get; set; }           // Extended details (optional)
    public bool IsCompleted { get; set; }             // Completion flag
    public bool IsImportant { get; set; }             // Star/Important flag
    public DateTime? DueDate { get; set; }             // When task is due
    public DateTime? ReminderTime { get; set; }        // Reminder notification time
    public RecurrencePattern? Recurrence { get; set; } // Repeat pattern
    public int DisplayOrder { get; set; }             // Sort position in list
    public DateTime CreatedAt { get; set; }           // Timestamp
    public DateTime UpdatedAt { get; set; }           // Last modified
    public DateTime? DeletedAt { get; set; }          // Soft delete timestamp
    public List<string> Tags { get; set; }            // Task tags (optional)
}

public enum RecurrencePattern
{
    None,
    Daily,
    Weekly,
    BiWeekly,
    Monthly,
    Yearly
}
```

### 9.2 List Model

```csharp
public class TaskList
{
    public string Id { get; set; }                    // Unique identifier
    public string UserId { get; set; }                // Owner
    public string Name { get; set; }                  // List name
    public string Color { get; set; }                 // Hex color code
    public string Icon { get; set; }                  // Icon name/symbol
    public bool IsSystemList { get; set; }            // Built-in vs custom
    public int DisplayOrder { get; set; }             // Sort position
    public DateTime CreatedAt { get; set; }           // Timestamp
    public List<Task> Tasks { get; set; }             // Navigation property
}
```

### 9.3 User Model

```csharp
public class User
{
    public string Id { get; set; }                    // Unique identifier
    public string Username { get; set; }              // Display name
    public string Email { get; set; }                 // Email (unique)
    public string PasswordHash { get; set; }          // Encrypted password
    public string ProfilePhotoUrl { get; set; }       // Avatar URL (optional)
    public bool IsVerified { get; set; }              // Email verified flag
    public DateTime CreatedAt { get; set; }           // Account creation
    public DateTime LastLoginAt { get; set; }         // Last login timestamp
    public List<TaskList> Lists { get; set; }         // User's task lists
}
```

### 9.4 Authentication Model

```csharp
public class AuthRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public User User { get; set; }
}

public class SignUpRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
```

---

## 10. ARCHITECTURE & MVVM

### 10.1 MVVM Structure

#### ViewModels Layer
- **AppShellViewModel**: Main shell/navigation state
- **AuthenticationViewModel**: Sign In, Sign Up, Forgot Password
- **TaskListViewModel**: Display and manage tasks
- **TaskDetailViewModel**: Edit/create task details
- **ProfileViewModel**: User account management
- **SearchViewModel**: Search functionality

#### Models Layer
- Task, TaskList, User (Domain Models)
- Request/Response DTOs (API Communication)
- Local Data Models (SQLite/Local Storage)

#### Views Layer
- Authentication Views (Sign In, Sign Up, Forgot Password)
- Main Shell View
- Task List Views
- Task Detail Views
- Profile Views
- Dialogs/Popups

#### Services Layer
- **AuthenticationService**: Login, signup, token management
- **TaskService**: CRUD operations for tasks
- **ListService**: Manage task lists
- **SearchService**: Full-text search
- **LocalStorageService**: SQLite persistence
- **SyncService**: Server sync (background)
- **NotificationService**: Toast/alerts
- **DialogService**: Modal dialogs

#### Data Access Layer
- **AppDbContext**: EF Core database
- **Repository Pattern**: Generic IRepository<T>
- **SQLiteConnection**: Local offline-first storage

### 10.2 Dependency Injection Setup

```csharp
// MauiProgram.cs
builder.Services
    .AddScoped<IAuthenticationService, AuthenticationService>()
    .AddScoped<ITaskService, TaskService>()
    .AddScoped<IListService, ListService>()
    .AddScoped<ISearchService, SearchService>()
    .AddScoped<ILocalStorageService, LocalStorageService>()
    .AddScoped<IDialogService, DialogService>()
    .AddScoped<AuthenticationViewModel>()
    .AddScoped<TaskListViewModel>()
    .AddScoped<ProfileViewModel>()
    .AddSingleton<AppShellViewModel>()
    .ConfigureSyncfusionCore();
```

### 10.3 State Management

**Pattern**: ObservableProperty + RelayCommand (MVVM Toolkit)

```csharp
public partial class TaskListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<TaskModel> tasks;
    
    [ObservableProperty]
    private bool isLoading;
    
    [RelayCommand]
    public async Task LoadTasks()
    {
        IsLoading = true;
        var taskList = await taskService.GetTasksAsync(SelectedListId);
        Tasks = new ObservableCollection<TaskModel>(taskList);
        IsLoading = false;
    }
}
```

### 10.4 Navigation Patterns

**Primary Navigation**: ShellContent + Routes  
**Secondary Navigation**: NavigationPage.Push/Pop  
**Modal Dialogs**: DisplayAlert, DisplayPrompt, custom flyouts  

---

## 11. SYNCFUSION CONTROL MAPPING

### 11.1 Recommended Syncfusion Controls

| UI Component | Syncfusion Control | Alternative | Reason |
|--------------|-------------------|-------------|--------|
| Task List | SfListView | CollectionView | Virtualization, item templates, selection modes |
| Search | Entry (MAUI) | - | Simple text input, sufficient |
| Date Picker | SfDateRangePicker | DatePicker | Range selection, calendar UI |
| Sidebar Navigation | CollectionView + Grid | - | Standard MAUI handles this well |
| Dialogs | DisplayPrompt (MAUI) | SfAlertDialog | MAUI native sufficient; Syncfusion more styled |
| Context Menu | Grid + TapGestureRecognizer | - | Custom implementation simpler |

### 11.2 Required NuGet Packages

```xml
<!-- Primary Syncfusion Packages -->
<PackageReference Include="Syncfusion.Maui.ListView" Version="24.1.x" />
<PackageReference Include="Syncfusion.Maui.Calendar" Version="24.1.x" />
<PackageReference Include="Syncfusion.Maui.Core" Version="24.1.x" />
<PackageReference Include="Syncfusion.Maui.Buttons" Version="24.1.x" />

<!-- Standard MAUI + Supporting Libraries -->
<PackageReference Include="Microsoft.Maui.Controls" Version="8.0.x" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.x" />
<PackageReference Include="sqlite-net-pcl" Version="1.8.x" />
<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.x" />
```

### 11.3 Usage Examples

#### SfListView for Task Display
```xaml
<syncfusionListView:SfListView 
    ItemsSource="{Binding Tasks}"
    ItemTemplate="{StaticResource TaskItemTemplate}"
    SelectionMode="None"
    ItemSpacing="6,0"
    ItemSize="66">
</syncfusionListView:SfListView>
```

#### SfDateRangePicker for Due Date
```xaml
<calendar:SfDateRangePicker 
    x:Name="dateRangePicker"
    SelectionMode="Single"
    AllowViewNavigation="True" />
```

---

## 12. RESOURCE STRUCTURE

### 12.1 Project Folder Structure

```
Todo/
├── App.xaml (.cs)
├── AppShell.xaml (.cs)
├── MauiProgram.cs
├── Resources/
│   ├── Styles/
│   │   ├── Colors.xaml
│   │   ├── Sizes.xaml
│   │   ├── Fonts.xaml
│   │   ├── Styles.xaml
│   │   └── Themes.xaml
│   ├── Images/
│   │   ├── logo.png
│   │   ├── backgrounds/
│   │   │   ├── bg_important.png
│   │   │   ├── bg_reminder.png
│   │   │   ├── bg_mynotes.png
│   │   │   └── bg_waves.png
│   │   └── icons/
│   │       ├── ic_menu.png
│   │       ├── ic_search.png
│   │       └── ... (more icons)
│   └── Strings/
│       └── AppStrings.resx (i18n)
├── Views/
│   ├── Authentication/
│   │   ├── SplashPage.xaml
│   │   ├── SignInPage.xaml
│   │   ├── SignUpPage.xaml
│   │   └── ForgotPasswordPage.xaml
│   ├── Main/
│   │   ├── TaskListPage.xaml
│   │   ├── TaskDetailPage.xaml
│   │   └── ProfileMenuPopup.xaml
│   ├── Shared/
│   │   ├── TaskItemTemplate.xaml
│   │   └── ConfirmDialog.xaml
│   └── Dialogs/
│       ├── RenameListDialog.xaml
│       └── DeleteConfirmDialog.xaml
├── ViewModels/
│   ├── AppShellViewModel.cs
│   ├── AuthenticationViewModel.cs
│   ├── TaskListViewModel.cs
│   ├── TaskDetailViewModel.cs
│   └── ProfileViewModel.cs
├── Models/
│   ├── Task.cs
│   ├── TaskList.cs
│   ├── User.cs
│   └── DTOs/
│       ├── AuthRequest.cs
│       └── AuthResponse.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthenticationService.cs
│   │   ├── ITaskService.cs
│   │   ├── IListService.cs
│   │   └── ILocalStorageService.cs
│   ├── Authentication/
│   │   └── AuthenticationService.cs
│   ├── Task/
│   │   ├── TaskService.cs
│   │   └── ListService.cs
│   ├── Storage/
│   │   └── LocalStorageService.cs
│   └── Infrastructure/
│       ├── SyncService.cs
│       ├── NotificationService.cs
│       └── DialogService.cs
├── Data/
│   ├── AppDbContext.cs
│   └── Repositories/
│       ├── IRepository.cs
│       ├── Repository.cs
│       ├── TaskRepository.cs
│       └── ListRepository.cs
├── Converters/
│   ├── BoolToColorConverter.cs
│   ├── DateTimeToStringConverter.cs
│   └── BoolToOpacityConverter.cs
├── Behaviors/
│   └── NoSelectableBehavior.cs
└── Utilities/
    ├── Constants.cs
    ├── Enums.cs
    └── Helpers.cs
```

### 12.2 Resource Dictionary Structure

#### Colors.xaml
```xml
<!-- Primary -->
<Color x:Key="PrimaryColor">#5C4EAE</Color>
<Color x:Key="PrimaryDark">#453685</Color>
<Color x:Key="PrimaryLight">#8A7FD9</Color>

<!-- Surface -->
<Color x:Key="SurfaceColor">#FFFFFF</Color>
<Color x:Key="BackgroundColor">#FAFAFA</Color>

<!-- Semantic -->
<Color x:Key="ErrorColor">#B3261E</Color>
<Color x:Key="SuccessColor">#188A3D</Color>
<Color x:Key="WarningColor">#F57C00</Color>

<!-- Category Colors -->
<Color x:Key="MyNotesColor">#E8E8E8</Color>
<Color x:Key="ImportantColor">#9C27B0</Color>
<Color x:Key="ReminderColor">#2196F3</Color>
<Color x:Key="BinColor">#F44336</Color>

<!-- Text Colors -->
<Color x:Key="TextPrimaryColor">#333333</Color>
<Color x:Key="TextSecondaryColor">#666666</Color>
<Color x:Key="TextTertiaryColor">#999999</Color>

<!-- Border Colors -->
<Color x:Key="BorderColor">#CCCCCC</Color>
<Color x:Key="DividerColor">#E0E0E0</Color>
```

#### Sizes.xaml
```xml
<!-- Spacing -->
<x:Double x:Key="SpacingXXS">4</x:Double>
<x:Double x:Key="SpacingXS">8</x:Double>
<x:Double x:Key="SpacingS">12</x:Double>
<x:Double x:Key="SpacingM">16</x:Double>
<x:Double x:Key="SpacingL">24</x:Double>
<x:Double x:Key="SpacingXL">32</x:Double>

<!-- Icon Sizes -->
<x:Double x:Key="IconSizeSmall">20</x:Double>
<x:Double x:Key="IconSizeMedium">24</x:Double>
<x:Double x:Key="IconSizeLarge">32</x:Double>

<!-- Corner Radius -->
<x:Double x:Key="CornerRadiusSmall">4</x:Double>
<x:Double x:Key="CornerRadiusMedium">8</x:Double>
<x:Double x:Key="CornerRadiusLarge">16</x:Double>
<x:Double x:Key="CornerRadiusFull">50</x:Double>
```

#### Fonts.xaml
```xml
<x:Double x:Key="FontSizeCaption">11</x:Double>
<x:Double x:Key="FontSizeSmall">12</x:Double>
<x:Double x:Key="FontSizeLabel">14</x:Double>
<x:Double x:Key="FontSizeBody">16</x:Double>
<x:Double x:Key="FontSizeTitle">18</x:Double>
<x:Double x:Key="FontSizeHeading">20</x:Double>
<x:Double x:Key="FontSizeLarge">24</x:Double>

<!-- Font Families -->
<FontFamily x:Key="FontFamilyRegular">SegoeUI</FontFamily>
<FontFamily x:Key="FontFamilySemiBold">SegoeUI</FontFamily>

<!-- Font Attributes -->
<FontAttributes x:Key="FontAttributesRegular">None</FontAttributes>
<FontAttributes x:Key="FontAttributesMedium">Bold</FontAttributes>
<FontAttributes x:Key="FontAttributesSemiBold">Bold</FontAttributes>
<FontAttributes x:Key="FontAttributesBold">Bold</FontAttributes>
```

#### Styles.xaml
```xml
<Style TargetType="Button" x:Key="PrimaryButtonStyle">
    <Setter Property="Background" Value="{StaticResource PrimaryColor}" />
    <Setter Property="TextColor" Value="White" />
    <Setter Property="FontSize" Value="{StaticResource FontSizeLabel}" />
    <Setter Property="FontAttributes" Value="Bold" />
    <Setter Property="CornerRadius" Value="24" />
    <Setter Property="Padding" Value="16,12" />
</Style>

<Style TargetType="Entry" x:Key="TextInputStyle">
    <Setter Property="FontSize" Value="{StaticResource FontSizeLabel}" />
    <Setter Property="TextColor" Value="{StaticResource TextPrimaryColor}" />
    <Setter Property="PlaceholderColor" Value="{StaticResource TextTertiaryColor}" />
</Style>

<Style TargetType="Label" x:Key="HeadingStyle">
    <Setter Property="FontSize" Value="{StaticResource FontSizeHeading}" />
    <Setter Property="FontAttributes" Value="Bold" />
    <Setter Property="TextColor" Value="{StaticResource TextPrimaryColor}" />
</Style>
```

---

## 13. VALIDATION & ERROR HANDLING

### 13.1 Validation Rules

#### Email Validation
- Format: user@domain.com (standard email regex)
- Unique: Check against existing users
- Length: 5-254 characters

#### Password Validation
- Minimum length: 6 characters
- Recommended: Uppercase, number, special character
- Display strength indicator (optional)

#### Task Title Validation
- Required: Cannot be empty
- Maximum: 500 characters
- Trim whitespace

#### List Name Validation
- Required: Cannot be empty
- Unique: Within user's lists
- Maximum: 50 characters

#### Due Date Validation
- Cannot be in past (for new tasks)
- Optional field

### 13.2 Error Handling

#### Authentication Errors
```csharp
public enum AuthErrorCode
{
    InvalidEmail,
    InvalidPassword,
    IncorrectCredentials,
    AccountNotVerified,
    AccountLocked,
    ServerError,
    NetworkError
}
```

#### Task Operation Errors
```csharp
public enum TaskErrorCode
{
    TitleRequired,
    ListNotFound,
    TaskNotFound,
    OperationFailed,
    SyncFailed,
    OfflineMode
}
```

#### Error Messages (Localized)
- Display user-friendly messages
- Provide recovery actions when possible
- Log technical errors for debugging
- Implement retry logic for network errors

### 13.3 Try-Catch Patterns

```csharp
public async Task<Result<bool>> CreateTaskAsync(Task task)
{
    try
    {
        var result = await _taskService.CreateTaskAsync(task);
        if (result.IsSuccess)
        {
            await RefreshTasksAsync();
            return Result<bool>.Success(true);
        }
        else
        {
            return Result<bool>.Failure(result.Error);
        }
    }
    catch (NetworkException ex)
    {
        // Offline scenario - save locally
        return Result<bool>.Failure("Network unavailable. Saved locally.");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error: {ex.Message}");
        return Result<bool>.Failure("An error occurred. Please try again.");
    }
}
```

---

## 14. ACCESSIBILITY REQUIREMENTS

### 14.1 WCAG 2.1 AA Compliance

#### Color Contrast
- Normal text: 4.5:1 ratio (min)
- Large text (18sp+): 3:1 ratio (min)
- UI components: 3:1 ratio (min)
- Avoid conveying info through color alone

#### Font Sizes
- Minimum: 12sp (body text)
- Recommended: 14sp or larger
- Adjustable: Support system font scaling

#### Touch Targets
- Minimum: 48x48 dp
- Exceptions: Inline icons (44x44 minimum)
- Spacing: 8dp between adjacent targets

#### Screen Readers (iOS/Android)
- Label all interactive elements
- Use semantic elements
- Test with TalkBack (Android), VoiceOver (iOS)
- Implement AutomationId for testing

#### Keyboard Navigation
- Full Tab support through all interactive elements
- Logical tab order (left-to-right, top-to-bottom)
- Enter/Space for activation
- Escape to dismiss dialogs

#### Focus Indicators
- Visible focus ring (2-3px)
- Color: High contrast (e.g., #000080 on light)
- Not removed or hidden

### 14.2 Implementation Checklist

- [ ] All buttons, links have semantic labels
- [ ] Images have alt text
- [ ] Form labels associated with inputs
- [ ] Error messages linked to inputs
- [ ] Heading hierarchy logical
- [ ] Lists marked as lists
- [ ] Sufficient color contrast
- [ ] Focus indicators visible
- [ ] Keyboard-accessible
- [ ] Screen reader tested

---

## 15. RESPONSIVE DESIGN REQUIREMENTS

### 15.1 Breakpoints

| Category | Width Range | Sidebar | Layout | Column Layout |
|----------|-------------|---------|--------|---------------|
| Phone | 320-480px | Hidden (drawer) | Single column | N/A |
| Phablet | 481-599px | Hidden (drawer) | Single column | N/A |
| Tablet | 600-1024px | Visible, condensed | 2-column | 2 columns |
| Desktop | 1025px+ | Visible, full | 3-column | 3+ columns |

### 15.2 Layout Adjustments

#### Phone (< 600px)
- Sidebar hidden by default (drawer menu)
- Full-width content
- Bottom input bar remains fixed
- Optimized touch targets (48dp+)
- Vertical task item layout

#### Tablet (600-1024px)
- Sidebar visible, narrower (180px)
- Content area flexible
- Task items in vertical list or grid
- Two-column layout possible

#### Desktop (> 1024px)
- Sidebar visible, full width (230px)
- Content centered with max-width
- Multi-column task grids (optional)
- Wider dialogs and modals

### 15.3 Image Scaling
- Use vector formats (SVG) where possible
- Provide @1x, @2x, @3x PNG assets
- Responsive background images (CSS background-size: cover)
- Icon sizing scales with text

### 15.4 Typography Scaling
- Base font size: 14sp (mobile), 16sp (desktop)
- Scale headings proportionally
- Maintain line-height ratio: 1.4-1.6
- Use system font scaling (if user enables)

---

## APPENDIX: DESIGN TOKENS SUMMARY

### A.1 Color Palette

```json
{
  "primary": {
    "main": "#5C4EAE",
    "dark": "#453685",
    "light": "#8A7FD9"
  },
  "semantic": {
    "error": "#B3261E",
    "success": "#188A3D",
    "warning": "#F57C00"
  },
  "category": {
    "myNotes": "#E8E8E8",
    "important": "#9C27B0",
    "reminder": "#2196F3",
    "bin": "#F44336"
  }
}
```

### A.2 Typography

```json
{
  "fonts": {
    "family": "Segoe UI / Roboto",
    "sizes": {
      "caption": 11,
      "small": 12,
      "label": 14,
      "body": 16,
      "title": 18,
      "heading": 20,
      "large": 24
    }
  }
}
```

### A.3 Spacing

```json
{
  "spacing": {
    "xxs": 4,
    "xs": 8,
    "s": 12,
    "m": 16,
    "l": 24,
    "xl": 32
  }
}
```

---

## DOCUMENT HISTORY

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Oct 2026 | AI Assistant | Initial specification |

---

**END OF SPECIFICATION**
