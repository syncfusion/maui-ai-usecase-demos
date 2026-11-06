using System.ComponentModel;
using System.Runtime.CompilerServices;
using AIProcessDiagramDrafting.Models;
using AIProcessDiagramDrafting.Services;

namespace AIProcessDiagramDrafting.ViewModels;

public sealed class ProcessDiagramViewModel : INotifyPropertyChanged
{
    private readonly IWorkflowExtractionService workflowExtractionService;
    private readonly WorkflowResponseValidator validator;
    private readonly DiagramDraftBuilder draftBuilder;

    private string processDescription = "An employee submits a purchase request. The manager reviews it. If the amount is below ₹50,000, the manager approves it. Otherwise, Finance reviews it. Approved requests create a purchase order. Rejected requests notify the employee.";
    private DiagramDraft? currentDiagram;
    private string statusMessage = string.Empty;
    private bool isBusy;

    public ProcessDiagramViewModel(
        IWorkflowExtractionService workflowExtractionService,
        WorkflowResponseValidator validator,
        DiagramDraftBuilder draftBuilder)
    {
        this.workflowExtractionService = workflowExtractionService;
        this.validator = validator;
        this.draftBuilder = draftBuilder;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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
        }
    }

    public async Task GenerateDraftAsync()
    {
        var description = ProcessDescription.Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            StatusMessage = "Enter a process description before generating a draft.";
            return;
        }

        if (description.Length > WorkflowLimits.ProcessDescription)
        {
            StatusMessage = $"The process description must be {WorkflowLimits.ProcessDescription} characters or fewer.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Generating draft...";

        try
        {
            var workflow = await workflowExtractionService.ExtractAsync(description);
            if (!validator.Validate(workflow, out var errorMessage))
            {
                StatusMessage = errorMessage;
                return;
            }

            CurrentDiagram = draftBuilder.Build(workflow);
            StatusMessage = "Draft generated.";
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

    public async Task RegenerateAsync()
        => await GenerateDraftAsync();

    public void AcceptDraft()
    {
        StatusMessage = "Draft accepted.";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
