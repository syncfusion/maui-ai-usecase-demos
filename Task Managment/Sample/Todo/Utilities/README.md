# Utilities

Cross-cutting helper code shared across the application:

- `Constants.cs` — App-wide compile-time constants (DB filename, timeouts,
  route names, feature flags, validation limits, …).  Added in
  **Task 1-03**.
- `Enums.cs` — App-wide enums (`ListCategory`, `RecurrenceType`,
  `TaskStatus`, `NotificationSeverity`, `NavigationTarget`).  Added in
  **Task 1-03**.
- `Helpers.cs` — Stateless helper methods for date/time formatting,
  relative-date descriptions, string / e-mail / password validation,
  hex-colour parsing, and chunking in-memory collections.  Added in
  **Task 1-03**.
- `Result.cs` — Lightweight discriminated result type returned by the
  `ViewModelBase.ExecuteAsync` wrappers so that ViewModels can report
  success / failure without leaking raw exceptions to the View layer.
