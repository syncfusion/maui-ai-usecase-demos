# Syncfusion To Do List Application - Development Harness

**Version:** 1.0  
**Date:** October 2026  
**Framework:** .NET MAUI  
**Methodology:** Vertical Slice Development with AI-Assisted Implementation

---

## TABLE OF CONTENTS

1. [Overview](#overview)
2. [Development Strategy](#development-strategy)
3. [Vertical Slice Modules](#vertical-slice-modules)
4. [Module Details](#module-details)
5. [Cross-Cutting Concerns](#cross-cutting-concerns)
6. [Validation Checklist](#validation-checklist)
7. [Definition of Done](#definition-of-done)

---

## OVERVIEW

### Purpose
This harness provides a structured roadmap for implementing the Syncfusion To Do List application using vertical slice development. Each module is independently implementable and contains all necessary work to be considered complete.

### Success Criteria
- [ ] All modules implemented
- [ ] All acceptance criteria met
- [ ] All tests passing
- [ ] Code reviewed and approved
- [ ] No outstanding defects
- [ ] Responsive on all breakpoints
- [ ] Accessible (WCAG 2.1 AA)
- [ ] Performance targets met

### Architecture Principles
- **Vertical Slices**: Each module is a complete feature from UI to data
- **MVVM Pattern**: Separation of concerns
- **Dependency Injection**: Loose coupling
- **Local-First**: Offline capability with sync
- **Testability**: Unit and UI tests for each slice

---

## DEVELOPMENT STRATEGY

### Phase Overview

```
PHASE 1: Foundation & Setup (2-3 days)
  ├─ Project Structure
  ├─ MVVM Framework Setup
  ├─ Resource Dictionaries
  ├─ Navigation Infrastructure
  └─ Database/Storage Setup

PHASE 2: Authentication (3-4 days)
  ├─ Splash Screen
  ├─ Sign In
  ├─ Sign Up
  ├─ Forgot Password
  ├─ Token Management
  └─ Auto-Login

PHASE 3: Core Task Management (4-5 days)
  ├─ My Notes List
  ├─ Create/Edit Tasks
  ├─ Complete Tasks
  ├─ Delete/Restore Tasks
  ├─ Task Details
  └─ Bottom Input Bar

PHASE 4: Task Organization (2-3 days)
  ├─ Important/Star Tasks
  ├─ Reminder Filter
  ├─ Bin Management
  └─ Custom Lists

PHASE 5: Advanced Features (2-3 days)
  ├─ Search
  ├─ Move Tasks Between Lists
  ├─ Due Dates & Reminders
  ├─ Recurring Tasks (optional)
  └─ Drag & Drop

PHASE 6: User Profile & Settings (1-2 days)
  ├─ Profile Menu
  ├─ Account Management
  ├─ Settings
  └─ Sign Out

PHASE 7: Polish & Testing (2-3 days)
  ├─ Responsive Design Testing
  ├─ Accessibility Testing
  ├─ Performance Optimization
  ├─ Bug Fixes
  └─ Launch Preparation
```

### Implementation Order
Modules are ordered to minimize cross-module dependencies:

1. **Foundation & Setup** - Required by all other modules
2. **Authentication** - Required before task management
3. **My Notes Module** - Core functionality
4. **Task Management (CRUD)** - Foundation for advanced features
5. **Filtering & Organization** - Depends on tasks existing
6. **Search** - Depends on tasks existing
7. **Profile & Settings** - Independent of core features
8. **Polish & Testing** - All modules available

---

## VERTICAL SLICE MODULES

### Module Matrix

| Module | Phase | Duration | Dependencies | Priority |
|--------|-------|----------|--------------|----------|
| Foundation & Setup | 1 | 2-3d | None | Critical |
| Authentication | 2 | 3-4d | Foundation | Critical |
| My Notes Module | 3 | 2-3d | Auth | High |
| Task CRUD | 3 | 3-4d | My Notes | High |
| Important Module | 4 | 1-2d | Task CRUD | Medium |
| Reminder Module | 4 | 1-2d | Task CRUD | Medium |
| Bin Module | 4 | 1-2d | Task CRUD | Medium |
| Custom Lists | 4 | 1-2d | Task CRUD | Medium |
| Search | 5 | 1-2d | Task CRUD | Low |
| Advanced Features | 5 | 2-3d | Bin, Custom Lists | Low |
| Profile & Settings | 6 | 1-2d | Auth | Low |
| Polish & Testing | 7 | 2-3d | All | Critical |

---

## MODULE DETAILS

---

## MODULE 1: FOUNDATION & SETUP

**Phase:** 1 | **Duration:** 2-3 days | **Priority:** Critical

### Overview
Establish the project structure, MVVM framework, resource dictionaries, and infrastructure needed by all other modules.

### Scope

#### 1.1 Project Structure & NuGet Dependencies
**Tasks:**
- [ ] Create folder structure (Views, ViewModels, Models, Services, Data, Resources, Converters)
- [ ] Add NuGet packages: Syncfusion.Maui.ListView, .Calendar, .Core, .Buttons, CommunityToolkit.MVVM, sqlite-net-pcl
- [ ] Verify package versions compatibility (Syncfusion 24.1.x, MVVM 8.4.x)
- [ ] Build project (should compile without errors)

**Validation:**
- [ ] All folders exist
- [ ] All NuGet packages installed
- [ ] Project compiles
- [ ] No missing dependencies

#### 1.2 Resource Dictionaries
**Tasks:**
- [ ] Create Colors.xaml with color tokens (primary, semantic, categories)
- [ ] Create Sizes.xaml with spacing and sizing tokens
- [ ] Create Fonts.xaml with typography tokens
- [ ] Create Styles.xaml with button, entry, label styles
- [ ] Merge all dictionaries into App.xaml
- [ ] Verify resources accessible across app

**Validation:**
- [ ] All resource files created
- [ ] No missing resource keys
- [ ] Resources compile without errors
- [ ] Can reference in XAML (e.g., Background="{StaticResource PrimaryColor}")

#### 1.3 MVVM Framework Setup
**Tasks:**
- [ ] Create base ViewModelBase class (ObservableObject)
- [ ] Setup CommunityToolkit.MVVM with ObservableProperty and RelayCommand
- [ ] Create INotifyPropertyChanged implementations
- [ ] Setup MauiProgram.cs with DI container
- [ ] Configure Syncfusion core: builder.ConfigureSyncfusionCore()
- [ ] Verify MVVM bindings work in basic test

**Validation:**
- [ ] ViewModelBase compiles
- [ ] ObservableProperty works (test with simple bool property)
- [ ] RelayCommand works (test with button command)
- [ ] DI container resolves services
- [ ] Syncfusion core configured

#### 1.4 Navigation Infrastructure
**Tasks:**
- [ ] Create AppShell.xaml with ShellContent routes
- [ ] Setup route registration in MauiProgram.cs
- [ ] Create AppNavigator helper class for programmatic navigation
- [ ] Implement NavigationPage fallback for modal navigation
- [ ] Create navigation state management in AppShellViewModel
- [ ] Test route navigation (no-op test, just verify routing works)

**Validation:**
- [ ] AppShell.xaml compiles
- [ ] Routes registered successfully
- [ ] Can navigate programmatically
- [ ] Navigation state tracked

#### 1.5 Database & Storage Setup
**Tasks:**
- [ ] Create AppDbContext.cs with EF Core or raw SQLite connection
- [ ] Define DbSet properties for Task, TaskList, User
- [ ] Create migrations folder structure
- [ ] Implement LocalStorageService interface and class
- [ ] Setup SQLite initialization in startup
- [ ] Test database connection and basic CRUD

**Validation:**
- [ ] Database file created on app startup
- [ ] Tables created with correct schema
- [ ] Can insert/read test records
- [ ] Migrations infrastructure ready

#### 1.6 Dependency Injection Configuration
**Tasks:**
- [ ] Register all services in MauiProgram.cs
- [ ] Register all ViewModels with appropriate lifetimes
- [ ] Register database context
- [ ] Setup logging (optional, but recommended)
- [ ] Verify all dependencies resolve

**Validation:**
- [ ] MauiProgram.cs compiles
- [ ] All services registered
- [ ] No circular dependencies
- [ ] Can construct complex objects via DI

#### 1.7 Converters & Utilities
**Tasks:**
- [ ] Create BoolToVisibilityConverter
- [ ] Create BoolToOpacityConverter
- [ ] Create DateTimeToStringConverter
- [ ] Create Constants.cs (app-wide constants)
- [ ] Create Enums.cs (all app enums)
- [ ] Create Helpers.cs (utility methods)

**Validation:**
- [ ] All converters compile
- [ ] Converters can be used in XAML
- [ ] Helper methods work correctly

### User Stories

**US-1.1:** As a developer, I can have a clean, organized project structure so that code is maintainable and follows MVVM best practices.
- **AC:** Project compiles with no errors
- **AC:** All required folders exist
- **AC:** Resource dictionaries organized and merged correctly

**US-1.2:** As an app, I can use a consistent design language across all screens by leveraging centralized resource dictionaries.
- **AC:** Colors, fonts, sizes defined in resources
- **AC:** Resources apply consistently across screens
- **AC:** Design tokens match specification

**US-1.3:** As a developer, I can leverage MVVM Toolkit features (ObservableProperty, RelayCommand) to reduce boilerplate code.
- **AC:** ObservableProperty generates backing fields
- **AC:** RelayCommand auto-generates command execution
- **AC:** Bindings work without manual INotifyPropertyChanged

**US-1.4:** As a user, I can navigate between screens seamlessly using a routing system.
- **AC:** Navigation routes registered
- **AC:** Programmatic navigation works
- **AC:** Shell navigation functional

**US-1.5:** As the app, I can persist user data locally and sync with server when offline first.
- **AC:** SQLite database initialized
- **AC:** CRUD operations work locally
- **AC:** Tables created with correct schema

### Development Tasks

1. **Create project folder structure** (1 task)
   - [ ] Create Models, Views, ViewModels, Services, Data, Resources folders
   - [ ] Create subfolder structure (Authentication, Task, etc.)

2. **Install and verify NuGet packages** (1 task)
   - [ ] Add Syncfusion packages
   - [ ] Add MVVM Toolkit
   - [ ] Add SQLite packages
   - [ ] Run dotnet restore

3. **Implement base MVVM classes** (2 tasks)
   - [ ] Create ViewModelBase with ObservableObject
   - [ ] Create interface definitions for services

4. **Create resource dictionaries** (4 tasks)
   - [ ] Colors.xaml
   - [ ] Sizes.xaml
   - [ ] Fonts.xaml
   - [ ] Styles.xaml

5. **Setup MauiProgram and DI** (1 task)
   - [ ] Configure MauiProgram.cs with all registrations
   - [ ] Add Syncfusion core configuration

6. **Create navigation infrastructure** (2 tasks)
   - [ ] Setup AppShell.xaml
   - [ ] Create AppNavigator helper

7. **Implement database layer** (2 tasks)
   - [ ] Create AppDbContext
   - [ ] Create LocalStorageService

8. **Create utility classes** (1 task)
   - [ ] Converters, Constants, Helpers, Enums

### Testing Tasks

- [ ] Unit test: ViewModelBase observable property notifications
- [ ] Unit test: Converters (BoolToVisibility, DateTimeToString)
- [ ] Integration test: DI container resolves all services
- [ ] Integration test: Database initialization creates tables
- [ ] UI test: Basic navigation works

### Acceptance Criteria

- [ ] Project builds without errors or warnings
- [ ] All NuGet packages installed (appropriate versions)
- [ ] Resource dictionaries compile and merge
- [ ] MVVM framework fully functional
- [ ] Navigation routes registered
- [ ] Database initialized with tables
- [ ] DI container fully configured
- [ ] No compiler errors or missing references

### Definition of Done

**Code Quality:**
- [ ] Code follows MVVM pattern
- [ ] No hardcoded values (use constants/resources)
- [ ] Proper separation of concerns
- [ ] All files in appropriate folders

**Testing:**
- [ ] Project compiles
- [ ] Manual verification: Resources accessible
- [ ] Manual verification: Navigation works
- [ ] Manual verification: Database created

**Documentation:**
- [ ] README.md updated with setup instructions
- [ ] All classes documented with XML comments
- [ ] Constants documented in Constants.cs

---

## MODULE 2: AUTHENTICATION

**Phase:** 2 | **Duration:** 3-4 days | **Priority:** Critical | **Dependencies:** Foundation

### Overview
Implement complete authentication flow: Splash, Sign In, Sign Up, Forgot Password, and token/session management.

### Scope

#### 2.1 Authentication Service
**Tasks:**
- [ ] Create IAuthenticationService interface
- [ ] Implement AuthenticationService with mock or real backend
- [ ] Methods: SignInAsync, SignUpAsync, ForgotPasswordAsync, SignOutAsync
- [ ] Implement token storage (SecureStorage)
- [ ] Implement auto-login logic (RememberMe)
- [ ] Handle authentication errors (mapping, user messages)

**Validation:**
- [ ] Service interface matches specification
- [ ] All methods async and cancellation-supported
- [ ] Error handling with Result pattern
- [ ] Token persistence and retrieval works

#### 2.2 Splash Screen
**Tasks:**
- [ ] Create SplashPage.xaml with centered logo and branding
- [ ] Create SplashPageViewModel
- [ ] Implement auto-navigation logic (check authentication status)
- [ ] Add 2-3 second delay for branding visibility
- [ ] Styling: White background, centered elements

**Validation:**
- [ ] Splash screen displays for 2-3 seconds
- [ ] Auto-navigates to Sign In (not authenticated) or Main (authenticated)
- [ ] Logo and text centered and visible
- [ ] No errors during navigation

#### 2.3 Sign In Screen
**Tasks:**
- [ ] Create SignInPage.xaml (layout per specification)
- [ ] Create SignInPageViewModel with commands
- [ ] Input fields: Email, Password (with eye icon toggle)
- [ ] Remember Me checkbox
- [ ] Forgot Password link
- [ ] Sign In button (disabled until form valid)
- [ ] Social login buttons (Google, Microsoft - placeholders)
- [ ] Sign Up link
- [ ] Validation: Email format, password required
- [ ] Error message display

**Validation:**
- [ ] Layout matches specification
- [ ] Validation works (button disabled if invalid)
- [ ] Error messages display
- [ ] Password toggle works
- [ ] All controls have appropriate sizes

#### 2.4 Sign Up Screen
**Tasks:**
- [ ] Create SignUpPage.xaml (layout per specification)
- [ ] Create SignUpPageViewModel with commands
- [ ] Input fields: Username, Email, Password, ConfirmPassword
- [ ] Sign Up button (validation as Sign In)
- [ ] Social signup buttons (placeholders)
- [ ] Sign In link
- [ ] Validation: Unique username/email, password strength
- [ ] Error message display
- [ ] Success navigation to Main screen

**Validation:**
- [ ] Layout matches specification
- [ ] Validation enforces unique email
- [ ] ConfirmPassword matches Password
- [ ] Error messages clear and helpful
- [ ] Auto-login after successful signup

#### 2.5 Forgot Password Screen
**Tasks:**
- [ ] Create ForgotPasswordPage.xaml (layout per specification)
- [ ] Create ForgotPasswordPageViewModel with commands
- [ ] Input field: Email
- [ ] Send Reset Link button
- [ ] Return to Sign In link
- [ ] Validation: Email format required
- [ ] Success message after submission
- [ ] Navigation back to Sign In

**Validation:**
- [ ] Layout matches specification
- [ ] Email validation works
- [ ] Success message displays
- [ ] Navigation returns to Sign In

#### 2.6 Token Management & Auto-Login
**Tasks:**
- [ ] Implement SecureStorage for tokens
- [ ] Implement refresh token logic (if needed)
- [ ] Implement auto-login with RememberMe
- [ ] Handle expired tokens (redirect to Sign In)
- [ ] Clear tokens on Sign Out

**Validation:**
- [ ] Tokens stored securely
- [ ] Can retrieve tokens on app restart
- [ ] Auto-login works when enabled
- [ ] Tokens cleared on logout

#### 2.7 Sign Out Flow
**Tasks:**
- [ ] Create sign-out confirmation dialog
- [ ] Clear all user data on logout
- [ ] Navigate to Sign In screen
- [ ] Close all open screens/dialogs

**Validation:**
- [ ] Confirmation dialog shows
- [ ] Tokens cleared
- [ ] Navigation to Sign In works
- [ ] No data remains on device (optional deep clean)

### User Stories

**US-2.1:** As a new user, I can create an account so that I can start managing my tasks.
- **AC:** Sign Up form accepts username, email, password
- **AC:** Validation prevents weak passwords and duplicate emails
- **AC:** Successful signup logs in user and navigates to main screen

**US-2.2:** As an existing user, I can sign in with my credentials so that I can access my tasks.
- **AC:** Sign In accepts email and password
- **AC:** Invalid credentials show error message
- **AC:** Successful login navigates to main screen

**US-2.3:** As a user, I can reset my password if I forget it.
- **AC:** Forgot Password accepts email
- **AC:** Reset link sent to email (or success shown)
- **AC:** User can navigate back to Sign In

**US-2.4:** As a user, I can choose to be remembered so that I don't need to sign in on next app launch.
- **AC:** Remember Me checkbox available
- **AC:** When enabled, credentials stored securely
- **AC:** App auto-logs in on next launch

**US-2.5:** As a user, I can sign out of my account so that my data is protected.
- **AC:** Sign Out option available in profile menu
- **AC:** Confirmation dialog shown
- **AC:** Tokens and session data cleared
- **AC:** Redirected to Sign In screen

### Development Tasks

1. **Implement AuthenticationService** (2 tasks)
   - [ ] Create interface and implementation
   - [ ] Implement token storage and retrieval
   - [ ] Mock or connect to real backend

2. **Create Splash Screen** (1 task)
   - [ ] SplashPage.xaml and ViewModel
   - [ ] Auto-navigation logic

3. **Create Sign In Screen** (2 tasks)
   - [ ] SignInPage.xaml with all controls
   - [ ] SignInPageViewModel with validation and commands

4. **Create Sign Up Screen** (2 tasks)
   - [ ] SignUpPage.xaml with all controls
   - [ ] SignUpPageViewModel with validation and unique check

5. **Create Forgot Password Screen** (1 task)
   - [ ] ForgotPasswordPage.xaml
   - [ ] ForgotPasswordPageViewModel

6. **Implement token management** (2 tasks)
   - [ ] Secure token storage
   - [ ] Auto-login logic
   - [ ] Token refresh (if needed)

7. **Implement sign-out flow** (1 task)
   - [ ] Sign-out command
   - [ ] Confirmation dialog
   - [ ] Data cleanup

### Testing Tasks

- [ ] Unit test: Email validation regex
- [ ] Unit test: Password strength validation
- [ ] Unit test: AuthenticationService methods
- [ ] Unit test: Token storage/retrieval
- [ ] UI test: Navigate Splash → Sign In → Sign Up → Main
- [ ] UI test: Sign In with valid/invalid credentials
- [ ] UI test: Forgot Password email submission
- [ ] UI test: Remember Me auto-login

### Acceptance Criteria

- [ ] All authentication screens visible and functional
- [ ] Validation working correctly
- [ ] Navigation flow correct (Splash → Auth → Main)
- [ ] Tokens stored and retrieved securely
- [ ] Auto-login works when enabled
- [ ] Error messages clear and helpful
- [ ] No sensitive data logged or exposed

### Definition of Done

**Code Quality:**
- [ ] ViewModels use MVVM Toolkit (ObservableProperty, RelayCommand)
- [ ] Services abstracted via interfaces
- [ ] Error handling with Result pattern
- [ ] No hardcoded strings (use resources)

**Testing:**
- [ ] Manual verification: Splash displays for 2-3 seconds
- [ ] Manual verification: Sign In works with valid credentials
- [ ] Manual verification: Sign Up creates new account
- [ ] Manual verification: Forgot Password navigates back to Sign In
- [ ] Manual verification: Remember Me works
- [ ] Manual verification: Sign Out clears data

**Accessibility:**
- [ ] All inputs labeled
- [ ] Keyboard navigation works
- [ ] Touch targets minimum 48x48dp
- [ ] Color contrast meets 4.5:1

---

## MODULE 3: MY NOTES LIST MODULE

**Phase:** 3 | **Duration:** 2-3 days | **Priority:** High | **Dependencies:** Auth, Foundation

### Overview
Implement the default "My notes" list display with task items, bottom input bar, and core list navigation.

### Scope

#### 3.1 Task List Page
**Tasks:**
- [ ] Create TaskListPage.xaml with sidebar navigation + main content
- [ ] Create TaskListPageViewModel with bindable properties
- [ ] Implement SfListView for task display
- [ ] Styling per specification (colors, spacing, fonts)
- [ ] Responsive layout (desktop sidebar visible, mobile hidden)
- [ ] Background image support

**Validation:**
- [ ] Page displays with correct layout
- [ ] Sidebar shows navigation items
- [ ] Main content area displays correctly
- [ ] Responsive on phone, tablet, desktop

#### 3.2 Sidebar Navigation
**Tasks:**
- [ ] Create sidebar panel with navigation items
- [ ] Items: My notes, Important, Reminder, Bin, Custom lists
- [ ] Icons and labels for each item
- [ ] Active state styling
- [ ] Count badges (number of items)
- [ ] Create New List button
- [ ] Search box in sidebar
- [ ] Collapsible on mobile (drawer or hidden)

**Validation:**
- [ ] All nav items visible
- [ ] Active state styling works
- [ ] Count badges update
- [ ] Mobile drawer works

#### 3.3 Header Bar
**Tasks:**
- [ ] Create fixed header bar
- [ ] Components: Menu icon, App title, Search icon, Profile avatar
- [ ] Menu icon toggles sidebar (mobile)
- [ ] Profile avatar opens menu
- [ ] Styling per specification

**Validation:**
- [ ] Header displays correctly
- [ ] Menu toggle works (mobile)
- [ ] Profile avatar clickable
- [ ] Icons visible and appropriately sized

#### 3.4 Task Item Template
**Tasks:**
- [ ] Create TaskItemTemplate.xaml data template
- [ ] Layout: Checkbox | Title | Star | More Menu
- [ ] Metadata row: Date, time, recurring indicator
- [ ] Card styling: Border radius, shadow, spacing
- [ ] Completed task styling: Strikethrough, opacity
- [ ] Important task styling: Filled star
- [ ] Hover state (if desktop)

**Validation:**
- [ ] Template renders correctly
- [ ] Checkbox toggles visibly
- [ ] Star shows when important
- [ ] Metadata displays
- [ ] Completed tasks styled differently

#### 3.5 SfListView Integration
**Tasks:**
- [ ] Configure SfListView with TaskItemTemplate
- [ ] Bind ItemsSource to Tasks collection
- [ ] Enable virtualization for performance
- [ ] Set selection mode to None (using tap instead)
- [ ] Add item spacing per specification

**Validation:**
- [ ] List renders multiple items
- [ ] Scrolling works smoothly
- [ ] Items render correctly using template

#### 3.6 Bottom Input Bar
**Tasks:**
- [ ] Create fixed bottom input bar
- [ ] Components: Input field, icons, checkmark button
- [ ] Icons: Calendar, Bell, Tag (optional, may be hidden)
- [ ] Input field placeholder: "Add a note"
- [ ] Checkmark button saves new task
- [ ] Input clears after submission
- [ ] Button disabled when input empty

**Validation:**
- [ ] Input bar displays at bottom
- [ ] Stays in place while scrolling
- [ ] Input field accepts text
- [ ] Checkmark button works

#### 3.7 Empty State
**Tasks:**
- [ ] Create empty state UI (when no tasks)
- [ ] Icon, message, optional create button
- [ ] Styling per specification
- [ ] Shown only when list is empty

**Validation:**
- [ ] Empty state shows when no tasks
- [ ] Creates new task when button clicked
- [ ] Disappears when task added

#### 3.8 Background Image
**Tasks:**
- [ ] Add background image (decorative)
- [ ] Full-bleed coverage (entire main area)
- [ ] Remain behind scrolling content
- [ ] Parallax effect (optional)

**Validation:**
- [ ] Background image visible
- [ ] Stays behind content
- [ ] Not blocked by task items

### User Stories

**US-3.1:** As a user, I can see my tasks in a clean list so that I can stay organized.
- **AC:** Tasks display in SfListView
- **AC:** Task items show title, date, important flag
- **AC:** List scrolls smoothly with 100+ items
- **AC:** Completed tasks visually distinguished

**US-3.2:** As a user, I can navigate between predefined categories so that I can organize my tasks.
- **AC:** Sidebar shows My notes, Important, Reminder, Bin
- **AC:** Clicking category filters tasks
- **AC:** Active category highlighted
- **AC:** Sidebar responsive on mobile

**US-3.3:** As a user, I can quickly add a task from the main screen so that I can capture tasks without friction.
- **AC:** Bottom input bar always visible
- **AC:** Checkmark button creates task
- **AC:** Input clears after submission
- **AC:** New task appears in list

**US-3.4:** As a user, I can see task metadata (date, time, recurrence) so that I know task details at a glance.
- **AC:** Dates display in task item
- **AC:** Recurrence indicator shows
- **AC:** Formatting clear and concise

### Development Tasks

1. **Create TaskListPage** (2 tasks)
   - [ ] TaskListPage.xaml with layout
   - [ ] TaskListPageViewModel

2. **Implement sidebar navigation** (2 tasks)
   - [ ] Sidebar control with nav items
   - [ ] Mobile drawer/collapsible

3. **Create header bar** (1 task)
   - [ ] HeaderBar control with components

4. **Create task item template** (1 task)
   - [ ] TaskItemTemplate.xaml
   - [ ] Styling for all states

5. **Integrate SfListView** (1 task)
   - [ ] Configure SfListView
   - [ ] Test with sample data

6. **Create bottom input bar** (1 task)
   - [ ] BottomInputBar control
   - [ ] Styling and functionality

7. **Implement empty state** (1 task)
   - [ ] EmptyStateView control

8. **Add background image** (1 task)
   - [ ] Setup image asset
   - [ ] Add to page

### Testing Tasks

- [ ] Unit test: Task filtering logic (My notes)
- [ ] Unit test: Task count calculation
- [ ] UI test: Load page, verify layout
- [ ] UI test: Scroll task list, verify smooth performance
- [ ] UI test: Click nav item, verify filter
- [ ] UI test: Type in input field, click checkmark
- [ ] UI test: Verify task appears in list

### Acceptance Criteria

- [ ] Page renders without errors
- [ ] Layout matches specification
- [ ] SfListView displays tasks
- [ ] Bottom input bar functional
- [ ] Sidebar navigation works
- [ ] Responsive on all breakpoints
- [ ] Performance smooth (60 FPS)
- [ ] Empty state appears when needed

### Definition of Done

**Code Quality:**
- [ ] No hardcoded values
- [ ] Resources used for styling
- [ ] MVVM pattern followed
- [ ] Services abstracted

**Testing:**
- [ ] Page navigable from Main
- [ ] Tasks display correctly
- [ ] Input bar works
- [ ] Sidebar responsive

**Accessibility:**
- [ ] Keyboard navigation works
- [ ] Touch targets 48x48dp
- [ ] Color contrast sufficient
- [ ] Screen reader support

---

## MODULE 4: TASK CRUD OPERATIONS

**Phase:** 3 | **Duration:** 3-4 days | **Priority:** High | **Dependencies:** My Notes Module

### Overview
Implement complete Create, Read, Update, Delete operations for tasks.

### Scope

#### 4.1 Task Service
**Tasks:**
- [ ] Create ITaskService interface
- [ ] Implement TaskService with local storage + sync
- [ ] Methods: CreateTaskAsync, UpdateTaskAsync, GetTasksAsync, DeleteTaskAsync, RestoreTaskAsync
- [ ] Implement local repository pattern
- [ ] Handle optimistic updates
- [ ] Error handling and retry logic

**Validation:**
- [ ] All methods async
- [ ] Error handling with Result pattern
- [ ] Local operations work immediately
- [ ] Sync logic implemented

#### 4.2 Create Task Dialog
**Tasks:**
- [ ] Create EditTaskPage.xaml (detailed task form)
- [ ] Create EditTaskPageViewModel
- [ ] Fields: Title (required), Description, Due Date, Reminder, Tags, Recurrence
- [ ] Save and Cancel buttons
- [ ] Validation: Title required, max 500 chars
- [ ] Success notification

**Validation:**
- [ ] Form validates input
- [ ] Save button creates task
- [ ] New task appears in list
- [ ] Success message displays

#### 4.3 Edit Task Dialog
**Tasks:**
- [ ] Reuse EditTaskPage for editing
- [ ] Pre-populate form with existing task data
- [ ] Update existing task on save
- [ ] Validation same as create
- [ ] Success notification

**Validation:**
- [ ] Form pre-populates
- [ ] Edits save to database
- [ ] List updates with changes
- [ ] Success message displays

#### 4.4 Delete Task (Soft Delete)
**Tasks:**
- [ ] Implement soft delete (move to Bin, not permanent)
- [ ] Add DeletedAt timestamp
- [ ] Remove from main lists
- [ ] Show success notification
- [ ] Provide undo option (optional toast)

**Validation:**
- [ ] Task disappears from list
- [ ] Appears in Bin
- [ ] Can be restored
- [ ] Success message shown

#### 4.5 Complete Task (Toggle)
**Tasks:**
- [ ] Toggle completion on checkbox click
- [ ] Update IsCompleted flag
- [ ] Visual feedback (strikethrough, opacity)
- [ ] Task stays in list (configurable)
- [ ] Update list immediately (optimistic)

**Validation:**
- [ ] Checkbox toggles
- [ ] Visual changes
- [ ] Data persisted
- [ ] No delay in UI update

#### 4.6 Task Context Menu
**Tasks:**
- [ ] Create context menu (long press / three-dot menu)
- [ ] Options: Mark complete, Edit, Delete, Mark Important
- [ ] Icons for each option
- [ ] Positioning per specification

**Validation:**
- [ ] Menu appears on trigger
- [ ] All options functional
- [ ] Menu closes after selection
- [ ] Menu closes on dismiss

#### 4.7 Task Persistence
**Tasks:**
- [ ] Save tasks to SQLite
- [ ] Load tasks on app startup
- [ ] Sync with server (background)
- [ ] Handle offline mode
- [ ] Conflict resolution (server wins or user chooses)

**Validation:**
- [ ] Tasks persist across app restarts
- [ ] Offline creation works
- [ ] Sync works when online
- [ ] No data loss

### User Stories

**US-4.1:** As a user, I can create a task with a title so that I can add new items to my list.
- **AC:** Create dialog/form accessible
- **AC:** Title field required
- **AC:** Task saved and appears in list
- **AC:** Success notification shown

**US-4.2:** As a user, I can edit an existing task so that I can update its details.
- **AC:** Long press/menu opens edit dialog
- **AC:** Form pre-populated with existing data
- **AC:** Changes saved and reflected in list
- **AC:** Success notification shown

**US-4.3:** As a user, I can delete a task so that I can remove completed or unwanted items.
- **AC:** Delete option available (three-dot menu or swipe)
- **AC:** Task moved to Bin (soft delete)
- **AC:** Task disappears from main list
- **AC:** Success notification with undo option

**US-4.4:** As a user, I can mark a task as complete so that I can track progress.
- **AC:** Checkbox toggles completion
- **AC:** Completed task shows strikethrough
- **AC:** Completed task has reduced opacity
- **AC:** Change persists

**US-4.5:** As a user, I can access task actions through a context menu so that I can quickly perform common actions.
- **AC:** Menu appears on long press or three-dot click
- **AC:** Options: Complete, Edit, Delete, Important
- **AC:** Each option functional
- **AC:** Menu closes after action

### Development Tasks

1. **Implement TaskService** (2 tasks)
   - [ ] Create interface and implementation
   - [ ] Implement repository/local storage
   - [ ] Implement sync logic

2. **Create EditTaskPage** (2 tasks)
   - [ ] EditTaskPage.xaml with form fields
   - [ ] EditTaskPageViewModel with validation

3. **Implement Create** (1 task)
   - [ ] Create command in TaskListViewModel
   - [ ] Navigate to EditTaskPage
   - [ ] Save task on form submit

4. **Implement Edit** (1 task)
   - [ ] Edit command from context menu
   - [ ] Pre-populate form
   - [ ] Update task on save

5. **Implement Delete (Soft)** (1 task)
   - [ ] Delete command
   - [ ] Soft delete (set DeletedAt)
   - [ ] Remove from UI

6. **Implement Complete Toggle** (1 task)
   - [ ] Complete command
   - [ ] Toggle IsCompleted
   - [ ] Visual feedback

7. **Create context menu** (1 task)
   - [ ] Design and implement menu
   - [ ] Wire up all options

8. **Implement persistence** (2 tasks)
   - [ ] Save to SQLite
   - [ ] Load on startup
   - [ ] Sync logic

### Testing Tasks

- [ ] Unit test: Task creation validation
- [ ] Unit test: Task update logic
- [ ] Unit test: Soft delete (set DeletedAt)
- [ ] Unit test: TaskService CRUD methods
- [ ] UI test: Create task from input bar
- [ ] UI test: Long press task → Edit → Update
- [ ] UI test: Delete task → appears in Bin
- [ ] UI test: Toggle completion → visual change
- [ ] UI test: Context menu all options
- [ ] Integration test: Tasks persist on app restart

### Acceptance Criteria

- [ ] All CRUD operations work
- [ ] Data persists locally
- [ ] UI updates immediately (optimistic)
- [ ] Validation prevents invalid data
- [ ] Context menu functional
- [ ] Undo/restore available

### Definition of Done

**Code Quality:**
- [ ] TaskService follows interface
- [ ] Error handling with Result pattern
- [ ] No SQL injection vulnerabilities
- [ ] Proper async/await usage

**Testing:**
- [ ] Create task → appears in list
- [ ] Edit task → changes visible
- [ ] Delete task → disappears (appears in Bin)
- [ ] Complete task → strikethrough + opacity
- [ ] Tasks persist on restart

**Accessibility:**
- [ ] Context menu keyboard accessible
- [ ] Dialog keyboard navigable
- [ ] Labels on all inputs
- [ ] Error messages clear

---

## MODULE 5: IMPORTANT, REMINDER & BIN MODULES

**Phase:** 4 | **Duration:** 1-2 days each | **Priority:** Medium | **Dependencies:** Task CRUD

### Overview
Implement filtering and special list management for Important, Reminder, and Bin lists.

### MODULE 5A: IMPORTANT MODULE

#### 5A.1 Important Filter
**Tasks:**
- [ ] Filter tasks where IsImportant = true
- [ ] Star icon filled for important tasks
- [ ] Click star to toggle important
- [ ] Visual indicator in list (filled vs outline star)

#### 5A.2 Important List View
**Tasks:**
- [ ] Create ImportantListViewModel (filters tasks)
- [ ] Reuse TaskListPage with different data source
- [ ] Display only important tasks
- [ ] Empty state: "No important tasks"

**Validation:**
- [ ] Only starred tasks show
- [ ] Toggle star removes/adds to Important
- [ ] List updates immediately

### MODULE 5B: REMINDER MODULE

#### 5B.1 Reminder Filtering
**Tasks:**
- [ ] Filter tasks where ReminderTime is set
- [ ] Show reminder indicator (bell icon)
- [ ] Click bell to set/edit reminder

#### 5B.2 Reminder List View
**Tasks:**
- [ ] Create ReminderListViewModel (filters tasks)
- [ ] Display only tasks with reminders
- [ ] Empty state: "No reminders set"
- [ ] Show reminder time in task item

**Validation:**
- [ ] Only tasks with reminders show
- [ ] Reminder time displayed
- [ ] List updates when reminder set/removed

### MODULE 5C: BIN MODULE

#### 5C.1 Bin Filter
**Tasks:**
- [ ] Filter tasks where DeletedAt is not null
- [ ] Display soft-deleted tasks
- [ ] Show deletion date (optional)

#### 5C.2 Restore Task
**Tasks:**
- [ ] Implement restore command (clear DeletedAt)
- [ ] Task returns to original list
- [ ] Success notification

#### 5C.3 Permanent Delete
**Tasks:**
- [ ] Implement permanent delete (remove from database)
- [ ] Confirmation dialog required
- [ ] Cannot undo

#### 5C.4 Bin List View
**Tasks:**
- [ ] Create BinListViewModel (filters soft-deleted tasks)
- [ ] Display soft-deleted tasks
- [ ] Context menu: Restore, Permanent Delete
- [ ] Empty state: "No deleted items"
- [ ] Background color: Light pink/red tint

**Validation:**
- [ ] Only deleted tasks show
- [ ] Restore works
- [ ] Permanent delete works
- [ ] Confirmation shown before permanent delete

### User Stories

**US-5A.1:** As a user, I can mark tasks as important so that I can prioritize my work.
- **AC:** Star icon visible on all tasks
- **AC:** Clicking star toggles important flag
- **AC:** Important tasks appear in Important list
- **AC:** Visual difference (filled star vs outline)

**US-5B.1:** As a user, I can filter tasks by reminder so that I can see upcoming reminders.
- **AC:** Reminder filter available in sidebar
- **AC:** Only tasks with reminders show
- **AC:** Reminder time displayed

**US-5C.1:** As a user, I can recover deleted tasks from Bin so that I can undo accidental deletions.
- **AC:** Deleted tasks appear in Bin
- **AC:** Restore option available
- **AC:** Restored task returns to original list

**US-5C.2:** As a user, I can permanently delete tasks so that I can remove them completely.
- **AC:** Permanent Delete option in Bin
- **AC:** Confirmation dialog required
- **AC:** Task removed from database
- **AC:** Cannot undo permanent delete

### Development Tasks (per module)

**Important Module:**
- [ ] Add IsImportant filter to TaskListViewModel
- [ ] Create ImportantListViewModel
- [ ] Add star toggle command
- [ ] Test filtering and toggle

**Reminder Module:**
- [ ] Add ReminderTime filter to TaskListViewModel
- [ ] Create ReminderListViewModel
- [ ] Display reminder time in task
- [ ] Test filtering

**Bin Module:**
- [ ] Add DeletedAt filter to TaskListViewModel
- [ ] Create BinListViewModel
- [ ] Implement Restore command
- [ ] Implement Permanent Delete command
- [ ] Add confirmation dialog

### Testing Tasks

- [ ] Unit test: Important filter
- [ ] Unit test: Reminder filter
- [ ] Unit test: Bin filter (DeletedAt != null)
- [ ] UI test: Toggle important → appears in Important list
- [ ] UI test: Set reminder → appears in Reminder list
- [ ] UI test: Delete task → appears in Bin
- [ ] UI test: Restore task → returns to original list
- [ ] UI test: Permanent delete → removed completely

### Acceptance Criteria (per module)

**Important:**
- [ ] Tasks with IsImportant=true show in Important list
- [ ] Star toggle works
- [ ] Visual distinction clear

**Reminder:**
- [ ] Tasks with ReminderTime set show in Reminder list
- [ ] Reminder time visible

**Bin:**
- [ ] Soft-deleted tasks show in Bin
- [ ] Restore returns task to original list
- [ ] Permanent delete removes completely
- [ ] Confirmation required before permanent delete

---

## MODULE 6: CUSTOM LISTS MODULE

**Phase:** 4 | **Duration:** 1-2 days | **Priority:** Medium | **Dependencies:** My Notes Module

### Overview
Implement custom list creation, management, and task categorization.

### Scope

#### 6.1 Create Custom List
**Tasks:**
- [ ] Create CreateListDialog.xaml
- [ ] Input: List name, color (optional, color picker)
- [ ] Save button creates new list
- [ ] Validation: Unique name, required
- [ ] New list appears in sidebar
- [ ] Success notification

**Validation:**
- [ ] Dialog displays
- [ ] Input validates
- [ ] List created and appears in sidebar
- [ ] List is selectable

#### 6.2 Rename Custom List
**Tasks:**
- [ ] Reuse dialog or create RenameListDialog
- [ ] Pre-populate with current name
- [ ] Save button updates name
- [ ] Validation: Unique name
- [ ] Sidebar updates immediately

**Validation:**
- [ ] Dialog shows current name
- [ ] Rename works and persists
- [ ] Sidebar updates

#### 6.3 Delete Custom List
**Tasks:**
- [ ] Delete option in context menu or sidebar
- [ ] Confirmation dialog required
- [ ] Move tasks to another list or delete them
- [ ] List removed from sidebar

**Validation:**
- [ ] Confirmation shown
- [ ] List removed
- [ ] Tasks handled appropriately

#### 6.4 List Service
**Tasks:**
- [ ] Create IListService interface
- [ ] Implement ListService with CRUD
- [ ] Methods: CreateListAsync, UpdateListAsync, DeleteListAsync, GetListsAsync
- [ ] Persist to SQLite
- [ ] Sync with server

**Validation:**
- [ ] All methods work
- [ ] Lists persist
- [ ] Lists appear in sidebar

#### 6.5 Display Custom Lists in Sidebar
**Tasks:**
- [ ] Load user's custom lists on app startup
- [ ] Display in sidebar below predefined lists
- [ ] Show count of tasks per list
- [ ] Clicking list filters tasks
- [ ] Active state styling

**Validation:**
- [ ] Custom lists visible in sidebar
- [ ] Count badges show
- [ ] Filtering works
- [ ] Active state visible

### User Stories

**US-6.1:** As a user, I can create custom lists so that I can organize tasks by category or project.
- **AC:** Create button in sidebar
- **AC:** Dialog accepts list name
- **AC:** New list created and appears in sidebar
- **AC:** Tasks can be assigned to custom list

**US-6.2:** As a user, I can rename a list so that I can update it if needed.
- **AC:** Rename option in context menu
- **AC:** Dialog pre-populated with current name
- **AC:** Changes saved and sidebar updates

**US-6.3:** As a user, I can delete a custom list so that I can remove categories I no longer need.
- **AC:** Delete option in context menu
- **AC:** Confirmation dialog required
- **AC:** List removed from sidebar
- **AC:** Tasks moved or deleted appropriately

### Development Tasks

1. **Implement ListService** (2 tasks)
   - [ ] Create interface and implementation
   - [ ] CRUD methods
   - [ ] Persistence to SQLite

2. **Create CreateListDialog** (1 task)
   - [ ] Dialog XAML
   - [ ] ViewModel with validation

3. **Create RenameListDialog** (1 task)
   - [ ] Dialog XAML
   - [ ] Pre-populate current name

4. **Update sidebar** (1 task)
   - [ ] Load custom lists on startup
   - [ ] Display with count badges
   - [ ] Add filtering command

5. **Implement delete** (1 task)
   - [ ] Delete command
   - [ ] Confirmation dialog

### Testing Tasks

- [ ] Unit test: List creation validation
- [ ] Unit test: List rename validation
- [ ] UI test: Create custom list → appears in sidebar
- [ ] UI test: Rename list → sidebar updates
- [ ] UI test: Delete list → confirmation shown
- [ ] Integration test: Tasks can be assigned to custom list

### Acceptance Criteria

- [ ] Custom lists can be created, renamed, deleted
- [ ] Lists persist across app restart
- [ ] Tasks filter by custom list
- [ ] Sidebar updates immediately

---

## MODULE 7: SEARCH MODULE

**Phase:** 5 | **Duration:** 1-2 days | **Priority:** Low | **Dependencies:** Task CRUD

### Overview
Implement full-text search across all tasks.

### Scope

#### 7.1 Search Service
**Tasks:**
- [ ] Create ISearchService interface
- [ ] Implement local full-text search (SQLite FTS or LINQ)
- [ ] Search title and description
- [ ] Case-insensitive matching
- [ ] Return matching tasks sorted by relevance

**Validation:**
- [ ] Search finds matching tasks
- [ ] Search is case-insensitive
- [ ] Results reasonably fast (< 500ms)

#### 7.2 Search UI
**Tasks:**
- [ ] Create SearchResultsPage.xaml
- [ ] Input field in header (or full search page)
- [ ] Display search results in list
- [ ] Empty state: "You can search the created notes here"
- [ ] Clear search button
- [ ] Highlight matching text (optional)

**Validation:**
- [ ] Search input visible
- [ ] Results display correctly
- [ ] Empty state shown when no results
- [ ] Clear button works

#### 7.3 Search Integration
**Tasks:**
- [ ] Add search command to main view
- [ ] Navigate to search page on search icon click
- [ ] Real-time search as user types (debounced)
- [ ] Search across all lists

**Validation:**
- [ ] Clicking search icon navigates to search page
- [ ] Typing updates results
- [ ] Results accurate

### User Stories

**US-7.1:** As a user, I can search for tasks so that I can find specific items quickly.
- **AC:** Search box accessible from header
- **AC:** Entering text shows matching results
- **AC:** Search works across all lists
- **AC:** Results display with task details
- **AC:** Empty state when no matches

### Development Tasks

1. **Implement SearchService** (1 task)
   - [ ] Create interface and implementation
   - [ ] FTS or LINQ search

2. **Create SearchResultsPage** (2 tasks)
   - [ ] SearchResultsPage.xaml
   - [ ] SearchResultsPageViewModel

3. **Integrate search** (1 task)
   - [ ] Add search command to main view
   - [ ] Wire up navigation

### Testing Tasks

- [ ] Unit test: Search filters correctly
- [ ] Unit test: Case-insensitive matching
- [ ] UI test: Search finds tasks
- [ ] UI test: Empty state displayed

### Acceptance Criteria

- [ ] Search finds matching tasks
- [ ] Results display correctly
- [ ] Search responsive (< 500ms)
- [ ] Works across all lists

---

## MODULE 8: ADVANCED FEATURES

**Phase:** 5 | **Duration:** 2-3 days | **Priority:** Low | **Dependencies:** Task CRUD, Custom Lists, Bin

### Overview
Implement drag & drop, move tasks, due dates, reminders, and recurring tasks.

### Scope

#### 8.1 Due Dates
**Tasks:**
- [ ] Update Task model with DueDate field
- [ ] Create date picker dialog
- [ ] Set due date on task
- [ ] Display due date in task item
- [ ] Filter tasks by due date (today, overdue, upcoming)
- [ ] Visual indicator for overdue tasks (red)

**Validation:**
- [ ] Date picker works
- [ ] Due date persisted
- [ ] Visual indicator correct
- [ ] Filtering works

#### 8.2 Reminders
**Tasks:**
- [ ] Update Task model with ReminderTime field
- [ ] Create reminder picker (time selection)
- [ ] Set reminder on task
- [ ] Show notification when reminder triggers
- [ ] Handle background notifications

**Validation:**
- [ ] Reminder picker works
- [ ] Reminder time persisted
- [ ] Notification triggers at time

#### 8.3 Recurring Tasks
**Tasks:**
- [ ] Update Task model with Recurrence field
- [ ] Create recurrence picker (daily, weekly, monthly)
- [ ] Auto-create task after completion
- [ ] Show recurrence indicator
- [ ] Edit recurrence pattern

**Validation:**
- [ ] Recurrence picker works
- [ ] New task created after completion
- [ ] Recurrence indicator visible

#### 8.4 Move Tasks Between Lists
**Tasks:**
- [ ] Add "Move to" option in context menu
- [ ] Submenu with list options
- [ ] Update task's ListId
- [ ] Remove from current list
- [ ] Add to destination list
- [ ] Success notification

**Validation:**
- [ ] Context menu shows move option
- [ ] Task moves to destination list
- [ ] Disappears from source list

#### 8.5 Drag & Drop (Optional)
**Tasks:**
- [ ] Enable SfListView drag & drop
- [ ] Drag task to reorder
- [ ] Drag task to different list (optional)
- [ ] Visual feedback during drag
- [ ] Update DisplayOrder on drop

**Validation:**
- [ ] Drag reorders tasks
- [ ] DisplayOrder updates
- [ ] Changes persist

### User Stories

**US-8.1:** As a user, I can set a due date on tasks so that I know when items are due.
- **AC:** Due date picker accessible
- **AC:** Date persisted and displayed
- **AC:** Overdue tasks highlighted

**US-8.2:** As a user, I can set reminders so that I'm notified before task deadlines.
- **AC:** Reminder picker accessible
- **AC:** Reminder time persisted
- **AC:** Notification triggered at specified time

**US-8.3:** As a user, I can set recurring tasks so that repeating items are automated.
- **AC:** Recurrence pattern selectable
- **AC:** New task created after completion
- **AC:** Recurrence shown in task

**US-8.4:** As a user, I can move tasks between lists so that I can reorganize work.
- **AC:** Move option in context menu
- **AC:** Task moves to destination list
- **AC:** Changes persist

### Development Tasks

1. **Implement due dates** (2 tasks)
   - [ ] Create date picker dialog
   - [ ] Display in task item
   - [ ] Filtering (overdue, upcoming)

2. **Implement reminders** (2 tasks)
   - [ ] Create reminder picker
   - [ ] Notification service
   - [ ] Background notification handling

3. **Implement recurring tasks** (2 tasks)
   - [ ] Create recurrence picker
   - [ ] Auto-create task logic
   - [ ] Display recurrence indicator

4. **Implement move tasks** (1 task)
   - [ ] Add move option to context menu
   - [ ] Move logic and UI

5. **Implement drag & drop** (1 task - optional)
   - [ ] Enable SfListView drag
   - [ ] Update DisplayOrder

### Testing Tasks

- [ ] UI test: Set due date → displayed in task
- [ ] UI test: Set reminder → notification triggered
- [ ] UI test: Set recurrence → new task created after completion
- [ ] UI test: Move task → appears in destination list

### Acceptance Criteria

- [ ] All features implemented
- [ ] Data persists
- [ ] Notifications work
- [ ] UI responsive

---

## MODULE 9: PROFILE & SETTINGS

**Phase:** 6 | **Duration:** 1-2 days | **Priority:** Low | **Dependencies:** Auth

### Overview
Implement user profile, account management, and settings pages.

### Scope

#### 9.1 Profile Menu
**Tasks:**
- [ ] Create ProfileMenuPopup.xaml
- [ ] Display user info: Name, email
- [ ] Menu items: Manage accounts, Settings, Sign out
- [ ] Position: Anchored to profile avatar
- [ ] Click outside closes menu

**Validation:**
- [ ] Menu displays when avatar clicked
- [ ] User info correct
- [ ] All menu items functional

#### 9.2 Account Management Page
**Tasks:**
- [ ] Create AccountManagementPage.xaml
- [ ] Display user profile info
- [ ] Options: Edit profile, change password, delete account
- [ ] Edit profile form
- [ ] Change password form with validation

**Validation:**
- [ ] Profile info displays
- [ ] Can edit profile
- [ ] Can change password
- [ ] Changes persist

#### 9.3 Settings Page
**Tasks:**
- [ ] Create SettingsPage.xaml
- [ ] Options: Theme (light/dark), notifications, language
- [ ] Toggle switches for features
- [ ] Settings persist to preferences

**Validation:**
- [ ] Settings page opens
- [ ] Settings changeable
- [ ] Changes persist

#### 9.4 Sign Out
**Tasks:**
- [ ] Sign Out option in profile menu
- [ ] Confirmation dialog
- [ ] Clear all data
- [ ] Navigate to Sign In

**Validation:**
- [ ] Confirmation shown
- [ ] Data cleared
- [ ] Navigation to Sign In works

### User Stories

**US-9.1:** As a user, I can view my profile information so that I know my account details.
- **AC:** Profile menu accessible
- **AC:** User name and email displayed
- **AC:** Account management link available

**US-9.2:** As a user, I can change my password so that I can keep my account secure.
- **AC:** Change password option available
- **AC:** Old password verification required
- **AC:** New password validated
- **AC:** Changes saved

**US-9.3:** As a user, I can access settings so that I can customize the app experience.
- **AC:** Settings page accessible
- **AC:** Theme setting available
- **AC:** Notification preferences available
- **AC:** Changes persist

### Development Tasks

1. **Create ProfileMenuPopup** (1 task)
   - [ ] ProfileMenuPopup.xaml
   - [ ] Wire up menu items

2. **Create AccountManagementPage** (2 tasks)
   - [ ] AccountManagementPage.xaml
   - [ ] AccountManagementPageViewModel

3. **Create SettingsPage** (2 tasks)
   - [ ] SettingsPage.xaml
   - [ ] SettingsPageViewModel

4. **Implement settings persistence** (1 task)
   - [ ] Save to Preferences
   - [ ] Load on startup

### Testing Tasks

- [ ] UI test: Click profile avatar → menu opens
- [ ] UI test: Click manage accounts → page opens
- [ ] UI test: Click settings → page opens
- [ ] UI test: Change setting → persists on restart

### Acceptance Criteria

- [ ] Profile menu functional
- [ ] Account management page works
- [ ] Settings page works
- [ ] Settings persist

---

## MODULE 10: POLISH & TESTING

**Phase:** 7 | **Duration:** 2-3 days | **Priority:** Critical | **Dependencies:** All modules

### Overview
Comprehensive testing, bug fixes, and optimization before launch.

### Scope

#### 10.1 Responsive Design Testing
**Tasks:**
- [ ] Test on phone (320-480px)
- [ ] Test on phablet (481-599px)
- [ ] Test on tablet (600-1024px)
- [ ] Test on desktop (1025px+)
- [ ] Verify layout changes at breakpoints
- [ ] Verify touch targets appropriate for each size
- [ ] Verify navigation responsive

**Validation:**
- [ ] All layouts correct
- [ ] No horizontal scrolling on mobile
- [ ] Navigation appropriate per size
- [ ] Touch targets adequate

#### 10.2 Accessibility Testing
**Tasks:**
- [ ] Run WCAG 2.1 AA checklist
- [ ] Test keyboard navigation on all screens
- [ ] Test with screen reader (TalkBack/VoiceOver)
- [ ] Verify color contrast (4.5:1 min)
- [ ] Verify font sizes (12sp min)
- [ ] Verify focus indicators visible
- [ ] Test high contrast mode

**Validation:**
- [ ] All interactive elements keyboard accessible
- [ ] Screen reader announces labels
- [ ] Color contrast sufficient
- [ ] Font sizes readable
- [ ] High contrast mode works

#### 10.3 Performance Testing
**Tasks:**
- [ ] Measure app launch time (< 2 sec)
- [ ] Measure list scroll performance (60 FPS)
- [ ] Measure search response (< 500ms)
- [ ] Profile memory usage
- [ ] Optimize images
- [ ] Implement virtualization for long lists
- [ ] Cache frequently accessed data

**Validation:**
- [ ] Launch time < 2 seconds
- [ ] Scroll smooth at 60 FPS
- [ ] Search fast (< 500ms)
- [ ] Memory usage reasonable

#### 10.4 Cross-Browser Testing
**Tasks:**
- [ ] Test on iOS 14+
- [ ] Test on Android 10+
- [ ] Test on Windows (UWP)
- [ ] Test on macOS (Mac Catalyst)
- [ ] Verify platform-specific features work
- [ ] Test navigation platform-specific gestures

**Validation:**
- [ ] App works on all platforms
- [ ] Platform gestures work correctly
- [ ] No platform-specific crashes

#### 10.5 Data Security Testing
**Tasks:**
- [ ] Verify tokens stored securely (SecureStorage)
- [ ] Verify no sensitive data in logs
- [ ] Verify no SQL injection vulnerabilities
- [ ] Verify no XSS vulnerabilities
- [ ] Test logout clears all data
- [ ] Test expired token redirect

**Validation:**
- [ ] Tokens encrypted
- [ ] No data breaches in logs
- [ ] No injection vulnerabilities
- [ ] Logout secure

#### 10.6 Bug Fixes & Refinement
**Tasks:**
- [ ] Address reported issues
- [ ] Fix UI inconsistencies
- [ ] Fix animation jank
- [ ] Polish transitions
- [ ] Refine error messages
- [ ] Improve loading states

**Validation:**
- [ ] All reported issues fixed
- [ ] UI consistent
- [ ] Animations smooth
- [ ] Error messages helpful

#### 10.7 Documentation & Release
**Tasks:**
- [ ] Write README.md with setup instructions
- [ ] Document architecture in README
- [ ] Document configuration steps
- [ ] Write user guide (in-app or separate)
- [ ] Update version number
- [ ] Create CHANGELOG.md
- [ ] Tag release in git

**Validation:**
- [ ] Documentation complete
- [ ] Version updated
- [ ] Release tagged

### User Stories

**US-10.1:** As a developer, I can understand the app architecture and setup so that I can maintain and extend it.
- **AC:** README.md describes architecture
- **AC:** Setup instructions clear
- **AC:** Configuration documented

**US-10.2:** As a user on mobile, I can use the app effectively on small screens.
- **AC:** Layout responsive
- **AC:** Touch targets adequate
- **AC:** No content hidden

**US-10.3:** As a user, I can use the app with accessibility features.
- **AC:** Screen reader support
- **AC:** Keyboard navigation
- **AC:** High contrast support

### Development Tasks

1. **Responsive design testing** (2 tasks)
   - [ ] Test all breakpoints
   - [ ] Fix layout issues

2. **Accessibility testing** (2 tasks)
   - [ ] WCAG 2.1 AA audit
   - [ ] Fix accessibility issues

3. **Performance testing** (2 tasks)
   - [ ] Profile app
   - [ ] Optimize bottlenecks

4. **Cross-platform testing** (2 tasks)
   - [ ] Test iOS
   - [ ] Test Android, Windows, macOS

5. **Security review** (1 task)
   - [ ] Security audit
   - [ ] Fix vulnerabilities

6. **Bug fixes & refinement** (2 tasks)
   - [ ] Fix reported issues
   - [ ] Polish UI

7. **Documentation** (1 task)
   - [ ] Write README, guides, release notes

### Testing Tasks

- [ ] Manual responsive design testing
- [ ] Automated WCAG 2.1 audit
- [ ] Performance profiling
- [ ] Cross-platform manual testing
- [ ] Security audit
- [ ] User acceptance testing (UAT)

### Acceptance Criteria

- [ ] All modules functional
- [ ] Responsive on all breakpoints
- [ ] Accessible (WCAG 2.1 AA)
- [ ] Performant (targets met)
- [ ] Secure
- [ ] No critical bugs
- [ ] Documentation complete

### Definition of Done

**Code Quality:**
- [ ] Code reviewed
- [ ] All tests passing
- [ ] No compiler warnings
- [ ] Performance optimized

**Testing:**
- [ ] Responsive design verified
- [ ] Accessibility verified
- [ ] Performance targets met
- [ ] Cross-platform tested
- [ ] UAT passed

**Documentation:**
- [ ] README.md complete
- [ ] Architecture documented
- [ ] Setup instructions clear
- [ ] Release tagged

---

## CROSS-CUTTING CONCERNS

### Error Handling & Logging

**Pattern:** Result<T> pattern with error codes
```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string Error { get; set; }
    public ErrorCode ErrorCode { get; set; }
}
```

**Logging:** Implement ILogger for all services
- Log errors with context
- Log performance metrics
- Don't log sensitive data (passwords, tokens)

### Data Validation

**Pattern:** FluentValidation or custom validators
- Validate on input (UI level)
- Validate on submission (VM level)
- Validate on save (Service level)
- Provide clear error messages

### Navigation & State

**Pattern:** AppShellViewModel manages navigation state
- Track current page
- Track navigation history
- Handle modal dialogs
- Prevent unintended navigation

### Testing Strategy

**Unit Tests:** Services, ViewModels, Converters
- Mock dependencies
- Test happy path and error cases
- Aim for 70%+ coverage

**Integration Tests:** Database, Services
- Test CRUD operations
- Test data persistence
- Test error handling

**UI Tests:** Page navigation, user interactions
- Test critical user flows
- Test responsive design
- Test accessibility

### Localization (i18n)

**Pattern:** Resource strings in RESX files
- All user-facing strings in resources
- Support multiple languages (future-proof)
- Use string keys for DI-friendly access

### Theme Support (Light/Dark)

**Pattern:** AppTheme enum + Resource override
- Light theme default
- Dark theme available
- User preference stored
- Apply on startup

---

## VALIDATION CHECKLIST

### Code Quality Checklist

- [ ] All code follows MVVM pattern
- [ ] Services abstracted via interfaces
- [ ] Dependency injection properly configured
- [ ] No hardcoded values (use resources/constants)
- [ ] Error handling with Result pattern
- [ ] Async/await used correctly
- [ ] Null checks in place
- [ ] XML documentation on public members
- [ ] No compiler warnings
- [ ] Code formatted consistently

### Functional Checklist

- [ ] Splash screen displays
- [ ] Authentication flow complete
- [ ] My Notes list displays tasks
- [ ] Tasks CRUD operations work
- [ ] Important/Reminder/Bin filters work
- [ ] Custom lists work
- [ ] Search works
- [ ] Profile menu accessible
- [ ] Settings accessible
- [ ] Sign out works

### UI/UX Checklist

- [ ] Layout matches specification
- [ ] Spacing and alignment correct
- [ ] Colors applied correctly
- [ ] Typography correct
- [ ] Icons visible and appropriately sized
- [ ] Responsive on phone/tablet/desktop
- [ ] Smooth scrolling and animations
- [ ] No UI jank or flicker
- [ ] Empty states display correctly
- [ ] Loading states show when needed
- [ ] Error messages clear and helpful

### Performance Checklist

- [ ] App launch < 2 seconds
- [ ] List scroll 60 FPS
- [ ] Search < 500ms
- [ ] Memory usage reasonable
- [ ] No memory leaks
- [ ] Virtualization enabled for long lists
- [ ] Images optimized
- [ ] Offline mode works

### Accessibility Checklist

- [ ] Color contrast 4.5:1 (minimum)
- [ ] Font sizes 12sp or larger
- [ ] Touch targets 48x48dp
- [ ] Keyboard navigation works
- [ ] Screen reader support
- [ ] Focus indicators visible
- [ ] No color-only information
- [ ] Error messages linked to inputs
- [ ] Form labels present
- [ ] High contrast mode works

### Security Checklist

- [ ] Tokens stored securely
- [ ] No sensitive data in logs
- [ ] No SQL injection vulnerabilities
- [ ] No XSS vulnerabilities
- [ ] HTTPS used for API calls
- [ ] Password validation enforced
- [ ] Logout clears all data
- [ ] Expired tokens handled
- [ ] Input validation on all forms

### Data Persistence Checklist

- [ ] Tasks persist across app restart
- [ ] User session persists (RememberMe)
- [ ] Settings persist
- [ ] Offline changes sync when online
- [ ] No data loss on logout
- [ ] No data loss on app crash (for unsaved)
- [ ] Deleted items recoverable (Bin)

---

## DEFINITION OF DONE

### Per Module

**Each module is complete when:**

1. **Code Complete**
   - [ ] All features implemented
   - [ ] All validation in place
   - [ ] Error handling implemented
   - [ ] Code reviewed
   - [ ] No compiler warnings

2. **Testing Complete**
   - [ ] Unit tests written and passing
   - [ ] Integration tests passing
   - [ ] UI tests for critical flows
   - [ ] Manual testing completed
   - [ ] Bug fixes verified

3. **Documentation Complete**
   - [ ] Public APIs documented
   - [ ] Complex logic explained in comments
   - [ ] README updated if needed
   - [ ] Known limitations documented

4. **Quality Standards Met**
   - [ ] Code follows MVVM pattern
   - [ ] No hardcoded values
   - [ ] Proper error handling
   - [ ] No performance regressions
   - [ ] Accessibility standards met

5. **Acceptance Criteria Met**
   - [ ] All user stories satisfied
   - [ ] All AC items verified
   - [ ] Stakeholder approval (if applicable)

### Final Release Criteria

**Before shipping, verify:**

- [ ] All modules complete
- [ ] All tests passing
- [ ] No critical bugs
- [ ] Performance targets met
- [ ] Accessibility verified (WCAG 2.1 AA)
- [ ] Security audit passed
- [ ] Documentation complete
- [ ] Version bumped
- [ ] Release notes written
- [ ] Changelog updated
- [ ] Git release tagged

---

## TIMELINE ESTIMATE

| Phase | Duration | Start | End |
|-------|----------|-------|-----|
| 1: Foundation | 2-3d | Day 1 | Day 3 |
| 2: Authentication | 3-4d | Day 4 | Day 7 |
| 3: Core Tasks | 5-6d | Day 8 | Day 13 |
| 4: Organization | 4-5d | Day 14 | Day 18 |
| 5: Advanced | 3-4d | Day 19 | Day 22 |
| 6: Profile | 1-2d | Day 23 | Day 24 |
| 7: Polish | 2-3d | Day 25 | Day 27 |
| **Total** | **20-27 days** | **Day 1** | **Day 27** |

*Note: Timeline assumes full-time development with AI assistance. Actual timeline depends on team size, experience, and external factors.*

---

## MONITORING & PROGRESS TRACKING

### Metrics to Track

- **Code Coverage**: Unit test coverage (target: 70%+)
- **Build Health**: Successful builds, warning count
- **Performance**: App launch time, list scroll FPS, search latency
- **Accessibility**: WCAG failures remaining
- **Defect Count**: Open bugs by severity

### Status Reporting

**Daily:**
- Completed tasks
- Blockers/issues
- Plan for next day

**Weekly:**
- Modules completed
- Progress vs. timeline
- Key metrics
- Risks/issues

**Release:**
- Final checklist
- Sign-off
- Launch readiness

---

**END OF HARNESS**
