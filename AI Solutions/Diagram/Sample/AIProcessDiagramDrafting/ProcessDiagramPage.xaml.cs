using System.ComponentModel;
using AIProcessDiagramDrafting.Models;
using AIProcessDiagramDrafting.ViewModels;

namespace AIProcessDiagramDrafting;

public partial class ProcessDiagramPage : ContentPage
{
    private readonly ProcessDiagramViewModel viewModel;
    private bool isApplyingDraft;

    public ProcessDiagramPage(ProcessDiagramViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        this.viewModel.ConfirmReplaceDraftAsync = ConfirmReplaceDraftAsync;
        this.viewModel.PropertyChanged += OnViewModelPropertyChanged;

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;

        DiagramSurface.Created += (_, __) => DiagramSurface.FitToPage();
        DiagramSurface.PositionChanged += (_, __) => OnDiagramEdited();
        DiagramSurface.TextEdited += (_, __) => OnDiagramEdited();
        DiagramSurface.ConnectionChanged += (_, __) => OnDiagramEdited();

        ApplyLayout(Width);
    }

    private void OnProcessDescriptionTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (viewModel.ProcessDescription != e.NewTextValue)
        {
            viewModel.ProcessDescription = e.NewTextValue ?? string.Empty;
        }
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        ApplyLayout(Width);
        ApplyCurrentDiagram();
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        ApplyLayout(Width);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProcessDiagramViewModel.CurrentDiagram))
        {
            ApplyCurrentDiagram();
        }
    }

    private void OnDiagramEdited()
    {
        if (isApplyingDraft)
        {
            return;
        }

        viewModel.MarkDraftDirty();
    }

    private async Task<bool> ConfirmReplaceDraftAsync(string message)
    {
        return await DisplayAlert("Replace current draft?", message, "Replace", "Cancel");
    }

    private void ApplyCurrentDiagram()
    {
        if (viewModel.CurrentDiagram is not DiagramDraft draft)
        {
            DiagramSurface.Nodes.Clear();
            DiagramSurface.Connectors.Clear();
            return;
        }

        isApplyingDraft = true;
        try
        {
            DiagramSurface.Nodes.Clear();
            DiagramSurface.Connectors.Clear();

            foreach (var node in draft.Nodes)
            {
                DiagramSurface.Nodes.Add(node);
            }

            foreach (var connector in draft.Connectors)
            {
                DiagramSurface.Connectors.Add(connector);
            }
        }
        finally
        {
            isApplyingDraft = false;
        }

        Dispatcher.Dispatch(() => DiagramSurface.FitToPage());
    }

    private void ApplyLayout(double width)
    {
        var compact = width > 0 && width < 900;
        viewModel.IsCompactLayout = compact;

        RootGrid.RowDefinitions.Clear();
        RootGrid.ColumnDefinitions.Clear();

        if (compact)
        {
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            Grid.SetRow(HeaderSection, 0);
            Grid.SetColumn(HeaderSection, 0);
            Grid.SetColumnSpan(HeaderSection, 1);

            Grid.SetRow(InputSection, 1);
            Grid.SetColumn(InputSection, 0);
            Grid.SetRow(ReviewSection, 3);
            Grid.SetColumn(ReviewSection, 0);

            Grid.SetRow(DiagramSection, 2);
            Grid.SetColumn(DiagramSection, 0);
            Grid.SetColumnSpan(DiagramSection, 1);
        }
        else
        {
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3, GridUnitType.Star)));

            Grid.SetRow(HeaderSection, 0);
            Grid.SetColumn(HeaderSection, 0);
            Grid.SetColumnSpan(HeaderSection, 2);

            Grid.SetRow(InputSection, 1);
            Grid.SetColumn(InputSection, 0);

            Grid.SetRow(ReviewSection, 2);
            Grid.SetColumn(ReviewSection, 0);

            Grid.SetRow(DiagramSection, 1);
            Grid.SetColumn(DiagramSection, 1);
            Grid.SetRowSpan(DiagramSection, 2);
        }
    }

    private void OnUndoClicked(object? sender, EventArgs e)
        => DiagramSurface.Undo();

    private void OnRedoClicked(object? sender, EventArgs e)
        => DiagramSurface.Redo();

    private void OnFitToViewClicked(object? sender, EventArgs e)
        => DiagramSurface.FitToPage();
}
