# AI-Powered Writing Assistant

![AI-Powered Writing Assistant demo](AIPoweredWritingAssistant.gif)

A .NET MAUI rich-text writing sample that combines Syncfusion `RichTextEditor` and `AIAssistView` with Azure OpenAI.

## Overview

The app provides paraphrasing, grammar checking, elaboration, and shortening actions for rich-text editor content. On Android and iOS it opens the assistant on a dedicated page; Windows and Mac Catalyst use an inline assistant surface.

## AI Capabilities

- Humanize, Professional, Simple, and Academic paraphrasing.
- Grammar and spelling correction.
- Text elaboration and shortening.
- Applying generated text back to the rich-text editor.

## Key Features

- Select a writing action from `SfComboBox`.
- Review suggestions in `SfAIAssistView`.
- Accept or ignore generated text.
- Apply accepted text to `SfRichTextEditor`.

## Application Workflow

1. The user enters text in `SfRichTextEditor`.
2. The user opens the assistant and selects a feature or tone.
3. `AssistViewViewModel` strips HTML and creates an action-specific prompt.
4. `AzureBaseService` sends the prompt through `IChatClient`.
5. The response is displayed in `AIAssistView`.
6. The user accepts the response and it is formatted and applied to the editor.

## AI Workflow

```text
RichTextEditor Content
    ↓
AssistViewViewModel
    ↓
Action-Specific Prompt
    ↓
Azure OpenAI IChatClient
    ↓
AIAssistView
    ↓
RichTextEditor
```

## AI Architecture

- **Views:** `MainPage` and `AssistViewPage`.
- **ViewModels:** `AssistViewViewModel` and `ComboBoxViewModel`.
- **Services:** `IAzureAIService` and `AzureBaseService`.
- **Helper:** `ServiceHelper` resolves the registered AI service for XAML-created pages.

## Syncfusion Controls Used

- `SfRichTextEditor`
- `SfAIAssistView`
- `SfComboBox`

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workloads for target platforms.
- Syncfusion packages referenced by `AIPoweredWritingAssistant.csproj`.
- Valid Azure OpenAI configuration.

## Configuration

### AI Configuration

Live credentials are constants in `Services/AzureBaseService.cs`:

```csharp
private const string endpoint = "END_POINT";
internal const string deploymentName = "DEPLOYMENT_NAME";
internal const string imageDeploymentName = "IMAGE_MODEL_NAME";
private const string key = "KEY";
```

Replace the constants with a valid endpoint, deployment name, and API key before live use. The sample does not read Azure credentials from environment variables or secure storage.

## Getting Started

Run from `Use Case Samples/AI-Powered Writing Assistant/Sample/AIPoweredWritingAssistant`:

```sh
dotnet restore AIPoweredWritingAssistant.csproj

dotnet build AIPoweredWritingAssistant.csproj -t:Run -f net10.0-android
dotnet build AIPoweredWritingAssistant.csproj -t:Run -f net10.0-ios
dotnet build AIPoweredWritingAssistant.csproj -t:Run -f net10.0-maccatalyst
dotnet build AIPoweredWritingAssistant.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Project Structure

```text
AIPoweredWritingAssistant/
├── Helper/
├── Platforms/
├── Resources/
├── Services/
├── ViewModel/
├── Views/
├── App.xaml
├── AppShell.xaml
└── MauiProgram.cs
```

## Troubleshooting

- **No AI response:** Replace placeholder credentials and verify Azure connectivity.
- **Suggestions do not appear:** Verify the suggestion command bindings in `AssistViewViewModel`.
- **Wrong platform layout:** Android/iOS use `AssistViewPage`; Windows/Mac Catalyst use inline UI.

## Responsible AI Considerations

- Do not hard-code or commit production credentials.
- Avoid sending confidential text to Azure OpenAI.
- Review AI output before accepting it into published content.

## Related Documentation

- [Syncfusion RichTextEditor for .NET MAUI](https://help.syncfusion.com/maui/rich-text-editor/getting-started)
- [Syncfusion AIAssistView for .NET MAUI](https://help.syncfusion.com/maui/aiassistview/getting-started)
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)

## Supported Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## License

Syncfusion controls require a valid Syncfusion license for production use.
