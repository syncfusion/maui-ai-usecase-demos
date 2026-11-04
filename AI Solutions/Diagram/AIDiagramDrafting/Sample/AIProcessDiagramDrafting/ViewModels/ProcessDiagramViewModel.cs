using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AIProcessDiagramDrafting.Models;
using AIProcessDiagramDrafting.Services;

namespace AIProcessDiagramDrafting.ViewModels;

public sealed class ProcessDiagramViewModel : INotifyPropertyChanged
{
    private readonly IWorkflowExtractionService workflowExtractionService;
    private readonly WorkflowResponseValidator validator;
    private readonly DiagramDraftBuilder draftBuilder;

    private string processDescription = string.Empty;
    private WorkflowResponse? currentWorkflow;
    private object? currentDiagram;
    private string statusMessage = string.Empty;

    public ProcessDiagramViewModel(
        IWorkflowExtractionService workflowExtractionService,
        WorkflowResponseValidator validator,
        DiagramDraftBuilder draftBuilder)
    {
        this.workflowExtractionService = workflowExtractionService;
        this.validator = validator;
        this.draftBuilder = draftBuilder;

        GenerateDraftCommand = new Command(async () => await GenerateDraftAsync(), () => !IsBusy);
        AcceptDraftCommand = new Command(() => StatusMessage = "Draft accepted.");
        RegenerateCommand = new Command(async () => await GenerateDraftAsync(), () => !IsBusy);
        DiscardCommand = new Command(DiscardDraft);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand GenerateDraftCommand { get; }
    public ICommand AcceptDraftCommand { get; }
    public ICommand RegenerateCommand { get; }
    public ICommand DiscardCommand { get; }

    public string ProcessDescription
    {
        get => processDescription;
        set { if (processDescription != value) { processDescription = value; OnPropertyChanged(); } }
    }

    public WorkflowResponse? CurrentWorkflow
    {
        get => currentWorkflow;
        private set { if (currentWorkflow != value) { currentWorkflow = value; OnPropertyChanged(); } }
    }

    public object? CurrentDiagram
    {
        get => currentDiagram;
        private set { if (currentDiagram != value) { currentDiagram = value; OnPropertyChanged(); } }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set { if (statusMessage != value) { statusMessage = value; OnPropertyChanged(); } }
    }

    public bool IsBusy { get; private set; }

    private async Task GenerateDraftAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(ProcessDescription))
        {
            return;
        }

        IsBusy = true;
        RaiseCommandCanExecuteChanged();

        try
        {
            var workflow = await workflowExtractionService.ExtractAsync(ProcessDescription);
            if (!validator.Validate(workflow, out var errorMessage))
            {
                StatusMessage = errorMessage;
                return;
            }

            CurrentWorkflow = workflow;
            CurrentDiagram = draftBuilder.Build(workflow);
            StatusMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
            RaiseCommandCanExecuteChanged();
        }
    }

    private void DiscardDraft()
    {
        CurrentWorkflow = null;
        CurrentDiagram = null;
        StatusMessage = string.Empty;
    }

    private void RaiseCommandCanExecuteChanged()
    {
        if (GenerateDraftCommand is Command generateDraftCommand)
        {
            generateDraftCommand.ChangeCanExecute();
        }

        if (RegenerateCommand is Command regenerateCommand)
        {
            regenerateCommand.ChangeCanExecute();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
