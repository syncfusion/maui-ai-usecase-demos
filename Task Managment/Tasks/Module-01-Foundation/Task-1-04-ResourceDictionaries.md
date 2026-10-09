# Task 1-04: Create Resource Dictionaries (Colors, Sizes, Fonts, Styles)

**Module:** 1 - Foundation & Setup  
**Phase:** 1  
**Priority:** Critical  
**Duration:** 1 day  
**Dependencies:** Task 1-02 (NuGet packages)  
**Status:** ⬜ Not Started

---

## 📋 TASK DESCRIPTION

Create centralized resource dictionaries in XAML for design tokens (colors, spacing, typography, styles). These resources enable consistent theming across the entire application and make design changes global.

## 🎯 OBJECTIVES

- [ ] Create Colors.xaml with all design system colors
- [ ] Create Sizes.xaml with spacing and dimensions
- [ ] Create Fonts.xaml with typography system
- [ ] Create Styles.xaml with component styles
- [ ] Merge all dictionaries into App.xaml
- [ ] Test resource resolution and binding
- [ ] Verify theming works globally

---

## 📝 FILES TO CREATE

```
Resources/
├── Styles/
│   ├── Colors.xaml
│   ├── Sizes.xaml
│   ├── Fonts.xaml
│   └── Styles.xaml
App.xaml                    ← Modify to include resources
```

---

## 💻 DESIGN TOKENS (From SPECIFICATION.md)

### Color Palette

**Primary Colors:**
- Primary: `#5C4EAE` (Purple)
- Primary Dark: `#453685`
- Primary Light: `#8A7FD9`

**Category Colors:**
- Important: `#9C27B0` (Purple)
- Reminder: `#2196F3` (Blue)
- Bin: `#F44336` (Red)
- MyNotes: `#E8E8E8` (Gray)

**Semantic Colors:**
- Background: `#FFFFFF`
- Surface: `#F5F5F5`
- Text Primary: `#212121`
- Text Secondary: `#757575`
- Divider: `#BDBDBD`
- Error: `#D32F2F`
- Success: `#388E3C`
- Warning: `#F57C00`

### Spacing System

**Dimensions:**
- xxs: 4dp
- xs: 8dp
- s: 12dp
- m: 16dp
- l: 24dp
- xl: 32dp

**Component-Specific:**
- Header Height: 56dp
- Sidebar Width: 230px
- Bottom Input Height: 64dp
- Task Item Padding: 12dp
- Task Item Margin: 8dp

### Typography

**Font Family:**
- Primary: Segoe UI (Windows), Roboto (Android/Linux)

**Font Sizes:**
- Heading 1: 24sp
- Heading 2: 20sp
- Title: 16sp
- Body: 14sp
- Caption: 12sp
- Subtitle: 11sp

**Font Weights:**
- Regular: 400
- Medium: 500
- SemiBold: 600
- Bold: 700

### Corner Radius

- Small: 4dp
- Medium: 8dp
- Large: 16dp
- Full: 50%

---

## 💻 IMPLEMENTATION GUIDE

### 1. Create Colors.xaml

Location: `Resources/Styles/Colors.xaml`

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- Primary Colors -->
    <Color x:Key="PrimaryColor">#5C4EAE</Color>
    <Color x:Key="PrimaryDarkColor">#453685</Color>
    <Color x:Key="PrimaryLightColor">#8A7FD9</Color>

    <!-- Category Colors -->
    <Color x:Key="ImportantColor">#9C27B0</Color>
    <Color x:Key="ReminderColor">#2196F3</Color>
    <Color x:Key="BinColor">#F44336</Color>
    <Color x:Key="MyNotesColor">#E8E8E8</Color>

    <!-- Semantic Colors -->
    <Color x:Key="BackgroundColor">#FFFFFF</Color>
    <Color x:Key="SurfaceColor">#F5F5F5</Color>
    <Color x:Key="TextPrimaryColor">#212121</Color>
    <Color x:Key="TextSecondaryColor">#757575</Color>
    <Color x:Key="DividerColor">#BDBDBD</Color>
    <Color x:Key="ErrorColor">#D32F2F</Color>
    <Color x:Key="SuccessColor">#388E3C</Color>
    <Color x:Key="WarningColor">#F57C00</Color>

