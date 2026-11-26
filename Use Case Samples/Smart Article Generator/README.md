# Smart Article Generator

![Smart Article Generator demo](SmartArticleGenerator.gif)

A .NET MAUI article workspace that combines Syncfusion `AIAssistView` and `MarkdownViewer`, Microsoft Semantic Kernel, and Azure OpenAI.

## Overview

The app accepts a topic, generates article content, renders the response, extracts resource links, and maintains a resource collection for the workspace.

## AI Capabilities

- Azure OpenAI article generation.
- Markdown and HTML response rendering.
- Automatic resource-link extraction.
- Seeded offline responses for known prompts.

## Key Features

- Submit a custom topic or select a suggested topic.
- Render articles in `SfMarkdownViewer`.
- Extract `[Title](URL)` and HTML links.
- Add, delete, and open resources (`ResourcesService` also exposes `SearchResources`, which the UI does not currently call).
- Toggle the assist surface on small screens.

## Application Workflow

1. `ArticleViewModel` shows the greeting and suggested topics.
2. The user submits a topic.
3. `AzureAIService` sends the structured prompt through Semantic Kernel.
4. The response is displayed in the article workspace.
5. `ResponseParserService` extracts resources and splits the response into sections.
6. `ResourcesService` updates the in-memory resource collection.

## AI Workflow

```text
Topic
    ↓
ArticleViewModel
    ↓
AzureAIService
    ↓
Semantic Kernel / Azure OpenAI
    ↓
Article Content
    ↓
ResponseParserService
    ↓
Article + Resources
```

If credentials are placeholders or the live request fails, one of four seeded HTML responses is returned when the prompt matches exactly (case-insensitive): team collaboration, time management, engaging blog posts, and Syncfusion MAUI ListView. Other prompts return a connection message. The same fallback is used when the Azure call throws.

## AI Architecture

- **View:** `MainPage`, `ChatView`, `ResourcePanel`, and `AddResourceDialog` (a `ContentPage` used as a dialog).
- **ViewModel:** `ArticleViewModel`.
- **Services:** `AzureBaseService` (credentials and Semantic Kernel setup), `AzureAIService`, `ResourcesService`, and `ResponseParserService`.
- **Models:** `ResourceItem` and `ResponseSection`.

## Syncfusion Controls Used

- `SfAIAssistView`
- `SfMarkdownViewer`
- `SfChipGroup`
- `SfShimmer`

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workloads for target platforms.
- Syncfusion AIAssistView, MarkdownViewer, and Core packages (shimmer and chips ship in Core).
- Microsoft Semantic Kernel.
- Azure OpenAI credentials for live generation.

## Configuration

### AI Configuration

Credential constants are in `Services/AzureBaseService.cs`:

```csharp
private const string endpoint = "END_POINT";
internal const string deploymentName = "DEPLOYMENT_NAME";
internal const string imageDeploymentName = "IMAGE_MODEL_NAME";
private const string key = "KEY";
```

Replace the placeholders with valid Azure OpenAI values before live use. Chat generation uses `deploymentName`; `imageDeploymentName` is only checked for being non-empty. On Android, the certificate callback in `CreateHttpClientHandler` compares the request host with the `END_POINT` placeholder, so update it to your endpoint host. The sample has no environment-variable or secure-storage Azure configuration.

## Getting Started

Run from `Use Case Samples/Smart Article Generator/Sample/SmartArticleGenerator`. The iOS and Mac Catalyst targets build only on macOS, and the Windows target only on Windows:

```sh
dotnet restore SmartArticleGenerator.csproj

dotnet build SmartArticleGenerator.csproj -t:Run -f net10.0-android
dotnet build SmartArticleGenerator.csproj -t:Run -f net10.0-ios
dotnet build SmartArticleGenerator.csproj -t:Run -f net10.0-maccatalyst
dotnet build SmartArticleGenerator.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Project Structure

```text
SmartArticleGenerator/
├── Converters/
├── Models/
├── Platforms/
├── Resources/
├── Services/
├── ViewModels/
├── Views/
├── App.xaml
└── MauiProgram.cs
```

## Troubleshooting

- **Offline response appears:** Credential placeholders remain or Azure validation/live calls fail.
- **No resources appear:** The response may not contain supported Markdown or HTML links.
- **Build/package error:** Restore packages and verify target MAUI workloads.

## Responsible AI Considerations

- Live prompts and topics are sent to Azure OpenAI.
- Do not submit confidential research or user data.
- Verify generated facts and extracted links before publication.

## Related Documentation

- [Syncfusion AIAssistView for .NET MAUI](https://help.syncfusion.com/maui/aiassistview/getting-started)
- [Syncfusion MarkdownViewer for .NET MAUI](https://help.syncfusion.com/maui/markdownviewer/getting-started)
- [Microsoft Semantic Kernel](https://learn.microsoft.com/semantic-kernel/overview/)

## Supported Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## License

Syncfusion controls require a valid Syncfusion license for production use.
