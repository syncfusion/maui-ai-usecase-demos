using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using SupportReplyAIDraft.Models;

namespace SupportReplyAIDraft.Services;

public sealed class SupportReplyService : ISupportReplyService
{
    private readonly SupportReplyValidator _validator;
    private readonly SemaphoreSlim _clientInitializationLock = new(1, 1);
    private AzureOpenAIClient? _client;
    private string? _deploymentName;
    private string? _configurationError;

    public SupportReplyService(SupportReplyValidator validator)
    {
        _validator = validator;
    }

    public async Task<SupportReplyValidationResult> DraftReplyAsync(SupportReplyRequest request, CancellationToken cancellationToken)
    {
        if (!await EnsureClientInitializedAsync(cancellationToken).ConfigureAwait(false))
        {
            return SupportReplyValidationResult.Failure(_configurationError ?? "Azure OpenAI configuration is unavailable.");
        }

        try
        {
            var chatClient = _client!.GetChatClient(_deploymentName!);
            var systemPrompt = "You are a customer-support drafting assistant. Return exactly one JSON object with fields reply, requiresMoreInformation, requestedInformation, and warnings. Do not include markdown fences, comments, or additional keys. Do not include sourceIds, links, account-action claims, or verification claims. Keep reply under 1000 characters. Use only the selected customer message and supplied conversation context. Give a concise, accurate, professional, step-by-step support reply when steps are appropriate; do not invent account facts or promise an outcome. The reply must be customer-facing and ready to review/edit, but never sent automatically.";
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

    private async Task<bool> EnsureClientInitializedAsync(CancellationToken cancellationToken)
    {
        if (_client is not null)
        {
            return true;
        }

        await _clientInitializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_client is not null)
            {
                return true;
            }

            try
            {
                var settings = await AzureOpenAISettings.LoadAsync(cancellationToken).ConfigureAwait(false);
                _client = new AzureOpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
                _deploymentName = settings.DeploymentName;
                _configurationError = null;
                return true;
            }
            catch (FileNotFoundException)
            {
                _configurationError = "Azure OpenAI credentials are unavailable. Configure AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, and AZURE_OPENAI_DEPLOYMENT_NAME, or package azure-openai.local.json with the app.";
            }
            catch (InvalidOperationException exception)
            {
                _configurationError = exception.Message;
            }
            catch (JsonException)
            {
                _configurationError = "Azure OpenAI settings could not be read. Check the local settings file format.";
            }
            catch (Exception)
            {
                _configurationError = "Azure OpenAI credentials are unavailable or invalid.";
            }

            return false;
        }
        finally
        {
            _clientInitializationLock.Release();
        }
    }

    private static string BuildUserPrompt(SupportReplyRequest request)
    {
        var context = string.Join('\n', request.RecentConversationContext.Select(item => $"- {item}"));
        return $"Selected message id: {request.SelectedMessageId}\nSelected message:\n{request.SelectedMessageText}\n\nRecent conversation context (bounded to messages up to and including the selected message):\n{context}\n\nGenerate one concise, professional, grounded support reply for the selected message only. Include an ordered sequence of practical next steps when appropriate. If more information is needed, set requiresMoreInformation=true and include only the missing items in requestedInformation.";
    }
}