</ResourceDictionary>
```

**Key Points:**
- Use semantic names (e.g., `TextPrimaryColor`, not `DarkGrayColor`)
- All colors as hexadecimal with alpha channel if needed
- Group related colors together
- Add comments for category clarity

### 2. Create Sizes.xaml

Location: `Resources/Styles/Sizes.xaml`

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- Spacing Scale -->
    <x:Double x:Key="SpacingXxs">4</x:Double>
    <x:Double x:Key="SpacingXs">8</x:Double>
    <x:Double x:Key="SpacingS">12</x:Double>
    <x:Double x:Key="SpacingM">16</x:Double>
    <x:Double x:Key="SpacingL">24</x:Double>
    <x:Double x:Key="SpacingXl">32</x:Double>

    <!-- Component Dimensions -->
    <x:Double x:Key="HeaderHeight">56</x:Double>
    <x:Double x:Key="BottomInputHeight">64</x:Double>
    <x:Double x:Key="SidebarWidth">230</x:Double>
    <x:Double x:Key="TaskItemPadding">12</x:Double>
    <x:Double x:Key="TaskItemMargin">8</x:Double>

    <!-- Corner Radius -->
    <CornerRadius x:Key="CornerRadiusSmall">4</CornerRadius>
    <CornerRadius x:Key="CornerRadiusMedium">8</CornerRadius>
    <CornerRadius x:Key="CornerRadiusLarge">16</CornerRadius>

    <!-- Stroke Thickness -->
    <x:Double x:Key="StrokeThicknessThin">1</x:Double>
    <x:Double x:Key="StrokeThicknessMedium">2</x:Double>
    <x:Double x:Key="StrokeThicknessThick">4</x:Double>

</ResourceDictionary>
```

### 3. Create Fonts.xaml

Location: `Resources/Styles/Fonts.xaml`

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- Font Family -->
    <x:String x:Key="FontFamilyRegular">SegoeUI</x:String>

    <!-- Font Sizes -->
    <x:Double x:Key="FontSizeHeading1">24</x:Double>
    <x:Double x:Key="FontSizeHeading2">20</x:Double>
    <x:Double x:Key="FontSizeTitle">16</x:Double>
    <x:Double x:Key="FontSizeBody">14</x:Double>
    <x:Double x:Key="FontSizeCaption">12</x:Double>
    <x:Double x:Key="FontSizeSubtitle">11</x:Double>

    <!-- Font Attributes -->
    <FontAttributes x:Key="FontAttributesRegular">None</FontAttributes>
    <FontAttributes x:Key="FontAttributesMedium">Bold</FontAttributes>
    <FontAttributes x:Key="FontAttributesBold">Bold</FontAttributes>

</ResourceDictionary>
```

### 4. Create Styles.xaml

Location: `Resources/Styles/Styles.xaml`

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- Button Styles -->
    <Style x:Key="ButtonPrimary" TargetType="Button">
        <Setter Property="BackgroundColor" Value="{StaticResource PrimaryColor}" />
        <Setter Property="TextColor" Value="{StaticResource BackgroundColor}" />
        <Setter Property="Padding" Value="{StaticResource SpacingM}" />
        <Setter Property="CornerRadius" Value="24" />
        <Setter Property="FontSize" Value="{StaticResource FontSizeBody}" />
    </Style>

    <Style x:Key="ButtonSecondary" TargetType="Button">
        <Setter Property="BackgroundColor" Value="{StaticResource SurfaceColor}" />
        <Setter Property="TextColor" Value="{StaticResource PrimaryColor}" />
        <Setter Property="BorderColor" Value="{StaticResource PrimaryColor}" />
        <Setter Property="BorderWidth" Value="1" />
        <Setter Property="Padding" Value="{StaticResource SpacingM}" />
        <Setter Property="CornerRadius" Value="24" />
    </Style>

    <!-- Entry Styles -->
    <Style x:Key="EntryDefault" TargetType="Entry">
        <Setter Property="FontSize" Value="{StaticResource FontSizeBody}" />
        <Setter Property="Padding" Value="{StaticResource SpacingM}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimaryColor}" />
        <Setter Property="PlaceholderColor" Value="{StaticResource TextSecondaryColor}" />
        <Setter Property="BackgroundColor" Value="{StaticResource SurfaceColor}" />
    </Style>

    <!-- Label Styles -->
    <Style x:Key="LabelHeading1" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeHeading1}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimaryColor}" />
        <Setter Property="FontAttributes" Value="Bold" />
    </Style>

    <Style x:Key="LabelBody" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeBody}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimaryColor}" />
    </Style>

    <Style x:Key="LabelCaption" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeCaption}" />
        <Setter Property="TextColor" Value="{StaticResource TextSecondaryColor}" />
    </Style>

</ResourceDictionary>
```

### 5. Merge into App.xaml

Edit `App.xaml` to include all resource dictionaries:

