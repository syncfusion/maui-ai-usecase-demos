using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AIProcessDiagramDrafting.Models;
using AIProcessDiagramDrafting.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;

namespace AIProcessDiagramDrafting.ViewModels;

public sealed class ProcessDiagramViewModel : INotifyPropertyChanged
{
    private static readonly Color InfoColor = Color.FromArgb("#4F46E5");
    private static readonly Color SuccessColor = Color.FromArgb("#166534");
    private static readonly Color WarningColor = Color.FromArgb("#B45309");
    private static readonly Color ErrorColor = Color.FromArgb("#B91C1C");

    private readonly IWorkflowExtractionService workflowExtractionService;
    private readonly WorkflowResponseValidator validator;
    private readonly DiagramDraftBuilder draftBuilder;
    private readonly ObservableCollection<WorkflowWarning> warnings = new();

    private CancellationTokenSource? generationCts;
    private int generationVersion;

    private string processDescription = "An employee submits a purchase request. The manager reviews it. If the amount is below ₹50,000, the manager approves it. Otherwise, Finance reviews it. Approved requests create a purchase order. Rejected requests notify the employee.";
    private DiagramDraft? currentDiagram;
    private string draftTitle = string.Empty;
    private string statusMessage = string.Empty;
    private Color statusMessageColor = Colors.Transparent;
    private bool isBusy;
    private bool discardRequested;

    public ProcessDiagramViewModel(
        IWorkflowExtractionService workflowExtractionService,
        WorkflowResponseValidator validator,
        DiagramDraftBuilder draftBuilder)
    {
        this.workflowExtractionService = workflowExtractionService;
        this.validator = validator;
        this.draftBuilder = draftBuilder;

        warnings.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(HasWarnings));
            OnPropertyChanged(nameof(HasDraftSummary));
        };

        GenerateDraftCommand = new AsyncRelayCommand(GenerateDraftAsync, () => !IsBusy);
        DiscardDraftCommand = new RelayCommand(DiscardDraft, () => CurrentDiagram is not null || IsBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IAsyncRelayCommand GenerateDraftCommand { get; }

    public IRelayCommand DiscardDraftCommand { get; }

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
            NotifyCommandStates();
        }
    }

    public string DraftTitle
    {
        get => draftTitle;
        private set
        {
            if (draftTitle == value)
            {
                return;
            }

            draftTitle = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasDraftTitle));
            OnPropertyChanged(nameof(HasDraftSummary));
        }
    }

    public ObservableCollection<WorkflowWarning> Warnings => warnings;

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

    public Color StatusMessageColor
    {
        get => statusMessageColor;
        private set
        {
            if (statusMessageColor == value)
            {
                return;
            }

            statusMessageColor = value;
            OnPropertyChanged();
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public bool HasDraftTitle => !string.IsNullOrWhiteSpace(DraftTitle);

    public bool HasWarnings => warnings.Count > 0;

    public bool HasDraftSummary => HasDraftTitle || HasWarnings;

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
            NotifyCommandStates();
        }
    }

    public async Task GenerateDraftAsync()
    {
        var description = ProcessDescription.Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            SetStatusMessage("Enter a process description before generating a draft.", ErrorColor);
            return;
        }

        if (description.Length > WorkflowLimits.ProcessDescription)
        {
            SetStatusMessage($"The process description must be {WorkflowLimits.ProcessDescription} characters or fewer.", ErrorColor);
            return;
        }

        var requestVersion = Interlocked.Increment(ref generationVersion);
        StartGeneration();

        try
        {
            var cts = CreateGenerationTokenSource();
            var workflow = await workflowExtractionService.ExtractAsync(description, cts.Token);
            cts.Token.ThrowIfCancellationRequested();

            if (!validator.Validate(workflow, out var errorMessage))
            {
                SetStatusMessage(errorMessage, ErrorColor);
                return;
            }

            DraftTitle = workflow.Title.Trim();
            Warnings.Clear();
            foreach (var warning in workflow.Warnings.Where(warning => !string.IsNullOrWhiteSpace(warning.Message)))
            {
                Warnings.Add(warning);
            }

            CurrentDiagram = draftBuilder.Build(workflow);
            SetStatusMessage(Warnings.Count == 0 ? "Draft accepted." : "Draft accepted with warnings.", Warnings.Count == 0 ? SuccessColor : WarningColor);
        }
        catch (OperationCanceledException)
        {
            if (requestVersion == generationVersion && !discardRequested)
            {
                SetStatusMessage("Generation canceled.", WarningColor);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Azure OpenAI credentials are not configured.", StringComparison.OrdinalIgnoreCase))
        {
            if (requestVersion == generationVersion)
            {
                SetStatusMessage("Azure OpenAI credentials are required to generate a draft.", ErrorColor);
            }
        }
        catch
        {
            if (requestVersion == generationVersion)
            {
                SetStatusMessage("Unable to generate the draft right now. Please try again.", ErrorColor);
            }
        }
        finally
        {
            if (requestVersion == generationVersion)
            {
                IsBusy = false;
                DisposeGenerationTokenSource();
                discardRequested = false;
            }
        }
    }

    public void DiscardDraft()
    {
        if (CurrentDiagram is null && !IsBusy)
        {
            return;
        }

        var wasBusy = IsBusy;
        discardRequested = true;
        CancelGeneration();
        ClearDraftResult();
        SetStatusMessage(wasBusy ? "Generation canceled." : "Draft discarded.", WarningColor);
    }

    private void StartGeneration()
    {
        CancelGeneration();
        discardRequested = false;
        IsBusy = true;
        ClearDraftResult();
        SetStatusMessage("Generating draft...", InfoColor);
    }

    private void ClearDraftResult()
    {
        DraftTitle = string.Empty;
        Warnings.Clear();
        CurrentDiagram = null;
    }

    private CancellationTokenSource CreateGenerationTokenSource()
    {
        var cts = new CancellationTokenSource();
        generationCts = cts;
        return cts;
    }

    private void CancelGeneration()
    {
        if (generationCts is null)
        {
            return;
        }

        generationCts.Cancel();
        generationCts.Dispose();
        generationCts = null;
    }

    private void DisposeGenerationTokenSource()
    {
        generationCts?.Dispose();
        generationCts = null;
    }

    private void SetStatusMessage(string message, Color color)
    {
        StatusMessageColor = color;
        StatusMessage = message;
    }

    private void NotifyCommandStates()
    {
        GenerateDraftCommand.NotifyCanExecuteChanged();
        DiscardDraftCommand.NotifyCanExecuteChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
