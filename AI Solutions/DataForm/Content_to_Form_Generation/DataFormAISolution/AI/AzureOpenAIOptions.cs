using Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Chat;
using System.Diagnostics;
using System.Net;

namespace DataFormAISolution;

/// <summary>
/// Stores Azure OpenAI configuration and validates credentials.
/// </summary>
public class AzureOpenAIOptions
{
	/// <summary>
	/// Gets the default Azure OpenAI endpoint.
	/// </summary>
	public const string Endpoint = "AZURE_OPENAI_ENDPOINT";

	/// <summary>
	/// Gets the default Azure OpenAI deployment name.
	/// </summary>
	public const string DeploymentName = "AZURE_OPENAI_DEPLOYMENT";

	/// <summary>
	/// Gets the default Azure OpenAI API key.
	/// </summary>
	public const string Key = "AZURE_OPENAI_KEY";

	/// <summary>
	/// Gets the resolved Azure OpenAI endpoint.
	/// </summary>
	public string EndpointValue { get; }

	/// <summary>
	/// Gets the resolved Azure OpenAI deployment name.
	/// </summary>
	public string DeploymentNameValue { get; }

	/// <summary>
	/// Gets the resolved Azure OpenAI API key.
	/// </summary>
	public string KeyValue { get; }


	/// <summary>
	/// Initializes a new instance of the <see cref="AzureOpenAIOptions"/> class.
	/// </summary>
	public AzureOpenAIOptions()
	{
		EndpointValue =
			(Environment.GetEnvironmentVariable(
				"AZURE_OPENAI_ENDPOINT")
			 ?? Endpoint).Trim();

		DeploymentNameValue =
			(Environment.GetEnvironmentVariable(
				"AZURE_OPENAI_DEPLOYMENT")
			 ?? DeploymentName).Trim();

		KeyValue =
			(Environment.GetEnvironmentVariable(
				"AZURE_OPENAI_KEY")
			 ?? Key).Trim();
	}


	/// <summary>
	/// Validates the configured Azure OpenAI credentials.
	/// </summary>
	/// <returns>A validation error message, or <c>null</c> when valid.</returns>
	public string? ValidateCredentials()
	{
		if (string.IsNullOrWhiteSpace(EndpointValue))
		{
			return "Azure OpenAI endpoint is missing.";
		}

		if (string.IsNullOrWhiteSpace(DeploymentNameValue))
		{
			return "Azure OpenAI deployment name is missing.";
		}

		if (string.IsNullOrWhiteSpace(KeyValue))
		{
			return "Azure OpenAI API key is missing.";
		}

		if (!Uri.TryCreate(EndpointValue,UriKind.Absolute,out var uri) || uri.Scheme != Uri.UriSchemeHttps)
		{
			return "Azure OpenAI endpoint is invalid.";
		}

		return null;
	}
}