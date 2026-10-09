using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Text.Json;

namespace DataFormAISolution;

/// <summary>
/// Uses Azure OpenAI to turn free-text content into structured form data.
/// </summary>
public class AzureFormAIService : IAIFormService
{
	/// <summary>
	/// Creates a new instance of the <see cref="AzureFormAIService"/> class.
	/// </summary>
	private readonly AzureOpenAiOptions _options;

	private ChatClient? _chatClient;


	private static readonly JsonSerializerOptions
		SerializerOptions =
			new()
			{
				PropertyNameCaseInsensitive = true
			};


	/// <summary>
	/// Initializes a new instance of the <see cref="AzureFormAIService"/> class.
	/// </summary>
	public AzureFormAIService()
	{
		_options =
			new AzureOpenAiOptions();
	}


	/// <summary>
	/// Generates a structured form response from the supplied text.
	/// </summary>
	/// <param name="userInput">The free-text content to analyze.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A structured response describing the generated form.</returns>
	public async Task<AIFormResponse?>
		GenerateFormAsync(
			string userInput,
			CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(
			userInput))
		{
			return null;
		}


		string? validationError =
			_options.ValidateCredentials();


		if (!string.IsNullOrWhiteSpace(
			validationError))
		{
			throw new InvalidOperationException(
				validationError);
		}


		try
		{
			// Create client only once

			EnsureChatClient();


			// System prompt

			string systemPrompt =
				"""
                You are a strict Dynamic Form Generation Engine.

                Analyze the user's free-text content and extract
                meaningful fields and their corresponding values.

                Return ONLY valid JSON.

                Do not return:
                - Markdown
                - Code fences
                - Explanations
                - Comments
                - Additional text

                Allowed FieldType values:

                Text
                Number
                Email
                Phone

                Field rules:

                - Person names => Text
                - Gender => Text
                - Address => Text
                - Department => Text
                - Designation => Text
                - Email addresses => Email
                - Phone and mobile numbers => Phone
                - Age => Number
                - Percentage => Number
                - Marks => Number
                - Quantity => Number

                Do not invent information that does not exist
                in the user's input.

                Extract a value ONLY when the user actually provides
                a value for the field.

                NEVER use the field name itself as the field value.

                If the user mentions a field but does not provide
                a value, return an empty string for Value.

                Create human-readable field names.

                Examples:

                Ph => Phone Number
                Mob => Mobile Number
                Exp => Experience
                Dept => Department
                SSLC => SSLC Percentage
                HSC => HSC Percentage

                For Number fields, Value must contain only
                the numeric component.

                Correct:

                {
                    "FieldName": "Age",
                    "Value": "34",
                    "FieldType": "Number"
                }

                Incorrect:

                {
                    "FieldName": "Age",
                    "Value": "34 Years",
                    "FieldType": "Number"
                }

                Expected response format:

                {
                    "Fields":
                    [
                        {
                            "FieldName": "Name",
                            "Value": "Karthi",
                            "FieldType": "Text"
                        },
                        {
                            "FieldName": "Age",
                            "Value": "34",
                            "FieldType": "Number"
                        }
                    ]
                }
                """;


			// User prompt

			string userPrompt =
				$$"""
                Analyze the following user content and
                generate the form definition.

                User Content:

                {{userInput}}
                """;


			// Chat messages

			List<ChatMessage> messages =
			[
				new SystemChatMessage(
					systemPrompt),

				new UserChatMessage(
					userPrompt)
			];


			// Chat options

			var chatOptions = new ChatCompletionOptions
			{
				ResponseFormat =
		ChatResponseFormat.CreateJsonObjectFormat()
			};


			// Azure OpenAI request

			ClientResult<ChatCompletion> result =
	await _chatClient!
		.CompleteChatAsync(
			messages,
			chatOptions,
			cancellationToken);

			ChatCompletion completion =
				result.Value;

			if (completion.Content == null ||
				completion.Content.Count == 0)
			{
				throw new InvalidOperationException(
					"Azure OpenAI returned no content.");
			}

			string rawJson =
				string.Concat(
					completion.Content
						.Select(part => part.Text ?? string.Empty));


			if (string.IsNullOrWhiteSpace(
				rawJson))
			{
				return null;
			}


			rawJson =
				CleanJson(rawJson);


			AIFormResponse? response =
				JsonSerializer.Deserialize<
					AIFormResponse>(
						rawJson,
						SerializerOptions);


			if (response == null)
			{
				Debug.WriteLine(
					"AIFormResponse is null.");

				return null;
			}


			if (response.Fields == null ||
				response.Fields.Count == 0)
			{
				Debug.WriteLine(
					"Azure AI returned no form fields.");

				return null;
			}

			foreach (AIField field
				in response.Fields)
			{
				Debug.WriteLine(
					$"{field.FieldName} | " +
					$"{field.Value} | " +
					$"{field.FieldType}");
			}


			return response;
		}
		catch (RequestFailedException ex)
		{
			Debug.WriteLine(
				$"Azure request failed: " +
				$"{ex.Status} | " +
				$"{ex.ErrorCode} | " +
				$"{ex.Message}");

			throw new InvalidOperationException(
				$"Azure OpenAI request failed " +
				$"({ex.Status}: {ex.ErrorCode}). " +
				ex.Message,
				ex);
		}
		catch (ClientResultException ex)
		{
			Debug.WriteLine(
				$"OpenAI client request failed: " +
				$"{ex.Status} | {ex.Message}");

			throw new InvalidOperationException(
				$"Azure OpenAI request failed " +
				$"({ex.Status}). {ex.Message}",
				ex);
		}
		catch (OperationCanceledException)
		{
			Debug.WriteLine(
				"Azure OpenAI request cancelled.");

			throw;
		}
		catch (JsonException ex)
		{
			Debug.WriteLine(
				$"Invalid JSON returned by Azure AI: " +
				$"{ex.Message}");

			throw new InvalidOperationException(
				"Azure AI returned an invalid form response.",
				ex);
		}
		catch (Exception ex)
		{
			Debug.WriteLine(
				$"AzureFormAIService Error: {ex}");

			throw;
		}
	}

	// CREATE AZURE OPENAI CLIENT

	private void EnsureChatClient()
	{
		if (_chatClient != null)
		{
			return;
		}


		AzureOpenAIClient client =
			new(
				new Uri(
					_options.EndpointValue),

				new AzureKeyCredential(
					_options.KeyValue));


		_chatClient =
			client.GetChatClient(
				_options.DeploymentNameValue);
	}

	// CLEAN JSON

	private static string CleanJson(
		string content)
	{
		content =
			content.Trim();


		if (content.StartsWith(
			"```json",
			StringComparison.OrdinalIgnoreCase))
		{
			content =
				content[7..];
		}


		if (content.StartsWith(
			"```",
			StringComparison.Ordinal))
		{
			content =
				content[3..];
		}


		if (content.EndsWith(
			"```",
			StringComparison.Ordinal))
		{
			content =
				content[..^3];
		}


		return content.Trim();
	}
}