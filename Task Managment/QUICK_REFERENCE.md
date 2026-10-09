# Quick Reference Guide

**For:** Syncfusion MAUI To Do List Application  
**Created:** October 2026

---

## 📋 SPECIFICATION FILES

| Document | Purpose | Size | Location |
|----------|---------|------|----------|
| **SPECIFICATION.md** | Complete technical requirements | ~15K words | Project root |
| **HARNESS.md** | Development roadmap with 10 modules | ~12K words | Project root |
| **IMPLEMENTATION_SUMMARY.md** | Executive summary & quick start | ~3K words | Project root |
| **QUICK_REFERENCE.md** | This document | Quick lookup | Project root |

---

## 🎯 MODULE CHECKLIST

### Phase 1: Foundation (2-3d)
- [ ] Project folder structure
- [ ] NuGet packages installed
- [ ] MauiProgram.cs configured
- [ ] Resource dictionaries created
- [ ] MVVM base classes
- [ ] Navigation setup
- [ ] Database initialized

### Phase 2: Authentication (3-4d)
- [ ] AuthenticationService
- [ ] Splash screen
- [ ] Sign In screen
- [ ] Sign Up screen
- [ ] Forgot Password screen
- [ ] Token management
- [ ] Auto-login (RememberMe)
- [ ] Sign out flow

### Phase 3: Core Tasks (5-6d)
- [ ] TaskListPage (main view)
- [ ] Sidebar navigation
- [ ] Header bar
- [ ] SfListView integration
- [ ] Task item template
- [ ] Bottom input bar
- [ ] Create task
- [ ] Edit task
- [ ] Delete task (soft)
- [ ] Complete task
- [ ] Context menu
- [ ] Empty states

### Phase 4: Organization (4-5d)
- [ ] Important filter & list
- [ ] Reminder filter & list
- [ ] Bin filter & restore
- [ ] Permanent delete
- [ ] Custom list creation
- [ ] Rename custom list
- [ ] Delete custom list
- [ ] Move tasks between lists

### Phase 5: Advanced (3-4d)
- [ ] Search functionality
- [ ] Due dates
- [ ] Reminders & notifications
- [ ] Recurring tasks (optional)
- [ ] Drag & drop (optional)

### Phase 6: Profile & Settings (1-2d)
- [ ] Profile menu
- [ ] Account management
- [ ] Settings page
- [ ] Theme support (optional)
- [ ] Localization (optional)

### Phase 7: Polish & Testing (2-3d)
- [ ] Responsive design testing
- [ ] Accessibility audit (WCAG 2.1 AA)
- [ ] Performance testing
- [ ] Cross-platform testing
- [ ] Security audit
- [ ] Bug fixes & refinement
- [ ] Documentation

---

## 🎨 DESIGN TOKENS

### Colors
```
Primary:       #5C4EAE (Purple)
Primary Dark:  #453685
Primary Light: #8A7FD9

Error:         #B3261E (Red)
Success:       #188A3D (Green)
Warning:       #F57C00 (Orange)

Important:     #9C27B0 (Purple)
Reminder:      #2196F3 (Blue)
Bin:           #F44336 (Red)
My Notes:      #E8E8E8 (Gray)

Text Primary:   #333333
Text Secondary: #666666
Text Tertiary:  #999999
```

### Spacing
```
XS:  4 dp    S:  12 dp    L: 24 dp
XS:  8 dp    M:  16 dp    XL: 32 dp
```

### Typography
```
Caption:   11sp, Regular
Small:     12sp, Regular
Label:     14sp, Medium
Body:      16sp, Regular
Title:     18sp, SemiBold
Heading:   20sp, SemiBold
Large:     24sp, SemiBold
```

### Border Radius
```
Small:  4 dp
Medium: 8 dp
Large:  16 dp
Full:   50% (avatars)
```

---

## 📱 RESPONSIVE BREAKPOINTS

| Device | Width | Layout | Sidebar | Navigation |
|--------|-------|--------|---------|------------|
| Phone | 320-480px | Single | Drawer | Hamburger |
| Phablet | 481-599px | Single | Drawer | Hamburger |
| Tablet | 600-1024px | 2-col | Visible | Sidebar |
| Desktop | 1025px+ | 3-col | Visible | Sidebar |

---

## 🏗️ PROJECT STRUCTURE

