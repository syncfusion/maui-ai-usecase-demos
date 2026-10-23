using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Syncfusion.Maui.AIAssistView;

namespace AIChatSample;

/// <summary>
/// Tiny Copilot view-model for the chat that sits at the bottom of the
/// Configure Agent page. It mirrors Microsoft Copilot's flow:
///   1. The user types a free-form description of the agent.
///   2. We send it to <see cref="IAIService.DraftAgentAsync"/>.
///   3. The resulting <see cref="AgentDraft"/> is written directly
///      into the bound form (Agent Name / Description / Behaviour) —
///      no navigation, no separate draft picker.
/// </summary>
public partial class AgentConfigAssistantViewModel : ObservableObject
{
    private readonly AgentConfigurationViewModel _target;
    private readonly IAIService? _aiService;

    public AgentConfigAssistantViewModel(
        AgentConfigurationViewModel target,
        IAIService? aiService = null)
    {
        _target = target;
        _aiService = aiService ?? ServiceHelper.GetService<IAIService>();
        Messages = new ObservableCollection<IAssistItem>();
        RequestCommand = new Command<object>(OnUserPrompt);
    }

    private bool _showResponseLoader;
    /// <summary>Drives SfAIAssistView.ShowResponseLoader while a draft is being generated.</summary>
    public bool ShowResponseLoader
    {
        get => _showResponseLoader;
        set
        {
            if (_showResponseLoader == value) return;
            _showResponseLoader = value;
            OnPropertyChanged(nameof(ShowResponseLoader));
        }
    }

    /// <summary>Chat history surfaced in the SfAIAssistView.</summary>
    public ObservableCollection<IAssistItem> Messages { get; }

    /// <summary>Live prompt text (used to show/hide the busy state).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    private bool isGenerating;

    /// <summary>Bound to <c>SfAIAssistView.RequestCommand</c>; called when the user sends a message.</summary>
    public ICommand RequestCommand { get; }

    public bool IsBusy => IsGenerating;

    private async void OnUserPrompt(object? parameter)
    {
        if (parameter is not RequestEventArgs args || args.RequestItem is not IAssistItem userItem)
        {
            return;
        }

        // Prevent concurrent requests from firing multiple times and
        // ensure the assist view doesn't perform its default handling.
        if (IsGenerating)
        {
            args.Handled = true;
            return;
        }

        var prompt = userItem.Text;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }
        // Mark handled immediately and flip busy flags so the UI shows a
        // single in-flight state and the request cannot be retriggered.
        args.Handled = true;
        IsGenerating = true;
        ShowResponseLoader = true;

        try
        {
            if (_aiService is null)
            {
                Messages.Add(new AssistItem
                {
                    Text = "AI service is not configured. Fill the form manually and pick a logo.",
                    IsRequested = false,
                });
                return;
            }

            var draft = await _aiService.DraftAgentAsync(prompt).ConfigureAwait(true);
            if (draft is null)
            {
                Messages.Add(new AssistItem
                {
                    Text = "I couldn't draft an agent from that description. Try adding more details or fill the form manually.",
                    IsRequested = false,
                });
                return;
            }

            // Write the AI's proposal directly into the configure form
            // (Microsoft Copilot behaviour — the user stays on the same
            // page and just sees the form auto-fill).
            if (!string.IsNullOrWhiteSpace(draft.Name))
            {
                _target.AgentName = draft.Name.Trim();
            }
            if (!string.IsNullOrWhiteSpace(draft.Description))
            {
                _target.Description = draft.Description.Trim();
            }
            if (!string.IsNullOrWhiteSpace(draft.Instructions))
            {
                _target.Instructions = draft.Instructions.Trim();
            }

            Messages.Add(new AssistItem
            {
                Text = $"Here's a draft based on your description:\n• Name: {draft.Name}\n• Description: {draft.Description}",
                IsRequested = false,
            });
        }
        catch (Exception ex)
        {
            Messages.Add(new AssistItem
            {
                Text = $"Sorry, the draft failed: {ex.Message}",
                IsRequested = false,
            });
        }
        finally
        {
            IsGenerating = false;
            ShowResponseLoader = false;
        }
    }
}
