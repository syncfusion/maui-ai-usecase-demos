using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace AIPoweredWritingAssistant
{
    /// <summary>
    /// Class representing the base service for Azure AI interactions, providing common functionality for validating credentials and managing the chat client used to communicate with Azure's AI services.
    /// </summary>
    public abstract class AzureBaseService
    {
        #region Fields

        /// <summary>
        /// Field to store the endpoint value
        /// </summary>
        private const string endpoint = "END_POINT";

        /// <summary>
        /// Field to store the deployment name
        /// </summary>
        internal const string deploymentName = "DEPLOYMENT_NAME";

        /// <summary>
        /// Field to store the image deployment name
        /// </summary>
        internal const string imageDeploymentName = "IMAGE_MODEL_NAME";

        /// <summary>
        /// Field to store the api key value
        /// </summary>
        private const string key = "KEY";

        /// <summary>
        /// Field to store the chat client
        /// </summary>
        internal IChatClient? Client { get; set; }

        /// <summary>
        /// Field to store the chat history
        /// </summary>
        internal string? ChatHistory { get; set; }

        /// <summary>
        /// Field to store the is credential valid value
        /// </summary>
        private static bool _isCredentialValid = false;

        /// <summary>
        /// The already credential validated field
        /// </summary>
        private static bool isAlreadyValidated = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the AzureBaseService class and validates the service credentials.
        /// </summary>
        public AzureBaseService()
        {
            ValidateCredential();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Set a value indicating whether an credentials are valid or not.
        /// </summary>
        internal static bool IsCredentialValid
        {
            get => _isCredentialValid;
            set
            {
                if (_isCredentialValid != value)
                {
                    _isCredentialValid = value;
                    OnCredentialChanged?.Invoke(value);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validate Azure Credentials
        /// </summary>
        private async void ValidateCredential()
        {
            this.GetAzureOpenAIKernal();

            if (isAlreadyValidated)
            {
                return;
            }

            try
            {
                if (Client != null)
                {
                    await Client!.CompleteAsync("Hello, Test Check");
                    ChatHistory = string.Empty;
                    IsCredentialValid = true;
                    isAlreadyValidated = true;
                }
                else
                {
                    ShowAlertAsync();
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Show Alert Popup
        /// </summary>
        private async void ShowAlertAsync()
        {
            var page = Application.Current?.Windows[0].Page;
            if (page != null && !IsCredentialValid)
            {
                isAlreadyValidated = true;
#if NET10_0
                await page.DisplayAlertAsync("Alert", "The Azure API key or endpoint is missing or incorrect. Please verify your credentials. You can also continue with the offline data.", "OK");
#else
                await page.DisplayAlert("Alert", "The Azure API key or endpoint is missing or incorrect. Please verify your credentials. You can also continue with the offline data.", "OK");
#endif
            }
        }

        #endregion

        #region Azure OpenAI

        /// <summary>
        /// To get the Azure open ai kernal method
        /// </summary>
        private void GetAzureOpenAIKernal()
        {
            var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
                      .AsChatClient(modelId: deploymentName);
            this.Client = client;
        }

        #endregion

        #region Events

        /// <summary>
        /// Represents the method that will handle IsCredentialValid change event.
        /// </summary>
        internal delegate void CredentialValidated(bool isCredentialValid);

        /// <summary>
        /// Represents the event when IsCredentialValid value is changed.
        /// </summary>
        internal static event CredentialValidated? OnCredentialChanged;

        #endregion

    }

    /// <summary>
    /// Provides functionality for interacting with an Azure-based AI service, enabling the execution of AI-powered operations such as generating responses to user prompts.
    /// </summary>
    public class AzureAIService : AzureBaseService, IAzureAIService
    {
        #region Methods

        /// <summary>
        /// Method to get the results from AI service.
        /// </summary>
        /// <param name="userAIPrompt"></param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal async Task<string> GetResultsFromAI(string userAIPrompt)
        {
            if (IsCredentialValid && Client != null)
            {
                ChatHistory = string.Empty;
                // Add the system message and user message to the options
                ChatHistory = ChatHistory + userAIPrompt;
                try
                {
                    var response = await Client.CompleteAsync(ChatHistory);
                    return response.ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        #endregion
    }
}