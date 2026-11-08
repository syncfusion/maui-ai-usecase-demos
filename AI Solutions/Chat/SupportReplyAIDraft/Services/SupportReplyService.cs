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

            var systemPrompt = "You are a customer-support drafting assistant. Return exactly one JSON object with fields reply, requiresMoreInformation, requestedInformation, and warnings. Do not include markdown fences, comments, or additional keys. Do not include sourceIds, links, account-action claims, or verification claims. Keep reply under 1000 characters. The reply must be customer-facing and ready to review/edit, but never sent automatically.";
            var userPrompt = BuildUserPrompt(request);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var result = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken).ConfigureAwait(false);
            var completion = result.Value;
            var text = completion.Content.Count > 0 && !string.IsNullOrWhiteSpace(completion.Content[0].Text)
                ? completion.Content[0].Text
                : string.Empty;
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
        return $"Selected message id: {request.SelectedMessageId}\nSelected message:\n{request.SelectedMessageText}\n\nRecent conversation context (bounded to messages up to and including the selected message):\n{context}\n\nGenerate one concise, professional support reply for the selected message only. If more information is needed, set requiresMoreInformation=true and include only the missing items in requestedInformation.";
    }
}
