using Todo.ViewModels;

namespace Todo.Views.Main;

/// <summary>
/// XAML-loaded partial of <see cref="TaskListPage"/>.  The companion
/// <c>TaskListPage.cs</c> file holds the dependency-injected constructor
/// used by the .NET MAUI Shell — the two halves together form the same
/// class so the XAML UI is inflated when the Shell constructs the page
/// via DI.
/// </summary>
public partial class TaskListPage : ContentPage
{
    /// <summary>
    /// Initialises a new instance of the <see cref="TaskListPage"/> class
    /// and inflates the XAML UI.  This constructor is invoked implicitly
    /// by <c>TaskListPage.cs</c>; the Shell never resolves it directly.
    /// </summary>
    public TaskListPage()
    {
        InitializeComponent();
    }
}