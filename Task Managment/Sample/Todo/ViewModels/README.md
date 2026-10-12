# ViewModels

This folder contains the MVVM ViewModels for the Todo application.

Each ViewModel is paired with a View and is responsible for:

- Exposing observable state (`[ObservableProperty]` from
  `CommunityToolkit.Mvvm`).
- Exposing user-intent commands (`[RelayCommand]` from
  `CommunityToolkit.Mvvm`).
- Coordinating with the Services and Data layers injected through the
  .NET MAUI dependency-injection container.

## `ViewModelBase.cs`

Added in **Task 1-03 — Implement Base MVVM Classes and Framework**.

`ViewModelBase` is the abstract base class every feature ViewModel must
inherit from.  It provides:

- `INotifyPropertyChanged` through `CommunityToolkit.Mvvm.ComponentModel.ObservableObject`.
- An injected `ILogger<ViewModelBase>` for structured diagnostics.
- Standard observable properties:
  - `IsLoading` — bind to a busy indicator.
  - `HasError` / `ErrorMessage` — bind to an error banner.
  - `Title` — bind to a page or section header.
- A generic, exception-safe `ExecuteAsync<T>(...)` wrapper that turns a
  throwing `Func<Task<T?>>` into a `Result<T>` while managing the
  `IsLoading` / `ErrorMessage` state automatically.
- A non-generic `ExecuteAsync(Func<Task>, ...)` overload for commands
  that do not produce a value.
- A `RaiseAllPropertiesChanged()` helper used by external "refresh"
  triggers (for example a `MessagingCenter` event).

### Example

```csharp
public partial class TaskListViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<TaskModel> tasks = new();

    public TaskListViewModel(ILogger<TaskListViewModel> logger)
        : base(logger)
    {
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Result<IReadOnlyList<TaskModel>> result = await ExecuteAsync(
            () => taskService.GetTasksAsync(),
            nameof(RefreshAsync));

        if (result.IsSuccess && result.Value is { } list)
        {
            Tasks = new ObservableCollection<TaskModel>(list);
        }
    }
}
```
