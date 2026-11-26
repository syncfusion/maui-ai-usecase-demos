# AI-Powered Writing Assistant

![AI-Powered Writing Assistant demo](AIPoweredWritingAssistant.gif)

A .NET MAUI rich-text writing sample that combines Syncfusion `RichTextEditor` and `AIAssistView` with Azure OpenAI.

## Overview

The app provides paraphrasing, grammar checking, elaboration, and shortening actions for rich-text editor content. On Android and iOS it opens the assistant on a dedicated page; Windows and Mac Catalyst use an inline assistant surface.

## AI Capabilities

### Paraphraser with Four Tones
- Choosing **Paraphraser** does not call the AI at once. The assistant first replies "Do you want to improve the text?" and shows four suggestions: **Humanize**, **Professional**, **Simple**, and **Academic**.
- Tapping a tone sends the editor text with the prompt "Please rephrase while preserving meaning". The prompt lists the four tones and asks for the tone named in the user command.
- The prompt asks the model to keep the length close to the original and to return only the changed text, with no comments.
- This lets the user restyle a draft for a different reader without retyping it.

### Grammar Checker
- The **Grammar Checker** prompt tells the model to act as a strict grammar and copy editor.
- It allows fixes only for grammar, spelling, subject–verb agreement, punctuation, pronouns, capitalization, and spacing. It does not allow rewording, tone changes, or changes to sentence order.
- The prompt asks the model to keep line breaks and paragraph breaks.
- If the text is already correct, the model must return it unchanged. A single misspelled word is corrected, and a correct single word is returned as it is.
- The model must return only the corrected text, with no notes.

### Elaborate
- The **Elaborate** prompt asks the model to expand the ideas and add clarifying detail while keeping the meaning, intent, and facts.
- It also corrects grammar and punctuation in the source.
- The prompt does not allow new facts, examples, opinions, or entities. It limits the growth to about 30% and asks the model to keep the tone and paragraph breaks.

### Shorten
- The **Shorten** prompt asks the model to remove repeated words and tighten the wording while keeping the meaning, intent, and facts.
- It aims for a length about 20% to 40% shorter, keeps the tone and paragraph breaks, and corrects grammar in the result.
- The prompt does not allow the model to drop essential details or add information.

### How Requests Are Built and Handled
- Every request uses the full editor content. The app removes the HTML tags and sends the plain text with the action name.
- Each request is sent as a single prompt without earlier messages. The assistant has no conversation memory, and Retry sends a new request with the editor's current content.
- The reply is shown as text in the assistant, and line breaks become `<br>` tags.
- **Copy** removes HTML tags and `&nbsp;` and puts the plain text on the clipboard. **Retry** repeats the request. **Apply** sets the editor content to the reply, wrapped in a paragraph if it is plain text.
- Requests go through `IChatClient`, which is created from `AzureOpenAIClient` with the endpoint, key, and deployment name in `AzureBaseService`.
- If a request fails, or the credentials have not been validated, the service returns an empty string. The app shows no error message for this case.

## Key Features

### Rich Text Editor
- The main page is an `SfRichTextEditor` with its toolbar turned on, for formatting text before or after it goes to the AI.
- The editor content is bound two-way to the view model as HTML. The AI reads from it, and **Apply** writes back to it.

### AI Assistant Panel
- An AI button opens the assistant. It sits at the top right on Windows and Mac Catalyst. On Android and iOS it sits at the bottom right.
- On Windows and Mac Catalyst, the assistant opens as a side panel next to the editor. The editor uses about 70% of the width and the panel about 30%. The panel has a title, a close button, and the chat area.
- On Android and iOS, the button opens `AssistViewPage` as a full page. The page uses the same view model, so the chat history is kept.
- The close button hides the panel and clears the chat.
- The chat uses `SfAIAssistView` with a "How can I help you?" header.

### Choosing an Action
- Four suggestion chips are shown in the chat: **Paraphraser**, **Grammar Checker**, **Elaborate**, and **Shorten**.
- An `SfComboBox` in the panel title bar (and in the page title bar on Android and iOS) lists the same four actions. It shows only a dropdown icon.
- Choosing an action from the combo box adds it to the chat as a request, then adds the response.

### Response Actions
- Each response has three buttons: copy, retry, and **Apply**. The buttons use a custom response template that is set on `ResponseActionsView`.
- **Apply** puts the new text into the editor in one step, so the user can see the result in place and keep editing.

### Startup Check
- The first time the AI service is created, it sends a short test request to check the credentials. Until a check succeeds, AI requests return no text.

## Application Workflow

1. The user enters text in `SfRichTextEditor`.
2. The user opens the assistant and selects a feature or tone.
3. `AssistViewViewModel` strips HTML and creates an action-specific prompt.
4. `AzureBaseService` sends the prompt through `IChatClient`.
5. The response is displayed in `AIAssistView`.
6. The user can copy or retry the response, or apply it; applied text is converted to HTML and set on the editor.

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
- `SfComboBox` (from `Syncfusion.Maui.Inputs`)

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workloads for target platforms.
- Syncfusion packages referenced by `AIPoweredWritingAssistant.csproj` (AIAssistView, Core, Inputs, RichTextEditor) and `Microsoft.Extensions.AI.OpenAI`.
- Target frameworks: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, plus `net10.0-windows10.0.19041.0` on Windows hosts.
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

Replace `endpoint`, `deploymentName`, and `key` with valid values before live use (`imageDeploymentName` is declared but not used by this sample's text features). Credentials are validated with a test request at startup; if the client cannot be created, an alert is shown. Failed AI requests return empty text rather than an error message. The sample does not read Azure credentials from environment variables or secure storage.

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

- **No AI response:** Replace placeholder credentials and verify Azure connectivity. Errors are swallowed, so an empty response usually means a credential or network problem.
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
