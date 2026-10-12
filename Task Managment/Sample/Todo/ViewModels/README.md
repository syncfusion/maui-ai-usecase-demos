# ViewModels

This folder contains the MVVM ViewModels for the Todo application.

Each ViewModel is paired with a View and is responsible for:

- Exposing observable state (`[ObservableProperty]` from
  `CommunityToolkit.Mvvm`).
- Exposing user-intent commands (`[RelayCommand]` from
  `CommunityToolkit.Mvvm`).
- Coordinating with the Services and Data layers injected through the
  .NET MAUI dependency-injection container.

A shared `ViewModelBase` is created in Task 1-03
(`Implement Base MVVM Classes`); all feature ViewModels will inherit from it.
