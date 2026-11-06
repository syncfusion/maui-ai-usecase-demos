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

        DiagramSurface.Created += (_, __) => DiagramSurface.FitToPage();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        ApplyCurrentDiagram();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProcessDiagramViewModel.CurrentDiagram))
        {
            ApplyCurrentDiagram();
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
}
