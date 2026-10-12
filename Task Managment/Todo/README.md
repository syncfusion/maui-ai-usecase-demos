# Todo - .NET MAUI Application

A cross-platform Todo application built with .NET MAUI and Syncfusion controls, following the MVVM architectural pattern.

## Project Status

This project is in **Phase 1: Foundation & Setup**. The folder structure for the MVVM architecture has been scaffolded. Code files, resources, and platform-specific implementations will be added in subsequent tasks.

## Folder Structure

The project follows the MVVM (Model-View-ViewModel) pattern with additional organization for services, data, and resources.

```
Todo/
├── Views/                          # XAML pages and UI components
│   ├── Authentication/             # Sign-in, sign-up, forgot password, splash
│   ├── Main/                       # Task list, task detail, profile
��   ├── Shared/                     # Reusable views and templates
│   └── Dialogs/                    # Modal dialogs and popups
├── ViewModels/                     # MVVM view models (presentation logic)
├── Models/                         # Domain models and data transfer objects
│   └── DTOs/                       # API request/response DTOs
├── Services/                       # Application services
│   ├── Interfaces/                 # Service contract definitions
│   ├── Authentication/             # Auth-related implementations
│   ├── Task/                       # Task CRUD, list management
│   ├── Search/                     # Search functionality
│   ├── Storage/                    # Local persistence
│   └── Infrastructure/             # Sync, notifications, dialogs
├── Data/                           # Data access layer
│   └── Repositories/               # Repository pattern implementations
├── Resources/                      # Application resources
│   ├── Styles/                     # XAML resource dictionaries
│   ├── Images/                     # Image assets
│   │   ├── backgrounds/            # Background images
│   │   └── icons/                  # Icon assets
│   └── Strings/                    # Localization (resx)
├── Converters/                     # XAML value converters
├── Behaviors/                      # Custom MAUI behaviors
├── Utilities/                      # Constants, enums, helpers
└── Platforms/                      # Platform-specific code
    ├── Android/
    ├── iOS/
    ├── MacCatalyst/
    └── Windows/
```

## Architecture

- **Framework:** .NET MAUI 8.0+
- **Pattern:** MVVM with CommunityToolkit.Mvvm
- **UI Library:** Syncfusion MAUI controls
- **Data:** SQLite (offline-first) with optional server sync
- **DI:** Microsoft.Extensions.DependencyInjection

## Tasks Completed

- [x] **Task 1-01:** Project folder structure created

## Next Tasks

- Task 1-02: Install and verify NuGet packages
- Task 1-03: Implement base MVVM classes
- Task 1-04: Create resource dictionaries
- Task 1-05: Setup MauiProgram with DI
- Task 1-06: Configure navigation infrastructure
- Task 1-07: Setup database
- Task 1-08: Create utility classes

## References

- [Specification](../SPECIFICATION.md) - Section 10: Architecture & MVVM
- [Harness](../HARNESS.md) - Module 1: Foundation & Setup
- [Quick Reference](../QUICK_REFERENCE.md) - Project Structure section
