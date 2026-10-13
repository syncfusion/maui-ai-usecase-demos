using System.ClientModel;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using DataForm_FormGenerator.Configuration;
using OpenAI.Chat;

namespace DataForm_FormGenerator;

/// <summary>
/// Schema generator backed by Azure OpenAI.
/// </summary>
public sealed class AzureOpenAiSchemaService : IAiSchemaService
{
    private const string PromptSystem = """
            You are a form schema generator. Return ONLY valid JSON that matches this shape:
            { "title": string, "fields": [ { "name": string, "label": string,
              "type": "string"|"number"|"decimal"|"boolean"|"date"|"dropdown"|"combo"|"multiline",
              "required": bool, "min": number?, "max": number?, "minLength": int?,
              "maxLength": int?, "defaultValue": string?, "options": [string]?,
              "multiline": bool? } ] }
            Do not include Markdown, prose, or code fences.
            """;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AzureOpenAiOptions options;
    private ChatClient? chat;
    internal (bool Ok, string? Error)? LastCredentialStatus;

    public AzureOpenAiSchemaService(AzureOpenAiOptions options)
    {
        this.options = options;
    }

    public async Task<GenerationResult> GenerateAsync(string educationType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(educationType);
        await GetResultsFromAI(cancellationToken).ConfigureAwait(false);

        var start = DateTimeOffset.UtcNow;
        var user = $"Create an education form schema for: {educationType}. " +
                   "Cover institution, program, status, dates, and any metrics relevant to this level of education." +
                   $" Match the canonical field set expected for \"{educationType}\" (e.g. High School requires SAT/ACT scores" +
                   " and State; Bachelor's Degree requires university, major, GPA, graduation year, and country; etc.)." +
                   " Stick to JSON only — no prose, no code fences.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(PromptSystem),
            new UserChatMessage(user)
        };

        var chatOptions = new ChatCompletionOptions
        {
            Temperature = 0f,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            MaxOutputTokenCount = 4096
        };

        ClientResult<ChatCompletion> result;
        try
        {
            result = await this.chat!.CompleteChatAsync(messages, chatOptions, cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException($"Invalid API key details ({ex.Status}: {ex.ErrorCode}). {ex.Message}",ex);
        }
        catch (ClientResultException ex)
        {
            throw new InvalidOperationException($"Invalid API key details ({ex.Status}). {ex.Message}",ex);
        }

        var rawJson = string.Concat(
            result.Value.Content.Select(part => part.Text ?? string.Empty));

        var trimmed = rawJson.TrimStart();
        if (trimmed.Length == 0 || trimmed[0] != '{')
        {
            throw new InvalidOperationException(
                "Azure OpenAI returned a non-JSON response. " +
                "Verify the Endpoint URL points at the resource's chat-completion surface. " +
                "First chars: " + rawJson[..Math.Min(120, rawJson.Length)]);
        }

        var schema = JsonSerializer.Deserialize<EducationFormSchema>(rawJson, SerializerOptions)
                     ?? throw new InvalidOperationException("Model returned a null schema.");

        if (string.IsNullOrWhiteSpace(schema.Title))
        {
            schema.Title = $"{educationType.Trim()} Education";
        }

        if (schema.Fields.Count == 0)
        {
            throw new InvalidOperationException("Azure OpenAI returned an empty field set. Please retry — if this persists, tighten the system prompt.");
        }

        return new GenerationResult
        {
            Schema = schema,
            Json = rawJson,
            Duration = DateTimeOffset.UtcNow - start
        };
    }

    /// <summary>
    /// Probes the configured Azure OpenAI deployment with a tiny prompt.
    /// </summary>
    public async Task GetResultsFromAI(CancellationToken cancellationToken = default)
    {
        var validationError = options.ValidationError;
        if (validationError is not null)
        {
            LastCredentialStatus = (false, validationError);
            throw new InvalidOperationException($"Invalid API key details. {validationError}");
        }

        try
        {
            if (this.chat is null)
            {
                var client = new AzureOpenAIClient(
                    new Uri(options.Endpoint),
                    new AzureKeyCredential(options.ApiKey));
                this.chat = client.GetChatClient(options.DeploymentName);
            }
        }
        catch (Exception ex)
        {
            LastCredentialStatus = (false, ex.Message);
            throw new InvalidOperationException($"Invalid API key details. Could not initialise the Azure OpenAI client: {ex.Message}", ex);
        }

        try
        {
            var probe = new ChatCompletionOptions
            {
                Temperature = 0f,
                MaxOutputTokenCount = 1,
            };
            await this.chat.CompleteChatAsync(new List<ChatMessage> { new UserChatMessage("ping") },probe,cancellationToken).ConfigureAwait(false);

            LastCredentialStatus = (true, null);
        }
        catch (RequestFailedException ex)
        {
            LastCredentialStatus = (false, $"HTTP {ex.Status} ({ex.ErrorCode}) {ex.Message}");
            throw new InvalidOperationException($"Invalid API key details. Azure OpenAI returned HTTP {ex.Status} ({ex.ErrorCode}). {ex.Message}",ex);
        }
        catch (ClientResultException ex)
        {
            LastCredentialStatus = (false, $"HTTP {ex.Status} {ex.Message}");
            throw new InvalidOperationException($"Invalid API key details. Azure OpenAI returned HTTP {ex.Status}. {ex.Message}", ex);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }
}