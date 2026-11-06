using System.ComponentModel;
using AIProcessDiagramDrafting.Models;
using AIProcessDiagramDrafting.ViewModels;

namespace AIProcessDiagramDrafting;

public partial class ProcessDiagramPage : ContentPage
{
    private readonly ProcessDiagramViewModel viewModel;

    public ProcessDiagramPage(ProcessDiagramViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        this.viewModel.PropertyChanged += OnViewModelPropertyChanged;

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
        DiagramSurface.Created += (_, __) => DiagramSurface.FitToPage();
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
        }
    }

    private void SyncProcessDescriptionFromEditor()
    {
        viewModel.ProcessDescription = ProcessDescriptionEditor.Text ?? string.Empty;
    }

    private void ApplyCurrentDiagram()
    {
        if (viewModel.CurrentDiagram is not DiagramDraft draft)
        {
            DiagramSurface.Nodes.Clear();
            DiagramSurface.Connectors.Clear();
            return;
        }

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

        Dispatcher.Dispatch(() => DiagramSurface.FitToPage());
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
