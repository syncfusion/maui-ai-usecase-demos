using Syncfusion.Maui.AIAssistView;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace RichTextEditorAssistViewSample
{
    /// <summary>
    /// Represents the view model for the Assist feature, providing properties, commands, and logic to manage assist items, suggestions, and user interactions within the editor interface.
    /// </summary>
    public class AssistViewViewModel : INotifyPropertyChanged
    {
        #region Field

        /// <summary>
        /// Field to hold the collection of assist items.
        /// </summary>
        private ObservableCollection<IAssistItem> _assistItems;

        /// <summary>
        /// Field to hold the collection of suggestions.
        /// </summary>
        private ObservableCollection<ISuggestion> _suggestions;

        /// <summary>
        /// Field to hold the Azure AI service instance.
        /// </summary>
        private readonly IAzureAIService azureAIService;

        /// <summary>
        /// Field to hold the suggestion item selected command.
        /// </summary>
        private ICommand suggestionItemSelectedCommand = null!;

        /// <summary>
        /// Field to hold the HTML markup for the editor content.
        /// </summary>
        private string editorHtml = string.Empty;

        /// <summary>
        /// Field to hold the current request item being processed.
        /// </summary>
        private IAssistItem? requestItem;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the AssistViewViewModel class with default assist items, suggestions, and command bindings.
        /// </summary>
        public AssistViewViewModel(IAzureAIService azureAIService)
        {
            this.azureAIService = azureAIService;
            this._assistItems = new ObservableCollection<IAssistItem>();
            _suggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion { Text = "Paraphraser" },
                new AssistSuggestion { Text = "Grammar Checker" },
                new AssistSuggestion { Text = "Elaborate" },
                new AssistSuggestion { Text = "Shorten" }
            };

            this.SuggestionItemSelectedCommand = new Command(obj => _ = OnSuggestionTapCommandAsync(obj));
            this.CopyCommand = new Command(async (obj) => await ExecuteCopyCommand(obj));
            this.RetryCommand = new Command(async (obj) => await ExecuteRetryCommandAsync(obj));
            this.ApplyCommand = new Command(async (obj) => await ExecuteApplyCommand(obj));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the command that copies the selected content to the clipboard.
        /// </summary>
        public ICommand CopyCommand { get;}

        /// <summary>
        /// Gets or sets the command that executes a retry operation.
        /// </summary>
        public ICommand RetryCommand { get;}

        /// <summary>
        /// Gets or sets the command that applies the current changes or settings.
        /// </summary>
        public ICommand ApplyCommand { get;}

        /// <summary>
        /// Gets or sets the collection of assist items associated with this instance.
        /// </summary>
        public ObservableCollection<IAssistItem> AssistItems
        {
            get
            {
                return this._assistItems;
            }

            set
            {
                this._assistItems = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the HTML markup used for the editor content.
        /// </summary>
        public string EditorHtml
        {
            get => editorHtml;
            set
            {
                if (editorHtml != value)
                {
                    editorHtml = value;
                    RaisePropertyChanged(nameof(EditorHtml));
                }
            }
        }

        /// <summary>
        /// Gets the collection of suggestions currently available.
        /// </summary>
        public ObservableCollection<ISuggestion> Suggestions => _suggestions;

        /// <summary>
        /// Gets or sets the suggestion item selected command.
        /// </summary>
        public ICommand SuggestionItemSelectedCommand
        {
            get
            {
                return this.suggestionItemSelectedCommand;
            }
            set
            {
                this.suggestionItemSelectedCommand = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to execute the copy command, which copies the text of the selected assist item to the clipboard.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ExecuteApplyCommand(object obj)
        {
            if (obj is AssistItem assistItem && assistItem.Text != null)
            {
                EditorHtml = EnsureHtml(assistItem.Text);
            }
        }

        /// <summary>
        /// Copies the plain text content of the specified assist item to the clipboard, removing any HTML tags and non-breaking spaces.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ExecuteCopyCommand(object obj)
        {
            if (obj is AssistItem assistItem && assistItem.Text != null)
            {
                string text = assistItem.Text;
                text = Regex.Replace(text, "<.*?>|&nbsp;", string.Empty);
                await Clipboard.SetTextAsync(text);
            }
        }

        /// <summary>
        /// Attempts to execute a retry command asynchronously using the specified object as context.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ExecuteRetryCommandAsync(object obj)
        {
            if (obj is AssistItem assistItem && assistItem.RequestItem is IAssistItem item)
            {
                requestItem = item;
                await this.GetResult(requestItem).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handles the tap event for a suggestion item, executing the appropriate action based on the selected suggestion.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task OnSuggestionTapCommandAsync(object obj)
        {
            var args = obj as SuggestionItemSelectedEventArgs;
            if (args == null || args.SelectedItem is not ISuggestion s) return;
            await InputProcessingAsync(s.Text).ConfigureAwait(true);
        }

        /// <summary>
        /// Processes the specified input query and initiates an asynchronous request based on its value.
        /// </summary>
        /// <param name="inputQuery">The input query to be processed. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task InputProcessingAsync(string inputQuery)
        {
            if (string.IsNullOrEmpty(inputQuery))
            {
                return;
            }

            if (inputQuery == "Paraphraser")
            {
                var request = new AssistItem { Text = inputQuery, IsRequested = true };
                await GetResponseWithSuggestion(request).ConfigureAwait(true);
            }
            else
            {
                var request = new AssistItem { Text = inputQuery, IsRequested = true };
                await GetResult(request).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Processes the specified input from a ComboBox and initiates the appropriate asynchronous request based on the input value.
        /// </summary>
        /// <param name="selectedInputQuery">The user input string from the ComboBox to be processed. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ComboBoxInputProcessingAsync(string selectedInputQuery)
        {
            if (string.IsNullOrEmpty(selectedInputQuery))
            {
                return;
            }

            if (selectedInputQuery == "Paraphraser")
            {
                var request = new AssistItem { Text = selectedInputQuery, IsRequested = true };
                AssistItems.Add(request);
                await GetResponseWithSuggestion(request).ConfigureAwait(true);
            }
            else
            {
                var request = new AssistItem { Text = selectedInputQuery, IsRequested = true };
                AssistItems.Add(request);
                await GetResult(request).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Generates a response item with a suggestion for improving the provided input and adds it to the collection of assist items.
        /// </summary>
        /// <param name="inputQuery">The input query for which to generate a response with a suggestion. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task GetResponseWithSuggestion(object inputQuery)
        {
            await Task.Delay(1000).ConfigureAwait(true);
            if (inputQuery != null)
            {
                var suggestion = this.GetParaphraserSuggestion();
                await Task.Delay(1000).ConfigureAwait(true);
                AssistItem responseItem = new AssistItem() { Text = "Do you want to improve the text?", Suggestion = suggestion };
                responseItem.RequestItem = inputQuery;
                this.AssistItems.Add(responseItem);
            }
        }

        /// <summary>
        /// Creates and returns an assist item suggestion containing a predefined set of paraphrasing options.
        /// </summary>
        /// <returns>An <see cref="AssistItemSuggestion"/> that includes paraphrasing suggestions such as "Humanize",
        /// "Professional", "Simple", and "Academic".</returns>
        private AssistItemSuggestion GetParaphraserSuggestion()
        {
            var promptSuggestions = new AssistItemSuggestion();
            var paraphraserSuggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion { Text = "Humanize" },
                new AssistSuggestion { Text = "Professional" },
                new AssistSuggestion { Text = "Simple" },
                new AssistSuggestion { Text = "Academic" }
            };
            promptSuggestions.Items = paraphraserSuggestions;
            return promptSuggestions;
        }

        /// <summary>
        /// Processes the specified input query, retrieves a response from the AI service, and adds the result to the collection of assist items.
        /// </summary>
        /// <param name="inputQuery">The input query to be processed and sent to the AI service. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task GetResult(object inputQuery)
        {
            await Task.Delay(1000).ConfigureAwait(true);
            AssistItem request = (AssistItem)inputQuery;
            if (request != null)
            {
                var userAIPrompt = GetUserAIPrompt(request.Text, EditorHtml);
                var response = await azureAIService!.GetResultsFromAI(userAIPrompt).ConfigureAwait(true);
                response = response.Replace("\n", "<br>");
                AssistItem responseItem = new AssistItem() { Text = response};
                responseItem.RequestItem = inputQuery;
                this.AssistItems.Add(responseItem);
            }
        }

        /// <summary>
        /// Generates an AI prompt string based on the user's command and the provided editor content, tailoring the instructions for grammar checking or rephrasing as appropriate.
        /// </summary>
        /// <param name="editorHtmlContent">The HTML content from the editor to be included in the AI prompt. Can be null or empty.</param>
        /// <param name="userPrompt">The user's command or prompt that indicates the desired AI operation (e.g., "Grammar Checker", "Elaborate", "Shorten"). Cannot be null or empty.</param>
        /// <returns>A formatted string containing the AI prompt.</returns>
        private string GetUserAIPrompt(string userPrompt, string editorHtmlContent)
        {
            // If the AI expects plain text, strip tags; otherwise pass HTML as-is
            var editorPlainText = StripHtml(editorHtmlContent ?? string.Empty);

            if (string.Equals(userPrompt, "Grammar Checker", StringComparison.OrdinalIgnoreCase))
            {
                return $"Given user command: {userPrompt}.\n" +
                       $"Source text (from editor):\n{editorPlainText}\n\n" +
                       $@"Act as a STRICT GRAMMAR/COPY EDITOR.

                       Task:
                       - Correct ONLY grammar, spelling, subject–verb, punctuation, pronouns, capitalization, and spacing.
                       - Do NOT rephrase, simplify, or change wording, synonyms, tone, style, or sentence order.
                       - Keep all original meaning and structure.
                       - If the input is single word and it is misspelled, correct it; if it is correct, return input word as it is without additional words.

                       Scope (apply to every sentence in the paragraph, then check cross‑sentence consistency):
                       - Agreement: subject–verb, pronouns, number, person.
                       - Tense/Aspect consistency across sentences.
                       - Articles/determiners (a/an/the), prepositions, conjunctions.
                       - Punctuation: commas (including serial/Oxford if clearly required by the structure), periods, colons/semicolons, em/en dashes, quotes.
                       - Run‑ons and fragments: fix only by minimal punctuation or capitalization changes (no rewriting).
                       - Capitalization (sentences, proper nouns), spelling (US/UK spelling may be preserved consistently).
                       - Spacing: single spaces between words; spaces around punctuation as appropriate.
                       - Numbers, dates, technical terms, names: do not alter wording—only fix capitalization/punctuation if incorrect.
                       - Hyphenation for compound modifiers when needed (e.g., “state‑of‑the‑art”).

                       Formatting and output rules:
                       - Preserve all original line breaks and paragraph breaks.
                       - Preserve whitespace except where a grammar fix requires change.
                       - If the input is already correct, return it EXACTLY unchanged.
                       - OUTPUT: Return ONLY the corrected text, with no explanations, notes, or extra characters.

                       Text:
                       {{plain}}";
            }
            else if (string.Equals(userPrompt, "Elaborate", StringComparison.OrdinalIgnoreCase))
            {
                return $"Given user command: {userPrompt}.\n" +
                       $"Source text (from editor):\n{editorPlainText}\n\n" +
                       "Instructions:\n" +
                       "- Elaborate the input text by expanding ideas, adding necessary clarifying details, and improving fluency while preserving the original meaning, intent, and facts.\n" +
                       "- Correct any grammatical, punctuation, capitalization, and spacing errors introduced in the source.\n" +
                       "- Do not introduce new factual assertions, examples, opinions, or entities that change the original meaning.\n" +
                       "- Maintain the original tone unless the user explicitly requests a tone change; do not change sentence order except to improve clarity when absolutely necessary.\n" +
                       "- Keep length roughly similar to the original; modest expansion is acceptable (e.g., up to ~30% longer) but avoid verbosity.\n" +
                       "- Preserve paragraph and line breaks from the source unless a minor adjustment is required for grammatical correctness.\n" +
                       "- OUTPUT: Return ONLY the elaborated and corrected text with no explanations, notes, or extra characters.";
            }
            else if (string.Equals(userPrompt, "Shorten", StringComparison.OrdinalIgnoreCase))
            {
                return $"Given user command: {userPrompt}.\n" +
                       $"Source text (from editor):\n{editorPlainText}\n\n" +
                       "Instructions:\n" +
                       "- Shorten the input text by removing redundancy and tightening phrasing while fully preserving the original meaning, intent, and facts.\n" +
                       "- Correct any grammatical, punctuation, capitalization, and spacing errors in the resulting shortened text.\n" +
                       "- Do not omit essential details, change facts, or introduce new information that alters the meaning.\n" +
                       "- Maintain the original tone unless the user explicitly requests a tone change; do not change sentence order except to improve clarity when absolutely necessary.\n" +
                       "- Aim to make the text more concise—typically reducing length by roughly 20–40%—but keep it natural and readable.\n" +
                       "- Preserve paragraph and line breaks from the source unless a minor adjustment is required for grammatical correctness.\n" +
                       "- OUTPUT: Return ONLY the shortened and corrected text with no explanations, notes, or extra characters.";
            }
            else
            {
                return $"Given user command: {userPrompt}.\n" +
                       $"Source text (from editor):\n{editorPlainText}\n\n" +
                       "Please rephrase while preserving meaning.\n" +
                       "- Provide a humanized tone if requested.\n" +
                       "- Provide a professional tone if requested.\n" +
                       "- Provide a simple tone if requested.\n" +
                       "- Provide an academic tone if requested.\n" +
                       "- Fix grammatical errors without any alternative words if requested.\n" +
                       "- Keep length roughly similar.\n" +
                       "- Return only the modified text without additional commentary.";
            }
        }

        /// <summary>
        /// Ensures that the specified content is formatted as HTML.
        /// </summary>
        /// <param name="content">The content to be formatted as HTML. Can be null or empty.</param>
        /// <returns>A string containing the HTML-formatted content.</returns>
        private static string EnsureHtml(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;
            // If it already looks like HTML, return as is; else wrap in <p> and replace line breaks
            return content.Contains("<") ? content : $"<p>{content.Replace("\n", "<br>")}</p>";
        }

        /// <summary>
        /// Removes all HTML tags from the specified string.
        /// </summary>
        /// <param name="html">The string containing HTML content to be stripped of tags. Can be null or empty.</param>
        /// <returns>A string with all HTML tags removed. Returns an empty string if the input is null or empty.</returns>
        private static string StripHtml(string html) =>
            string.IsNullOrEmpty(html) ? string.Empty : Regex.Replace(html, "<.*?>", string.Empty);

        /// <summary>
        /// Handles the selection change event for a ComboBox and processes the newly selected feature.
        /// </summary>
        /// <param name="e">The event data containing information about the selection change, including the items added and removed.</param>
        public void GetComboBoxSelection(Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
        {
            if (e?.AddedItems == null || e.AddedItems.Count == 0)
            {
                return;
            }

            var selectedItem = e.AddedItems[0];
            if (selectedItem == null || selectedItem is not Feature s)
            {
                return;
            }

            if (!string.IsNullOrEmpty(s.FeatureName))
            {
                _ = ComboBoxInputProcessingAsync(s.FeatureName);
            }
        }

        #endregion

        #region PropertyChanged

        /// <summary>
        /// Property changed handler.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when property is changed.
        /// </summary>
        /// <param name="propName">changed property name</param>
        public void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName ?? string.Empty));
        }

        #endregion
    }
}