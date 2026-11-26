# AI Chat Sample

![AI Chat Sample demo](AIChatSample.gif)

A .NET MAUI multi-agent chat sample built with Syncfusion controls. It demonstrates agent creation and configuration, chat navigation, follow-up suggestions, voice input (native on Windows only), and switching between a local mock AI service and Azure OpenAI.

## Overview

The sample presents a chat workspace with recent conversations, search, library, profile, and agent configuration areas. Agents have names, descriptions, instructions, colors, initials, and optional avatar images. Agent instructions are sent to Azure OpenAI as the system message for chat requests.

## AI Capabilities

- Multi-agent conversational chat.
- Per-agent system instructions.
- Follow-up suggestions and contextual chat responses.
- Agent creation and branding.
- Local mock responses when Azure OpenAI is not configured.

## Key Features

- Create and configure agents.
- Start and switch between chat sessions.
- Search conversations and browse the library.
- Customize agent avatars and profile data.
- Use voice input: native speech recognition on Windows; other platforms show a text prompt fallback instead of a microphone.

## Application Workflow

1. `MauiProgram` initializes in-memory agent, chat, and profile services.
2. The user selects or creates an agent.
3. The user enters a prompt or selects a suggested action.
4. `AIMainLayoutViewModel`/chat view models add the request to the active session.
5. `IAIService` sends the prompt through Azure OpenAI or `MockAIService`.
6. The response is parsed into answer and follow-up suggestion data.
7. The chat UI updates the active session and navigation state.

## AI Workflow

```text
User Prompt
    ↓
Active Agent Configuration
    ↓
IAIService
    ↓
AzureOpenAIService or MockAIService
    ↓
ContextSuggestionResponse
    ↓
Chat UI and Follow-up Suggestions
```

## AI Architecture

- **Views:** `Views/` contains chat, agent, library, profile, search, and navigation pages.
- **ViewModels:** `ViewModel/` contains chat layout, agent configuration, suggestions, and library logic.
- **Models:** `Models/` contains agent, chat session, navigation, profile, and library data.
- **Services:** `Services/` contains in-memory agent, chat, and helper services.
- **AI:** `AIService/` contains `IAIService`, Azure and mock implementations, and response data.

Data is kept in memory. Restarting the app resets agents, profiles, and chat sessions.

## Syncfusion Controls Used

- `SfAIAssistView` for chat interactions.
- `SfListView` for agent and navigation collections.
- `SfExpander` for configuration sections.
- `SfPopup` for branding and editor dialogs.
- `SfChipGroup` for configuration selections.
- `SfAvatarView` for agent avatars.
- `SfButton` for actions.
- `SfTextInputLayout` for search and agent form fields.

## Prerequisites

- .NET 10 SDK.
- A .NET MAUI workload for the target platform.
- The Syncfusion and Azure NuGet packages referenced by `AIChatSample.csproj`.
- Azure OpenAI credentials only for live AI responses.

## Configuration

### AI Configuration

Azure OpenAI is selected only when all required values are valid HTTPS environment variables. Otherwise `MockAIService` is registered.

| Variable | Required | Purpose |
| --- | --- | --- |
| `AZURE_OPENAI_ENDPOINT` | Azure mode | HTTPS Azure OpenAI endpoint |
| `AZURE_OPENAI_KEY` | Azure mode | Azure OpenAI API key |
| `AZURE_OPENAI_DEPLOYMENT` | Azure mode | Chat deployment name |
| `AZURE_OPENAI_TIMEOUT_SECONDS` | No | Request timeout; defaults to 100 seconds |

Do not commit API keys to source control.

### Supported Target Frameworks

- `net10.0-android`
- `net10.0-ios` and `net10.0-maccatalyst` (not available when building on Linux)
- `net10.0-windows10.0.19041.0` (Windows hosts only)

## Getting Started

Run from `Use Case Samples/AI Chat Sample/AIChatSample` (the folder containing `AIChatSample.slnx`):

```sh
dotnet restore AIChatSample/AIChatSample.csproj

dotnet build AIChatSample/AIChatSample.csproj -t:Run -f net10.0-android
dotnet build AIChatSample/AIChatSample.csproj -t:Run -f net10.0-ios
dotnet build AIChatSample/AIChatSample.csproj -t:Run -f net10.0-maccatalyst
dotnet build AIChatSample/AIChatSample.csproj -t:Run -f net10.0-windows10.0.19041.0
```

Set the Azure environment variables before starting the app to use live Azure responses.

## Project Structure

```text
AIChatSample/
└── AIChatSample/
    ├── AIService/
    ├── Controls/
    ├── Converters/
    ├── Models/
    ├── Platforms/
    ├── Resources/
    ├── Services/
    ├── Styles/
    ├── Themes/
    ├── ViewModel/
    ├── Views/
    ├── App.xaml
    ├── AppShell.xaml
    └── MauiProgram.cs
```

## Troubleshooting

- **Mock service is used:** Verify the endpoint, key, and deployment environment variables.
- **Azure endpoint is rejected:** The endpoint must be an absolute HTTPS URL.
- **Chat state resets:** Agent and chat state is intentionally in memory.
- **Path Too Long on Windows:** Use a shorter repository path.

## Responsible AI Considerations

- Agent instructions and user prompts are sent to Azure OpenAI in live mode.
- Do not include sensitive or confidential information in prompts.
- Review Azure OpenAI deployment and data-handling settings for production use.

## Related Documentation

- [Syncfusion AIAssistView for .NET MAUI](https://help.syncfusion.com/maui/aiassistview/getting-started)
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)

## Supported Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## License

This sample is provided for demonstration purposes. Syncfusion controls require a valid Syncfusion license for production use.
