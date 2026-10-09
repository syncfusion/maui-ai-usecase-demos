using Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Chat;
using System.Diagnostics;
using System.Net;

namespace DataFormAISolution.AI;

public class AzureOpenAiOptions
{
	/// <summary>
	/// The EndPoint
	/// </summary>
	public const string Endpoint = "https://YOUR-RESOURCE.openai.azure.com/";

	/// <summary>
	/// The Deployment name
	/// </summary>
	public const string DeploymentName = "YOUR-DEPLOYMENT-NAME";

	/// <summary>
	/// The API key
	/// </summary>
	public const string Key = "YOUR-AZURE-OPENAI-KEY";

	public string EndpointValue { get; }

	public string DeploymentNameValue { get; }

	public string KeyValue { get; }


	public AzureOpenAiOptions()
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


	public string? ValidateCredentials()
	{
		if (string.IsNullOrWhiteSpace(
				EndpointValue))
		{
			return "Azure OpenAI endpoint is missing.";
		}

		if (string.IsNullOrWhiteSpace(
				DeploymentNameValue))
		{
			return "Azure OpenAI deployment name is missing.";
		}

		if (string.IsNullOrWhiteSpace(
				KeyValue))
		{
			return "Azure OpenAI API key is missing.";
		}

		if (!Uri.TryCreate(
				EndpointValue,
				UriKind.Absolute,
				out var uri) ||
			uri.Scheme != Uri.UriSchemeHttps)
		{
			return "Azure OpenAI endpoint is invalid.";
		}

		return null;
	}
}