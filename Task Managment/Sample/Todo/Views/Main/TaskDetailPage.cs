using Todo.ViewModels;

namespace Todo.Views.Main;

/// <summary>
/// Placeholder task detail / editor page.  The full XAML layout (title,
/// description, due date picker, recurrence controls) is added in Module 4
/// (Task 4-xx).
/// </summary>
public sealed class TaskDetailPage : ContentPage
{
    /// <summary>
    /// Initialises a new instance of the <see cref="TaskDetailPage"/> class.
    /// </summary>
    /// <param name="viewModel">ViewModel injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> is <see langword="null"/>.</exception>
    public TaskDetailPage(TaskDetailPageViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        Title = viewModel.Title;
        Content = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "Task details",
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center,
                },
            },
        };
    }
}
