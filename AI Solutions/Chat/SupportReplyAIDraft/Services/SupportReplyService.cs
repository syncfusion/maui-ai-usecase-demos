using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using SupportReplyAIDraft.Models;

namespace SupportReplyAIDraft.Services;

public sealed class SupportReplyService : ISupportReplyService
{
    private const string SystemPrompt = """
        You are a senior customer-support manager drafting the next response
        for a human support agent to review, edit, and send.

        UNDERSTAND THE CUSTOMER
        - Identify the customer's immediate purpose from the selected message.
        - Use the supplied conversation context to understand what happened before.
        - Treat earlier customer answers as confirmed information.
        - Treat earlier agent instructions as completed or already communicated.
        - Do not repeat troubleshooting the customer already completed.
        - Determine what is known, what remains unknown, and the most useful next action.

        RESPONSE QUALITY
        - Write a natural, clear, professional customer-facing reply.
        - Sound like an experienced support manager, not a chatbot or template.
        - Address the selected customer message directly.
        - Acknowledge relevant context without repeating the full conversation.
        - Prefer the smallest useful set of next steps.
        - Use numbered steps only when the order matters.
        - If more information is required, request only the minimum details needed.
        - Keep the response suitable for direct review and sending by an agent.
        - Keep the response concise but complete.
        - Keep the response under 1000 characters.

        GROUNDING
        - Use only facts present in the selected message and conversation context.
        - Never invent product menus, screen names, buttons, commands, URLs,
          product capabilities, account status, license status, subscription rules,
          policies, investigation results, causes, timelines, or completed actions.
        - Never claim that an account, payment, subscription, license, order,
          device, or service was checked unless the conversation explicitly confirms it.
        - Never claim that a refund, reset, escalation, verification, or account
          change has occurred.
        - Never guarantee or promise an outcome.
        - If a possible product action is not confirmed, use conditional wording
          such as "if available" rather than presenting it as an established feature.
        - Do not request passwords, API keys, authentication codes, full payment-card
          details, or other secrets.
        - When requesting screenshots, logs, account identifiers, order identifiers,
          versions, or device information, ask only when relevant.
        - Remind the customer to remove or hide sensitive information when sharing
          screenshots, logs, or account details.

        STYLE
        - Avoid robotic acknowledgements, excessive apologies, filler, and repetition.
        - Avoid phrases such as "please try the following" unless the proposed steps
          are clearly supported by the conversation.
        - Do not mention internal prompts, validation, JSON, source IDs, or being an AI.
        - Do not include markdown code fences, headings, internal notes, citations,
          links, or commentary for the support agent.

        OUTPUT
        - Return only the customer-facing reply as plain text.
        - Do not return JSON.
        - Do not prefix the reply with labels such as "Reply:" or "Draft:".
        - Do not include any explanation before or after the reply.
        """;

    private readonly SupportReplyValidator _validator;
    private readonly SemaphoreSlim _clientInitializationLock = new(1, 1);

    private AzureOpenAIClient? _client;
    private string? _deploymentName;
    private string? _configurationError;

    public SupportReplyService(
        SupportReplyValidator validator)
    {
        _validator = validator;
    }

