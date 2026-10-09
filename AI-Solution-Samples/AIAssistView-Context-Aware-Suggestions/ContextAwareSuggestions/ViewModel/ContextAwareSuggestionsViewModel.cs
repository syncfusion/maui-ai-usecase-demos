using CommunityToolkit.Mvvm.ComponentModel;
using Syncfusion.Maui.AIAssistView;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ContextAwareSuggestions;

/// <summary>
/// Represents the ViewModel for the Context-Aware Suggestions sample.
/// </summary>
/// <remarks>
/// This ViewModel manages AI conversations, user prompts, and context-aware follow-up suggestions displayed in the Syncfusion AI AssistView control.
/// It communicates with the Azure AI service to generate responses and dynamically updates the conversation history.
/// </remarks>
public partial class ContextAwareSuggestionsViewModel : ObservableObject
{
    #region Fields
    private ObservableCollection<IAssistItem> messages = new();

    private ObservableCollection<ISuggestion> suggestions = new();

    private readonly IAzureAIService aiService;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the collection of conversation items displayed in the AI AssistView.
    /// </summary>
    public ObservableCollection<IAssistItem> Messages
    {
        get { return messages; }
        set
        {
            messages = value;
            OnPropertyChanged(nameof(this.Messages));
        }
    }

    /// <summary>
    /// Gets or sets the collection of suggestion items displayed in the AI AssistView.
    /// </summary>
    /// <remarks>
    /// These suggestions are shown before a conversation starts and can also be updated dynamically based on the current conversation context.
    /// </remarks>
    public ObservableCollection<ISuggestion> Suggestions
    {
        get { return suggestions; }
        set 
        {
            suggestions = value;
            OnPropertyChanged(nameof(this.Suggestions));
        }
    }

    #endregion

    #region Commands
    /// <summary>
    /// Gets the command that handles user requests submitted from the AI AssistView.
    /// </summary>
    public ICommand RequestCommand { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextAwareSuggestionsViewModel"/> class.
    /// </summary>
    public ContextAwareSuggestionsViewModel()
    {
        aiService = IPlatformApplication.Current!.Services.GetRequiredService<IAzureAIService>();

        RequestCommand = new Command<object>(
            async (obj) =>
            {
                string prompt = ExtractPrompt(obj);
                if (!string.IsNullOrWhiteSpace(prompt))
                {
                    await SendPromptAsync(prompt);
                }
            });

        LoadInitialSuggestions();
    }

    #endregion

    #region Methods
    /// <summary>
    /// Extracts the user prompt from the specified request object.
    /// </summary>
    /// <param name="obj">
    /// The request object received from the AI AssistView command.
    /// </param>
    /// <returns>
    /// The extracted prompt text if available; otherwise, an empty string.
    /// </returns>
    private static string ExtractPrompt(object? obj)
    {
        if (obj == null)
            return string.Empty;

        if (obj is ISuggestion s)
            return s.Text ?? string.Empty;

        if (obj is string str)
            return str;

        if (obj is IAssistItem item)
            return item.Text ?? string.Empty;

        var type = obj.GetType();

        // Try to get RequestItem property (from Syncfusion RequestEventArgs)
        try
        {
            var requestItemProp = type.GetProperty("RequestItem");
            if (requestItemProp != null)
            {
                var requestItem = requestItemProp.GetValue(obj);
                if (requestItem is IAssistItem assistItem && !string.IsNullOrWhiteSpace(assistItem.Text))
                {
                    return assistItem.Text;
                }
            }
        }
        catch { }

        // Avoid returning the CLR type name
        var toStr = obj.ToString() ?? string.Empty;
        if (toStr == type.FullName || toStr == type.Name)
            return string.Empty;

        return toStr;
    }

    /// <summary>
    /// Loads the initial suggestion items displayed before the user starts a conversation.
    /// </summary>
    private void LoadInitialSuggestions()
    {
        Suggestions.Clear();

        var sugg = new ObservableCollection<ISuggestion>();
        sugg.Add(
            new AssistSuggestion()
            {
                Text = "Python Basics"
            });

        sugg.Add(
            new AssistSuggestion()
            {
                Text = "Python Roadmap"
            });

        sugg.Add(
            new AssistSuggestion()
            {
                Text = "Python Practice Projects"
            });

        sugg.Add(
            new AssistSuggestion()
            {
                Text = "Best Resources for Python"
            });

        this.Suggestions = sugg;
    }

    /// <summary>
    /// Sends the specified prompt to the Azure AI service and adds the generated response to the conversation along with context-aware suggestions.
    /// </summary>
    /// <param name="prompt">
    /// The user prompt sent to the AI service.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    private async Task SendPromptAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return;

        // NOTE: The AssistView control automatically adds the request to Messages
        // when RequestCommand is triggered. We do NOT add it manually here to avoid duplication.
        // We only add the AI response.

        try
        {
            var response = await aiService.GetResponseAsync(prompt);

            if (response == null)
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Create list of suggestion items to display with this response
                var suggestionItems = new ObservableCollection<ISuggestion>();
                if (response.Suggestions != null && response.Suggestions.Count > 0)
                {
                    foreach (var suggestion in response.Suggestions)
                    {
                        suggestionItems.Add(new AssistSuggestion { Text = suggestion });
                    }
                }

                // Add the AI response bubble with suggestions footer
                Messages.Add(
                    new AssistItem
                    {
                        Text = response.Answer,
                        IsRequested = false,
                        Suggestion = new AssistItemSuggestion
                        {
                            Items = suggestionItems,
                            Orientation = SuggestionsOrientation.Horizontal
                        }
                    });
            });
        }
        catch (Exception ex)
        {

            // Show error to user
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Messages.Add(
                    new AssistItem
                    {
                        Text = $"Error: {ex.Message}"
                    });
            });
        }
    }

    #endregion
}