```xml
<?xml version = "1.0" encoding = "UTF-8" ?>
<Application
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="Todo.App">

    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
                <ResourceDictionary Source="Resources/Styles/Sizes.xaml" />
                <ResourceDictionary Source="Resources/Styles/Fonts.xaml" />
                <ResourceDictionary Source="Resources/Styles/Styles.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>

</Application>
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] All 4 XAML files created (Colors, Sizes, Fonts, Styles)
- [ ] All design tokens from SPECIFICATION.md included
- [ ] Colors use hexadecimal format
- [ ] Sizes use semantic naming (Spacing prefix)
- [ ] Font sizes in device-independent units
- [ ] CornerRadius and stroke thickness defined
- [ ] Button, Entry, and Label styles created
- [ ] All dictionaries merged into App.xaml
- [ ] Project builds without errors
- [ ] Resources accessible in XAML bindings
- [ ] No duplicate resource names

---

## 🧪 TESTING

### Unit Tests

**Test: Resource Resolution**
```
Given: App with merged resource dictionaries
When: Access color resource "PrimaryColor"
Then: Returns correct hex value #5C4EAE
```

**Test: Dynamic Resource Binding**
```
Given: Label with DynamicResource BackgroundColor
When: Theme changes
Then: Label background updates automatically
```

### Manual Tests

- [ ] Open App.xaml, verify all dictionaries merged
- [ ] Create test page with color binding: `Background="{StaticResource PrimaryColor}"`
- [ ] Create test control with spacing: `Padding="{StaticResource SpacingM}"`
- [ ] Create test label: `Style="{StaticResource LabelHeading1}"`
- [ ] Verify all colors display correctly
- [ ] Verify all sizes render correctly
- [ ] Verify all styles apply correctly
- [ ] Build and run app (resources should load without error)

---

## 📊 DEFINITION OF DONE

**Code Quality:**
- [ ] XAML is well-formatted and readable
- [ ] Resources organized logically (colors, sizes, fonts, styles)
- [ ] No hardcoded values in dictionaries
- [ ] Consistent naming convention (x:Key)
- [ ] Comments on complex resource definitions

**Testing:**
- [ ] All resources resolve without errors
- [ ] Bindings work in test pages
- [ ] Color hex values correct
- [ ] Dimensions in correct units
- [ ] Styles apply to controls

**Documentation:**
- [ ] Design tokens documented
- [ ] Resource names consistent with guidelines
- [ ] Task marked complete
- [ ] Any custom resources noted

---

## 🔗 CROSS-REFERENCES

### Related Tasks
- Task 1-02: Install NuGet packages ✓
- Task 1-05: Setup MauiProgram (will use these resources)
- Task 3-01: Create TaskListPage (will bind to these resources)

### Reference Materials
- **Specification:** SPECIFICATION.md → Section 6: Design System & UI Requirements
- **Quick Reference:** QUICK_REFERENCE.md → Design Tokens section
- **MAUI Docs:** https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/styles

---

## 💡 TIPS & BEST PRACTICES

### Tip 1: Organize by Type
Keep resources organized:
- All colors together
- All dimensions together
- All fonts together
- All styles together

### Tip 2: Use Semantic Naming
Good: `TextPrimaryColor`, `SpacingM`, `CornerRadiusMedium`  
Bad: `DarkGray`, `Size16`, `Rounded8`

### Tip 3: Reference Other Resources
```xml
<Style x:Key="ButtonPrimary" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{StaticResource PrimaryColor}" />
    <!-- Reference another resource -->
</Style>
```

### Tip 4: Test Early
After creating each dictionary, build and test binding works.

### Tip 5: Document Special Values
```xml
<!-- Header height matches platform standards: 56dp -->
<x:Double x:Key="HeaderHeight">56</x:Double>
```

---

## ⚠️ COMMON ISSUES

**Issue:** Resources not resolving in XAML
- **Cause:** Path to XAML file incorrect or file not in right location
- **Solution:** Verify path in App.xaml matches actual file location

**Issue:** StaticResource binding fails at runtime
- **Cause:** Resource key misspelled or doesn't exist
- **Solution:** Use IntelliSense to verify resource names

**Issue:** Color shows incorrectly
- **Cause:** Hex value wrong or platform rendering difference
- **Solution:** Verify hex value, test on actual device

**Issue:** Style not applying to control
- **Cause:** TargetType mismatch or SetterProperty wrong
- **Solution:** Verify TargetType matches control type, check property name

---

## 📋 IMPLEMENTATION CHECKLIST

- [ ] Create Colors.xaml with all colors from design system
- [ ] Create Sizes.xaml with spacing scale
- [ ] Create Fonts.xaml with typography system
- [ ] Create Styles.xaml with component styles
- [ ] Edit App.xaml to merge all dictionaries
- [ ] Verify all resources defined
- [ ] Test resource binding in test page
- [ ] Test all colors render correctly
- [ ] Test all sizes render correctly
- [ ] Test all styles apply correctly
- [ ] Build project without errors
- [ ] Commit to version control

---

## 🔮 FUTURE ENHANCEMENTS

After task complete:
- [ ] Add dark theme resources (Colors.Dark.xaml)
- [ ] Add animation resources (Animations.xaml)
- [ ] Add platform-specific styles (Styles.Windows.xaml)
- [ ] Create theme switcher command

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