```
Todo/
├── App.xaml
├── App.xaml.cs
├── AppShell.xaml
├── MauiProgram.cs
├── SPECIFICATION.md
├── HARNESS.md
├── IMPLEMENTATION_SUMMARY.md
├── QUICK_REFERENCE.md
│
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
│   │   └── EmptyStateView.xaml
│   └── Dialogs/
│       ├── CreateListDialog.xaml
│       ├── RenameListDialog.xaml
│       └── ConfirmDialog.xaml
│
├── ViewModels/
│   ├── AppShellViewModel.cs
│   ├── AuthenticationViewModel.cs
│   ├── TaskListViewModel.cs
│   ├── TaskDetailViewModel.cs
│   ├── ProfileViewModel.cs
│   └── SearchViewModel.cs
│
├── Models/
│   ├── Task.cs
│   ├── TaskList.cs
│   ├── User.cs
│   └── DTOs/
│       ├── AuthRequest.cs
│       └── AuthResponse.cs
│
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthenticationService.cs
│   │   ├── ITaskService.cs
│   │   ├── IListService.cs
│   │   ├── ISearchService.cs
│   │   └── ILocalStorageService.cs
│   ├── Authentication/
│   │   └── AuthenticationService.cs
│   ├── Task/
│   │   ├── TaskService.cs
│   │   └── ListService.cs
│   ├── Search/
│   │   └── SearchService.cs
│   ├── Storage/
│   │   └── LocalStorageService.cs
│   └── Infrastructure/
│       ├── NotificationService.cs
│       └── DialogService.cs
│
├── Data/
│   ├── AppDbContext.cs
│   └── Repositories/
│       ├── IRepository.cs
│       ├── TaskRepository.cs
│       └── ListRepository.cs
│
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
│   │   └── icons/
│   └── Strings/
│       └── AppStrings.resx
│
├── Converters/
│   ├── BoolToVisibilityConverter.cs
│   ├── BoolToOpacityConverter.cs
│   └── DateTimeToStringConverter.cs
│
├── Behaviors/
│   └── NoSelectableBehavior.cs
│
└── Utilities/
    ├── Constants.cs
    ├── Enums.cs
    └── Helpers.cs
```

---

## 📦 NUGET PACKAGES

### Syncfusion
```
Syncfusion.Maui.ListView 24.1.x       SfListView
Syncfusion.Maui.Calendar 24.1.x       SfDateRangePicker
Syncfusion.Maui.Core 24.1.x           Base framework
Syncfusion.Maui.Buttons 24.1.x        SfButton (future)
```

### MVVM & Data
```
CommunityToolkit.MVVM 8.4.x           MVVM generation
sqlite-net-pcl 1.8.x                  SQLite ORM
SQLitePCLRaw.bundle_green 2.1.x       SQLite native
```

### Standard MAUI
```
Microsoft.Maui.Controls 8.0.x         Framework
```

---

## 🧑‍💻 DEVELOPMENT QUICK START

### 1. Setup MauiProgram.cs
```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .AddServices()
            .AddViewModels()
            .AddPages();
        
        return builder.Build();
    }
}
```

### 2. Configure DI
```csharp
private static MauiAppBuilder AddServices(this MauiAppBuilder builder)
{
    builder.Services
        .AddScoped<IAuthenticationService, AuthenticationService>()
        .AddScoped<ITaskService, TaskService>()
        .AddScoped<IListService, ListService>()
        .AddScoped<ILocalStorageService, LocalStorageService>();
    return builder;
}
```

### 3. Create ViewModel Base
```csharp
public partial class ViewModelBase : ObservableObject
{
    protected ILogger<ViewModelBase> Logger { get; set; }
    
    public ViewModelBase(ILogger<ViewModelBase> logger)
    {
        Logger = logger;
    }
}
```

### 4. Use ObservableProperty
```csharp
public partial class TaskListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Task> tasks;
    
    [RelayCommand]
    public async Task LoadTasks()
    {
        // Implementation
    }
}
```

---

## 🧪 TESTING CHECKLIST

### Unit Tests
- [ ] Email validation
- [ ] Password strength validation
- [ ] Task title validation
- [ ] Task filtering logic
- [ ] Converters (all types)
- [ ] Constants and helpers

### Integration Tests
- [ ] Database CRUD
- [ ] Service methods
- [ ] Sync logic
- [ ] Offline mode

### UI Tests
- [ ] Page navigation
- [ ] Form submission
- [ ] List display and scrolling
- [ ] Filter application
- [ ] Context menus

### Accessibility Tests
- [ ] Keyboard navigation
- [ ] Screen reader support
- [ ] Color contrast
- [ ] Touch targets (48x48dp)
- [ ] Focus indicators

### Performance Tests
- [ ] App launch time (< 2 sec)
- [ ] List scroll FPS (60 FPS)
- [ ] Search latency (< 500ms)
- [ ] Memory usage (< 150 MB)

---

## ♿ ACCESSIBILITY TARGETS

### WCAG 2.1 AA Compliance
- [ ] Color contrast 4.5:1 for text
- [ ] Font size minimum 12sp
- [ ] Touch targets minimum 48x48 dp
- [ ] Full keyboard navigation
- [ ] Screen reader support
- [ ] Visible focus indicators
- [ ] High contrast mode support
- [ ] No color-only information

---

## 📊 PERFORMANCE TARGETS

| Metric | Target | Unit |
|--------|--------|------|
| App Launch | < 2 | seconds |
| List Scroll | 60 | FPS |
| Search Response | < 500 | ms |
| First Paint | < 1 | second |
| Memory Usage | < 150 | MB |
| DB Query | < 100 | ms |

---

## 🔐 SECURITY CHECKLIST

