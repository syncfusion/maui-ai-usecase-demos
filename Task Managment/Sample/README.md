# Todo — Syncfusion MAUI Sample

This sample implements the **Module 01 – Foundation & Setup** tasks of the
Syncfusion MAUI To Do List specification. The solution is the entry point
that subsequent modules (Authentication, My Notes, Task CRUD, …) build on.

> Built with: **.NET 8 + .NET MAUI 8.0** and **Syncfusion MAUI 24.1.47**.

## Solution layout

```
Task Managment/Sample/
├── Todo.slnx                          ← solution
├── NuGet.config                       ← single nuget.org source
└── Todo/
    ├── Todo.csproj                    ← MAUI app + NuGet package references
    ├── MauiProgram.cs                 ← MAUI bootstrap + Syncfusion core
    ├── App.xaml(.cs)                  ← Application root + resource dictionaries
    ├── AppShell.xaml(.cs)             ← Shell root (route registration in Task 1-06)
    ├── MainPage.xaml(.cs)             ← Placeholder landing page
    ├── Platforms/                     ← Android, iOS, MacCatalyst, Windows
    ├── Resources/                     ← Styles, fonts, splash, icons
    ├── Views/                         ← UI by feature (Authentication, Main, Shared, Dialogs)
    ├── ViewModels/                    ← MVVM ViewModels (Task 1-03)
    ├── Models/ (+ DTOs/)              ← Domain models and DTOs
    ├── Services/                      ← Service implementations by area
    │   ├── Interfaces/
    │   ├── Authentication/
    │   ├── Task/
    │   ├── Search/
    │   ├── Storage/
    │   └── Infrastructure/
    ├── Data/Repositories/             ← SQLite repositories (Task 1-07)
    ├── Converters/                    ← XAML value converters (Task 1-08)
    ├── Behaviors/                     ← MAUI behaviors (Task 1-08)
    └── Utilities/                     ← Constants, Enums, Helpers (Task 1-08)
```

## Module-01 task map

| Task | Description                                  | Status                  |
|------|----------------------------------------------|-------------------------|
| 1-01 | Project folder structure                     | ✅ Folders + READMEs     |
| 1-02 | Install and verify NuGet packages            | ✅ `Todo.csproj`         |
| 1-03 | Base MVVM framework (`ViewModelBase`)        | ⬜ next                 |
| 1-04 | Resource dictionaries (Colors, Sizes, Fonts) | ⬜ next                 |
| 1-05 | `MauiProgram` and DI registrations           | ⬜ next                 |
| 1-06 | Navigation infrastructure (`AppShell` etc.)  | ⬜ next                 |
| 1-07 | Database setup (`AppDbContext`, SQLite init) | ⬜ next                 |
| 1-08 | Utility classes (Constants, Enums, Helpers)  | ⬜ next                 |

## Task 1-02 — Installed NuGet packages

The exact package versions added to `Todo.csproj` are documented in
`Task Managment/Tasks/Module-01-Foundation/Task-1-02-NuGetPackages.md` and
`Task Managment/SPECIFICATION.md § 11.2`.

| Package                              | Version    | Purpose                       |
|--------------------------------------|------------|-------------------------------|
| Microsoft.Maui.Controls              | 8.0.100    | MAUI framework (8.0.x)        |
| Microsoft.Extensions.Logging.Debug   | 8.0.0      | Debug log provider            |
| Syncfusion.Maui.ListView             | 24.1.47    | Task list display             |
| Syncfusion.Maui.Calendar             | 24.1.47    | Date picker                   |
| Syncfusion.Maui.Core                 | 24.1.47    | Core framework / theme        |
| Syncfusion.Maui.Buttons              | 24.1.47    | Button controls (future)      |
| CommunityToolkit.Mvvm                | 8.4.0      | ObservableProperty / Command  |
| sqlite-net-pcl                       | 1.8.116    | SQLite ORM (1.8.x)            |
| SQLitePCLRaw.bundle_green            | 2.1.11     | SQLite native bindings (2.1.x)|

The `Microsoft.Maui.Controls.Hosting` package referenced in the task
description is part of the `Microsoft.Maui.Controls` meta-package in MAUI
8.0.x; the `IAppHostBuilder` extensions (e.g. `UseMauiApp`,
`ConfigureSyncfusionCore`) live there.

## Building

```powershell
# Restore NuGet packages and build the Android target (illustrative).
cd Task\ Managment/Sample
dotnet restore Todo/Todo.csproj
dotnet build   Todo/Todo.csproj -f net8.0-android
```

> Requires the MAUI workload (`dotnet workload install maui`) and a
> connected device/emulator for the mobile targets.

## Running tests

Module 1 does not yet contain automated tests. They are introduced
alongside the features they cover (e.g. `ViewModelBase` unit tests in
Task 1-03, `AppDbContext` integration tests in Task 1-07).
