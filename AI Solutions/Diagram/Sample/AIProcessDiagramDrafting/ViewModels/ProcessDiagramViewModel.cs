using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

    private readonly ObservableCollection<WorkflowExample> examples = new()
    {
        new WorkflowExample(
            "Purchase request approval",
            "An employee submits a purchase request. The manager reviews it. If the amount is below ₹50,000, the manager approves it. Otherwise, Finance reviews it. Approved requests create a purchase order. Rejected requests notify the employee."),
        new WorkflowExample(
            "Request approval flow",
            "A request is submitted. The manager reviews it. If the request is valid, the manager approves it. Otherwise, the manager rejects it. Approved requests create a purchase order. Rejected requests notify the employee."),
        new WorkflowExample(
            "Support ticket triage",
            "A customer submits a support ticket. The support team reviews it. If the issue is urgent, the team escalates it. Otherwise, the team resolves the issue. Escalated tickets are sent to engineering. Resolved tickets are closed.")
    };

    private readonly ObservableCollection<string> warnings = new();

    private string processDescription = string.Empty;
    private WorkflowExample? selectedExample;
    private WorkflowResponse? currentWorkflow;
    private DiagramDraft? currentDiagram;
    private string statusMessage = string.Empty;
    private bool isBusy;
    private bool isDraftDirty;
    private bool isCompactLayout;

    public ProcessDiagramViewModel(
        IWorkflowExtractionService workflowExtractionService,
        WorkflowResponseValidator validator,
        DiagramDraftBuilder draftBuilder)
    {
        this.workflowExtractionService = workflowExtractionService;
        this.validator = validator;
        this.draftBuilder = draftBuilder;

        warnings.CollectionChanged += OnWarningsChanged;

        selectedExample = examples[0];
        processDescription = selectedExample.Description;

        GenerateDraftCommand = new Command(async () => await GenerateDraftAsync());
        AcceptDraftCommand = new Command(AcceptDraft);
        RegenerateCommand = new Command(async () => await RegenerateAsync());
        DiscardCommand = new Command(DiscardDraft);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Func<string, Task<bool>>? ConfirmReplaceDraftAsync { get; set; }

    public ICommand GenerateDraftCommand { get; }

    public ICommand AcceptDraftCommand { get; }

    public ICommand RegenerateCommand { get; }

    public ICommand DiscardCommand { get; }

    public ObservableCollection<WorkflowExample> Examples => examples;

    public ObservableCollection<string> Warnings => warnings;

    public string ProcessDescription
    {
        get => processDescription;
        set
        {
            if (processDescription == value)
            {
                return;
            }

            processDescription = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanGenerate));
            OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public WorkflowExample? SelectedExample
    {
        get => selectedExample;
        set
        {
            if (selectedExample == value)
            {
                return;
            }

            selectedExample = value;
            OnPropertyChanged();

            if (value is not null)
            {
                ProcessDescription = value.Description;
            }
        }
    }

    public WorkflowResponse? CurrentWorkflow
    {
        get => currentWorkflow;
        private set
        {
            if (currentWorkflow == value)
            {
                return;
            }

            currentWorkflow = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasDraft));
            OnPropertyChanged(nameof(CanAccept));
            OnPropertyChanged(nameof(CanRegenerate));
            OnPropertyChanged(nameof(CanDiscard));
            OnPropertyChanged(nameof(IsInputVisible));
        }
    }

    public DiagramDraft? CurrentDiagram
    {
        get => currentDiagram;
        private set
        {
            if (currentDiagram == value)
            {
                return;
            }

            currentDiagram = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set
        {
            if (statusMessage == value)
            {
                return;
            }

            statusMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (isBusy == value)
            {
                return;
            }

            isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanGenerate));
            OnPropertyChanged(nameof(CanAccept));
            OnPropertyChanged(nameof(CanRegenerate));
            OnPropertyChanged(nameof(CanDiscard));
        }
    }

    public bool IsDraftDirty
    {
        get => isDraftDirty;
        private set
        {
            if (isDraftDirty == value)
            {
                return;
            }

            isDraftDirty = value;
            OnPropertyChanged();
        }
    }

    public bool IsCompactLayout
    {
        get => isCompactLayout;
        set
        {
            if (isCompactLayout == value)
            {
                return;
            }

            isCompactLayout = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsInputVisible));
        }
    }

    public bool IsInputVisible => !IsCompactLayout || CurrentWorkflow is null;

    public bool HasDraft => CurrentWorkflow is not null;

    public bool HasWarnings => Warnings.Count > 0;

    public bool CanGenerate => !IsBusy && !string.IsNullOrWhiteSpace(ProcessDescription) && ProcessDescription.Length <= WorkflowLimits.ProcessDescription;

    public bool CanAccept => !IsBusy && CurrentWorkflow is not null;

    public bool CanRegenerate => !IsBusy && CurrentWorkflow is not null;

    public bool CanDiscard => !IsBusy && CurrentWorkflow is not null;

    public void MarkDraftDirty()
    {
        if (CurrentWorkflow is null)
        {
            return;
        }

        IsDraftDirty = true;
    }

    private async Task GenerateDraftAsync()
    {
        if (!CanGenerate)
        {
            if (string.IsNullOrWhiteSpace(ProcessDescription))
            {
                StatusMessage = "Enter a process description before generating a draft.";
            }
            else if (ProcessDescription.Length > WorkflowLimits.ProcessDescription)
            {
                StatusMessage = $"The process description must be {WorkflowLimits.ProcessDescription} characters or fewer.";
            }

            return;
        }

        IsBusy = true;
        StatusMessage = "Generating draft...";

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

            Warnings.Clear();
            foreach (var warning in workflow.Warnings)
            {
                if (!string.IsNullOrWhiteSpace(warning.Message))
                {
                    Warnings.Add(warning.Message);
                }
            }

            IsDraftDirty = false;
            StatusMessage = HasWarnings ? "Draft generated with warnings." : string.Empty;
        }
        catch
        {
            StatusMessage = "Unable to generate the draft right now. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegenerateAsync()
    {
        if (IsBusy || CurrentWorkflow is null)
        {
            return;
        }

        if (IsDraftDirty && ConfirmReplaceDraftAsync is not null)
        {
            var confirmed = await ConfirmReplaceDraftAsync("You have unsaved diagram edits. Regenerate and replace the current draft?");
            if (!confirmed)
            {
                return;
            }
        }

        await GenerateDraftAsync();
    }

    private void AcceptDraft()
    {
        if (CurrentWorkflow is null)
        {
            return;
        }

        IsDraftDirty = false;
        StatusMessage = "Draft accepted.";
    }

    private void DiscardDraft()
    {
        CurrentWorkflow = null;
        CurrentDiagram = null;
        Warnings.Clear();
        IsDraftDirty = false;
        StatusMessage = string.Empty;
    }

    private void OnWarningsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasWarnings));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
