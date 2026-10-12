using Microsoft.Extensions.Logging;

namespace Todo.ViewModels;

/// <summary>
/// ViewModel for the task detail / editor page.  The full implementation
/// (binding to <see cref="Todo.Services.Interfaces.ITaskService"/>, due dates,
/// reminders, recurrence) is added in Module 4 (Task 4-xx).
/// </summary>
public sealed partial class TaskDetailPageViewModel : ViewModelBase
{
    /// <summary>
    /// Initialises a new instance of the <see cref="TaskDetailPageViewModel"/>
    /// class.
    /// </summary>
    /// <param name="logger">Logger injected by the DI container.</param>
    public TaskDetailPageViewModel(ILogger<TaskDetailPageViewModel> logger)
        : base(logger)
    {
        Title = "Task details";
    }
}
