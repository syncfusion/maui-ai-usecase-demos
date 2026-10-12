using Microsoft.Extensions.Logging;
using Todo.ViewModels;

namespace Todo.Views.Main;

/// <summary>
/// Code-only partial of <see cref="TaskListPage"/> that adds the
/// dependency-injected constructor required by the .NET MAUI Shell.
/// The XAML UI is inflated by <c>InitializeComponent()</c> in
/// <c>TaskListPage.xaml.cs</c> and bound to the injected
/// <see cref="TaskListPageViewModel"/> here.
///
/// <para>
/// Splitting the XAML-loaded UI (<c>TaskListPage.xaml.cs</c>) and the
/// DI-injected constructor (<c>TaskListPage.cs</c>) mirrors the
/// convention already used by <see cref="SignInPage"/>,
/// <see cref="SignUpPage"/> and <see cref="SplashPage"/> so the navigation
/// surface stays uniform across the application.
/// </para>
/// </summary>
public sealed partial class TaskListPage : ContentPage
{
    private readonly TaskListPageViewModel _viewModel;

    /// <summary>
    /// Initialises a new instance of the <see cref="TaskListPage"/>
    /// class.
    /// </summary>
    /// <param name="viewModel">
    /// ViewModel injected by the DI container.  Resolved through the
    /// .NET MAUI service provider registered in
    /// <c>MauiProgram.AddPages</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="viewModel"/> is <see langword="null"/>.
    /// </exception>
    public TaskListPage(TaskListPageViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        Title = viewModel.Title;

        // Apply the initial breakpoint once the page is constructed.  The
        // page-level handler (see SizeChanged in TaskListPage.xaml.cs)
        // takes over from there on every subsequent resize.
        if (Width > 0)
        {
            _viewModel.ApplyWidth(Width);
        }
    }

    /// <summary>
    /// Triggered whenever the page's rendered size changes — covers
    /// orientation changes on phones, window resizes on desktop and
    /// the very first layout pass on every target.  Forwards the new
    /// width to the ViewModel so the responsive flags are always in sync.
    /// </summary>
    /// <param name="sender">Sender of the event (unused).</param>
    /// <param name="e">Size-changed event payload.</param>
    protected void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width > 0)
        {
            _viewModel.ApplyWidth(Width);
        }
    }
}