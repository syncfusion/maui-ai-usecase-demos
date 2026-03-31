using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using Syncfusion.Maui.AIAssistView;
using ArticleGenerationSample;
using ArticleGenerationSampl;

namespace ArticleGenerationSample
{
    /// <summary>
    /// Provides state and commands for generating articles, rendering content, and managing resources.
    /// </summary>
    public class ArticleViewModel : BaseViewModel
    {
        #region Fields

        /// <summary>
        /// Field to backing the Messages property.
        /// </summary>
        private ObservableCollection<IAssistItem> messages;

        /// <summary>
        /// Field to backing the Resources property.
        /// </summary>
        private ObservableCollection<ResourceItem> resources;

        /// <summary>
        /// Field to backing the Topics property.
        /// </summary>
        private ObservableCollection<string> topics;

        /// <summary>
        /// Field to backing the HasValidResponse property.
        /// </summary>
        private bool _hasValidResponse = false;

        /// <summary>
        /// Field to backing the HtmlContent property.
        /// </summary>
        private string _htmlContent = string.Empty;

        /// <summary>
        /// Field to backing the ResearchQuestion property.
        /// </summary>
        private string _researchQuestion = string.Empty;

        /// <summary>
        /// Field to backing the SelectedTopic property.
        /// </summary>
        private string _selectedTopic = string.Empty;

        /// <summary>
        /// Field to backing the SelectedResourceId property.
        /// </summary>
        private int _selectedResourceId = -1;

        /// <summary>
        /// Field to backing the IsAssistVisible property, which controls the visibility of the AssistView overlay on mobile platforms.
        /// </summary>
        private bool _isAssistVisible = false;

        /// <summary>
        /// Represents the Azure AI service instance used for performing AI-related operations.
        /// </summary>
        internal ArticleGenerationSample.IAzureAIService azureAIService;

        /// <summary>
        /// Field to manage the collection of resources extracted from AI responses and user interactions.
        /// </summary>
        private ResourcesService _resourcesService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleViewModel"/> class using DI.
        /// </summary>
        /// <param name="azureAIService">Injected <see cref="IAzureAIService"/> implementation.</param>
        public ArticleViewModel(ArticleGenerationSample.IAzureAIService azureAIService)
        {
            this.azureAIService = azureAIService;
            this.messages = new ObservableCollection<IAssistItem>();
            this.resources = new ObservableCollection<ResourceItem>();
            this.topics = new ObservableCollection<string>();
            this._resourcesService = new ResourcesService();
            this.AssistViewRequestCommand = new Command(async (o) => await ExecuteRequestCommand(o));
            this.RegenerateCommand = new Command(ExecuteRegenerateCommand);
            this.ResourceSelectedCommand = new Command<Models.ResourceItem>(ExecuteResourceSelected);
            this.DeleteResourceCommand = new Command(async (o) => await ExecuteDeleteResource(o));
            this.AddResourceCommand = new Command(async (o) => await ExecuteAddResource(o));
            this.OpenUrlCommand = new Command(async (o) => await ExecuteOpenUrl(o));
            this.SelectTopicCommand = new Command<string>(ExecuteSelectTopic);
            this.ToggleAssistCommand = new Command(() => IsAssistVisible = !IsAssistVisible);
            this.AddInitialResponseText();
        }

        /// <summary>
        /// Parameterless fallback constructor for XAML and previews. Delegates to DI constructor with a local instance.
        /// </summary>
        public ArticleViewModel()
            : this(new AzureAIService())
        {
        }

        /// <summary>
        /// Adds the initial greeting message to the chat.
        /// </summary>
        private void AddInitialResponseText()
        {
            var greetingItem = new AssistItem
            {
                Text = "Hi! What topic would you like me to generate an article on today?",
                ShowAssistItemFooter = true
            };
            this.messages.Add(greetingItem);
        }

		#endregion

		#region Commands

        /// <summary>
        /// Handles view loaded/initialized events.        /// Fired when the AssistView submits a user request.
        /// </summary>
        public ICommand AssistViewRequestCommand { get; }

        /// <summary>
        /// Regenerates the AI response for the current prompt.
        /// </summary>
        public ICommand RegenerateCommand { get; }

        /// <summary>
        /// Handles selection of a resource item.
        /// </summary>
        public ICommand ResourceSelectedCommand { get; }
        
        /// <summary>
        /// Toggles visibility of the AssistView overlay on mobile platforms.
        /// </summary>
        public ICommand ToggleAssistCommand { get; }

        /// <summary>
        /// Deletes a resource from the collection.
        /// </summary>
        public ICommand DeleteResourceCommand { get; }

        /// <summary>
        /// Opens the add resource dialog and appends a new resource.
        /// </summary>
        public ICommand AddResourceCommand { get; }

        /// <summary>
        /// Opens a URL in the system browser.
        /// </summary>
        public ICommand OpenUrlCommand { get; }

