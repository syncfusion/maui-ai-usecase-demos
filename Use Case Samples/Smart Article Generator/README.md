# Smart Article Generator

![Smart Article Generator demo](SmartArticleGenerator.gif)

A .NET MAUI article workspace that combines Syncfusion `AIAssistView` and `MarkdownViewer`, Microsoft Semantic Kernel, and Azure OpenAI.

## Overview

The app accepts a topic, generates article content, renders the response, extracts resource links, and maintains a resource collection for the workspace.

## AI Capabilities

### Article Generation
- `ArticleViewModel` wraps the user's topic in a fixed prompt before it goes to the model. The prompt asks for GitHub-flavored Markdown, an H2 title (`## Title`), and article-style content with headings, paragraphs, bullet lists, and code fences where they fit.
- The prompt asks for a length of roughly 2000-4000 words and tells the model not to wrap the whole answer in quotes. The user types only a topic and gets a full draft.
- `AzureAIService` sends the prompt through Microsoft Semantic Kernel to the Azure OpenAI chat deployment named in `AzureBaseService`, and the reply is shown as the article.
- The chat history keeps the earlier prompts of the session. Once it holds more than five messages, the two oldest are removed to stay within the token limit. Only the user prompts are added to this history, not the model's replies.

### Source Suggestions in the Prompt
- The prompt asks the model to end the article with a `References` section that lists 3 to 6 external sources as Markdown links in the form `[Title](URL)`.
- These links are the input for the Article Sources panel, so the sources come from the same model call as the article. No second AI call is made.
- The model writes the links, so they can be wrong or outdated. The user should check them before publishing.

### Source Extraction from the Response
- `ResponseParserService.ExtractResourcesFromResponse` reads `[Title](URL)` Markdown links and HTML `<a href>` anchors from the reply. It removes duplicate URLs and keeps at most five.
- If a link has no title text, the host name of the URL is used as the title. Each source is typed `Link` and gets a descending relevance score that starts at 88.
- If the reply has no links, the parser builds suggestions from the text. It adds an "Article: <topic>" entry and up to four "Deep Dive: <keyword>" entries. Each links to a Google search for that topic or keyword, and its source type is picked at random from a fixed list. These entries are search shortcuts, not AI-verified references.
- The sources are rebuilt after every valid response, so they always match the latest article.

### Offline Fallback
- A response is treated as invalid when it contains the text "Please connect to your preferred AI service for real-time queries".
- If the Azure call throws (for example, because the credential placeholders were not replaced), `GetSolutionToPrompt` is used. It returns a seeded HTML article only when the prompt exactly matches (ignoring case) one of four built-in prompts: "How to improve team collaboration at work?", "Why is time management important?", "Give me tips for writing engaging blog posts", or "Help me draft article content for Syncfusion MAUI ListView".
- Any other prompt returns the connection message above. In that case the sources list is cleared.
- When the credential check fails at startup, the app tries to show an alert that says the key or endpoint is missing or incorrect and that the user can continue with the offline data.

## Key Features

### Article Workspace
- The app has one page, `MainPage`, with an **Article Generator** header, an **Article Sources** panel, and an **Article Draft** area.
- On Windows and Mac Catalyst the page is a two-panel layout. The left panel (about 70% of the width) holds the sources and the draft. The right panel (about 30%) holds the AI chat.
- On Android and iOS the page is a single column with the sources and the draft. A floating **AI Assist** button opens the chat as a full-screen overlay with a close icon in its header.

### Chat with SfAIAssistView
- The user enters a topic in `SfAIAssistView`, which starts with the greeting "Hi! What topic would you like me to generate an article on today?".
- After a request, the chat adds a message that names the topic and invites further edits. The article itself appears in the draft area, not in the chat.
- On desktop, a `CustomAssistView` subclass exposes the chat part of the control so it can be placed in the right panel of the layout.

### Article Draft
- `ChatView` shows the article in an `SfMarkdownViewer` inside a white rounded card. A CSS rule set makes headings and bold text bold and keeps body text at normal weight.
- The viewer height is set per platform: 600 on Windows, Mac Catalyst, and iOS, and a scrollable 560 on Android.
- The viewer is bound to `MarkdownContent`, which holds the raw response text of the latest article. A new response replaces the previous draft.

### Article Sources
- `ResourcePanel` shows the sources as horizontally scrolling cards. Each card is a chip in an `SfChipGroup` (`ChipType="Action"`) with a custom item template. The header shows the count as "Added N source(s)".
- Each card shows the title, relevance percentage, source type, a tappable underlined URL, and a description limited to two lines.
- Tapping the URL opens it in the system browser. `https://` is added if the link has no scheme. A failed open shows an error alert.
- The red delete icon on a card removes that source from the list.
- **Add Source** (text and icon) opens `AddResourceDialog`, a modal page with fields for URL, title, and description. The title is required. The new source is typed `Custom` with a relevance of 100%.
- Adding a source is allowed only after a valid article has been generated. Before that, an alert tells the user to generate an article first.

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