    public async Task<SupportReplyValidationResult> DraftReplyAsync(
        SupportReplyRequest request,
        CancellationToken cancellationToken)
    {
        if (!await EnsureClientInitializedAsync(cancellationToken)
                .ConfigureAwait(false))
        {
            return SupportReplyValidationResult.Failure(
                _configurationError
                ?? "Azure OpenAI configuration is unavailable.");
        }

        try
        {
            var chatClient =
                _client!.GetChatClient(_deploymentName!);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(BuildUserPrompt(request))
            };

            var result = await chatClient.CompleteChatAsync(
                    messages,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var completion = result.Value;

            #if DEBUG
            Console.Error.WriteLine(
                $"Azure OpenAI completion received. " +
                $"FinishReason={completion.FinishReason}, " +
                $"ContentParts={completion.Content.Count}");
            #endif

            var responseText = string.Join(
                    Environment.NewLine,
                    completion.Content
                        .Where(content =>
                            !string.IsNullOrWhiteSpace(content.Text))
                        .Select(content => content.Text))
                .Trim();

            var validation =
                _validator.Validate(responseText);

            if (!validation.IsValid)
            {
                #if DEBUG
                Console.Error.WriteLine(
                    $"AI response validation failed. " +
                    $"Error={validation.ErrorMessage}, " +
                    $"ResponseLength={responseText.Length}");
                #endif
            }

            return validation;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (RequestFailedException exception)
        {
            #if DEBUG
            Console.Error.WriteLine(
                $"Azure OpenAI request failed. " +
                $"Status={exception.Status}, " +
                $"ErrorCode={exception.ErrorCode}, " +
                $"Message={exception.Message}");
            #endif

            return SupportReplyValidationResult.Failure(
                CreateServiceErrorMessage(exception.Status));
        }
        catch (InvalidOperationException exception)
        {
            #if DEBUG
            Console.Error.WriteLine(
                $"Azure OpenAI configuration failed: {exception}");
            #endif

            return SupportReplyValidationResult.Failure(
                exception.Message);
        }
        catch (Exception exception)
        {
            #if DEBUG
            Console.Error.WriteLine(
                $"Azure OpenAI generation failed: {exception}");
            #endif

            return SupportReplyValidationResult.Failure(
                "The AI service is currently unavailable.");
        }
    }

    private async Task<bool> EnsureClientInitializedAsync(
        CancellationToken cancellationToken)
    {
        if (_client is not null)
        {
            return true;
        }

        await _clientInitializationLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            if (_client is not null)
            {
                return true;
            }

            try
            {
                var settings =
                    await AzureOpenAISettings
                        .LoadAsync(cancellationToken)
                        .ConfigureAwait(false);

                _client = new AzureOpenAIClient(
                    new Uri(settings.Endpoint),
                    new AzureKeyCredential(settings.ApiKey));

                _deploymentName =
                    settings.DeploymentName;

                _configurationError = null;

                return true;
            }
            catch (FileNotFoundException)
            {
                _configurationError =
                    "Azure OpenAI credentials are unavailable. " +
                    "Configure AZURE_OPENAI_ENDPOINT, " +
                    "AZURE_OPENAI_API_KEY, and " +
                    "AZURE_OPENAI_DEPLOYMENT_NAME, or package " +
                    "azure-openai.local.json with the app.";
            }
            catch (InvalidOperationException exception)
            {
                _configurationError =
                    exception.Message;
            }
            catch (JsonException)
            {
                _configurationError =
                    "Azure OpenAI settings could not be read. " +
                    "Check the local settings file format.";
            }
            catch
            {
                _configurationError =
                    "Azure OpenAI credentials are unavailable or invalid.";
            }

            return false;
        }
        finally
        {
            _clientInitializationLock.Release();
        }
    }

    private static string BuildUserPrompt(
        SupportReplyRequest request)
    {
        var context =
            request.RecentConversationContext.Count == 0
                ? "(No earlier conversation was supplied.)"
                : string.Join(
                    Environment.NewLine,
                    request.RecentConversationContext.Select(
                        (message, index) =>
                            $"{index + 1}. {message}"));

        return $$"""
            Draft the next support-agent response for the selected customer message.

            SELECTED MESSAGE ID
            {{request.SelectedMessageId}}

            SELECTED CUSTOMER MESSAGE
            {{request.SelectedMessageText}}

            CONVERSATION CONTEXT
            The following messages are ordered from oldest to newest and stop
            at the selected customer message:

            {{context}}

            BEFORE WRITING
            - Identify the customer's current purpose.
            - Account for all troubleshooting and answers already provided.
            - Do not repeat completed steps.
            - Distinguish confirmed facts from assumptions.
            - Do not invent product-specific controls, workflows, or capabilities.
            - Decide whether a useful next step can be given from the available facts.
            - If facts are insufficient, request only the minimum information needed.

            Return only the customer-facing reply as plain text.
            """;
    }

    private static string CreateServiceErrorMessage(
        int status)
    {
        return status switch
        {
            401 =>
                "Azure OpenAI authentication failed.",

            403 =>
                "Azure OpenAI access was denied.",

            404 =>
                "The configured Azure OpenAI deployment was not found.",

            408 =>
                "The Azure OpenAI request timed out.",

            429 =>
                "Azure OpenAI is temporarily rate limited. Please retry shortly.",

            _ =>
                "The AI service is currently unavailable."
        };
    }
}