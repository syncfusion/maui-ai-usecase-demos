using DataFormAISolution.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataFormAISolution.AI
{
	public interface IAIFormService
	{
		Task<AIFormResponse?> GenerateFormAsync(
		string userInput,
		CancellationToken cancellationToken = default);
	}
}
