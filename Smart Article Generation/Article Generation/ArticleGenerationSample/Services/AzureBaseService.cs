using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Net;

namespace ArticleGenerationSample
{
    /// <summary>
    /// Abstract base class for Azure AI services
    /// </summary>
    public abstract class AzureBaseService
    {
        #region Fields

        /// <summary>
        /// The EndPoint
        /// </summary>
        private const string endpoint = "END_POINT";

        /// <summary>
        /// The Deployment name
        /// </summary>
        internal const string deploymentName = "DEPLOYMENT_NAME";

        /// <summary>
        /// The Image Deployment name
        /// </summary>
        internal const string imageDeploymentName = "IMAGE_MODEL_NAME";

        /// <summary>
        /// The API key
        /// </summary>
        private const string key = "KEY";

        /// <summary>
        /// The chat completion service
        /// </summary>
        private IChatCompletionService? chatCompletions;

        /// <summary>
        /// The kernal
        /// </summary>
        private Kernel? kernel;

        /// <summary>
        /// The chat histroy
        /// </summary>
        private ChatHistory? chatHistory;

        /// <summary>
        /// Field to store whether the credentials are valid
        /// </summary>
        private bool isCredentialValid = false;

        /// <summary>
        /// Field to store whether the credentials have already been validated
        /// </summary>
        private bool isAlreadyValidated = false;

        /// <summary>
        /// Field to store the URI result
        /// </summary>
        private Uri? uriResult;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBaseService"/> class.
        /// </summary>

        public AzureBaseService()
        {
            _ = ValidateCredential();
        }

        #region Properties

        /// <summary>
        /// Gets or Set a value indicating whether an credentials are valid or not.
        /// Returns <c>true</c> if the credentials are valid; otherwise, <c>false</c>.
        /// </summary>
        public bool IsCredentialValid
        {
            get
            {
                return isCredentialValid;
            }
            set
            {
                isCredentialValid = value;
            }
        }

        /// <summary>
        /// Gets or sets the chat history object
        /// </summary>
        public ChatHistory? ChatHistory
        {
            get
            {
                return chatHistory;
            }
            set
            {
                chatHistory = value;
            }
        }

        /// <summary>
        /// Gets or sets the chat completion service object
        /// </summary>
        public IChatCompletionService? ChatCompletions
        {
            get
            {
                return chatCompletions;
            }
            set
            {
                chatCompletions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the kernal object
        /// </summary>
        public Kernel? Kernel
        {
            get
            {
                return kernel;
            }
            set
            {
                kernel = value;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validate Azure Credentials
        /// </summary>
        private async Task ValidateCredential()
        {
            // Defer any UI until a page is ready; focus on wiring the kernel so AI can run
            #region Azure OpenAI
            this.GetAzureOpenAIKernal();
            #endregion

            if (isAlreadyValidated)
            {
                return;
            }

            bool isValidUri = Uri.TryCreate(endpoint, UriKind.Absolute, out uriResult)
                 && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            // If config looks wrong, just mark invalid and exit without showing any alert (avoid XamlRoot issues)
            if (!isValidUri || !endpoint.Contains("http") || string.IsNullOrEmpty(key) || key.Contains("API key") || string.IsNullOrEmpty(deploymentName) || deploymentName.Contains("deployment name") || string.IsNullOrEmpty(imageDeploymentName))
            {
                // Credentials/config are invalid; mark as invalid and show a one-time alert when UI is ready
                IsCredentialValid = false;
                ShowAlertAsync();
                return;
            }

            try
            {
                if (ChatHistory != null && chatCompletions != null)
                {
                    ChatHistory.AddSystemMessage("Hello, Test Check");
                    await chatCompletions.GetChatMessageContentAsync(chatHistory: ChatHistory, kernel: kernel);
                }
            }
            catch (Exception)
            {
                // Online call failed; mark invalid and surface a one-time alert when UI is ready
                IsCredentialValid = false;
                ShowAlertAsync();
                return;
            }

            IsCredentialValid = true;
            isAlreadyValidated = true;
        }

        /// <summary>
        /// Show Alert Popup
        /// </summary>
        private async void ShowAlertAsync()
        {
            if (IsCredentialValid)
                return;

            var window = Application.Current?.Windows?.FirstOrDefault(w => w?.Page != null);
            var page = window?.Page;

            // If the page/handler is not ready yet, retry shortly on the UI thread
            if (page == null || window?.Handler == null)
            {
                await Task.Delay(500);
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(300);
                });
                return;
            }

            // Ensure we invoke on UI thread to avoid XamlRoot errors on Windows
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!IsCredentialValid && !isAlreadyValidated)
                {
                    isAlreadyValidated = true;
                    await page.DisplayAlertAsync("Alert", "The Azure API key or endpoint is missing or incorrect. Please verify your credentials. You can also continue with the offline data.", "OK");
                }
            });
        }

        #endregion

        #region Azure OpenAI

        /// <summary>
        /// To get the Azure open ai kernal method
        /// </summary>
        private void GetAzureOpenAIKernal()
        {
            // Create the chat history
            chatHistory = new ChatHistory();
            
            // Create HttpClient with custom handler for Android SSL certificate handling
            HttpClientHandler httpClientHandler = CreateHttpClientHandler();
            HttpClient httpClient = new HttpClient(httpClientHandler);
            
            var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(deploymentName, endpoint, key, httpClient: httpClient);

            // Get the kernal from build
            kernel = builder.Build();

            //Get the chat completions from kernal
            chatCompletions = kernel.GetRequiredService<IChatCompletionService>();
        }

        /// <summary>
        /// Creates an HttpClientHandler with proper SSL/TLS certificate handling for all platforms.
        /// On Android, this ensures compatibility with certificate validation while maintaining security.
        /// </summary>
        private static HttpClientHandler CreateHttpClientHandler()
        {
            var handler = new HttpClientHandler();

#if __ANDROID__
            // On Android, we need to configure the handler to properly handle SSL certificates
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                // Validate that we're connecting to a trusted endpoint
                if (message.RequestUri?.Host == "END_POINT")
                {
                    return errors == System.Net.Security.SslPolicyErrors.None;
                }
                return false;
            };
#endif

            return handler;
        }

        #endregion

    }
}
