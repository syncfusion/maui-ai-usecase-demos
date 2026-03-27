using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RichTextEditorAssistViewSample
{
    public interface IAzureAIService
    {
        Task<string> GetResultsFromAI(string userAIPrompt);
    }
}