        /// <summary>
        /// Sets the research question from a selected topic.
        /// </summary>
        public ICommand SelectTopicCommand { get; }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets or sets the collection of research resources displayed in the UI.
        /// </summary>
        public ObservableCollection<ResourceItem> Resources
        {
            get => this.resources;
            set
            {
                this.resources = value;
                RaisePropertyChanged(nameof(Resources));
            }
        }

        /// <summary>
        /// Gets or sets the list of previously used or suggested topics.
        /// </summary>
        public ObservableCollection<string> Topics
        {
            get => this.topics;
            set
            {
                this.topics = value;
                RaisePropertyChanged(nameof(Topics));
            }
        }

        /// <summary>
        /// Gets or sets the currently selected topic. Also updates <see cref="ResearchQuestion"/>.
        /// </summary>
        public string SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                if (_selectedTopic != value)
                {
                    _selectedTopic = value;
                    ResearchQuestion = value;
                    RaisePropertyChanged(nameof(SelectedTopic));
                }
            }
        }

        /// <summary>
        /// Gets or sets the ID of the currently selected resource.
        /// </summary>
        public int SelectedResourceId
        {
            get => _selectedResourceId;
            set
            {
                _selectedResourceId = value;
                RaisePropertyChanged(nameof(SelectedResourceId));
            }
        }

        /// <summary>
        /// Gets or sets the AssistView message collection.
        /// </summary>
        public ObservableCollection<IAssistItem> Messages
        {
            get => this.messages;
            set
            {
                this.messages = value;
                RaisePropertyChanged(nameof(Messages));
            }
        }

        /// <summary>
        /// Gets or sets the generated HTML content to render in the chat view.
        /// </summary>
        public string HtmlContent
        {
            get => _htmlContent;
            set
            {
                _htmlContent = value;
                RaisePropertyChanged(nameof(HtmlContent));
                RaisePropertyChanged(nameof(MarkdownContent));
                RaisePropertyChanged(nameof(TitleText));
            }
        }

        /// <summary>
        /// Markdown for Android SfMarkdownView; maps from HtmlContent.
        /// </summary>
        public string MarkdownContent
        {
            get
            {
                // AI now returns Markdown directly; no conversion required
                return _htmlContent ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets whether the Assist chat overlay is visible on mobile.
        /// </summary>
        public bool IsAssistVisible
        {
            get => _isAssistVisible;
            set
            {
                if (_isAssistVisible != value)
                {
                    _isAssistVisible = value;
                    RaisePropertyChanged(nameof(IsAssistVisible));
                }
            }
        }

        /// <summary>
        /// Gets or sets the user's research question / article topic.
        /// </summary>
        public string ResearchQuestion
        {
            get => _researchQuestion;
            set
            {
                _researchQuestion = value;
                RaisePropertyChanged(nameof(ResearchQuestion));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the last AI response was valid (not fallback text).
        /// </summary>
        public bool HasValidResponse
        {
            get => _hasValidResponse;
            set
            {
                _hasValidResponse = value;
                RaisePropertyChanged(nameof(HasValidResponse));
            }
        }

        /// <summary>
        /// Gets a short title derived from the generated HTML content for display in the chat header.
        /// </summary>
        public string TitleText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(HtmlContent))
                    return "Generate Articles"; 

                var cleanedResponse = RemoveHtmlTags(HtmlContent);
                cleanedResponse = cleanedResponse.Replace("\n", " ").Trim();

                var words = cleanedResponse.Split(new[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length == 0)
                    return "Generate Articles";

                var firstFewWords = string.Join(" ", words.Take(3));
                var title = firstFewWords.Length > 0 ? firstFewWords : "Generate Articles";

                if (words.Length > 3)
                    title += "...";

                return title;
            }
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Handles AssistView request submission and triggers result generation.
        /// </summary>
        /// <param name="obj">The request event args containing the AssistItem.</param>
        private async Task ExecuteRequestCommand(object obj)
        {
            var request = (obj as Syncfusion.Maui.AIAssistView.RequestEventArgs)?.RequestItem;
            if (request != null)
            {
                // Update the Article Topic with the text entered in the AssistView
                ResearchQuestion = request.Text;

                await this.GetResult(request).ConfigureAwait(true);
            }
  }

        /// <summary>
        /// Shows a simple notification indicating regenerate action.
        /// </summary>
        private void ExecuteRegenerateCommand()
        {
            var mainPage = Application.Current?.Windows[0].Page;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Regenerate", "Regenerating response...", "OK");
                }
            });
        }

        /// <summary>
        /// Handles selection of a resource from the list.
        /// </summary>
        /// <param name="resource">The selected resource.</param>
        private void ExecuteResourceSelected(ResourceItem resource)
        {
            if (resource != null)
            {
                SelectedResourceId = resource.Id;
            }
        }

        /// <summary>
        /// Removes the specified resource from the collection.
        /// </summary>
        /// <param name="obj">The resource object to remove.</param>
        private async Task ExecuteDeleteResource(object obj)
        {
            var resource = obj as Models.ResourceItem;
            if (resource != null)
            {
                Resources.Remove(resource);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Opens the specified URL using the system browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private async Task ExecuteOpenUrl(object obj)
        {
            var url = obj as string;
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    {
                        url = "https://" + url;
                    }

                    await Browser.Default.OpenAsync(new Uri(url), BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    var mainPage = Application.Current?.Windows[0].Page;
                    if (mainPage != null)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                            mainPage.DisplayAlertAsync("Error", $"Failed to open URL: {ex.Message}", "OK"));
                    }
                }
            }
        }

        /// <summary>
        /// Opens the add-resource dialog and appends the result to the collection.
        /// </summary>
        private async Task ExecuteAddResource(object obj)
        {
            var mainPage = Application.Current?.Windows[0].Page;

            if (!HasValidResponse)
            {
                if (mainPage != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                        mainPage.DisplayAlertAsync(
                            "No Valid Response",
                            "Please generate a valid article first before adding sources.",
                            "OK"));
                }
                return;
            }

            var (url, title, description) = await AddResourceDialog.ShowAsync();

            if (!string.IsNullOrEmpty(title))
            {
                var newResource = new ResourceItem
                {
                    Id = Resources.Count + 1,
                    Icon = "🌐",
                    Title = title,
                    Url = url,
                    Description = description,
                    SourceType = "Custom",
                    RelevanceScore = 100,
                    Author = "User"
                };

                Resources.Add(newResource);
            }
        }

        /// <summary>
        /// Sets the selected topic and updates related state.
        /// </summary>
        /// <param name="topic">The topic text.</param>
        private void ExecuteSelectTopic(string topic)
        {
            if (!string.IsNullOrEmpty(topic))
            {
                SelectedTopic = topic;
                
                // Add to topics if not already there
                if (!Topics.Contains(topic))
                {
                    Topics.Insert(0, topic);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Builds a prompt to steer the AI to produce article-like content.
        /// </summary>
        /// <param name="userPrompt">The user's query.</param>
        /// <returns>The composed AI prompt.</returns>
        private string GetUserAIPrompt(string userPrompt)
        {
            // Request Markdown output directly to avoid HTML-to-Markdown conversion
            return $"Given User query: {userPrompt}." +
                   $"\nFollow these conditions:" +
                   $"\n- Produce the response in GitHub-flavored Markdown (GFM)." +
                   $"\n- Start with a clear H2 title (## Title) and provide article-style content." +
                   $"\n- Use headings, paragraphs, bullet lists, and code fences as appropriate." +
                   $"\n- Keep total length roughly between 2000-4000 words." +
                   $"\n- Do not wrap the entire response in quotes." +
                   $"\n- At the end, add a 'References' section listing 3-6 external sources as Markdown links in the form [Title](URL).";
        }

        /// <summary>
        /// Generates the AI response, updates the content, and refreshes resources.
        /// </summary>
        /// <param name="inputQuery">The AssistItem request.</param>
        private async Task GetResult(object inputQuery)
        {
            await Task.Delay(1000).ConfigureAwait(true);
            
            var request = inputQuery as AssistItem;
            if (request == null)
                return;

            IsBusy = true;
            
            try
            {
                var userAIPrompt = this.GetUserAIPrompt(request.Text);
                string response = await azureAIService.GetResultsFromAI(request.Text, userAIPrompt).ConfigureAwait(true);
                // Keep original response HTML/text; MarkdownContent will convert as needed per platform
                HtmlContent = response;

                // Check if this is a valid response or the fallback message
                bool isValidResponse = !response.Contains("Please connect to your preferred AI service for real-time queries");
                HasValidResponse = isValidResponse;

                if (isValidResponse)
                {
                    // Extract resources from valid response
                    var extractedResources = ResponseParserService.ExtractResourcesFromResponse(response, request.Text);
                    _resourcesService.UpdateResources(extractedResources);
                    
                    // Update resources collection
                    Resources.Clear();
                    foreach (var resource in _resourcesService.GetResources())
                    {
                        Resources.Add(resource);
                    }
                }
                else
                {
                    // Clear resources for fallback message
                    Resources.Clear();
                }

                var responseItem = new AssistItem
                { 
                    Text = $"I've created a response for your request titled '{request.Text}'. Please refer to it and let me know if you need any further edits or changes!.", 
                    ShowAssistItemFooter = false,
                    RequestItem = inputQuery
                };
                
                this.Messages.Add(responseItem);
            }
            finally
            {
                await Task.Delay(1000);
                IsBusy = false;
            }
        }

        /// <summary>
        /// Removes HTML tags from a string and normalizes whitespace.
        /// </summary>
        /// <param name="input">Input string with HTML.</param>
        /// <returns>Plain text without tags.</returns>
        private string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.Replace("<br>", " ");
            input = Regex.Replace(input, "<.*?>", string.Empty);
            input = input.Replace("&nbsp;", " ");
            input = Regex.Replace(input, @"\s+", " ").Trim();

            return input;
        }

        #endregion
    }
}
