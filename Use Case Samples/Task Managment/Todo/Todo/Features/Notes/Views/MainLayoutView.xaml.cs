using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

/// <summary>
/// Hosts the application shell: header + sidebar + main content area.
/// The sidebar column collapses to zero width when
/// <see cref="NotesListViewModel.IsSidebarCollapsed"/> is true, so the
/// main content area automatically occupies the full available width.
/// </summary>
public partial class MainLayoutView : ContentView
{
    private readonly NotesListViewModel _viewModel;

    public MainLayoutView(NotesListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Apply initial sidebar state, then keep the grid in sync.
        UpdateSidebarColumn(viewModel.IsSidebarCollapsed);
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(NotesListViewModel.IsSidebarCollapsed))
                UpdateSidebarColumn(viewModel.IsSidebarCollapsed);
        };
    }

    /// <summary>
    /// Swaps the page-specific content view (e.g. a notes list rendered
    /// for a given filter) in the main content area.
    /// </summary>
    public void SetContent(View content) => MainContent.Content = content;

    private void UpdateSidebarColumn(bool isCollapsed)
    {
        // Collapse the sidebar column completely so the main content
        // area occupies the full available width.
        RootGrid.ColumnDefinitions[0].Width = isCollapsed ? new GridLength(0) : new GridLength(220);
        Sidebar.IsVisible = !isCollapsed;
    }
}
