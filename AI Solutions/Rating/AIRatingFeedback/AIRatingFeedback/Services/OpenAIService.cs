using System.ClientModel;
using System.Diagnostics;
using System.Globalization;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AIRatingFeedback.Services;

/// <summary>
/// Wraps the Azure OpenAI chat client used to suggest a customer-support
/// satisfaction rating from free-form feedback text.
/// </summary>
/// <remarks>
/// Credentials are read from the following environment variables:
/// <list type="bullet">
///   <item><c>AZURE_OPENAI_ENDPOINT</c> – e.g. <c>https://your-resource.openai.azure.com/</c></item>
///   <item><c>AZURE_OPENAI_DEPLOYMENT_NAME</c> – name of the chat deployment</item>
///   <item><c>AZURE_OPENAI_API_KEY</c> – API key for the Azure OpenAI resource</item>
/// </list>
/// If any value is missing or the endpoint is malformed, <see cref="IsConfigured"/>
/// is <c>false</c> and <see cref="GetSuggestedRatingAsync"/> throws
/// <see cref="InvalidOperationException"/> so the caller can surface the error.
/// </remarks>
public sealed class OpenAIService
{
    /// <summary>
    /// System prompt that instructs the model to return a single decimal
    /// rating between 0.0 and 5.0.
    /// </summary>
    private const string SystemPrompt =
        """
        You are an AI assistant that analyzes customer support feedback and determines an appropriate satisfaction rating.

        Your task is to understand the customer's overall experience and assign a decimal rating from 1.0 to 5.0 based on the sentiment expressed in the feedback.

        Consider factors such as:
            - Issue resolution effectiveness
            - Response time
            - Professionalism of the support agent
            - Helpfulness of the support provided
            - Customer satisfaction or frustration
            - Ease of resolving the issue
            - Whether the customer would likely have a positive or negative impression of the support experience

        Rating Guidelines:
            0.0: Not enough information, incomplete feedback, unfinished sentences, random text, gibberish, unrelated content, or feedback that does not clearly describe a customer support experience.
            0.1 - 1.0: The customer is extremely dissatisfied. Feedback indicates a very poor experience, unresolved issues, repeated problems, strong frustration, or disappointment.
            1.1 - 2.0: The customer is dissatisfied. Feedback indicates significant delays, inadequate assistance, poor communication, or unresolved concerns.
            2.1 - 3.0: The experience is mixed or neutral. The feedback contains both positive and negative aspects, or the customer's satisfaction level is unclear.
            3.1 - 4.0: The customer is generally satisfied. The issue was resolved, but there may be minor concerns such as slightly delayed responses or small frustrations.
            4.1 - 5.0: The customer is highly satisfied. Feedback indicates excellent service, quick resolution, professionalism, clear communication, or an overall positive experience.

        Return ONLY a single decimal number between 0.0 and 5.0.
        Do not return explanations, labels, punctuation, or additional text.
        """;

    private readonly AzureOpenAIClient? _client;
    private readonly string _deploymentName;

    public OpenAIService()
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        _deploymentName = deploymentName?.Trim() ?? string.Empty;

        if (TryCreateClient(endpoint, _deploymentName, apiKey, out var client))
        {
            _client = client;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the Azure OpenAI client was successfully
    /// configured from environment variables.
    /// </summary>
    public bool IsConfigured => _client is not null;

    public async Task<double> GetSuggestedRatingAsync(string feedback, CancellationToken cancellationToken = default)
    {
        if (_client is null)
        {
            throw new InvalidOperationException(
                "Azure OpenAI is not configured. Set the AZURE_OPENAI_ENDPOINT, " +
                "AZURE_OPENAI_DEPLOYMENT_NAME, and AZURE_OPENAI_API_KEY environment variables.");
        }

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(feedback)
        };

        try
        {
            ChatCompletion response = await _client
                .GetChatClient(_deploymentName)
                .CompleteChatAsync(messages, cancellationToken: cancellationToken);

            string result = response.Content[0].Text.Trim();

            // Note: NumberStyles.Float deliberately excludes AllowThousands so that a
            // model response like "1,5" is not parsed as 15 (which would then clamp to 5).
            if (double.TryParse(result, NumberStyles.Float, CultureInfo.InvariantCulture, out double rating))
            {
                return Math.Clamp(rating, 0.0, 5.0);
            }

            throw new InvalidOperationException(
                $"AI returned a value that could not be parsed as a number: '{result}'.");
        }
        catch (OperationCanceledException)
        {
            // Debounce / superseded by a newer keystroke — propagate so the
            // caller can ignore the cancellation.
            throw;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException(
                $"Azure OpenAI request failed (HTTP {(int)ex.Status} {ex.Status}). {ex.Message}", ex);
        }
    }

    private static bool TryCreateClient(
        string? endpoint,
        string deploymentName,
        string? apiKey,
        out AzureOpenAIClient? client)
    {
        client = null;

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(deploymentName))
        {
            return false;
        }

        if (!Uri.TryCreate(endpoint.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        try
        {
            client = new AzureOpenAIClient(uri, new ApiKeyCredential(apiKey));
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AzureOpenAIClient construction failed: {ex.Message}");
            return false;
        }
    }
}