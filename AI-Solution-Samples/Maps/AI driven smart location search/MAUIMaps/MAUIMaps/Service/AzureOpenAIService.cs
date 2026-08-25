namespace MAUIMaps
{
    using Azure;
    using Azure.AI.OpenAI;

    internal class AzureOpenAIService
    {
        const string endpoint = "";
        const string deploymentName = "GPT-4O";
        const string imageDeploymentName = "DALL-E";
        string key = "";

        OpenAIClient? client;
        ChatCompletionsOptions? chatCompletions;

        internal AzureOpenAIService()
        {
            if (key != string.Empty && endpoint != string.Empty)
            {
                this.Client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            }
        }

        /// <summary>
        /// Get or Set Azure OpenAI client.
        /// </summary>
        public OpenAIClient? Client
        {
            get
            {
                return client;
            }
            set
            {
                client = value;
            }
        }


        /// <summary>
        /// Retrieves an answer from the deployment name model using the provided user prompt.
        /// </summary>
        /// <param name="userPrompt">The user prompt.</param>
        /// <returns>The AI response.</returns>
        internal async Task<string> GetResultsFromAI(string userPrompt)
        {
            this.chatCompletions = new ChatCompletionsOptions
            {
                DeploymentName = deploymentName,
                Temperature = (float)0.5,
                MaxTokens = 800,
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            };

            if (this.Client != null)
            {
                // Add the user's prompt as a user message to the conversation.
                this.chatCompletions?.Messages.Add(new ChatRequestSystemMessage("You are a predictive analytics assistant."));

                // Add the user's prompt as a user message to the conversation.
                this.chatCompletions?.Messages.Add(new ChatRequestUserMessage(userPrompt));
                try
                {
                    // Send the chat completion request to the OpenAI API and await the response.
                    var response = await this.Client.GetChatCompletionsAsync(this.chatCompletions);

                    // Return the content of the first choice in the response, which contains the AI's answer.
                    return response.Value.Choices[0].Message.Content;
                }
                catch
                {
                    // If an exception occurs (e.g., network issues, API errors), return an empty string.
                    return "";
                }
            }

            return "";
        }

        /// <summary>
        /// Retrieves an image from the image deployment name model using the provided location name.
        /// </summary>
        /// <param name="locationName">The location name.</param>
        /// <returns>The AI response.</returns>
        public async Task<Uri> GetImageFromAI(string? locationName)
        {
            var imageGenerations = await Client!.GetImageGenerationsAsync(
                new ImageGenerationOptions()
                {
                    Prompt = $"Share the {locationName} image. If the image is not available share common image based on the location",
                    Size = ImageSize.Size1024x1024,
                    Quality = ImageGenerationQuality.Standard,
                    DeploymentName = imageDeploymentName,
                });

            var imageUrl = imageGenerations.Value.Data[0].Url;
            return new Uri(imageUrl.ToString());
        }
    }
}