using Microsoft.Extensions.Logging;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the main task list page (sidebar + task items).  The full
/// implementation (binding to <see cref="Todo.Services.Interfaces.ITaskService"/>,
/// selection state, swipe gestures) is added in Module 4 (Task 4-xx).
/// </summary>
public sealed partial class TaskListPageViewModel : ViewModelBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="TaskListPageViewModel"/>
    /// class.
    /// </summary>
    /// <param name="logger">Logger injected by the DI container.</param>
    public TaskListPageViewModel(ILogger<TaskListPageViewModel> logger)
        : base(logger)
    {
        Title = "Tasks";
    }
}
