# Task 1-06: Create Navigation Infrastructure (AppShell, Routing)

**Module:** 1 - Foundation & Setup  
**Phase:** 1  
**Priority:** Critical  
**Duration:** 1 day  
**Dependencies:** Task 1-03 (MVVM Framework), Task 1-05 (MauiProgram DI)  
**Status:** ⬜ Not Started

---

## 📋 TASK DESCRIPTION

Setup the navigation infrastructure using NavigationPage pattern (not Shell, which caused issues in reference workspace). Create AppShell.xaml, configure route registration, and establish the navigation structure for all screens.

## 🎯 OBJECTIVES

- [ ] Create AppShell.xaml with navigation structure
- [ ] Create AppShellViewModel with navigation commands
- [ ] Register all routes in MauiProgram
- [ ] Implement navigation service
- [ ] Test navigation between pages
- [ ] Verify back button functionality
- [ ] Setup deep linking (optional)

---

## 📝 FILES TO CREATE

```
App.xaml                        ← Modify to use NavigationPage
App.xaml.cs                     ← Modify startup logic
AppShell.xaml                   ← Main navigation container
AppShellViewModel.cs           ← Navigation logic
Services/Infrastructure/
├── INavigationService.cs      ← Navigation interface
└── NavigationService.cs       ← Navigation implementation
ViewModels/
└── AppShellViewModel.cs       ← ViewModel for AppShell
```

---

## 🏗️ NAVIGATION STRUCTURE

### App Launch Flow
```
App.xaml.cs → InitializeApp()
    ↓
Check if user authenticated?
    ├─ No → NavigationPage(SplashPage)
    ├─ Cached token → NavigationPage(TaskListPage)
    └─ Yes → NavigationPage(SignInPage)
```

### Page Hierarchy
```
NavigationPage (Root)
├── SplashPage (Startup screen)
├── SignInPage (Authentication)
├── SignUpPage (Registration)
├── ForgotPasswordPage (Password reset)
├── TaskListPage (Main screen)
│   ├── TaskDetailPage
│   └── EditTaskPage
├── SearchResultsPage
├── ProfilePage
└── SettingsPage
```

---

## 💻 IMPLEMENTATION GUIDE

### 1. Create Navigation Service Interface

Location: `Services/Infrastructure/INavigationService.cs`

```csharp
public interface INavigationService
{
    // Navigate to page with route
    Task NavigateToAsync(string route, IDictionary<string, object> parameters = null);
    
    // Navigate back to previous page
    Task GoBackAsync();
    
    // Clear navigation stack and navigate to page
    Task NavigateToAsync(string route, bool clearStack = false);
    
    // Navigate to page (generic, strongly-typed)
    Task NavigateToAsync<T>(IDictionary<string, object> parameters = null);
    
    // Get current page
    Page GetCurrentPage();
}
```

### 2. Implement Navigation Service

Location: `Services/Infrastructure/NavigationService.cs`

```csharp
using Microsoft.Maui.Controls;

public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(ILogger<NavigationService> logger)
    {
        _logger = logger;
    }

    public async Task NavigateToAsync(string route, IDictionary<string, object> parameters = null)
    {
        try
        {
            _logger.LogInformation($"Navigating to route: {route}");
            
            var navigationParameter = parameters == null 
                ? route 
                : $"{route}?{BuildQueryString(parameters)}";
            
            await Shell.Current.GoToAsync(navigationParameter);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Navigation error: {ex.Message}");
            throw;
        }
    }

    public async Task GoBackAsync()
    {
        try
        {
            _logger.LogInformation("Going back");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Navigation error: {ex.Message}");
            throw;
        }
    }

    public Page GetCurrentPage()
    {
        return Application.Current?.MainPage;
    }

    private string BuildQueryString(IDictionary<string, object> parameters)
    {
        var query = string.Join("&", 
            parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value?.ToString() ?? "")}"));
        return query;
    }
}
```

### 3. Create AppShell.xaml

