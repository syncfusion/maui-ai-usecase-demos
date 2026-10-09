# AI-Powered Context-Aware Suggestions in .NET MAUI AI AssistView

This sample demonstrates how to build an intelligent conversational experience using the Syncfusion® .NET MAUI AI AssistView (`SfAIAssistView`) control and Azure OpenAI.

The application generates AI responses and dynamically provides context-aware suggestions based on the current conversation. These suggestions help users continue learning or explore related topics without manually typing additional prompts.

## Features

- Azure OpenAI integration
- AI-generated responses
- Context-aware follow-up suggestions
- Built-in response actions (Like, Dislike, and Copy)
- Initial prompt suggestions
- MVVM architecture
- .NET MAUI implementation using Syncfusion AI AssistView

## How It Works

1. The user submits a prompt through the AI AssistView.
2. The prompt is sent to Azure OpenAI.
3. Azure OpenAI returns a structured JSON response containing:
   - AI-generated answer
   - Four context-aware suggestions
4. The response is displayed in the AI AssistView.
5. Suggested follow-up prompts are shown below the response, allowing users to continue the conversation with a single tap.

## Sample Use Case

For a prompt such as:

```text
Python Roadmap
```

## Demo

The following video demonstrates AI-generated responses with context-aware follow-up suggestions.

![.NET MAUI AIAssistView With Context Aware Suggestions.](/AIAssistView-Context-Aware-Suggestions/ContextAwareSuggestions/Resources/Context_Aware_Suggestions.gif)