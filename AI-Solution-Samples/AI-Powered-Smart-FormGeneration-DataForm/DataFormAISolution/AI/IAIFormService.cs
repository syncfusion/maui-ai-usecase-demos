using System;
using System.Collections.Generic;
using System.Text;

namespace DataFormAISolution
{
	/// <summary>
	/// Defines the contract for generating form data from free-text input.
	/// </summary>
	public interface IAIFormService
	{
		/// <summary>
		/// Generates a form definition for the specified user input.
		/// </summary>
		/// <param name="userInput">The free-text content to analyze.</param>
		/// <param name="cancellationToken">A cancellation token.</param>
		/// <returns>The generated form response, or <c>null</c> when no fields are produced.</returns>
		Task<AIFormResponse?> GenerateFormAsync(
		string userInput,
		CancellationToken cancellationToken = default);
	}
}
