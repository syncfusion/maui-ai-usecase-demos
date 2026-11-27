using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerAISample
{
	/// <summary>
	/// Provides Azure OpenAI configuration settings and validation for endpoint, deployment name, and API key values.
	/// </summary>
	public class AzureOpenAIOptions
	{
        /// <summary>
        /// Gets the default Azure OpenAI endpoint environment variable name.
        /// </summary>
        public static readonly string Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
        
		/// <summary>
		/// Gets the default Azure OpenAI API key environment variable name.
		/// </summary>
		public static readonly string Key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
        
		/// <summary>
		/// Gets the default Azure OpenAI deployment name environment variable name.
		/// </summary>
		public static readonly string DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? string.Empty;

        /// <summary>
        /// Gets the Azure OpenAI endpoint value.
        /// </summary>
        public string EndpointValue { get; }

		/// <summary>
		/// Gets the Azure OpenAI deployment name value.
		/// </summary>
		public string DeploymentNameValue { get; }

		/// <summary>
		/// Gets the Azure OpenAI API key value.
		/// </summary>
		public string KeyValue { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AzureOpenAIOptions"/> class and loads the Azure OpenAI configuration values from environment variables.
		/// </summary>
		public AzureOpenAIOptions()
		{
			EndpointValue = (Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? Endpoint).Trim();
			DeploymentNameValue = (Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? DeploymentName).Trim();
			KeyValue = (Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? Key).Trim();
		}

		/// <summary>
		/// Validates the Azure OpenAI configuration values and returns a validation message when a required value is missing.
		/// </summary>
		/// <returns>
		/// An error message if validation fails; otherwise, <see langword="null"/>.
		/// </returns>
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

			return null;
		}
	}
}
