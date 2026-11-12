using System.ComponentModel;
using AIProcessDiagramDrafting.Models;
using AIProcessDiagramDrafting.ViewModels;
using Syncfusion.Maui.Diagram;

namespace AIProcessDiagramDrafting;

public partial class ProcessDiagramPage : ContentPage
{
    private readonly ProcessDiagramViewModel viewModel;
    private SfDiagram diagramSurface;
    private int fitToPageRequestId;

    public ProcessDiagramPage(ProcessDiagramViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        this.viewModel.PropertyChanged += OnViewModelPropertyChanged;

        diagramSurface = CreateDiagramSurface();
        DiagramHost.Children.Add(diagramSurface);

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        ApplyLayout(Width);
        ApplyCurrentDiagram();
        ScheduleFitToPage();
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        ApplyLayout(Width);
        ScheduleFitToPage();
    }

    private void OnProcessDescriptionTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (viewModel.ProcessDescription != e.NewTextValue)
        {
            viewModel.ProcessDescription = e.NewTextValue ?? string.Empty;
        }
    }

    private async void OnGenerateDraftClicked(object? sender, EventArgs e)
    {
        SyncProcessDescriptionFromEditor();
        await viewModel.GenerateDraftAsync();
    }

    private async void OnRegenerateClicked(object? sender, EventArgs e)
    {
        SyncProcessDescriptionFromEditor();
        await viewModel.RegenerateAsync();
    }

    private void OnAcceptDraftClicked(object? sender, EventArgs e)
    {
        viewModel.AcceptDraft();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProcessDiagramViewModel.CurrentDiagram))
        {
            ApplyCurrentDiagram();
            ScheduleFitToPage();
        }
    }

    private void SyncProcessDescriptionFromEditor()
    {
        viewModel.ProcessDescription = ProcessDescriptionEditor.Text ?? string.Empty;
    }

    private void ApplyCurrentDiagram()
    {
        var draft = viewModel.CurrentDiagram;
        ResetDiagramSurface();

        if (draft is null)
        {
            return;
        }

        foreach (var node in draft.Nodes)
        {
            diagramSurface.Nodes.Add(node);
        }

        foreach (var connector in draft.Connectors)
        {
            diagramSurface.Connectors.Add(connector);
        }
    }

    private SfDiagram CreateDiagramSurface()
    {
        var surface = new SfDiagram();
        surface.Created += (_, __) => ScheduleFitToPage();
        return surface;
    }

    private void ResetDiagramSurface()
    {
        fitToPageRequestId++;
        DiagramHost.Children.Clear();
        diagramSurface = CreateDiagramSurface();
        DiagramHost.Children.Add(diagramSurface);
    }

    private void ScheduleFitToPage()
    {
        if (viewModel.CurrentDiagram is null)
        {
            return;
        }

        var requestId = ++fitToPageRequestId;
        _ = FitToPageAsync(requestId);
    }

    private async Task FitToPageAsync(int requestId)
    {
        await Task.Delay(150);
        if (requestId != fitToPageRequestId || viewModel.CurrentDiagram is null)
        {
            return;
        }

        diagramSurface.FitToPage();
    }

    private void ApplyLayout(double width)
    {
        var compact = width > 0 && width < 900;

        RootGrid.RowDefinitions.Clear();
        RootGrid.ColumnDefinitions.Clear();

        if (compact)
        {
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            Grid.SetRow(HeaderSection, 0);
            Grid.SetColumn(HeaderSection, 0);
            Grid.SetColumnSpan(HeaderSection, 1);

            Grid.SetRow(DetailsSection, 1);
            Grid.SetColumn(DetailsSection, 0);

            Grid.SetRow(DiagramSection, 2);
            Grid.SetColumn(DiagramSection, 0);
        }
        else
        {
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3, GridUnitType.Star)));

            Grid.SetRow(HeaderSection, 0);
            Grid.SetColumn(HeaderSection, 0);
            Grid.SetColumnSpan(HeaderSection, 2);

            Grid.SetRow(DetailsSection, 1);
            Grid.SetColumn(DetailsSection, 0);

            Grid.SetRow(DiagramSection, 1);
            Grid.SetColumn(DiagramSection, 1);
        }
    }
}
