# Task 1-03: Implement Base MVVM Classes and Framework

**Module:** 1 - Foundation & Setup  
**Phase:** 1  
**Priority:** Critical  
**Duration:** 1 day  
**Dependencies:** Task 1-02 (NuGet packages)  
**Status:** ⬜ Not Started

---

## 📋 TASK DESCRIPTION

Create the base MVVM framework using CommunityToolkit.MVVM. Establish ViewModelBase class, implement observable patterns, and configure bindings for the entire application.

## 🎯 OBJECTIVES

- [ ] Create ViewModelBase class extending ObservableObject
- [ ] Implement [ObservableProperty] patterns
- [ ] Implement [RelayCommand] patterns
- [ ] Setup ILogger integration for diagnostics
- [ ] Create base classes for all ViewModels to inherit
- [ ] Verify bindings work in test

---

## 📝 FILES TO CREATE

```
ViewModels/
├── ViewModelBase.cs          ← Base class for all ViewModels
Utilities/
├── Constants.cs              ← App-wide constants
├── Enums.cs                  ← App-wide enumerations
└── Helpers.cs                ← Utility helper methods
```

---

## 💻 IMPLEMENTATION GUIDE

### 1. Create ViewModelBase.cs
Location: `ViewModels/ViewModelBase.cs`

Key components:
- Inherit from `ObservableObject`
- Include `ILogger<ViewModelBase>` dependency
- Add `IsLoading` observable property
- Add `Title` observable property
- Add base command infrastructure
- Include error handling pattern

```csharp
// Pseudocode structure
public partial class ViewModelBase : ObservableObject
{
    protected ILogger<ViewModelBase> Logger { get; }
    
    [ObservableProperty]
    private bool isLoading = false;
    
    [ObservableProperty]
    private string title = string.Empty;
    
    public ViewModelBase(ILogger<ViewModelBase> logger)
    {
        Logger = logger;
    }
    
    // Protected helper methods for error handling
    protected async Task<Result<T>> ExecuteAsync<T>(...) { ... }
}
```

### 2. Create Constants.cs
Location: `Utilities/Constants.cs`

Key constants:
- App name and version
- API endpoints
- Database constants
- Timeout durations
- Feature flags

### 3. Create Enums.cs
Location: `Utilities/Enums.cs`

Key enumerations:
- `ListCategory` (MyNotes, Important, Reminder, Bin)
- `RecurrenceType` (None, Daily, Weekly, Monthly)
- `TaskStatus` (Active, Completed, Deleted)

### 4. Create Helpers.cs
Location: `Utilities/Helpers.cs`

Key helpers:
- Date/time formatting
- String validation
- Color conversion
- Collection operations

---

## ✅ ACCEPTANCE CRITERIA

- [ ] ViewModelBase.cs created and compiles
- [ ] All ViewModels will inherit from ViewModelBase
- [ ] [ObservableProperty] attributes work correctly
- [ ] [RelayCommand] attributes generate commands
- [ ] ILogger integration functional
- [ ] Constants defined for app-wide usage
- [ ] Enums defined for type safety
- [ ] Helper functions available for common tasks
- [ ] Project builds without errors

---

## 🧪 TESTING

### Unit Tests to Create

**Test: ObservableProperty Notifications**
```
Given: ViewModel with [ObservableProperty] property
When: Property value changes
Then: PropertyChanged event fires
```

**Test: RelayCommand Execution**
```
Given: ViewModel with [RelayCommand] method
When: Command executes
Then: Method runs successfully
```

**Test: IsLoading Property**
```
Given: ViewModelBase instance
When: IsLoading set to true
Then: PropertyChanged event fires
```

### Manual Tests

- [ ] Open Visual Studio
- [ ] Create test ViewModel inheriting from ViewModelBase
- [ ] Add [ObservableProperty] int property
- [ ] Verify auto-generated backing field
- [ ] Verify property descriptor created
- [ ] Test [RelayCommand] async method
- [ ] Verify command created and callable

---

## 📊 DEFINITION OF DONE

**Code Quality:**
- [ ] ViewModelBase follows MVVM pattern
- [ ] No hardcoded values
- [ ] Proper async/await usage
- [ ] XML documentation on public members
- [ ] Follows C# style guidelines

**Testing:**
- [ ] Project compiles
- [ ] ObservableProperty works
- [ ] RelayCommand works
- [ ] ILogger injection works
- [ ] Constants and Helpers accessible

**Documentation:**
- [ ] XML comments on ViewModelBase
- [ ] Constants documented
- [ ] Enums documented
- [ ] Task marked complete

---

## 🔗 CROSS-REFERENCES

### Related Tasks
- Task 1-05: Setup MauiProgram and DI (uses ViewModelBase)
- Task 2-01: Implement AuthenticationService (first usage)

### Reference Materials
- CommunityToolkit.MVVM docs: https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/
- SPECIFICATION.md → Section 10: Architecture & MVVM
- QUICK_REFERENCE.md → 🧑‍💻 DEVELOPMENT QUICK START

---

## 📚 EXAMPLE CODE SNIPPETS

### Using ObservableProperty
```csharp
[ObservableProperty]
private string taskTitle;
// Automatically generates: TaskTitle property with backing field
```

### Using RelayCommand
```csharp
[RelayCommand]
public async Task SaveTask()
{
    // Command automatically created as SaveTaskCommand
    // Can be bound: Command="{Binding SaveTaskCommand}"
}
```

### XAML Binding
```xml
<Entry Text="{Binding TaskTitle, Mode=TwoWay}" />
<Button Command="{Binding SaveTaskCommand}" Text="Save" />
```

---

## ⚠️ COMMON ISSUES

**Issue:** ObservableProperty not generating
- **Cause:** CommunityToolkit.MVVM package not installed
- **Solution:** Verify Task 1-02 completed successfully

**Issue:** IntelliSense not showing ObservableProperty
- **Cause:** Compilation errors or incorrect using statements
- **Solution:** Add `using CommunityToolkit.Mvvm.ComponentModel;`

**Issue:** RelayCommand not binding
- **Cause:** Method visibility or signature incorrect
- **Solution:** Ensure method is public, returns Task or void

---

## 📝 IMPLEMENTATION CHECKLIST

- [ ] Create ViewModels folder structure
- [ ] Create ViewModelBase.cs with ObservableObject inheritance
- [ ] Add IsLoading and Title properties
- [ ] Add ILogger injection
- [ ] Create base command infrastructure
- [ ] Create Constants.cs with all app constants
- [ ] Create Enums.cs with all app enumerations
- [ ] Create Helpers.cs with utility methods
- [ ] Add XML documentation
- [ ] Build solution and verify no errors
- [ ] Test ObservableProperty in simple test
- [ ] Test RelayCommand in simple test
- [ ] Commit to version control

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
