# Task 1-05: Setup MauiProgram.cs and Dependency Injection

**Module:** 1 - Foundation & Setup  
**Phase:** 1  
**Priority:** Critical  
**Duration:** 1 day  
**Dependencies:** Task 1-03 (MVVM Framework)  
**Status:** ⬜ Not Started

---

## 📋 TASK DESCRIPTION

Configure MauiProgram.cs with complete dependency injection container setup, Syncfusion core initialization, and service registrations. This is the app initialization entry point.

## 🎯 OBJECTIVES

- [ ] Setup MauiProgram.CreateMauiApp() method
- [ ] Register all services (IAuthenticationService, ITaskService, etc.)
- [ ] Register all ViewModels
- [ ] Register all Pages
- [ ] Configure Syncfusion core
- [ ] Setup logging
- [ ] Test DI container resolution

---

## 📝 FILES TO MODIFY/CREATE

```
MauiProgram.cs              ← Main configuration file
Resources/Styles/
├── Colors.xaml             ← Will be created in Task 1-04
├── Sizes.xaml
├── Fonts.xaml
└── Styles.xaml
```

---

## 💻 IMPLEMENTATION COMPONENTS

### 1. Service Registration Pattern
```
Pattern: AddScoped<IInterface, Implementation>()

Services to register:
✓ IAuthenticationService → AuthenticationService
✓ ITaskService → TaskService
✓ IListService → ListService
✓ ISearchService → SearchService
✓ ILocalStorageService → LocalStorageService
```

### 2. ViewModel Registration Pattern
```
Pattern: AddSingleton<ViewModelClass>()

ViewModels to register:
✓ AppShellViewModel
✓ SplashPageViewModel (or similar)
✓ SignInPageViewModel
✓ TaskListPageViewModel
... (all ViewModels)
```

### 3. Page Registration Pattern
```
Pattern: RegisterRoute<T>() where T : Page

Pages to register:
✓ SplashPage
✓ SignInPage
✓ TaskListPage
... (all Pages)
```

### 4. Syncfusion Configuration
```csharp
builder.ConfigureSyncfusionCore();
// Must be called before returning MauiApp
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] MauiProgram.cs configures and compiles
- [ ] All services registered in DI container
- [ ] All ViewModels registered in DI container
- [ ] Syncfusion core configured
- [ ] Logging configured
- [ ] App builds successfully
- [ ] DI container can resolve all services
- [ ] No circular dependencies

---

## 🛠️ STEP-BY-STEP IMPLEMENTATION

### Step 1: Create Base MauiProgram
Edit `MauiProgram.cs`:
```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }
}
```

### Step 2: Add Syncfusion Configuration
```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder()
        .UseMauiApp<App>()
        .ConfigureSyncfusionCore()  // ← Add this
        .ConfigureFonts(fonts => { ... });
    
    return builder.Build();
}
```

### Step 3: Add Service Extension Methods
Create organized extension methods:
```csharp
// Add to MauiProgram.cs or separate file

private static MauiAppBuilder AddServices(this MauiAppBuilder builder)
{
    builder.Services
        .AddScoped<IAuthenticationService, AuthenticationService>()
        .AddScoped<ITaskService, TaskService>()
        .AddScoped<IListService, ListService>()
        .AddScoped<ISearchService, SearchService>()
        .AddSingleton<ILocalStorageService, LocalStorageService>();
    
    return builder;
}

private static MauiAppBuilder AddViewModels(this MauiAppBuilder builder)
{
    builder.Services
        .AddSingleton<AppShellViewModel>()
        .AddTransient<SplashPageViewModel>()
        .AddTransient<SignInPageViewModel>()
        .AddTransient<TaskListPageViewModel>();
    
    return builder;
}

