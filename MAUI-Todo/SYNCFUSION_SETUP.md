# Syncfusion .NET MAUI Controls - Setup & Configuration Guide

**Version:** 1.0  
**Framework:** .NET MAUI  
**Syncfusion Version:** 25.1.37+  

---

## Table of Contents

1. [NuGet Package Installation](#nuget-package-installation)
2. [MauiProgram Configuration](#mauiprogram-configuration)
3. [License Registration](#license-registration)
4. [Control-by-Control Setup](#control-by-control-setup)
5. [Namespace Reference](#namespace-reference)
6. [Platform-Specific Configuration](#platform-specific-configuration)
7. [Theming & Customization](#theming--customization)

---

## NuGet Package Installation

### Required Packages

Add the following packages to your `.csproj` file or install via NuGet Package Manager:

```xml
<ItemGroup>
    <!-- Core MAUI Framework -->
    <PackageReference Include="Microsoft.Maui.Controls" Version="8.0.0" />
    <PackageReference Include="Microsoft.Maui.Controls.Hosting" Version="8.0.0" />
    
    <!-- Syncfusion Controls -->
    <PackageReference Include="Syncfusion.Maui.Core" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.Controls" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.Buttons" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.Inputs" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.ListView" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.Popups" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.DataGrid" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.Sliders" Version="25.1.37" />
    <PackageReference Include="Syncfusion.Maui.DateTimePickers" Version="25.1.37" />
    
    <!-- MVVM & Dependencies -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
    
    <!-- Data & Storage -->
    <PackageReference Include="sqlite-net-pcl" Version="1.10.0" />
    <PackageReference Include="SQLiteNetExtensions" Version="4.0.0" />
    
    <!-- Utilities -->
    <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="4.7.0" />
</ItemGroup>
```

### Installation Commands

Using .NET CLI:

```bash
dotnet add package Syncfusion.Maui.Core
dotnet add package Syncfusion.Maui.Controls
dotnet add package Syncfusion.Maui.Buttons
dotnet add package Syncfusion.Maui.Inputs
dotnet add package Syncfusion.Maui.ListView
dotnet add package Syncfusion.Maui.Popups
dotnet add package Syncfusion.Maui.DateTimePickers
dotnet add package CommunityToolkit.Mvvm
```

---

## MauiProgram Configuration

### Basic Configuration

In `MauiProgram.cs`, add Syncfusion services:

```csharp
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Core;
using CommunityToolkit.Mvvm;

namespace MAUI_Todo;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            // Add Syncfusion License
            .ConfigureSyncfusion()
            // Add services
            .ConfigureAppServices();

        return builder.Build();
    }

    // Syncfusion Configuration
    private static MauiAppBuilder ConfigureSyncfusion(this MauiAppBuilder builder)
    {
        builder
            .UseSyncfusionCore() // Enable all Syncfusion controls
            .ConfigureSyncfusionTheme();
        
        return builder;
    }

    // Configure Syncfusion Theme
    private static MauiAppBuilder ConfigureSyncfusionTheme(this MauiAppBuilder builder)
    {
        var syncfusionTheme = new SyncfusionThemeData()
        {
            PrimaryColor = Color.FromArgb("#6366F1"), // Indigo
            SecondaryColor = Color.FromArgb("#EC4899"), // Pink
            Brightness = Brightness.Light,
        };

        Syncfusion.Maui.Core.ThemeHelper.SyncfusionTheme = syncfusionTheme;
        
        return builder;
    }

    // App Services Configuration
    private static MauiAppBuilder ConfigureAppServices(this MauiAppBuilder builder)
    {
        builder.Services
            // ViewModels
            .AddSingleton<SplashViewModel>()
            .AddSingleton<SignInViewModel>()
            .AddSingleton<SignUpViewModel>()
            // Views
            .AddSingleton<SplashView>()
            .AddSingleton<SignInView>()
            .AddSingleton<SignUpView>()
            // Services
            .AddSingleton<IAuthService, AuthService>()
            .AddSingleton<ITaskService, TaskService>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddSingleton<INavigationService, NavigationService>();
        
        return builder;
    }
}
```

---

## License Registration

### Using Syncfusion License Key

For commercial usage, register your Syncfusion license in `App.xaml.cs`:

```csharp
using Syncfusion.Licensing;

namespace MAUI_Todo;

public partial class App : Application
{
    public App()
    {
        // Register Syncfusion License
        SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
        
        InitializeComponent();

        MainPage = new AppShell();
    }
}
```

**Note:** For development/evaluation, you can skip this. For production, visit https://www.syncfusion.com/maui-controls/licensing

---

## Control-by-Control Setup

### 1. SfTextInputFieldOutline (Text Input)

**Purpose:** Email, password, task title, description inputs

**XAML Usage:**
```xml
<syncfusion:SfTextInputFieldOutline
    x:Name="EmailInput"
    Hint="Enter your email"
    HelperText="Please enter a valid email"
    Text="{Binding Email}"
    ContainerBackground="#F5F5F5"
    CornerRadius="8"
    BorderColor="#6366F1"
    HorizontalOptions="FillAndExpand"
    Margin="16,8">
    <syncfusion:SfTextInputFieldOutline.LeadingViewTemplate>
        <DataTemplate>
            <Label Text="✉️" FontSize="18" VerticalOptions="Center" />
        </DataTemplate>
    </syncfusion:SfTextInputFieldOutline.LeadingViewTemplate>
</syncfusion:SfTextInputFieldOutline>
```

**Code-Behind:**
```csharp
// For password field with toggle
var passwordInput = new SfTextInputFieldOutline
{
    Hint = "Password",
    IsPassword = true,
    Text = new Binding("Password", BindingMode.TwoWay),
    BindingContext = viewModel
};
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Inputs;assembly=Syncfusion.Maui.Inputs"`

---

### 2. SfButton (Buttons)

**Purpose:** All action buttons (Sign In, Save, Delete, Create)

**XAML Usage:**
```xml
<!-- Primary Button -->
<syncfusion:SfButton
    Text="SIGN IN"
    Command="{Binding SignInCommand}"
    BackgroundColor="#6366F1"
    TextColor="White"
    FontSize="16"
    FontAttributes="Bold"
    CornerRadius="8"
    Padding="16,12"
    HorizontalOptions="FillAndExpand"
    Margin="16,8" />

<!-- Secondary Button -->
<syncfusion:SfButton
    Text="CANCEL"
    Command="{Binding CancelCommand}"
    BackgroundColor="#F5F5F5"
    TextColor="#1F2937"
    FontSize="14"
    CornerRadius="8"
    Padding="16,12" />

<!-- Destructive Button -->
<syncfusion:SfButton
    Text="DELETE"
    Command="{Binding DeleteCommand}"
    BackgroundColor="#EF4444"
    TextColor="White"
    FontSize="14" />
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Buttons;assembly=Syncfusion.Maui.Buttons"`

---

### 3. SfListView (Task Lists)

**Purpose:** Display tasks, lists, reminders

**XAML Usage:**
```xml
<syncfusion:SfListView
    x:Name="TaskListView"
    ItemsSource="{Binding Tasks}"
    SelectionMode="Single"
    SelectedItem="{Binding SelectedTask}"
    SelectionChangedCommand="{Binding TaskSelectedCommand}"
    HorizontalOptions="FillAndExpand"
    VerticalOptions="FillAndExpand">
    <syncfusion:SfListView.ItemTemplate>
        <DataTemplate>
            <StackLayout Padding="16,8" Spacing="8">
                <Grid ColumnDefinitions="44,*,44" ColumnSpacing="12">
                    <!-- Checkbox -->
                    <CheckBox 
                        IsChecked="{Binding IsCompleted}"
                        Margin="0" />
                    
                    <!-- Task Content -->
                    <StackLayout Grid.Column="1" Spacing="4">
                        <Label 
                            Text="{Binding Title}"
                            FontSize="16"
                            FontAttributes="Bold"
                            TextDecorations="{Binding IsCompleted, Converter={StaticResource BoolToTextDecorationConverter}}" />
                        <Label 
                            Text="{Binding DueDate, StringFormat='Due: {0:MMM dd, yyyy}'}"
                            FontSize="12"
                            TextColor="#6B7280" />
                    </StackLayout>
                    
                    <!-- Menu Button -->
                    <Button 
                        Grid.Column="2"
                        Text="⋮"
                        Command="{Binding Source={RelativeSource AncestorType={x:Type local:MyNotesView}}, Path=BindingContext.MoreMenuCommand}"
                        CommandParameter="{Binding .}" />
                </Grid>
            </StackLayout>
        </DataTemplate>
    </syncfusion:SfListView.ItemTemplate>
</syncfusion:SfListView>
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.ListView;assembly=Syncfusion.Maui.ListView"`

---

### 4. SfDatePicker (Date Selection)

**Purpose:** Select due dates for tasks

**XAML Usage:**
```xml
<syncfusion:SfDatePicker
    x:Name="DueDatePicker"
    SelectedDate="{Binding DueDate}"
    ShowFooter="True"
    MonthHeaderFormat="MMMM"
    AllowNullInput="True"
    HorizontalOptions="FillAndExpand"
    Margin="16,8" />
```

**Code-Behind:**
```csharp
var datePicker = new SfDatePicker
{
    SelectedDate = DateTime.Now,
    MinimumDate = DateTime.Now,
    AllowNullInput = true,
    BindingContext = viewModel
};
datePicker.SetBinding(SfDatePicker.SelectedDateProperty, 
    new Binding("DueDate", BindingMode.TwoWay));
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Pickers;assembly=Syncfusion.Maui.Inputs"`

---

### 5. SfTimePicker (Time Selection)

**Purpose:** Select due times for tasks

**XAML Usage:**
```xml
<syncfusion:SfTimePicker
    x:Name="DueTimePicker"
    SelectedTime="{Binding DueTime}"
    Format="hh:mm tt"
    ShowFooter="True"
    HorizontalOptions="FillAndExpand"
    Margin="16,8" />
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Pickers;assembly=Syncfusion.Maui.Inputs"`

---

### 6. SfComboBox (Dropdowns)

**Purpose:** List selector, sort/filter options, reminder options

**XAML Usage:**
```xml
<syncfusion:SfComboBox
    x:Name="ListComboBox"
    DisplayMemberPath="Name"
    SelectedValuePath="Id"
    ItemsSource="{Binding TaskLists}"
    SelectedValue="{Binding SelectedListId}"
    Placeholder="Select a list"
    IsFilteringEnabled="True"
    AllowFiltering="True"
    ShowBorder="True"
    BorderColor="#E5E7EB"
    HorizontalOptions="FillAndExpand"
    Margin="16,8" />
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Inputs;assembly=Syncfusion.Maui.Inputs"`

---

### 7. SfProgressBar (Password Strength Indicator)

**Purpose:** Visual feedback for password strength

**XAML Usage:**
```xml
<StackLayout Margin="16,8">
    <Label Text="Password Strength" FontSize="12" TextColor="#6B7280" />
    <syncfusion:SfProgressBar
        x:Name="PasswordStrengthBar"
        Progress="{Binding PasswordStrength}"
        ProgressHeight="8"
        CornerRadius="4"
        ProgressColor="{Binding PasswordStrength, Converter={StaticResource StrengthToColorConverter}}"
        TrackColor="#E5E7EB"
        HorizontalOptions="FillAndExpand" />
    <Label 
        Text="{Binding PasswordStrengthText}"
        FontSize="11"
        TextColor="#6B7280"
        Margin="0,4,0,0" />
</StackLayout>
```

**Code-Behind (ViewModel):**
```csharp
private string _passwordStrengthText = "Weak";
private double _passwordStrength = 0;

public void ValidatePasswordStrength(string password)
{
    if (string.IsNullOrEmpty(password))
    {
        PasswordStrength = 0;
        PasswordStrengthText = "Weak";
    }
    else if (password.Length < 8)
    {
        PasswordStrength = 25;
        PasswordStrengthText = "Weak";
    }
    else if (password.Length < 12)
    {
        PasswordStrength = 50;
        PasswordStrengthText = "Fair";
    }
    else if (Regex.IsMatch(password, @"[A-Z]") && 
             Regex.IsMatch(password, @"[a-z]") && 
             Regex.IsMatch(password, @"[0-9]") &&
             Regex.IsMatch(password, @"[!@#$%^&*]"))
    {
        PasswordStrength = 100;
        PasswordStrengthText = "Strong";
    }
}
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Gauges;assembly=Syncfusion.Maui.Gauges"`

---

### 8. SfBusyIndicator (Loading Spinner)

**Purpose:** Show loading state on splash/during operations

**XAML Usage:**
```xml
<syncfusion:SfBusyIndicator
    x:Name="LoadingIndicator"
    IsRunning="True"
    AnimationType="CircularMaterial"
    TextColor="#6366F1"
    ViewBoxWidth="60"
    ViewBoxHeight="60"
    Title="Loading..."
    TitleFontSize="14"
    TitleFontColor="#6B7280" />
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Core;assembly=Syncfusion.Maui.Core"`

---

### 9. SfCheckBox (Checkboxes)

**Purpose:** Task completion, important toggle, terms acceptance

**XAML Usage:**
```xml
<!-- Task Completion -->
<syncfusion:SfCheckBox
    IsChecked="{Binding IsCompleted}"
    CheckedColor="#10B981"
    UncheckedColor="#E5E7EB"
    CornerRadius="4"
    CheckBoxSize="24" />

<!-- Important Toggle -->
<syncfusion:SfCheckBox
    Text="Mark as Important"
    IsChecked="{Binding IsImportant}"
    FontSize="14"
    CheckedColor="#EC4899" />

<!-- Terms Acceptance -->
<syncfusion:SfCheckBox
    Text="I agree to Terms &amp; Conditions"
    IsChecked="{Binding TermsAccepted}"
    FontSize="13"
    TextColor="#1F2937" />
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Buttons;assembly=Syncfusion.Maui.Buttons"`

---

### 10. SfSnackBar (Notifications)

**Purpose:** Show toast notifications for user actions

**XAML Usage:**
```xml
<syncfusion:SfSnackBar
    x:Name="TaskSnackBar"
    Message="Task completed successfully"
    ActionButtonText="UNDO"
    Duration="3000"
    BackgroundColor="#10B981"
    TextColor="White"
    ActionButtonTextColor="White" />
```

**Code-Behind:**
```csharp
public void ShowNotification(string message, string actionText = null, Action actionCommand = null)
{
    var snackBar = new SfSnackBar
    {
        Message = message,
        ActionButtonText = actionText ?? string.Empty,
        Duration = 3000,
        BackgroundColor = Colors.Green,
        TextColor = Colors.White
    };

    if (actionCommand != null)
    {
        snackBar.ActionButtonClicked += (s, e) => actionCommand?.Invoke();
    }

    snackBar.Show();
}
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Core;assembly=Syncfusion.Maui.Core"`

---

### 11. SfPopup (Modal Dialogs)

**Purpose:** Confirmation dialogs, task details, list creation modals

**XAML Usage:**
```xml
<syncfusion:SfPopup
    x:Name="ConfirmationPopup"
    Title="Confirm Delete?"
    Message="This action cannot be undone."
    PopupStyle="Default"
    HeaderBackground="#EF4444"
    HeaderTextColor="White"
    AcceptButtonText="DELETE"
    DeclineButtonText="CANCEL"
    AcceptButtonClicked="OnDeleteConfirmed"
    DeclineButtonClicked="OnDeleteCancelled" />
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Popup;assembly=Syncfusion.Maui.Popups"`

---

### 12. SfSegmentedControl (View Toggles - Optional)

**Purpose:** Switch between different view modes

**XAML Usage:**
```xml
<syncfusion:SfSegmentedControl
    x:Name="ViewToggle"
    ItemsSource="{Binding ViewOptions}"
    SelectedIndex="0"
    SelectionChanged="OnViewChanged"
    DisplayMemberPath="Name"
    CornerRadius="8"
    BorderColor="#E5E7EB"
    SelectedSegmentBackground="#6366F1"
    SelectedSegmentTextColor="White" />
```

**Namespace:** `xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Buttons;assembly=Syncfusion.Maui.Buttons"`

---

## Namespace Reference

Add these namespaces to your XAML files as needed:

```xml
<!-- Standard MAUI -->
xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
xmlns:local="clr-namespace:MAUI_Todo"

<!-- Syncfusion Core -->
xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Core;assembly=Syncfusion.Maui.Core"

<!-- Syncfusion Buttons -->
xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Buttons;assembly=Syncfusion.Maui.Buttons"

<!-- Syncfusion Inputs -->
xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Inputs;assembly=Syncfusion.Maui.Inputs"

<!-- Syncfusion ListView -->
xmlns:syncfusion="clr-namespace:Syncfusion.Maui.ListView;assembly=Syncfusion.Maui.ListView"

<!-- Syncfusion Popups -->
xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Popup;assembly=Syncfusion.Maui.Popups"

<!-- Syncfusion Pickers (Date/Time) -->
xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Pickers;assembly=Syncfusion.Maui.Inputs"

<!-- Syncfusion Gauges (Progress Bar) -->
xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Gauges;assembly=Syncfusion.Maui.Gauges"
```

**C# Using Statements:**

```csharp
using Syncfusion.Maui.Core;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.ListView;
using Syncfusion.Maui.Popup;
using Syncfusion.Maui.Pickers;
using Syncfusion.Maui.Gauges;
using Syncfusion.Licensing;
```

---

## Platform-Specific Configuration

### iOS Configuration

In `Platforms/iOS/Info.plist`:

```xml
<dict>
    <key>UIDeviceFamily</key>
    <array>
        <integer>1</integer>
        <integer>2</integer>
    </array>
    <key>UIRequiredDeviceCapabilities</key>
    <array>
        <string>armv7</string>
    </array>
    <key>UISupportedInterfaceOrientations</key>
    <array>
        <string>UIInterfaceOrientationPortrait</string>
        <string>UIInterfaceOrientationPortraitUpsideDown</string>
    </array>
    <key>UISupportedInterfaceOrientations~ipad</key>
    <array>
        <string>UIInterfaceOrientationPortrait</string>
        <string>UIInterfaceOrientationPortraitUpsideDown</string>
        <string>UIInterfaceOrientationLandscapeLeft</string>
        <string>UIInterfaceOrientationLandscapeRight</string>
    </array>
    <!-- Local Notifications Permission -->
    <key>NSUserNotificationAlertStyle</key>
    <string>alert</string>
</dict>
```

### Android Configuration

In `Platforms/Android/AndroidManifest.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <uses-sdk android:minSdkVersion="27" android:targetSdkVersion="34" />
    
    <uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
    <uses-permission android:name="android.permission.SCHEDULE_EXACT_ALARM" />
    
    <application android:usesCleartextTraffic="false" />
</manifest>
```

### Windows Configuration

In `Platforms/Windows/App.xaml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Application
    x:Class="MAUI_Todo.WinUI.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
    </Application.Resources>
</Application>
```

---

## Theming & Customization

### Light Theme Configuration

In `Resources/Themes/LightTheme.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Core;assembly=Syncfusion.Maui.Core">

    <!-- Light Theme Configuration -->
    <Color x:Key="PrimaryColor">#6366F1</Color>
    <Color x:Key="SecondaryColor">#EC4899</Color>
    <Color x:Key="BackgroundColor">#FFFFFF</Color>
    <Color x:Key="SurfaceColor">#F9FAFB</Color>
    <Color x:Key="TextColor">#1F2937</Color>
    <Color x:Key="TextSecondaryColor">#6B7280</Color>

    <!-- Syncfusion Theme -->
    <Style TargetType="syncfusion:SfButton">
        <Setter Property="BorderColor" Value="#E5E7EB" />
        <Setter Property="BorderWidth" Value="1" />
    </Style>

    <Style TargetType="syncfusion:SfTextInputFieldOutline">
        <Setter Property="ContainerBackground" Value="{StaticResource BackgroundColor}" />
        <Setter Property="BorderColor" Value="{StaticResource PrimaryColor}" />
    </Style>

</ResourceDictionary>
```

### Dark Theme Configuration

In `Resources/Themes/DarkTheme.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:syncfusion="clr-namespace:Syncfusion.Maui.Core;assembly=Syncfusion.Maui.Core">

    <!-- Dark Theme Configuration -->
    <Color x:Key="PrimaryColor">#8B5CF6</Color>
    <Color x:Key="SecondaryColor">#EC4899</Color>
    <Color x:Key="BackgroundColor">#1F2937</Color>
    <Color x:Key="SurfaceColor">#374151</Color>
    <Color x:Key="TextColor">#F9FAFB</Color>
    <Color x:Key="TextSecondaryColor">#D1D5DB</Color>

</ResourceDictionary>
```

---

## Troubleshooting

### Common Issues

**Issue:** Syncfusion controls not showing
- **Solution:** Ensure `UseSyncfusionCore()` is called in MauiProgram.cs
- **Solution:** Verify license registration if using commercial version

**Issue:** NuGet restore fails
- **Solution:** Check NuGet.org is in your package sources
- **Solution:** Update NuGet package manager to latest version

**Issue:** License validation warning
- **Solution:** Register valid license key in App.xaml.cs
- **Solution:** Suppress warnings for development if using community license

**Issue:** Performance issues with large lists
- **Solution:** Enable virtualization in SfListView
- **Solution:** Use async data loading
- **Solution:** Implement data filtering/pagination

**Issue:** Date/Time picker not showing correctly
- **Solution:** Ensure SfDatePicker/SfTimePicker namespace is correct
- **Solution:** Check platform-specific calendar permissions

---

## Resources

- **Syncfusion Documentation:** https://help.syncfusion.com/maui/introduction/overview
- **MAUI Documentation:** https://learn.microsoft.com/en-us/dotnet/maui/
- **Syncfusion Samples:** https://github.com/syncfusion/maui-demos
- **Community Toolkit:** https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/

---

**End of Syncfusion Setup Guide**

Version: 1.0 | Last Updated: 2024
