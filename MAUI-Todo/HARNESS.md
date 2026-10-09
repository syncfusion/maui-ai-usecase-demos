# MAUI To Do List Application - Development Harness

**Version:** 1.0  
**Status:** Ready for Development  
**Framework:** .NET MAUI with Syncfusion Controls  
**Architecture:** MVVM + Layered Services  

---

## Table of Contents

1. [Development Overview](#development-overview)
2. [Project Structure](#project-structure)
3. [Setup & Environment](#setup--environment)
4. [Development Phases](#development-phases)
5. [Vertical Slices / Feature Modules](#vertical-slices--feature-modules)
6. [Task Breakdown by Module](#task-breakdown-by-module)
7. [Acceptance Criteria](#acceptance-criteria)
8. [Definition of Done](#definition-of-done)
9. [Validation Checklist](#validation-checklist)
10. [Dependency Graph](#dependency-graph)

---

## Development Overview

### Approach

The application is being developed using an **AI-driven, task-oriented workflow** with emphasis on:

- **Vertical Slices:** Complete, independent feature areas
- **Minimal Cross-Dependencies:** Each module can be implemented separately
- **Modular Testing:** Each slice has its own acceptance criteria
- **Incremental Delivery:** Features completed in priority order

### Phases

**Phase 1:** Foundation & Infrastructure
**Phase 2:** Authentication Module
**Phase 3:** Core Task Management
**Phase 4:** Smart Views & Filtering
**Phase 5:** Advanced Features & Polish
**Phase 6:** Testing & Validation

---

## Project Structure

### Directory Layout

```
MAUI-Todo/
├── MAUI-Todo.csproj                          # Main project file
├── SPECIFICATION.md                          # (This document)
├── HARNESS.md                                # (Development harness)
│
├── App.xaml                                  # App-level resources
├── App.xaml.cs                               # App code-behind
├── AppShell.xaml                             # Navigation shell
├── AppShell.xaml.cs
│
├── Resources/
│   ├── Styles/
│   │   ├── Colors.xaml
│   │   ├── Fonts.xaml
│   │   ├── Spacing.xaml
│   │   ├── ButtonStyles.xaml
│   │   ├── InputStyles.xaml
│   │   ├── TextStyles.xaml
│   │   └── ControlStyles.xaml
│   ├── Themes/
│   │   ├── LightTheme.xaml
│   │   └── DarkTheme.xaml
│   ├── Converters/
│   │   ├── BoolToVisibilityConverter.cs
│   │   ├── DateTimeToStringConverter.cs
│   │   ├── ReminderTypeToStringConverter.cs
│   │   └── RecurringTypeToStringConverter.cs
│   └── Templates/
│       ├── TaskItemTemplate.xaml
│       ├── ListItemTemplate.xaml
│       └── EmptyStateTemplate.xaml
│
├── Models/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Task.cs
│   │   ├── TaskList.cs
│   │   └── Enums.cs
│   └── ViewModels/
│       ├── BaseViewModel.cs
│       ├── SplashViewModel.cs
│       ├── SignInViewModel.cs
│       ├── SignUpViewModel.cs
│       ├── ForgotPasswordViewModel.cs
│       ├── MyNotesViewModel.cs
│       ├── ImportantViewModel.cs
│       ├── RemindersViewModel.cs
│       ├── BinViewModel.cs
│       ├── TaskDetailsViewModel.cs
│       ├── TaskCreationViewModel.cs
│       ├── TaskEditingViewModel.cs
│       └── CustomListsViewModel.cs
│
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── ITaskService.cs
│   │   ├── ITaskListService.cs
│   │   ├── INotificationService.cs
│   │   ├── INavigationService.cs
│   │   ├── IStorageService.cs
│   │   └── ILocalDataService.cs
│   ├── Implementations/
│   │   ├── AuthService.cs
│   │   ├── TaskService.cs
│   │   ├── TaskListService.cs
│   │   ├── NotificationService.cs
│   │   ├── NavigationService.cs
│   │   ├── StorageService.cs
│   │   └── LocalDataService.cs
│   └── Repositories/
│       ├── UserRepository.cs
│       ├── TaskRepository.cs
│       └── TaskListRepository.cs
│
├── Views/
│   ├── Authentication/
│   │   ├── SplashView.xaml
│   │   ├── SplashView.xaml.cs
│   │   ├── SignInView.xaml
│   │   ├── SignInView.xaml.cs
│   │   ├── SignUpView.xaml
│   │   ├── SignUpView.xaml.cs
│   │   ├── ForgotPasswordView.xaml
│   │   └── ForgotPasswordView.xaml.cs
│   ├── Tasks/
│   │   ├── MyNotesView.xaml
│   │   ├── MyNotesView.xaml.cs
│   │   ├── ImportantView.xaml
│   │   ├── ImportantView.xaml.cs
│   │   ├── RemindersView.xaml
│   │   ├── RemindersView.xaml.cs
│   │   ├── BinView.xaml
│   │   ├── BinView.xaml.cs
│   │   ├── TaskDetailsView.xaml
│   │   ├── TaskDetailsView.xaml.cs
│   │   ├── TaskCreationView.xaml
│   │   ├── TaskCreationView.xaml.cs
│   │   ├── TaskEditingView.xaml
│   │   └── TaskEditingView.xaml.cs
│   ├── Lists/
│   │   ├── CustomListsView.xaml
│   │   ├── CustomListsView.xaml.cs
│   │   ├── ListDetailView.xaml
│   │   └── ListDetailView.xaml.cs
│   ├── Dialogs/
│   │   ├── ConfirmationDialog.xaml
│   │   ├── ConfirmationDialog.xaml.cs
│   │   ├── CreateListDialog.xaml
│   │   ├── CreateListDialog.xaml.cs
│   │   ├── MoveTaskDialog.xaml
│   │   └── MoveTaskDialog.xaml.cs
│   └── Components/
│       ├── TaskCardComponent.xaml
│       ├── TaskCardComponent.xaml.cs
│       ├── InputFieldComponent.xaml
│       ├── InputFieldComponent.xaml.cs
│       ├── EmptyStateComponent.xaml
│       ├── EmptyStateComponent.xaml.cs
│       ├── SnackbarComponent.xaml
│       └── SnackbarComponent.xaml.cs
│
├── Platforms/
│   ├── Android/
│   │   ├── AndroidManifest.xml
│   │   └── MainActivity.cs
│   ├── iOS/
│   │   ├── Info.plist
│   │   └── AppDelegate.cs
│   └── Windows/
│       └── App.xaml
│
└── MauiProgram.cs                            # Dependency injection setup
```

---

## Setup & Environment

### Prerequisites

- .NET 8.0 SDK or later
- MAUI workload: `dotnet workload install maui`
- Visual Studio 2022 or VS Code with C# extension
- Git for version control

### NuGet Dependencies

```xml
<ItemGroup>
    <!-- MAUI Core -->
    <PackageReference Include="Microsoft.Maui.Controls" Version="8.0.0" />
    <PackageReference Include="Microsoft.Maui.Controls.Hosting" Version="8.0.0" />
    
    <!-- Syncfusion Controls -->
    <PackageReference Include="Syncfusion.Maui.Controls" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.Core" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.Buttons" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.Inputs" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.ListView" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.Popups" Version="25.1.37" />
    
    <!-- MVVM & Dependency Injection -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    
    <!-- Data Persistence -->
    <PackageReference Include="sqlite-net-pcl" Version="1.10.0" />
    
    <!-- Utilities -->
    <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="4.7.0" />
</ItemGroup>
```

### Initial Setup Commands

```bash
# Create new MAUI project (if starting fresh)
dotnet new maui -n MAUI-Todo

# Or restore existing project
cd MAUI-Todo
dotnet restore

# Build project
dotnet build

# Run on Android emulator
dotnet build -t Run -f net8.0-android

# Run on iOS simulator
dotnet build -t Run -f net8.0-ios

# Run on Windows
dotnet build -t Run -f net8.0-windows10.0.19041
```

---

## Development Phases

### Phase 1: Foundation & Infrastructure (2-3 days)

**Objective:** Set up project structure, core services, and infrastructure

**Tasks:**
1. Create project structure and folder organization
2. Configure MauiProgram.cs with DI and Syncfusion setup
3. Create base classes: BaseViewModel, BaseService
4. Implement data models: User, Task, TaskList, Enums
5. Create Resource Dictionaries: Colors, Fonts, Spacing, Styles
6. Set up converters: BoolToVisibility, DateTime, Enums
7. Configure SQLite database and repositories
8. Create navigation service

**Acceptance Criteria:**
- [ ] Project builds without errors
- [ ] All dependencies installed successfully
- [ ] Project structure matches design
- [ ] DI container configured and functional
- [ ] Base classes created and tested
- [ ] Database schema initialized
- [ ] Resource dictionaries accessible

---

### Phase 2: Authentication Module (2-3 days)

**Objective:** Implement complete authentication flow

**Vertical Slice:** All authentication-related screens and services

**Tasks:**

#### 2.1 Auth Service Implementation
- [ ] Create IAuthService interface with methods: SignIn, SignUp, ForgotPassword, Logout
- [ ] Implement AuthService with mock/dummy backend
- [ ] Create password hashing utilities
- [ ] Implement token management (local storage)

#### 2.2 Authentication Models
- [ ] Create User entity model
- [ ] Create AuthRequest/AuthResponse DTOs
- [ ] Create validation models

#### 2.3 Splash Screen
- [ ] Create SplashView.xaml with logo, app name, loading indicator
- [ ] Create SplashViewModel with initialization logic
- [ ] Implement auto-navigation based on auth state
- [ ] Add branding and visual polish

**Task:** Design Splash Screen with Syncfusion SfBusyIndicator

**Acceptance Criteria:**
- [ ] View displays for 2-3 seconds
- [ ] Auto-navigates to Sign In or Dashboard
- [ ] Resources preload successfully
- [ ] Responsive on all screen sizes

#### 2.4 Sign In Screen
- [ ] Create SignInView.xaml with email, password, social buttons
- [ ] Create SignInViewModel with validation and error handling
- [ ] Implement email/password validation
- [ ] Add password toggle (show/hide)
- [ ] Add "Forgot Password" and "Sign Up" navigation
- [ ] Implement loading state during sign-in
- [ ] Add error message display

**Components Used:**
- Syncfusion SfTextInputFieldOutline (email, password)
- Syncfusion SfButton (Sign In, Social)
- Standard XAML controls (Label, StackLayout)

**Acceptance Criteria:**
- [ ] Fields validate correctly
- [ ] Email format validation works
- [ ] Password toggle functions
- [ ] Loading state shown during request
- [ ] Error messages displayed properly
- [ ] Navigation links work
- [ ] Responsive layout on mobile/tablet
- [ ] Touch targets >= 44x44 points

#### 2.5 Sign Up Screen
- [ ] Create SignUpView.xaml with full name, email, password, confirm password
- [ ] Create SignUpViewModel with all validation logic
- [ ] Implement password strength indicator (Syncfusion SfProgressBar)
- [ ] Add real-time validation feedback
- [ ] Add Terms & Conditions checkbox
- [ ] Implement email uniqueness check
- [ ] Add loading and error states

**Components Used:**
- Syncfusion SfTextInputFieldOutline (all inputs)
- Syncfusion SfProgressBar (password strength)
- Syncfusion SfCheckBox (terms)
- Syncfusion SfButton (Create Account)

**Acceptance Criteria:**
- [ ] All fields validate correctly
- [ ] Password strength indicator updates in real-time
- [ ] Passwords must match
- [ ] Email uniqueness checked
- [ ] Terms must be accepted
- [ ] Password strength requirements enforced
- [ ] Create Account button disabled until valid
- [ ] Loading and error states work
- [ ] Navigation to Sign In link works

#### 2.6 Forgot Password Screen
- [ ] Create ForgotPasswordView.xaml with email input
- [ ] Create ForgotPasswordViewModel with email validation
- [ ] Implement "Send Reset Link" functionality (mock)
- [ ] Add success message display
- [ ] Add navigation back to Sign In

**Components Used:**
- Syncfusion SfTextInputFieldOutline (email)
- Syncfusion SfButton (Send Reset Link)

**Acceptance Criteria:**
- [ ] Email validation works
- [ ] Success message displays after sending
- [ ] Error handling for non-existent email
- [ ] Navigation back to Sign In works
- [ ] Loading state during submission
- [ ] Auto-redirect after success

#### 2.7 AppShell Navigation Setup
- [ ] Configure AppShell.xaml with route registration
- [ ] Set up authentication state check
- [ ] Implement navigation logic (authenticated vs unauthenticated routes)

**Acceptance Criteria:**
- [ ] Unauthenticated users see auth screens
- [ ] Authenticated users see main app
- [ ] Navigation between screens works
- [ ] Back navigation works correctly

#### 2.8 Auth Module Integration & Testing
- [ ] End-to-end test: Sign Up flow
- [ ] End-to-end test: Sign In flow
- [ ] End-to-end test: Forgot Password flow
- [ ] Test navigation between screens
- [ ] Verify local storage of auth token
- [ ] Test session persistence

**Acceptance Criteria:**
- [ ] All auth flows work end-to-end
- [ ] Data persists correctly
- [ ] Navigation smooth and responsive
- [ ] No crashes or unhandled exceptions

---

### Phase 3: Core Task Management (3-4 days)

**Objective:** Implement task CRUD operations and main task list view

**Vertical Slice:** Task creation, editing, deletion, and My Notes view

**Tasks:**

#### 3.1 Task Service & Repository
- [ ] Create ITaskService interface
- [ ] Implement TaskService with CRUD operations
- [ ] Create TaskRepository for database access
- [ ] Implement task search and filtering logic
- [ ] Add sorting functionality (recent, alphabetical, due date)

#### 3.2 Data Models & Entities
- [ ] Create Task entity model with all properties
- [ ] Create TaskList model
- [ ] Create enums: ReminderType, RecurringType
- [ ] Create database migration/schema

#### 3.3 My Notes View (Main Dashboard)
- [ ] Create MyNotesView.xaml with task list UI
- [ ] Create MyNotesViewModel with ObservableCollection<Task>
- [ ] Implement Syncfusion SfListView for task display
- [ ] Add search functionality with real-time filtering
- [ ] Add sort dropdown (Recent, Oldest, Alphabetical, Due Date)
- [ ] Add filter dropdown (All, Completed, Incomplete, Overdue)
- [ ] Implement task card template
- [ ] Add empty state message when no tasks
- [ ] Add Floating Action Button for adding task

**Components Used:**
- Syncfusion SfListView (task list)
- Syncfusion SfTextInputFieldOutline (search)
- Syncfusion SfComboBox (sort/filter)
- Syncfusion SfButton (FAB)
- Custom TaskCardComponent

**Acceptance Criteria:**
- [ ] List displays all tasks
- [ ] Search filters tasks in real-time
- [ ] Sort options work correctly
- [ ] Filter options work correctly
- [ ] Empty state displays when no tasks
- [ ] FAB navigates to task creation
- [ ] Task cards show all relevant info
- [ ] Responsive on all screen sizes
- [ ] List scrolls smoothly
- [ ] Performance acceptable with 100+ tasks

#### 3.4 Task Creation Screen
- [ ] Create TaskCreationView.xaml with all input fields
- [ ] Create TaskCreationViewModel with full validation
- [ ] Implement title input (required)
- [ ] Implement description input (optional, multi-line)
- [ ] Implement list selector (Syncfusion SfComboBox)
- [ ] Implement due date picker (Syncfusion SfDatePicker)
- [ ] Implement due time picker (Syncfusion SfTimePicker, conditional)
- [ ] Implement reminder toggle + options (Syncfusion SfComboBox)
- [ ] Implement repeat toggle + options (Syncfusion SfComboBox)
- [ ] Implement important toggle (Syncfusion SfCheckBox)
- [ ] Add Save and Cancel buttons
- [ ] Implement form validation and error display

**Components Used:**
- Syncfusion SfTextInputFieldOutline (title, description)
- Syncfusion SfComboBox (list, reminder, repeat)
- Syncfusion SfDatePicker (due date)
- Syncfusion SfTimePicker (due time)
- Syncfusion SfCheckBox (important)
- Syncfusion SfButton (Save, Cancel)

**Acceptance Criteria:**
- [ ] All fields validate correctly
- [ ] Title is required
- [ ] Save button disabled until title filled
- [ ] Date/time optional but related
- [ ] Reminder only valid if date set
- [ ] Form submission creates task in database
- [ ] Cancel returns without saving
- [ ] Navigation back to task list after save
- [ ] Error handling for save failure
- [ ] Responsive layout on all devices

#### 3.5 Task Editing Screen
- [ ] Create TaskEditingView.xaml (extends creation view)
- [ ] Create TaskEditingViewModel (extends creation ViewModel)
- [ ] Pre-populate all fields with existing task data
- [ ] Add Delete Task button (red, destructive)
- [ ] Implement delete with confirmation dialog
- [ ] Implement update functionality
- [ ] Show loading state during update

**Acceptance Criteria:**
- [ ] All fields pre-filled correctly
- [ ] Updates persist to database
- [ ] Delete shows confirmation
- [ ] Delete moves task to Bin
- [ ] Navigation back after save
- [ ] Cancel discards changes

#### 3.6 Task Details Modal/View
- [ ] Create TaskDetailsView.xaml with read-only display
- [ ] Create TaskDetailsViewModel
- [ ] Display all task information
- [ ] Add Edit button → Navigate to editing
- [ ] Add Mark Complete checkbox → Toggle completion
- [ ] Add Delete button → Show confirmation
- [ ] Add Move to List button → Show list selector
- [ ] Add More menu button → Show context options
- [ ] Show metadata: created date, updated date

**Acceptance Criteria:**
- [ ] All task info displays correctly
- [ ] Checkbox toggles completion
- [ ] Edit button opens edit screen
- [ ] Delete shows confirmation
- [ ] Move to list shows selector
- [ ] Modal closes properly
- [ ] All actions work as expected

#### 3.7 Task Context Menu
- [ ] Implement context menu popup
- [ ] Add menu options: Edit, Mark Important, Duplicate, Move, Delete
- [ ] Wire up each menu action
- [ ] Add visual feedback for toggles
- [ ] Implement close behavior (tap outside, item selected)

**Acceptance Criteria:**
- [ ] Menu appears on ⋮ tap
- [ ] All menu items work
- [ ] Menu closes after action
- [ ] Toggle actions show visual feedback
- [ ] Responsive positioning on all screens

#### 3.8 Task Completion & State Management
- [ ] Implement checkbox toggle for task completion
- [ ] Update task state in database
- [ ] Show visual feedback (strikethrough completed tasks)
- [ ] Implement undo completion action
- [ ] Add completion snackbar notification

**Acceptance Criteria:**
- [ ] Checkbox toggles completion
- [ ] Database updates
- [ ] UI updates immediately
- [ ] Completed tasks styled correctly
- [ ] Snackbar shows with undo option

#### 3.9 Core Task Module Testing
- [ ] Test: Create task with all combinations of fields
- [ ] Test: Edit task and verify updates
- [ ] Test: Delete task and verify Bin behavior
- [ ] Test: Complete/incomplete toggle
- [ ] Test: Search, sort, filter combinations
- [ ] Test: Data persistence across app restarts
- [ ] Test: Large task lists (100+ items)
- [ ] Test: Responsive behavior on different screen sizes

**Acceptance Criteria:**
- [ ] All CRUD operations work
- [ ] Data persists correctly
- [ ] UI responsive and performant
- [ ] No crashes or unhandled exceptions
- [ ] Snackbar notifications work

---

### Phase 4: Smart Views & Filtering (2-3 days)

**Objective:** Implement Important, Reminders, and Bin views with specialized filtering

**Vertical Slices:** Each view is independent

#### 4.1 Important View
- [ ] Create ImportantView.xaml (reuses task list template)
- [ ] Create ImportantViewModel (extends task list ViewModel)
- [ ] Implement filtering: show only `isImportant = true`
- [ ] Add unmark as important action in context menu
- [ ] Implement same sort/filter as My Notes

**Acceptance Criteria:**
- [ ] Shows only important tasks
- [ ] All list actions work
- [ ] Unmark important removes from view
- [ ] Navigation works

#### 4.2 Reminders View
- [ ] Create RemindersView.xaml
- [ ] Create RemindersViewModel
- [ ] Implement filtering: show only tasks with reminders
- [ ] Sort by reminder time (default)
- [ ] Implement same list functionality as My Notes

**Acceptance Criteria:**
- [ ] Shows only tasks with reminders
- [ ] Sorted by reminder time
- [ ] All list actions work
- [ ] Navigation works

#### 4.3 Bin / Trash View
- [ ] Create BinView.xaml
- [ ] Create BinViewModel
- [ ] Implement filtering: show only deleted tasks
- [ ] Add Restore action for each task
- [ ] Add Delete Permanently action
- [ ] Add Delete All button (with confirmation)
- [ ] Show deletion date for each task
- [ ] Empty state when trash is empty

**Acceptance Criteria:**
- [ ] Shows only deleted tasks
- [ ] Restore action works
- [ ] Delete Permanently removes from database
- [ ] Delete All with confirmation works
- [ ] Empty state displays correctly

#### 4.4 Custom Lists View
- [ ] Create CustomListsView.xaml showing all lists
- [ ] Create CustomListsViewModel
- [ ] Display built-in lists (My Notes, Important, Reminders, Bin)
- [ ] Display custom lists with task counts
- [ ] Add Create New List button
- [ ] Implement Create List modal dialog
- [ ] Implement Edit/Delete for custom lists
- [ ] Add icon and color picker in create/edit modal

**Components Used:**
- Syncfusion SfListView (lists)
- Syncfusion SfButton (Create New List)
- Custom modal dialogs

**Acceptance Criteria:**
- [ ] All lists display with task counts
- [ ] Tap list → Open list view
- [ ] Create List modal works
- [ ] Edit/Delete custom lists works
- [ ] Icon/color pickers function
- [ ] Responsive layout

#### 4.5 List Detail View
- [ ] Create ListDetailView.xaml (reuses task list template)
- [ ] Create ListDetailViewModel
- [ ] Show tasks for selected list
- [ ] Implement same filters/sort as My Notes
- [ ] Support all task actions

**Acceptance Criteria:**
- [ ] Shows correct list tasks
- [ ] All filtering/sorting works
- [ ] Task actions work
- [ ] Navigation works

#### 4.6 Smart Views Integration & Testing
- [ ] Test navigation between all views
- [ ] Test data consistency across views
- [ ] Test action effects across views (e.g., mark important appears in Important)
- [ ] Test list creation and management
- [ ] Test empty states in each view

**Acceptance Criteria:**
- [ ] All views work correctly
- [ ] Data consistent across views
- [ ] Navigation smooth
- [ ] No crashes

---

### Phase 5: Advanced Features & Polish (2-3 days)

**Objective:** Add reminders, recurring tasks, notifications, and UI refinement

**Tasks:**

#### 5.1 Reminder System
- [ ] Implement reminder scheduling (local notifications)
- [ ] Add reminder time picker in task creation/editing
- [ ] Implement reminder notification on trigger
- [ ] Store reminder preferences in database
- [ ] Test notifications on Android and iOS

**Acceptance Criteria:**
- [ ] Reminders trigger at correct time
- [ ] Notifications display properly
- [ ] User can interact with notification
- [ ] Snooze/dismiss works

#### 5.2 Recurring Tasks
- [ ] Implement recurring task logic
- [ ] Add recurrence selector in task creation/editing
- [ ] Auto-create next recurrence when task completed
- [ ] Display recurrence info in task details
- [ ] Handle edge cases (completion, editing recurring task)

**Acceptance Criteria:**
- [ ] Recurring tasks create next occurrence
- [ ] Recurrence info displays correctly
- [ ] Editing recurring task handled properly

#### 5.3 Notifications & Snackbars
- [ ] Implement INotificationService for snackbars
- [ ] Add snackbar for: task created, task updated, task deleted, task completed, undo options
- [ ] Implement action buttons (undo, retry)
- [ ] Test stacking multiple notifications
- [ ] Implement proper auto-dismiss timing

**Acceptance Criteria:**
- [ ] Snackbars display for user actions
- [ ] Action buttons work (undo)
- [ ] Auto-dismiss works
- [ ] Multiple snackbars handled

#### 5.4 Search Functionality
- [ ] Implement full-text search in task title and description
- [ ] Real-time search filtering
- [ ] Search across all tasks or current view
- [ ] Display search results in list view
- [ ] Clear search functionality

**Acceptance Criteria:**
- [ ] Search finds relevant tasks
- [ ] Real-time filtering works
- [ ] Results are accurate
- [ ] Performance acceptable

#### 5.5 Navigation Menu / Sidebar
- [ ] Create navigation menu/sidebar (platform-specific)
- [ ] Display user profile section (avatar, name, email)
- [ ] List all built-in and custom lists
- [ ] Add Settings and Logout options
- [ ] Implement hamburger menu toggle
- [ ] Support touch and gesture navigation

**Acceptance Criteria:**
- [ ] Menu toggles open/close
- [ ] All navigation items work
- [ ] Menu displays on all screens
- [ ] Responsive on mobile/tablet/desktop
- [ ] Smooth animations

#### 5.6 Settings Screen (Optional, Phase 5)
- [ ] Create Settings page
- [ ] Display app preferences: theme, notifications, language
- [ ] Add logout functionality
- [ ] Add about/help section

**Acceptance Criteria:**
- [ ] Settings display correctly
- [ ] User can toggle options
- [ ] Changes persist
- [ ] Logout works

#### 5.7 UI Polish & Refinement
- [ ] Review all screens against specification
- [ ] Ensure consistent spacing, alignment, typography
- [ ] Verify responsive behavior on all screen sizes
- [ ] Test accessibility (color contrast, touch targets, screen reader)
- [ ] Add animations/transitions for smoothness
- [ ] Review and optimize performance
- [ ] Test on multiple devices/platforms

**Acceptance Criteria:**
- [ ] All screens match specification
- [ ] Responsive on all devices
- [ ] Accessible (WCAG 2.1 AA)
- [ ] Smooth animations
- [ ] Performance metrics acceptable
- [ ] No visual bugs or inconsistencies

#### 5.8 Error Handling & Edge Cases
- [ ] Handle network errors gracefully
- [ ] Handle database errors
- [ ] Handle invalid user input
- [ ] Handle app crashes and recovery
- [ ] Test offline functionality
- [ ] Test data sync when back online

**Acceptance Criteria:**
- [ ] All errors handled gracefully
- [ ] User informed of errors
- [ ] Recovery options provided
- [ ] No data loss on errors

---

### Phase 6: Testing & Validation (1-2 days)

**Objective:** Comprehensive testing, bug fixes, and release preparation

**Tasks:**

#### 6.1 Unit Testing
- [ ] Create unit tests for ViewModels
- [ ] Create unit tests for Services
- [ ] Create unit tests for Converters
- [ ] Create unit tests for Validators
- [ ] Achieve 80%+ code coverage (business logic)

#### 6.2 Integration Testing
- [ ] Test service integration
- [ ] Test database operations
- [ ] Test authentication flow end-to-end
- [ ] Test task CRUD operations end-to-end

#### 6.3 UI/UX Testing
- [ ] Manual testing on iOS simulator
- [ ] Manual testing on Android emulator
- [ ] Manual testing on Windows desktop
- [ ] Test responsive layouts on various screen sizes
- [ ] Test accessibility with screen reader

#### 6.4 Performance Testing
- [ ] Test app startup time
- [ ] Test list loading with 100+ tasks
- [ ] Test search performance
- [ ] Memory usage profiling
- [ ] Battery impact profiling (optional)

#### 6.5 Bug Fixes & Polish
- [ ] Fix all identified bugs
- [ ] Optimize performance bottlenecks
- [ ] Final visual polish
- [ ] Final copy/string review

#### 6.6 Release Preparation
- [ ] Update README and documentation
- [ ] Create release notes
- [ ] Prepare assets (icons, splash screens)
- [ ] Build release APK/IPA
- [ ] Create deployment guide

**Acceptance Criteria:**
- [ ] All tests passing
- [ ] No critical bugs
- [ ] Performance acceptable
- [ ] Documentation complete
- [ ] Ready for release

---

## Vertical Slices / Feature Modules

### Module 1: Authentication (Independent)

**Contains:**
- Splash Screen
- Sign In Screen
- Sign Up Screen
- Forgot Password Screen
- Auth Service
- Auth Repository
- User Model

**Can be completed without:** Any other module
**Depends on:** Foundation Phase infrastructure

**Testing Approach:**
- Unit: AuthService, ViewModel validation
- Integration: Full auth flow, token storage
- UI: Manual testing of each screen

---

### Module 2: Core Task Management (Depends on: Foundation, Authentication)

**Contains:**
- My Notes View
- Task Creation Screen
- Task Editing Screen
- Task Details Modal
- Task Context Menu
- Task Service
- Task Repository
- Task Model

**Can be completed without:** Smart Views, Advanced Features
**Depends on:** Authentication infrastructure

**Testing Approach:**
- Unit: TaskService, ViewModel
- Integration: Full CRUD operations, database persistence
- UI: Manual testing of all screens and interactions

---

### Module 3: Important View (Depends on: Core Task Management)

**Contains:**
- Important Screen
- ViewModel with filtering

**Can be completed without:** Other smart views
**Depends on:** Task infrastructure

---

### Module 4: Reminders View (Depends on: Core Task Management)

**Contains:**
- Reminders Screen
- ViewModel with filtering and sorting

**Can be completed without:** Other smart views
**Depends on:** Task infrastructure

---

### Module 5: Bin View (Depends on: Core Task Management)

**Contains:**
- Bin Screen
- Restore functionality
- Delete permanently functionality
- ViewModel with filtering

**Can be completed without:** Other smart views
**Depends on:** Task infrastructure

---

### Module 6: Custom Lists (Depends on: Core Task Management)

**Contains:**
- Custom Lists Screen
- List Detail Screen
- List Creation Modal
- List Edit Modal
- TaskList Service
- TaskList Repository
- TaskList Model

**Can be completed without:** Other modules (can be integrated with any)
**Depends on:** Task infrastructure

---

### Module 7: Advanced Features (Depends on: Core Task Management)

**Contains:**
- Reminder System
- Recurring Tasks
- Notifications/Snackbars
- Search Functionality
- Navigation Menu

**Can be completed without:** Each feature mostly independent
**Depends on:** Task infrastructure

---

## Task Breakdown by Module

### AUTHENTICATION MODULE

**1. Set Up Auth Service & Models**
- [ ] Create User entity model
- [ ] Create AuthRequest/AuthResponse DTOs
- [ ] Create IAuthService interface
- [ ] Implement AuthService with dummy backend
- [ ] Implement password hashing
- [ ] Set up local token storage
- **Estimated Time:** 2 hours
- **Dependencies:** None
- **Acceptance Criteria:**
  - [ ] Service methods callable
  - [ ] Token storage works
  - [ ] Password hashing functional

**2. Create Splash Screen**
- [ ] Design Splash view layout (XAML)
- [ ] Create SplashViewModel
- [ ] Implement app initialization logic
- [ ] Add auto-navigation based on auth state
- [ ] Test on multiple screen sizes
- **Estimated Time:** 3 hours
- **Dependencies:** Auth Service
- **Acceptance Criteria:**
  - [ ] Displays for 2-3 seconds
  - [ ] Auto-navigates correctly
  - [ ] Responsive layout
  - [ ] Smooth animations

**3. Create Sign In Screen**
- [ ] Design Sign In view layout
- [ ] Create SignInViewModel with validation
- [ ] Implement email/password validation
- [ ] Add password toggle UI
- [ ] Add navigation links (Sign Up, Forgot Password)
- [ ] Implement loading and error states
- [ ] Test all validation rules
- **Estimated Time:** 4 hours
- **Dependencies:** Auth Service, Form inputs (Syncfusion)
- **Acceptance Criteria:**
  - [ ] Email validation works
  - [ ] Password field toggle works
  - [ ] Loading state displays
  - [ ] Error messages shown
  - [ ] Navigation links work
  - [ ] Responsive layout

**4. Create Sign Up Screen**
- [ ] Design Sign Up view layout
- [ ] Create SignUpViewModel with validation
- [ ] Implement all field validations
- [ ] Add password strength indicator
- [ ] Add Terms & Conditions checkbox
- [ ] Implement email uniqueness check (mock)
- [ ] Test all validation combinations
- **Estimated Time:** 4 hours
- **Dependencies:** Auth Service, Syncfusion controls
- **Acceptance Criteria:**
  - [ ] All fields validate
  - [ ] Password strength updates in real-time
  - [ ] Passwords match
  - [ ] Terms must be accepted
  - [ ] Create button disabled until valid
  - [ ] Error handling works

**5. Create Forgot Password Screen**
- [ ] Design Forgot Password view layout
- [ ] Create ForgotPasswordViewModel
- [ ] Implement email validation
- [ ] Add send reset link functionality (mock)
- [ ] Add success message
- [ ] Test navigation back to Sign In
- **Estimated Time:** 2 hours
- **Dependencies:** Auth Service
- **Acceptance Criteria:**
  - [ ] Email validation works
  - [ ] Success message displays
  - [ ] Navigation works
  - [ ] Error handling for missing email

**6. Integrate Auth Module into AppShell**
- [ ] Configure route registration in AppShell
- [ ] Implement authentication state check
- [ ] Set up navigation to auth screens vs dashboard
- [ ] Test navigation flows
- **Estimated Time:** 2 hours
- **Dependencies:** All auth screens
- **Acceptance Criteria:**
  - [ ] Unauthenticated users see auth screens
  - [ ] Authenticated users see dashboard
  - [ ] Navigation smooth

**7. Auth Module End-to-End Testing**
- [ ] Test Sign Up flow
- [ ] Test Sign In flow
- [ ] Test Forgot Password flow
- [ ] Test token persistence
- [ ] Test logout and re-login
- **Estimated Time:** 2 hours
- **Dependencies:** All auth components
- **Acceptance Criteria:**
  - [ ] All flows work end-to-end
  - [ ] Data persists
  - [ ] No crashes

**AUTHENTICATION MODULE TOTAL: ~19 hours**

---

### CORE TASK MANAGEMENT MODULE

**1. Set Up Task Service & Repository**
- [ ] Create Task entity model
- [ ] Create TaskList model
- [ ] Create Enums (ReminderType, RecurringType)
- [ ] Create ITaskService interface
- [ ] Implement TaskService with CRUD operations
- [ ] Set up SQLite database and migrations
- [ ] Implement TaskRepository
- [ ] Test all database operations
- **Estimated Time:** 4 hours
- **Dependencies:** Database setup, models
- **Acceptance Criteria:**
  - [ ] CRUD operations work
  - [ ] Database persists data
  - [ ] Queries work correctly

**2. Create My Notes / Dashboard View**
- [ ] Design task list layout (XAML)
- [ ] Create MyNotesViewModel
- [ ] Implement Syncfusion SfListView
- [ ] Create task card template
- [ ] Add search input with real-time filtering
- [ ] Add sort dropdown
- [ ] Add filter dropdown
- [ ] Add empty state message
- [ ] Add FAB for adding task
- [ ] Implement page navigation
- **Estimated Time:** 5 hours
- **Dependencies:** Task Service, Syncfusion controls
- **Acceptance Criteria:**
  - [ ] List displays all tasks
  - [ ] Search works in real-time
  - [ ] Sort options work
  - [ ] Filter options work
  - [ ] Empty state shows
  - [ ] Responsive layout
  - [ ] Smooth scrolling

**3. Create Task Creation Screen**
- [ ] Design creation form layout
- [ ] Create TaskCreationViewModel
- [ ] Implement title input (required)
- [ ] Implement description input
- [ ] Implement list selector
- [ ] Implement due date picker
- [ ] Implement due time picker (conditional)
- [ ] Implement reminder options
- [ ] Implement repeat options
- [ ] Implement important toggle
- [ ] Add form validation
- [ ] Implement Save/Cancel buttons
- **Estimated Time:** 5 hours
- **Dependencies:** Task Service, Syncfusion controls
- **Acceptance Criteria:**
  - [ ] All validations work
  - [ ] Save creates task
  - [ ] Cancel discards
  - [ ] Navigation works
  - [ ] Form responsive

**4. Create Task Editing Screen**
- [ ] Create editing view (extends creation)
- [ ] Create TaskEditingViewModel
- [ ] Pre-populate fields
- [ ] Add Delete button
- [ ] Implement delete with confirmation
- [ ] Test update functionality
- **Estimated Time:** 3 hours
- **Dependencies:** Task Service, Creation screen
- **Acceptance Criteria:**
  - [ ] Fields pre-filled
  - [ ] Updates work
  - [ ] Delete works
  - [ ] Confirmation shows

**5. Create Task Details Modal**
- [ ] Design details modal layout
- [ ] Create TaskDetailsViewModel
- [ ] Display all task info (read-only)
- [ ] Add Edit button
- [ ] Add Mark Complete checkbox
- [ ] Add Delete button
- [ ] Add Move to List button
- [ ] Add More menu
- [ ] Implement all actions
- **Estimated Time:** 4 hours
- **Dependencies:** Task Service, editing/creation screens
- **Acceptance Criteria:**
  - [ ] All info displays
  - [ ] All buttons work
  - [ ] Modal closes properly
  - [ ] Actions execute correctly

**6. Implement Task Context Menu**
- [ ] Design context menu popup
- [ ] Implement menu options (Edit, Important, Duplicate, Move, Delete)
- [ ] Wire up each action
- [ ] Add visual feedback for toggles
- [ ] Test menu positioning and closing
- **Estimated Time:** 2 hours
- **Dependencies:** Task operations
- **Acceptance Criteria:**
  - [ ] Menu appears correctly
  - [ ] All options work
  - [ ] Visual feedback shows
  - [ ] Menu closes

**7. Implement Task Completion**
- [ ] Add checkbox toggle
- [ ] Implement state update
- [ ] Add visual feedback (strikethrough)
- [ ] Add undo capability
- [ ] Add snackbar notification
- **Estimated Time:** 2 hours
- **Dependencies:** Task Service, Notification Service
- **Acceptance Criteria:**
  - [ ] Toggle works
  - [ ] Database updates
  - [ ] UI updates
  - [ ] Undo works
  - [ ] Snackbar shows

**8. Core Task Module Integration Testing**
- [ ] Test all CRUD operations
- [ ] Test search/sort/filter combinations
- [ ] Test data persistence
- [ ] Test large lists (100+ tasks)
- [ ] Test responsive behavior
- [ ] Performance testing
- **Estimated Time:** 3 hours
- **Dependencies:** All components
- **Acceptance Criteria:**
  - [ ] All operations work
  - [ ] Data persists
  - [ ] No performance issues
  - [ ] Responsive on all devices

**CORE TASK MANAGEMENT MODULE TOTAL: ~28 hours**

---

### SMART VIEWS MODULE

**1. Create Important View**
- [ ] Design view (reuses task list template)
- [ ] Create ImportantViewModel
- [ ] Implement filtering (isImportant = true)
- [ ] Test all functionality
- **Estimated Time:** 2 hours
- **Dependencies:** Core Task Management
- **Acceptance Criteria:**
  - [ ] Shows only important tasks
  - [ ] All list actions work

**2. Create Reminders View**
- [ ] Design view (reuses task list template)
- [ ] Create RemindersViewModel
- [ ] Implement filtering (hasReminder = true)
- [ ] Sort by reminder time
- [ ] Test all functionality
- **Estimated Time:** 2 hours
- **Dependencies:** Core Task Management
- **Acceptance Criteria:**
  - [ ] Shows only reminder tasks
  - [ ] Sorted correctly
  - [ ] All actions work

**3. Create Bin View**
- [ ] Design view (reuses task list template)
- [ ] Create BinViewModel
- [ ] Implement filtering (isDeleted = true)
- [ ] Implement Restore action
- [ ] Implement Delete Permanently action
- [ ] Implement Delete All button
- [ ] Add empty state
- [ ] Test all functionality
- **Estimated Time:** 3 hours
- **Dependencies:** Core Task Management
- **Acceptance Criteria:**
  - [ ] Shows only deleted tasks
  - [ ] Restore works
  - [ ] Delete Permanently works
  - [ ] Delete All works

**4. Create Custom Lists View**
- [ ] Design lists view layout
- [ ] Create CustomListsViewModel
- [ ] Display built-in lists
- [ ] Display custom lists
- [ ] Add Create New List button
- [ ] Design Create List modal
- [ ] Implement create functionality
- [ ] Implement Edit/Delete for custom lists
- [ ] Test all functionality
- **Estimated Time:** 4 hours
- **Dependencies:** Core Task Management, TaskList Service
- **Acceptance Criteria:**
  - [ ] All lists display
  - [ ] Create/Edit/Delete works
  - [ ] Icon/color pickers work
  - [ ] Navigation works

**5. Create List Detail View**
- [ ] Design view (reuses task list template)
- [ ] Create ListDetailViewModel
- [ ] Implement list filtering
- [ ] Test all functionality
- **Estimated Time:** 2 hours
- **Dependencies:** Custom Lists View
- **Acceptance Criteria:**
  - [ ] Shows correct list tasks
  - [ ] All actions work

**6. Smart Views Integration Testing**
- [ ] Test navigation between all views
- [ ] Test data consistency
- [ ] Test action effects across views
- [ ] Performance testing with multiple views
- **Estimated Time:** 2 hours
- **Dependencies:** All smart views
- **Acceptance Criteria:**
  - [ ] All views work
  - [ ] Data consistent
  - [ ] No crashes

**SMART VIEWS MODULE TOTAL: ~15 hours**

---

### ADVANCED FEATURES MODULE

**1. Implement Reminder System**
- [ ] Design reminder notification
- [ ] Implement reminder scheduling (local notifications)
- [ ] Add reminder UI in task creation/editing
- [ ] Test on Android and iOS
- **Estimated Time:** 3 hours
- **Dependencies:** Task Service
- **Acceptance Criteria:**
  - [ ] Reminders trigger correctly
  - [ ] Notifications display
  - [ ] Works on iOS and Android

**2. Implement Recurring Tasks**
- [ ] Design recurring logic
- [ ] Add recurrence UI
- [ ] Auto-create next occurrence
- [ ] Handle edge cases
- [ ] Test recurring task behavior
- **Estimated Time:** 3 hours
- **Dependencies:** Task Service
- **Acceptance Criteria:**
  - [ ] Recurring tasks work
  - [ ] Next occurrence created
  - [ ] Edge cases handled

**3. Implement Notifications & Snackbars**
- [ ] Design snackbar UI
- [ ] Create INotificationService
- [ ] Implement snackbar display
- [ ] Add action buttons
- [ ] Test stacking
- **Estimated Time:** 2 hours
- **Dependencies:** Notification Service
- **Acceptance Criteria:**
  - [ ] Snackbars display
  - [ ] Auto-dismiss works
  - [ ] Actions work

**4. Implement Search Functionality**
- [ ] Implement full-text search logic
- [ ] Add search UI across app
- [ ] Real-time filtering
- [ ] Test search performance
- **Estimated Time:** 2 hours
- **Dependencies:** Task Service
- **Acceptance Criteria:**
  - [ ] Search finds tasks
  - [ ] Real-time filtering works
  - [ ] Performance acceptable

**5. Implement Navigation Menu / Sidebar**
- [ ] Design navigation menu
- [ ] Create navigation menu component
- [ ] Implement hamburger toggle
- [ ] Add profile section
- [ ] Implement all navigation items
- [ ] Test on different screen sizes
- **Estimated Time:** 3 hours
- **Dependencies:** Core App Navigation
- **Acceptance Criteria:**
  - [ ] Menu toggles
  - [ ] Navigation works
  - [ ] Responsive layout

**6. UI Polish & Refinement**
- [ ] Review all screens against spec
- [ ] Ensure consistency
- [ ] Test responsive behavior
- [ ] Optimize animations
- [ ] Performance review
- **Estimated Time:** 4 hours
- **Dependencies:** All components
- **Acceptance Criteria:**
  - [ ] All screens match spec
  - [ ] Responsive on all devices
  - [ ] Smooth animations
  - [ ] Performance acceptable

**7. Error Handling & Edge Cases**
- [ ] Handle network errors
- [ ] Handle database errors
- [ ] Handle invalid input
- [ ] Test offline functionality
- [ ] Recovery testing
- **Estimated Time:** 2 hours
- **Dependencies:** All services
- **Acceptance Criteria:**
  - [ ] All errors handled
  - [ ] User informed
  - [ ] Recovery works
  - [ ] No data loss

**ADVANCED FEATURES MODULE TOTAL: ~19 hours**

---

## Acceptance Criteria

### Global Acceptance Criteria (All Modules)

- [ ] Code compiles without errors
- [ ] No runtime crashes or unhandled exceptions
- [ ] App launches successfully on target platforms
- [ ] All XAML is valid and renders correctly
- [ ] ViewModels properly bind to Views
- [ ] Data persists to database correctly
- [ ] Navigation works as designed
- [ ] Responsive layout on mobile (< 600px), tablet (600-1024px), desktop (> 1024px)
- [ ] Touch targets minimum 44x44 points
- [ ] Color contrast WCAG 2.1 AA compliant
- [ ] No memory leaks detected
- [ ] Performance acceptable (app starts < 2s, list scrolls smoothly)
- [ ] No hard-coded strings (use resource strings)
- [ ] Consistent spacing, alignment, typography across all screens
- [ ] All features from specification implemented
- [ ] No visual bugs or inconsistencies

### Module-Specific Acceptance Criteria

See each module section above for detailed criteria.

---

## Definition of Done

A task is considered **Done** when:

1. **Code Completed**
   - [ ] All code written and committed
   - [ ] Follows project conventions and style guide
   - [ ] No compiler warnings
   - [ ] No TODO or FIXME comments (unless documented)

2. **Functionality Verified**
   - [ ] Feature works as specified
   - [ ] All acceptance criteria met
   - [ ] User can complete intended workflow
   - [ ] Happy path and error paths both work

3. **Testing**
   - [ ] Unit tests written (if applicable)
   - [ ] Manual testing completed
   - [ ] No known bugs
   - [ ] Performance tested (if applicable)

4. **Code Quality**
   - [ ] Code reviewed and approved
   - [ ] No technical debt introduced
   - [ ] Consistent with codebase patterns
   - [ ] Properly documented (comments for non-obvious logic)

5. **UI/UX Polish**
   - [ ] Responsive on all target screen sizes
   - [ ] Animations and transitions smooth
   - [ ] Accessibility standards met
   - [ ] Matches specification design

6. **Data Integrity**
   - [ ] Data persists correctly
   - [ ] No data loss on errors
   - [ ] Proper validation in place
   - [ ] Error states handled gracefully

7. **Documentation**
   - [ ] Code comments where needed
   - [ ] README updated if applicable
   - [ ] Architecture decisions documented
   - [ ] API or service changes documented

8. **Ready for Integration**
   - [ ] Code merged to main branch (or feature branch)
   - [ ] No conflicts
   - [ ] No regressions in other features
   - [ ] Ready for next phase/feature

---

## Validation Checklist

### Pre-Implementation Checklist

- [ ] Project structure created
- [ ] Dependency Injection configured
- [ ] Base classes created and tested
- [ ] Resource Dictionaries set up
- [ ] Database initialized
- [ ] Build successful
- [ ] Can run on target platforms

### Per-Screen Implementation Checklist

For each screen implementation, verify:

- [ ] XAML layout matches specification
- [ ] All controls used as specified (Syncfusion or standard MAUI)
- [ ] ViewModel created with proper structure
- [ ] Data binding works correctly
- [ ] Input validation implemented
- [ ] Error messages display correctly
- [ ] Navigation works
- [ ] Responsive on all screen sizes
- [ ] Accessibility standards met
- [ ] No visual inconsistencies
- [ ] Animations/transitions smooth
- [ ] Performance acceptable

### Per-Service Implementation Checklist

For each service implementation, verify:

- [ ] Interface defined
- [ ] Implementation complete
- [ ] Dependency injection configured
- [ ] Error handling in place
- [ ] Logging/diagnostics added
- [ ] Unit tests written
- [ ] Integration tests passed
- [ ] Documentation updated

### Feature Integration Checklist

When integrating features:

- [ ] No breaking changes to existing code
- [ ] All services properly injected
- [ ] Data models updated as needed
- [ ] Navigation updated
- [ ] Resource strings updated
- [ ] Help/documentation updated
- [ ] Full regression testing completed
- [ ] Performance profiled
- [ ] Ready for UAT

---

## Dependency Graph

### Module Dependencies

```
Foundation (Infrastructure)
├── DI Setup
├── Base Classes
├── Resources
└── Database
    │
    ├─→ Authentication Module
    │   ├── Splash Screen
    │   ├── Sign In
    │   ├── Sign Up
    │   └── Forgot Password
    │
    └─→ Core Task Management
        ├── Task Service
        ├── My Notes View
        ├── Task Creation
        ├── Task Editing
        ├── Task Details
        └── Task Context Menu
            │
            ├─→ Important View
            ├─→ Reminders View
            ├─→ Bin View
            └─→ Custom Lists
                │
                └─→ Advanced Features
                    ├── Reminders
                    ├── Recurring Tasks
                    ├── Notifications
                    ├── Search
                    └── Navigation Menu
```

### Task Execution Order (Independent Paths)

**Path 1: Authentication First**
1. Foundation → Auth Module → Dashboard/My Notes

**Path 2: Core App**
1. Foundation → Core Tasks → All Smart Views → Advanced Features

**Path 3: Hybrid**
1. Foundation (parallel start)
2. Auth Module (depends on Foundation) + Core Tasks (depends on Foundation)
3. Smart Views (depends on Core Tasks)
4. Advanced Features (depends on Core Tasks)

---

## Implementation Guidelines

### Code Organization

- **Views:** XAML + Code-behind only (minimal logic)
- **ViewModels:** Business logic, validation, state management
- **Services:** Data access, external integrations, notifications
- **Models:** Data entities, no methods
- **Resources:** Styles, colors, strings (centralized)

### Naming Conventions

- **Classes:** PascalCase (e.g., `MyNotesViewModel`)
- **Methods:** PascalCase (e.g., `LoadTasks()`)
- **Properties:** PascalCase (e.g., `IsLoading`)
- **Private fields:** _camelCase (e.g., `_taskService`)
- **Constants:** UPPER_CASE (e.g., `DEFAULT_TIMEOUT`)
- **Resource keys:** camelCase (e.g., `colorPrimary`)

### XAML Guidelines

- Use data binding for all dynamic values
- Use relative binding when possible
- Use command binding for all button clicks
- Use resource references for colors, fonts, spacing
- Use MVVM behaviors for complex interactions
- Avoid code-behind logic (use ViewModels)

### ViewModel Guidelines

- Inherit from `BaseViewModel`
- Use `ObservableCollection<T>` for lists
- Implement `INotifyPropertyChanged` (via CommunityToolkit)
- Create commands for all user actions
- Implement input validation
- Handle loading and error states
- Use dependency injection for services

### Service Guidelines

- Define interfaces for all services
- Implement services with clear separation of concerns
- Handle errors gracefully with try-catch
- Use logging for diagnostics
- Return meaningful error messages
- Use async/await for I/O operations

---

## Summary

This Harness provides a complete roadmap for implementing the MAUI To Do List Application. It includes:

- Clear phases and milestones
- Independent vertical slices for parallel development
- Detailed task breakdowns with estimated times
- Comprehensive acceptance criteria
- Definition of Done for quality assurance
- Dependency management to minimize blocking tasks
- Implementation guidelines for consistency

**Total Estimated Development Time:** ~80-100 hours (with AI-assisted development, expect 40-60% reduction)

The harness is structured to enable:
- Independent feature implementation
- Parallel development on different modules
- Incremental testing and validation
- Clear progress tracking
- Easy identification of blockers

**Ready to begin implementation!**

---

**End of Harness Document**

Version: 1.0 | Last Updated: 2024 | Status: Ready for Development