- [ ] Tokens stored in SecureStorage
- [ ] No sensitive data in logs
- [ ] No hardcoded credentials
- [ ] HTTPS for API calls
- [ ] Input validation on all forms
- [ ] SQL injection prevention
- [ ] XSS prevention
- [ ] CSRF token handling (if needed)
- [ ] Logout clears all data
- [ ] Expired token handling

---

## 📅 TIMELINE ESTIMATE

| Phase | Duration | Start | End |
|-------|----------|-------|-----|
| 1 | 2-3d | Day 1 | Day 3 |
| 2 | 3-4d | Day 4 | Day 7 |
| 3 | 5-6d | Day 8 | Day 13 |
| 4 | 4-5d | Day 14 | Day 18 |
| 5 | 3-4d | Day 19 | Day 22 |
| 6 | 1-2d | Day 23 | Day 24 |
| 7 | 2-3d | Day 25 | Day 27 |
| **TOTAL** | **20-27d** | **Day 1** | **Day 27** |

---

## 🚀 LAUNCH CHECKLIST

### Code Quality
- [ ] All tests passing
- [ ] No compiler warnings
- [ ] Code reviewed
- [ ] No hardcoded values
- [ ] Proper error handling

### Testing Complete
- [ ] Responsive design verified
- [ ] Accessibility audit passed
- [ ] Performance targets met
- [ ] Security audit passed
- [ ] Cross-platform tested

### Documentation
- [ ] README.md complete
- [ ] Architecture documented
- [ ] Setup instructions clear
- [ ] Release notes written

### Deployment
- [ ] Version bumped
- [ ] Git tagged
- [ ] Distribution configured
- [ ] Release approved

---

## 🔗 REFERENCE LINKS

### Syncfusion Documentation
- [SfListView](https://help.syncfusion.com/maui/listview/listview)
- [SfDateRangePicker](https://help.syncfusion.com/maui/calendar/date-range-picker)
- [Getting Started](https://help.syncfusion.com/maui/introduction/getting-started)

### MAUI Documentation
- [MVVM Toolkit](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction)
- [Navigation](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell)
- [Data Binding](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/)

### Standards
- [WCAG 2.1](https://www.w3.org/WAI/WCAG21/quickref/)
- [Material Design 3](https://m3.material.io/)
- [Semantic Versioning](https://semver.org/)

---

## 📝 COMMON PATTERNS

### Result Pattern (Error Handling)
```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string Error { get; set; }
}

// Usage
var result = await service.DoSomethingAsync();
if (result.IsSuccess) { /* success */ }
else { /* handle result.Error */ }
```

### Service Interface Pattern
```csharp
public interface IMyService
{
    Task<Result<IEnumerable<T>>> GetAllAsync();
    Task<Result<T>> GetByIdAsync(string id);
    Task<Result<T>> CreateAsync(T entity);
    Task<Result<bool>> UpdateAsync(T entity);
    Task<Result<bool>> DeleteAsync(string id);
}
```

### ViewModel Command Pattern
```csharp
[ObservableProperty]
private string title;

[RelayCommand]
public async Task SaveTask()
{
    // Validation
    if (string.IsNullOrEmpty(Title))
    {
        await App.Current.MainPage.DisplayAlert("Error", "Title required", "OK");
        return;
    }
    
    // Execute
    var result = await taskService.CreateTaskAsync(new Task { Title });
    
    // Handle result
    if (result.IsSuccess)
    {
        await Shell.Current.GoToAsync("back");
    }
}
```

---

## ⚡ PERFORMANCE TIPS

1. **Virtualization**: Use SfListView virtualization for large lists
2. **Lazy Loading**: Load tasks on-demand, not all at once
3. **Caching**: Cache frequently accessed data
4. **Async/Await**: Don't block UI thread
5. **Image Optimization**: Use appropriate image sizes
6. **Database Indexing**: Index frequently queried columns
7. **Local Storage**: Keep database optimized and small

---

## 🐛 DEBUGGING TIPS

### Common Issues

**Issue:** Tasks not displaying in SfListView
- [ ] Check ItemsSource binding
- [ ] Verify data collection populated
- [ ] Check ItemTemplate definition
- [ ] Verify virtualization enabled

**Issue:** Navigation not working
- [ ] Check route registration in AppShell
- [ ] Verify Shell.Current available
- [ ] Check navigation parameters
- [ ] Verify page exists

**Issue:** MVVM binding not updating
- [ ] Verify ObservableProperty used
- [ ] Check binding syntax (x:Name, Path)
- [ ] Verify Mode (TwoWay for edits)
- [ ] Check RelayCommand return type

**Issue:** Database not persisting
- [ ] Verify connection string
- [ ] Check DbContext initialization
- [ ] Verify SaveChangesAsync called
- [ ] Check database file location

---

## 📞 SUPPORT

For detailed information:
1. See **SPECIFICATION.md** for technical requirements
2. See **HARNESS.md** for development tasks
3. Check **/images/** for design references
4. Review Syncfusion documentation

---

**Last Updated:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
