# AI-Powered Maui Notes

![AI-Powered Maui Notes demo](MauiNotes_windows.gif)

A cross-platform .NET MAUI notes application that combines a custom rich-text editor adapter with Azure OpenAI writing actions.

## Overview

Users organize folders and notes, edit rich text, and request AI transformations for a selection or the full note. The app applies guarded replacements and allows cancellation, revert, retry, undo, and redo.

## AI Capabilities

- Grammar checking and proofreading.
- Rewriting and tone changes.
- Summarization and restructuring.
- Elaboration and shortening.
- Custom editing instructions.

## Key Features

- Create, rename, duplicate, move, delete, and lock notes.
- Organize notes in folders.
- Edit rich text, highlights, tables, links, and images.
- Apply built-in or custom AI actions.
- Persist notes locally as JSON.

## Application Workflow

1. The user selects text or chooses an AI action for the full note.
2. `NotesViewModel` captures the selection, HTML, and plain-text context.
3. `IAiEditingService` sends the editing request to Azure OpenAI.
4. The response is validated against the requested selection and source state.
5. `CustomRichTextEditor` applies or restores the returned HTML.
6. `JsonNotePersistenceService` persists note changes.

## AI Workflow

```text
Selected Text or Full Note
    ↓
NotesViewModel
    ↓
AzureOpenAiEditingService
    ↓
Azure OpenAI Chat Completions
    ↓
Validated HTML Replacement
    ↓
Editor Update or Revert
```

## AI Architecture

- **Views:** Desktop/mobile notes and editor pages.
- **ViewModels:** `NotesViewModel` and partial view-model files.
- **Models:** Note, folder, AI action, and AI request data.
- **AI:** `IAiEditingService` and `AzureOpenAiEditingService`.
- **Persistence:** `INotePersistenceService` and `JsonNotePersistenceService`.
- **Editor:** `CustomRichTextEditor` project handles selection and HTML replacement.

## Syncfusion Controls Used

- `SfRichTextEditor`
- `SfTreeView`
- `SfListView`
- `SfPopup`
- `SfToolbar`
- `SfButton`

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workloads for target platforms.
- Syncfusion package versions referenced by `MauiNotes.csproj` and `CustomRichTextEditor.csproj`.
- An Azure OpenAI chat deployment for live AI editing.

## Configuration

### AI Configuration

Set these environment variables before launch:

| Variable | Required | Purpose |
| --- | --- | --- |
| `MAUINOTES_AZURE_OPENAI_ENDPOINT` | Live AI | HTTPS Azure OpenAI endpoint |
| `MAUINOTES_AZURE_OPENAI_API_KEY` | Live AI | Azure OpenAI API key |
| `MAUINOTES_AZURE_OPENAI_DEPLOYMENT` | Live AI | Deployed chat model |
| `SYNCFUSION_LICENSE_KEY` | Production licensing | Syncfusion license key |

The current `HttpClient` timeout is 60 seconds in `MauiProgram.cs`.

Notes remain usable when AI configuration is missing, but AI actions return configuration errors.

## Getting Started

Run from `Use Case Samples/AI-Powered Maui Notes/MauiNotes`:

```sh
dotnet restore MauiNotes.slnx

dotnet build MauiNotes/MauiNotes/MauiNotes.csproj -t:Run -f net10.0-android
dotnet build MauiNotes/MauiNotes/MauiNotes.csproj -t:Run -f net10.0-ios
dotnet build MauiNotes/MauiNotes/MauiNotes.csproj -t:Run -f net10.0-maccatalyst
dotnet build MauiNotes/MauiNotes/MauiNotes.csproj -t:Run -f net10.0-windows10.0.19041.0
```

## Project Structure

```text
MauiNotes/
├── CustomRichTextEditor/
└── MauiNotes/
    ├── Controls/
    ├── Models/
    ├── Platforms/
    ├── Resources/
    ├── Services/
    ├── ViewModels/
    ├── Views/
    ├── App.xaml
    ├── AppShell.xaml
    └── MauiProgram.cs
```

## Troubleshooting

- **AI action is unavailable:** Set all three Azure OpenAI variables.
- **AI response is not applied:** The note or selection may have changed while the request was running.
- **Build fails:** Verify .NET 10 workloads and Syncfusion package compatibility.

## Responsible AI Considerations

- Send only note content required for the requested edit.
- Do not include confidential text in AI prompts.
- Review Azure OpenAI data handling before using sensitive notes.

## Related Documentation

- [Syncfusion RichTextEditor for .NET MAUI](https://help.syncfusion.com/maui/rich-text-editor/getting-started)
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)

## Supported Platforms

- Android
- iOS
- macOS (Mac Catalyst)
- Windows

## License

Syncfusion controls require a valid license for production use.
