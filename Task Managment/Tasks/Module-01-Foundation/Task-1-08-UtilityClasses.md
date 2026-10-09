# Task 1-08: Create Utility Classes (Converters, Helpers, Constants)

**Module:** 1 - Foundation & Setup  
**Phase:** 1  
**Priority:** High  
**Duration:** 0.5 days  
**Dependencies:** Task 1-03 (MVVM Framework), Task 1-04 (Resource Dictionaries)  
**Status:** ⬜ Not Started

---

## 📋 TASK DESCRIPTION

Create utility classes for value converters, helper functions, and application constants. These enable code reuse across the application and reduce boilerplate.

## 🎯 OBJECTIVES

- [ ] Create value converters (BoolToVisibility, DateToString, etc.)
- [ ] Create helper functions (date formatting, string utilities)
- [ ] Define application constants
- [ ] Define enumerations for type safety
- [ ] Test converters and helpers
- [ ] Verify all utilities compile and work

---

## 📝 FILES TO CREATE

```
Converters/
├── BoolToVisibilityConverter.cs
├── DateTimeToStringConverter.cs
├── InvertedBoolConverter.cs
└── EnumToStringConverter.cs
Utilities/
├── Constants.cs
├── Enums.cs
└── Helpers.cs
```

---

## 💻 IMPLEMENTATION GUIDE

### 1. Create BoolToVisibilityConverter

Location: `Converters/BoolToVisibilityConverter.cs`

```csharp
using System.Globalization;

namespace Todo.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? true : false;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue;
        }
        return false;
    }
}
```

### 2. Create DateTimeToStringConverter

Location: `Converters/DateTimeToStringConverter.cs`

```csharp
using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Converts DateTime to formatted string
/// Parameter: format string (e.g., "d" for short date, "t" for short time)
/// </summary>
public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            string format = parameter?.ToString() ?? "d";
            return dateTime.ToString(format, culture);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string strValue && DateTime.TryParse(strValue, culture, System.Globalization.DateTimeStyles.None, out var result))
        {
            return result;
        }
        return DateTime.MinValue;
    }
}
```

### 3. Create InvertedBoolConverter

Location: `Converters/InvertedBoolConverter.cs`

```csharp
using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Converts bool to inverted bool (!value)
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true; // Default: inverted unknown value to true
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }
}
```

### 4. Create EnumToStringConverter

Location: `Converters/EnumToStringConverter.cs`

```csharp
using System.Globalization;
using System.Reflection;

namespace Todo.Converters;

/// <summary>
/// Converts enum value to display string
/// </summary>
public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        var type = value.GetType();
        if (!type.IsEnum)
            return value.ToString();

        // Try to get Description attribute
        var field = type.GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        
        if (attribute != null)
            return attribute.Description;

        // Fallback to string representation
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string strValue && targetType.IsEnum)
        {
            try
            {
                return Enum.Parse(targetType, strValue);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }
}
```

### 5. Create Constants.cs

Location: `Utilities/Constants.cs`

```csharp
namespace Todo.Utilities;

/// <summary>
/// Application-wide constants
/// </summary>
public static class Constants
{
    // Application info
    public const string AppName = "To Do";
    public const string AppVersion = "1.0.0";
    
    // API & Backend (for future server integration)
    public const string ApiBaseUrl = "https://api.todo.example.com";
    public const int ApiTimeoutSeconds = 30;
    
    // Database
    public const string DatabaseFileName = "todo.db";
    
    // Storage keys (SecureStorage, Preferences)
    public const string StorageKeyAuthToken = "auth_token";
    public const string StorageKeyRefreshToken = "refresh_token";
    public const string StorageKeyUserId = "user_id";
    public const string StorageKeyRememberMe = "remember_me";
    
    // Preferences keys
    public const string PrefKeyTheme = "app_theme";
    public const string PrefKeyLanguage = "app_language";
    public const string PrefKeyNotificationsEnabled = "notifications_enabled";
    
    // Validation
    public const int MinPasswordLength = 8;
    public const int MaxTaskTitleLength = 200;
    public const int MaxTaskDescriptionLength = 2000;
    public const int MaxListNameLength = 100;
    
    // UI
    public const int DebounceMilliseconds = 300;
    public const int AnimationDurationMilliseconds = 300;
    public const int LoadingTimeoutMilliseconds = 5000;
    
    // Task limits
    public const int MaxTasksPerList = 10000;
    public const int MaxCustomLists = 50;
    public const int MaxRemindersPerTask = 5;
    
    // Date/Time
    public static readonly TimeSpan DefaultReminderOffset = TimeSpan.FromMinutes(15);
    public const string DateFormatShort = "d";          // 10/21/2026
    public const string DateFormatLong = "D";           // Monday, October 21, 2026
    public const string TimeFormatShort = "t";          // 2:00 PM
    public const string TimeFormatLong = "T";           // 2:00:30 PM
}
```