Location: `AppShell.xaml`

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="Todo.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    FlyoutBehavior="Disabled">

    <!-- Define shell content -->
    <ShellContent 
        Title="Main" 
        Icon="icon_about.png" 
        ContentTemplate="{DataTemplate local:SplashPage}" 
        Route="splash" />

</Shell>
```

### 4. Create AppShell Code-Behind

Location: `AppShell.xaml.cs`

```csharp
namespace Todo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes
        Routing.RegisterRoute("splash", typeof(SplashPage));
        Routing.RegisterRoute("signin", typeof(SignInPage));
        Routing.RegisterRoute("signup", typeof(SignUpPage));
        Routing.RegisterRoute("forgotpassword", typeof(ForgotPasswordPage));
        Routing.RegisterRoute("tasklist", typeof(TaskListPage));
        Routing.RegisterRoute("taskdetail", typeof(TaskDetailPage));
        Routing.RegisterRoute("search", typeof(SearchResultsPage));
        Routing.RegisterRoute("profile", typeof(ProfilePage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
    }
}
```

### 5. Modify App.xaml

Location: `App.xaml`

```xml
<?xml version = "1.0" encoding = "UTF-8" ?>
<Application
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="Todo.App">

    <Application.Resources>
        <!-- Resource dictionaries merged here (from Task 1-04) -->
    </Application.Resources>

</Application>
```

### 6. Modify App.xaml.cs

Location: `App.xaml.cs`

```csharp
namespace Todo;

public partial class App : Application
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<App> _logger;

    public App(IAuthenticationService authService, ILogger<App> logger)
    {
        InitializeComponent();

        _authService = authService;
        _logger = logger;

        MainPage = new AppShell();
    }

    protected override void OnStart()
    {
        base.OnStart();
        
        // Initialize app after a short delay to allow UI to settle
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(500);
            await InitializeAppAsync();
        });
    }

    private async Task InitializeAppAsync()
    {
        try
        {
            _logger.LogInformation("Initializing app");

            // Check if user is authenticated
            var isAuthenticated = await _authService.IsAuthenticatedAsync();
            
            if (isAuthenticated)
            {
                // Navigate to main app
                await Shell.Current.GoToAsync("tasklist");
            }
            else
            {
                // Navigate to splash screen
                await Shell.Current.GoToAsync("splash");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"App initialization error: {ex.Message}");
            await Shell.Current.GoToAsync("splash");
        }
    }
}
```

### 7. Create Routes Constants File (Optional)

Location: `Utilities/Routes.cs`

```csharp
namespace Todo.Utilities;

public static class Routes
{
    // Authentication routes
    public const string Splash = "splash";
    public const string SignIn = "signin";
    public const string SignUp = "signup";
    public const string ForgotPassword = "forgotpassword";
    
    // Main app routes
    public const string TaskList = "tasklist";
    public const string TaskDetail = "taskdetail";
    public const string Search = "search";
    public const string Profile = "profile";
    public const string Settings = "settings";
}
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] AppShell.xaml created
- [ ] All pages registered as routes
- [ ] Navigation service implemented
- [ ] App.xaml.cs initializes navigation
- [ ] Back navigation works
- [ ] Route parameters pass correctly
- [ ] No navigation errors on app launch
- [ ] Authenticated users go to TaskList
- [ ] Unauthenticated users go to Splash
- [ ] All pages navigable from each other

---

## 🧪 TESTING

### Unit Tests

**Test: Route Registration**
```
Given: AppShell initialized
When: Query route mapping
Then: All routes registered correctly
```

**Test: Navigation Service**
```
Given: NavigationService instance
When: Navigate to "signin"
Then: Navigates successfully without error
```

### Integration Tests

**Test: App Launch Flow**
```
Given: Unauthenticated user
When: App starts
Then: Navigates to Splash page
```

**Test: Authenticated App Launch**
```
Given: Authenticated user with valid token
When: App starts
Then: Navigates to TaskList page
```

### Manual Tests