private static MauiAppBuilder AddPages(this MauiAppBuilder builder)
{
    Routing.RegisterRoute("splash", typeof(SplashPage));
    Routing.RegisterRoute("signin", typeof(SignInPage));
    Routing.RegisterRoute("signup", typeof(SignUpPage));
    Routing.RegisterRoute("tasklist", typeof(TaskListPage));
    
    return builder;
}
```

### Step 4: Connect Extension Methods
Update main method:
```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder()
        .UseMauiApp<App>()
        .ConfigureSyncfusionCore()
        .AddServices()        // ← New
        .AddViewModels()      // ← New
        .AddPages()           // ← New
        .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });

    return builder.Build();
}
```

### Step 5: Add Logging (Optional but Recommended)
```csharp
private static MauiAppBuilder AddLogging(this MauiAppBuilder builder)
{
    #if DEBUG
    builder.Logging.AddDebug();
    #endif
    
    return builder;
}
```

---

## 🧪 TESTING

### Unit Tests

**Test: DI Container Resolves Services**
```
Given: Configured MauiProgram
When: Resolve IAuthenticationService
Then: Returns AuthenticationService instance
```

**Test: Singleton Lifetime**
```
Given: ILocalStorageService registered as singleton
When: Resolve twice
Then: Same instance returned both times
```

**Test: Scoped Lifetime**
```
Given: ITaskService registered as scoped
When: Resolve in different scopes
Then: Different instances returned
```

### Manual Tests

- [ ] Build project successfully
- [ ] App launches without DI errors
- [ ] Can navigate through pages
- [ ] Services are accessible in ViewModels
- [ ] Logging output appears in debug console

---

## 📊 DEFINITION OF DONE

**Code Quality:**
- [ ] MauiProgram.cs organized with extension methods
- [ ] Services use interface-based DI
- [ ] Lifetime scopes appropriate (Singleton/Scoped/Transient)
- [ ] No hardcoded types
- [ ] Follows C# conventions

**Testing:**
- [ ] Project builds
- [ ] No DI resolution errors
- [ ] App launches
- [ ] Can resolve services

**Documentation:**
- [ ] Comments on key registrations
- [ ] Task marked complete
- [ ] Notes on DI lifetime choices

---

## 📚 SERVICE LIFETIMES EXPLANATION

### Singleton
- **Usage:** ILocalStorageService, ILogger
- **Lifetime:** One instance for entire app lifetime
- **Thread-Safe:** Must be thread-safe

### Scoped  
- **Usage:** IAuthenticationService, ITaskService (per scope)
- **Lifetime:** One instance per scope (usually per request)
- **Use When:** Service maintains state per user action

### Transient
- **Usage:** Page and ViewModel instances
- **Lifetime:** New instance every time
- **Use When:** No shared state needed

---

## 🔗 CROSS-REFERENCES

### Related Tasks
- Task 1-03: Implement base MVVM classes ✓
- Task 1-04: Create resource dictionaries (before this ideally)
- Task 2-01: Implement AuthenticationService (first service)

### Reference Materials
- MAUI DI docs: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection
- SPECIFICATION.md → Section 10: Architecture & MVVM
- QUICK_REFERENCE.md → 🧑‍💻 DEVELOPMENT QUICK START

---

## 📝 EXAMPLE: Complete MauiProgram.cs

```csharp
namespace Todo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .AddServices()
            .AddViewModels()
            .AddPages()
            .AddLogging()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }

    private static MauiAppBuilder AddServices(this MauiAppBuilder builder)
    {
        builder.Services
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<ITaskService, TaskService>()
            .AddSingleton<ILocalStorageService, LocalStorageService>();

        return builder;
    }

    private static MauiAppBuilder AddViewModels(this MauiAppBuilder builder)
    {
        builder.Services
            .AddSingleton<AppShellViewModel>()
            .AddTransient<SplashPageViewModel>();

        return builder;
    }

    private static MauiAppBuilder AddPages(this MauiAppBuilder builder)
    {
        Routing.RegisterRoute("splash", typeof(SplashPage));
        return builder;
    }

    private static MauiAppBuilder AddLogging(this MauiAppBuilder builder)
    {
        #if DEBUG
        builder.Logging.AddDebug();
        #endif
        return builder;
    }
}
```

---

## ⚠️ COMMON ISSUES

**Issue:** DI container can't resolve service
- **Cause:** Service not registered or interface mismatch
- **Solution:** Verify registration in AddServices()

**Issue:** Circular dependency error
- **Cause:** ServiceA depends on ServiceB, ServiceB depends on ServiceA
- **Solution:** Refactor to eliminate circular dependency

**Issue:** Syncfusion controls not rendering
- **Cause:** ConfigureSyncfusionCore() not called
- **Solution:** Ensure .ConfigureSyncfusionCore() in builder chain

---

## 📋 CHECKLIST

- [ ] Add Syncfusion configuration
- [ ] Create AddServices extension method
- [ ] Register all services
- [ ] Create AddViewModels extension method
- [ ] Register all ViewModels
- [ ] Create AddPages extension method
- [ ] Register all routes
- [ ] Add logging configuration
- [ ] Build solution
- [ ] Test app launch
- [ ] Verify DI resolution
- [ ] Commit to version control

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