### 6. Create Enums.cs

Location: `Utilities/Enums.cs`

```csharp
using System.ComponentModel;

namespace Todo.Utilities;

/// <summary>
/// Task status in lifecycle
/// </summary>
public enum TaskStatus
{
    [Description("Active")]
    Active = 0,
    
    [Description("Completed")]
    Completed = 1,
    
    [Description("Deleted")]
    Deleted = 2
}

/// <summary>
/// List category or type
/// </summary>
public enum ListCategory
{
    [Description("My Notes")]
    MyNotes = 0,
    
    [Description("Important")]
    Important = 1,
    
    [Description("Reminder")]
    Reminder = 2,
    
    [Description("Bin")]
    Bin = 3,
    
    [Description("Custom")]
    Custom = 4
}

/// <summary>
/// Recurrence pattern for recurring tasks
/// </summary>
public enum RecurrenceType
{
    [Description("None")]
    None = 0,
    
    [Description("Daily")]
    Daily = 1,
    
    [Description("Weekly")]
    Weekly = 2,
    
    [Description("Monthly")]
    Monthly = 3,
    
    [Description("Yearly")]
    Yearly = 4
}

/// <summary>
/// Application theme
/// </summary>
public enum AppTheme
{
    [Description("Light")]
    Light = 0,
    
    [Description("Dark")]
    Dark = 1,
    
    [Description("System")]
    System = 2
}

/// <summary>
/// HTTP result status for API responses
/// </summary>
public enum ResultStatus
{
    [Description("Success")]
    Success = 0,
    
    [Description("Error")]
    Error = 1,
    
    [Description("Validation")]
    ValidationError = 2,
    
    [Description("Unauthorized")]
    Unauthorized = 3,
    
    [Description("Not Found")]
    NotFound = 4,
    
    [Description("Timeout")]
    Timeout = 5
}
```

### 7. Create Helpers.cs

Location: `Utilities/Helpers.cs`

```csharp
using System.Text.RegularExpressions;

namespace Todo.Utilities;

/// <summary>
/// Utility helper functions
/// </summary>
public static class Helpers
{
    // Email validation
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        return EmailRegex.IsMatch(email);
    }

    // String validation
    public static bool IsNullOrEmpty(string value)
        => string.IsNullOrWhiteSpace(value);

    public static string TruncateIfNeeded(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        
        return value.Length > maxLength 
            ? value.Substring(0, maxLength) + "..." 
            : value;
    }

    // Date helpers
    public static bool IsToday(DateTime date)
        => date.Date == DateTime.Today;

    public static bool IsTomorrow(DateTime date)
        => date.Date == DateTime.Today.AddDays(1);

    public static bool IsThisWeek(DateTime date)
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);
        return date >= startOfWeek && date < endOfWeek;
    }

    public static string GetRelativeDateString(DateTime date)
    {
        if (IsToday(date))
            return "Today";
        
        if (IsTomorrow(date))
            return "Tomorrow";
        
        if (IsThisWeek(date))
            return date.ToString("dddd"); // Monday, Tuesday, etc.
        
        return date.ToString("M/d/yyyy");
    }

    // Color helpers
    public static Color HexToColor(string hex)
    {
        try
        {
            hex = hex.TrimStart('#');
            
            if (hex.Length == 6)
                hex = $"FF{hex}";
            
            if (hex.Length != 8)
                return Colors.White;
            
            return Color.FromArgb(hex);
        }
        catch
        {
            return Colors.White;
        }
    }

    public static string ColorToHex(Color color)
    {
        if (color == null)
            return "#FFFFFF";
        
        var rgba = color.ToArgb();
        return $"#{rgba:X8}";
    }

    // Collection helpers
    public static bool HasItems<T>(IEnumerable<T> collection)
        => collection != null && collection.Any();

    public static int SafeCount<T>(IEnumerable<T> collection)
        => collection?.Count() ?? 0;
}
```

### 8. Register Converters in App.xaml

Add to `App.xaml` in ResourceDictionary:

