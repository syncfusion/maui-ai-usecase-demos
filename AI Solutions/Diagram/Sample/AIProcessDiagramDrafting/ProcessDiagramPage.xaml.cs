using System.ComponentModel;
using AIProcessDiagramDrafting.Models;
using AIProcessDiagramDrafting.ViewModels;
using Microsoft.Maui.Devices;
using Syncfusion.Maui.Diagram;

namespace AIProcessDiagramDrafting;

public partial class ProcessDiagramPage : ContentPage
{
    private readonly ProcessDiagramViewModel viewModel;
    private readonly SfDiagram diagramSurface;
    private int fitToPageRequestId;

    public ProcessDiagramPage(ProcessDiagramViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = this.viewModel;
        this.viewModel.PropertyChanged += OnViewModelPropertyChanged;

        diagramSurface = CreateDiagramSurface();
        DiagramHost.Children.Add(diagramSurface);

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            Content = new ScrollView
            {
                Orientation = ScrollOrientation.Vertical,
                Content = RootGrid
            };
        }

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        ApplyLayout(Width);
        UpdateAndroidDiagramHeight(viewModel.CurrentDiagram);
        ApplyCurrentDiagram();
        ScheduleFitToPage();
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        ApplyLayout(Width);
        UpdateAndroidDiagramHeight(viewModel.CurrentDiagram);
        ScheduleFitToPage();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProcessDiagramViewModel.CurrentDiagram))
        {
            ApplyCurrentDiagram();
            ScheduleFitToPage();
        }
    }

    private void ApplyCurrentDiagram()
    {
        fitToPageRequestId++;
        diagramSurface.Nodes.Clear();
        diagramSurface.Connectors.Clear();

        var draft = viewModel.CurrentDiagram;
        if (draft is null)
        {
            UpdateAndroidDiagramHeight(null);
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

        UpdateAndroidDiagramHeight(draft);
    }

    private SfDiagram CreateDiagramSurface()
    {
        return new SfDiagram();
    }

    private void ScheduleFitToPage()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android || viewModel.CurrentDiagram is null)
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

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            Grid.SetRow(HeaderSection, 0);
            Grid.SetColumn(HeaderSection, 0);
            Grid.SetColumnSpan(HeaderSection, 1);

            Grid.SetRow(DetailsSection, 1);
            Grid.SetColumn(DetailsSection, 0);

            Grid.SetRow(DiagramSection, 2);
            Grid.SetColumn(DiagramSection, 0);
            DiagramSection.VerticalOptions = LayoutOptions.Start;
        }
        else if (compact)
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
            DiagramSection.ClearValue(HeightRequestProperty);
            DiagramSection.ClearValue(MinimumHeightRequestProperty);
            DiagramSection.VerticalOptions = LayoutOptions.FillAndExpand;
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
            DiagramSection.ClearValue(HeightRequestProperty);
            DiagramSection.ClearValue(MinimumHeightRequestProperty);
            DiagramSection.VerticalOptions = LayoutOptions.FillAndExpand;
        }
    }

    private void UpdateAndroidDiagramHeight(DiagramDraft? draft)
    {
        if (DeviceInfo.Platform != DevicePlatform.Android)
        {
            return;
        }

        var contentHeight = draft is null ? 420 : 300 + (draft.Nodes.Count * 120);
        var availableHeight = Height > 0
            ? Math.Max(0, Height - HeaderSection.Height - DetailsSection.Height - (RootGrid.RowSpacing * 2))
            : 0;

        DiagramSection.ClearValue(HeightRequestProperty);
        DiagramSection.MinimumHeightRequest = Math.Max(contentHeight, availableHeight);
    }
}