- [ ] Launch app, verify Splash page loads
- [ ] From Splash, navigate to SignIn
- [ ] From SignIn, navigate to SignUp
- [ ] From SignUp, navigate to ForgotPassword
- [ ] After authentication, navigate to TaskList
- [ ] From TaskList, navigate to other pages
- [ ] Press back button, verify navigation history works
- [ ] Force app close and reopen, verify auth state persists

---

## 📊 DEFINITION OF DONE

**Code Quality:**
- [ ] Navigation service uses interfaces
- [ ] Routes registered in AppShell
- [ ] ILogger integrated
- [ ] Error handling in navigation
- [ ] No hardcoded route strings (use Routes constants)
- [ ] Null checks on parameters

**Testing:**
- [ ] App launches without errors
- [ ] Navigation works between pages
- [ ] Back button works
- [ ] Auth state persists on app restart
- [ ] Route parameters pass correctly
- [ ] All manual tests pass

**Documentation:**
- [ ] Routes documented in Routes.cs
- [ ] Navigation flow documented
- [ ] Any special behaviors noted
- [ ] Task marked complete

---

## 🔗 CROSS-REFERENCES

### Related Tasks
- Task 1-03: Implement MVVM framework ✓
- Task 1-05: Setup MauiProgram and DI ✓
- Task 2-02: Create SplashPage (first page)
- Task 2-03: Create SignInPage

### Reference Materials
- **Specification:** SPECIFICATION.md → Section 7: Navigation Structure
- **MAUI Shell Docs:** https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/
- **MAUI Navigation:** https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/navigation

---

## 📝 NAVIGATION PATTERNS

### Pattern 1: Simple Navigation
```csharp
// In ViewModel or code-behind
await Shell.Current.GoToAsync("signin");
```

### Pattern 2: Navigation with Parameters
```csharp
// Define route with parameter
await Shell.Current.GoToAsync($"taskdetail?id={taskId}");

// Receive in target ViewModel
[QueryProperty(nameof(Id), "id")]
public string Id { get; set; }
```

### Pattern 3: Back Navigation
```csharp
// Go back one page
await Shell.Current.GoToAsync("..");

// Or using navigation service
await _navigationService.GoBackAsync();
```

### Pattern 4: Clear Stack and Navigate
```csharp
// Clear history and navigate to new page
ShellNavigationState state = Shell.Current.CurrentState;
List<string> routes = new List<string>();
state.GetFullPath(routes);
routes.Clear();

// Then navigate
await Shell.Current.GoToAsync("tasklist");
```

---

## ⚠️ COMMON ISSUES

**Issue:** Routes not registering
- **Cause:** Routing.RegisterRoute() called before Shell initialized
- **Solution:** Register routes in AppShell.xaml.cs constructor

**Issue:** Query parameters not passing
- **Cause:** Query string malformed or receiver not decorated with [QueryProperty]
- **Solution:** Verify query format and add [QueryProperty] attribute

**Issue:** Back button shows on all pages
- **Cause:** NavigationPage auto-adds back button
- **Solution:** This is expected behavior; suppress if needed on specific pages

**Issue:** Memory leak with navigation**
- **Cause:** Event handlers not unsubscribed
- **Solution:** Unsubscribe from events in OnNavigatedFrom

---

## 🔮 ADVANCED FEATURES (Future)

After basic navigation works, consider:
- [ ] Deep linking (launch from URL)
- [ ] Modal navigation
- [ ] Tab-based navigation
- [ ] Gesture navigation
- [ ] Navigation animations

---

## 📋 IMPLEMENTATION CHECKLIST

- [ ] Create INavigationService interface
- [ ] Create NavigationService implementation
- [ ] Create/Update AppShell.xaml
- [ ] Register all routes in AppShell.cs
- [ ] Update App.xaml.cs with initialization logic
- [ ] Register NavigationService in MauiProgram
- [ ] Create Routes.cs constants file
- [ ] Build and test app launch
- [ ] Test navigation between pages
- [ ] Test back button
- [ ] Test auth state persistence
- [ ] Commit to version control

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