```xml
<ResourceDictionary.MergedDictionaries>
    <!-- Other merged dictionaries -->
</ResourceDictionary.MergedDictionaries>

<!-- Converters -->
<local:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter" />
<local:InvertedBoolConverter x:Key="InvertedBoolConverter" />
<local:DateTimeToStringConverter x:Key="DateTimeToStringConverter" />
<local:EnumToStringConverter x:Key="EnumToStringConverter" />
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] All converters created (4 converters)
- [ ] Constants.cs defined with 30+ constants
- [ ] Enums.cs defined with 5+ enumerations
- [ ] Helpers.cs has 15+ utility methods
- [ ] All converters registered in App.xaml
- [ ] Project builds without errors
- [ ] Converters work in XAML bindings
- [ ] All utility methods testable
- [ ] No hardcoded values in code

---

## 🧪 TESTING

### Unit Tests

**Test: Email Validation**
```
Given: Various email strings
When: Call Helpers.IsValidEmail()
Then: Returns correct true/false for each
```

**Test: Bool to Visibility Converter**
```
Given: true value
When: Convert to visibility
Then: Returns true
```

**Test: DateTime to String Converter**
```
Given: DateTime 10/21/2026
When: Convert with parameter "d"
Then: Returns "10/21/2026"
```

**Test: Enum to String Converter**
```
Given: TaskStatus.Completed enum
When: Convert to string
Then: Returns "Completed"
```

### Manual Tests

- [ ] Use each converter in XAML binding
- [ ] Verify converter output correct
- [ ] Test with null values
- [ ] Test with edge case values
- [ ] Verify all constants accessible
- [ ] Verify all enums can be used
- [ ] Verify all helpers work correctly

---

## 📊 DEFINITION OF DONE

**Code Quality:**
- [ ] All converters implement IValueConverter
- [ ] Null checks in all converters
- [ ] Constants organized by category
- [ ] Enums use DescriptionAttribute
- [ ] Helpers are static and reusable
- [ ] XML documentation on public items
- [ ] No magic numbers in code

**Testing:**
- [ ] All converters compile
- [ ] All constants accessible
- [ ] All enums accessible
- [ ] All helpers work
- [ ] No null reference exceptions

**Documentation:**
- [ ] Converters documented with XML comments
- [ ] Constants grouped with comments
- [ ] Enums fully enumerated
- [ ] Helper functions purpose clear
- [ ] Task marked complete

---

## 🔗 CROSS-REFERENCES

### Related Tasks
- Task 1-04: Create Resource Dictionaries ✓
- Task 3-01: Create TaskListPage (will use converters)
- Task 4-01: Implement TaskService (will use helpers)

### Reference Materials
- **Specification:** SPECIFICATION.md → Section 12: Validation & Error Handling
- **MAUI Converters:** https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/value-converters
- **QUICK_REFERENCE.md** → Common Patterns section

---

## 💡 USAGE EXAMPLES

### Using Converters in XAML
```xml
<!-- Bool to Visibility -->
<Label IsVisible="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}" />

<!-- DateTime to String -->
<Label Text="{Binding DueDate, Converter={StaticResource DateTimeToStringConverter}, ConverterParameter=d}" />

<!-- Inverted Bool -->
<Button IsEnabled="{Binding IsLoading, Converter={StaticResource InvertedBoolConverter}}" />
```

### Using Constants
```csharp
var token = Preferences.Get(Constants.StorageKeyAuthToken, string.Empty);
if (value.Length > Constants.MaxTaskTitleLength)
{
    // Show validation error
}
```

### Using Enums
```csharp
public ListCategory Category { get; set; } = ListCategory.MyNotes;

public RecurrenceType Recurrence { get; set; } = RecurrenceType.Daily;
```

### Using Helpers
```csharp
if (Helpers.IsValidEmail(email))
{
    // Proceed with registration
}

var dueDate = GetTaskDueDate();
Label.Text = Helpers.GetRelativeDateString(dueDate);

var shortTitle = Helpers.TruncateIfNeeded(longTitle, 50);
```

---

## ⚠️ COMMON ISSUES

**Issue:** Converter not found in XAML
- **Cause:** Converter not registered in App.xaml
- **Solution:** Add converter to ResourceDictionary with x:Key

**Issue:** Email validation too strict/loose
- **Cause:** Regex pattern not matching actual email formats
- **Solution:** Adjust regex pattern or use RFC 5322 compliant regex

**Issue:** DateTime converter returns wrong format
- **Cause:** Format parameter mismatch or culture issues
- **Solution:** Verify format string matches culture, test with CultureInfo

**Issue:** Enum Description not appearing
- **Cause:** DescriptionAttribute not imported or not used
- **Solution:** Add `using System.ComponentModel;` and verify attribute applied

---

## 🔮 FUTURE ENHANCEMENTS

After task complete:
- [ ] Add StringToUpperConverter
- [ ] Add CountToVisibilityConverter (for lists)
- [ ] Add TimeSpanToStringConverter
- [ ] Add ColorConverter (HSL to RGB)
- [ ] Add custom validation converters

---

## 📋 IMPLEMENTATION CHECKLIST

- [ ] Create Converters folder
- [ ] Create BoolToVisibilityConverter
- [ ] Create DateTimeToStringConverter
- [ ] Create InvertedBoolConverter
- [ ] Create EnumToStringConverter
- [ ] Create Utilities folder
- [ ] Create Constants.cs with 30+ constants
- [ ] Create Enums.cs with 5+ enumerations
- [ ] Create Helpers.cs with 15+ helpers
- [ ] Register converters in App.xaml
- [ ] Test all converters
- [ ] Test all constants accessible
- [ ] Test all enums accessible
- [ ] Test all helpers functional
- [ ] Build project successfully
- [ ] Commit to version control

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
