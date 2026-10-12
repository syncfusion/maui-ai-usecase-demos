using Todo.ViewModels;

namespace Todo.Views.Main;

/// <summary>
/// Placeholder main task list page.  The full XAML layout (Syncfusion
/// <c>SfListView</c>, sidebar, bottom input bar) is added in Module 4
/// (Task 4-xx).
/// </summary>
public sealed class TaskListPage : ContentPage
{
    /// <summary>
    /// Initialises a new instance of the <see cref="TaskListPage"/> class.
    /// </summary>
    /// <param name="viewModel">ViewModel injected by the DI container.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> is <see langword="null"/>.</exception>
    public TaskListPage(TaskListPageViewModel viewModel)
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
                    Text = "Tasks",
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center,
                },
            },
        };
    }
}
