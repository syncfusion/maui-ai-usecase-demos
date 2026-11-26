# Smart Daily Decision Companion

A responsive .NET MAUI task-prioritization sample that sends task context to Azure OpenAI and separates tasks into Do Today, Postpone, and Watch groups.

## Overview

`MainViewModel` analyzes the full task list and individual tasks for pressure, focus, urgency, importance, momentum, delay risk, mentorship, and recommendation. It provides accept/postpone actions and ignores stale async insight responses.

## AI Capabilities

- Whole-list pressure and focus analysis.
- Per-task mentorship, urgency, importance, momentum, and delay-risk analysis.
- Recommendation of a task for today.
- Stale-response protection during task selection changes.

## Key Features

- Add tasks with topic, description, deadline, project type, complexity, and priority.
- Categorize tasks across Do Today, Postpone, and Watch.
- Analyze the selected task or the full list.
- Apply accept/postpone recommendation actions.
- Use shared task templates across mobile and desktop layouts.

## Application Workflow

1. `MainPage.OnAppearing` initializes `MainViewModel` and validates credentials.
2. The user adds or selects a task.
3. `MainViewModel` builds the relevant prompt.
4. `AzureAIService` sends the request to Azure OpenAI.
5. Text or JSON responses are parsed into UI properties.
6. Accept/Postpone moves the task and refreshes the Today view.

## AI Workflow

```text
Task State
    ↓
MainViewModel Prompt Builder
    ↓
AzureBaseService / HttpClient
    ↓
Azure OpenAI Chat Completions
    ↓
Parsed Text or JSON
    ↓
Insight Cards and Actions
```

## AI Architecture

- **View:** `MainPage.xaml`.
- **ViewModel:** `MainViewModel`.
- **Model:** `TaskItem`.
- **Services:** `IAzureAIService`, `AzureBaseService`, and `AzureAIService`.
- **Startup:** `MauiProgram.cs` registers the AI service and page.

## Syncfusion Controls Used

- Syncfusion packages referenced by the project for `SfCore`, `SfListView`, and `SfTabView`.

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workloads for target platforms.
- The Syncfusion packages referenced by `SmartDailyDecisionCompanion.csproj`.
- Azure OpenAI endpoint, deployment, and API key.

## Configuration

### AI Configuration

Credentials are constants in `Services/AzureBaseService.cs`:

```csharp
private const string endpoint = "Enter your Azure OpenAI endpoint here";
private const string deploymentName = "Enter your Azure OpenAI deployment name here";
private const string key = "Enter your Azure OpenAI API key here";
private const string apiVersion = "2024-02-15-preview";
```

Replace them before use. `ValidateCredentialAsync` sends a lightweight Azure chat request and caches the result.

## Getting Started

Run from `Use Case Samples/Smart Decision Companion/SmartDailyDecisionCompanion`:

```sh
dotnet restore SmartDailyDecisionCompanion/SmartDailyDecisionCompanion.csproj

dotnet build SmartDailyDecisionCompanion/SmartDailyDecisionCompanion.csproj -t:Run -f net10.0-android
dotnet build SmartDailyDecisionCompanion/SmartDailyDecisionCompanion.csproj -t:Run -f net10.0-ios
dotnet build SmartDailyDecisionCompanion/SmartDailyDecisionCompanion.csproj -t:Run -f net10.0-maccatalyst
dotnet build SmartDailyDecisionCompanion/SmartDailyDecisionCompanion.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Project Structure

```text
SmartDailyDecisionCompanion/
├── Models/
├── Platforms/
├── Resources/
├── Services/
├── ViewModels/
├── App.xaml
├── AppShell.xaml
├── MainPage.xaml
└── MauiProgram.cs
```

## Troubleshooting

- **Credential alert appears:** Replace all placeholder constants.
- **Insights remain loading:** Verify network access and Azure configuration.
- **Stale insight is ignored:** The app intentionally protects the current task selection.
- **License watermark:** Register a valid Syncfusion license.

## Responsible AI Considerations

- Task content is sent to Azure OpenAI for live analysis.
- Avoid entering confidential task or project information.
- Treat AI recommendations as decision support, not authoritative instructions.

## Related Documentation

- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Syncfusion .NET MAUI Controls](https://help.syncfusion.com/maui/introduction/overview)

## Supported Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## Limitations

- Tasks are not persisted by a database in the current sample.
- Live AI requires valid credential constants.

## License

Syncfusion controls require a valid Syncfusion license for production use.
