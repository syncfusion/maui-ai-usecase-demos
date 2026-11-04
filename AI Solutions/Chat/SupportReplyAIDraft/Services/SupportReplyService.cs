using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using OpenAI.Chat;
using SupportReplyAIDraft.Models;

namespace SupportReplyAIDraft.Services;

public sealed class SupportReplyService : ISupportReplyService
{
    private readonly SupportReplyValidator _validator;
    private readonly AzureOpenAIClient? _client;
    private readonly string? _deploymentName;

    public SupportReplyService(SupportReplyValidator validator)
    {
        _validator = validator;
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        _deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(_deploymentName)
            || !Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri)
            || endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            return;
        }

        _client = new AzureOpenAIClient(endpointUri, new AzureKeyCredential(apiKey));
    }

    public async Task<SupportReplyValidationResult> DraftReplyAsync(SupportReplyRequest request, CancellationToken cancellationToken)
    {
        if (_client is null || string.IsNullOrWhiteSpace(_deploymentName))
        {
            return SupportReplyValidationResult.Failure("Azure OpenAI configuration is unavailable.");
        }

        try
        {
            var chatClient = _client.GetChatClient(_deploymentName);

            var systemPrompt = "You are a customer-support drafting assistant. Return a single JSON object with fields reply, requiresMoreInformation, requestedInformation, and warnings. Do not include sourceIds. Do not include markdown fences. Do not include URLs. Keep the reply under 1000 characters. Do not send messages.";
            var userPrompt = BuildUserPrompt(request);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var completion = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken).ConfigureAwait(false);
            var text = completion.Content.Count > 0 ? completion.Content[0].Text : string.Empty;
            return _validator.Validate(text);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return SupportReplyValidationResult.Failure("The AI service is currently unavailable.");
        }
    }

    private static string BuildUserPrompt(SupportReplyRequest request)
    {
        var context = string.Join('\n', request.RecentConversationContext.Select(item => $"- {item}"));
        return $"Selected message:\n{request.SelectedMessageText}\n\nRecent conversation context:\n{context}\n\nWrite a concise, professional support reply that addresses the selected message. If more information is needed, request only the relevant missing details.";
    }
}
