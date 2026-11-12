# Support Reply AI Draft

The Support Reply AI Draft sample demonstrates an AI-assisted customer-support workflow using .NET MAUI, Syncfusion .NET MAUI Chat (`SfChat`), MVVM, and Azure OpenAI.

A support agent can select a customer message, generate a context-aware reply, review and edit the draft, regenerate or discard it, and send the approved response through the native `SfChat` composer.

The AI never sends a reply automatically. Every generated response must be reviewed before sending.

## Demo

<!-- Add the final output demo video or GitHub user-attachment URL below. -->

## Key Features

- Uses a single Syncfusion `SfChat` conversation surface
- Allows only customer messages to be selected
- Supports one selected customer message at a time
- Supports toggling the selection by tapping the selected message again
- Generates a reply using the selected message and recent conversation context
- Displays drafting and regeneration progress inside the selected message
- Places the generated reply in the native `SfChat` composer
- Allows the generated draft to be reviewed and edited before sending
- Supports `Regenerate`, `Discard`, and `Retry` actions
- Preserves the previous draft if regeneration fails
- Prevents workflow-action text from becoming composer or outgoing message content
- Inserts the approved reply as a support-agent message
- Validates generated content before displaying it

## Workflow

1. Tap a customer message to select it.
2. Tap the selected message again to clear the selection.
3. Tap `Draft Reply` to generate a contextual response.
4. Review and edit the generated reply in the native composer.
5. Use `Regenerate` to generate another reply or `Discard` to clear the draft and selection.
6. If initial generation fails, use `Retry`.
7. Use the native Send button to add the approved reply to the conversation.

Support-agent messages cannot be selected, and the AI never sends a response automatically.

### Draft Reply

The Azure OpenAI request includes the selected customer message and the recent conversation context up to that message.

While generation is active:

- `Drafting reply...` appears inside the selected customer message
- The composer remains disabled
- Duplicate progress indicators are not displayed

After generation, the reply appears in the native composer for review and editing.

### Regenerate

During regeneration:

- The previous draft remains visible
- The composer becomes read-only
- `Regenerating reply...` appears inside the selected customer message
- The `Regenerate` action text is prevented from becoming composer content
- The new response replaces the previous draft after successful generation

If regeneration fails, the previous stable draft is preserved.

### Discard

Discarding a draft:

- Clears the composer
- Clears the selected customer message
- Removes the highlighted border and `Selected for reply` indicator
- Removes the workflow suggestions
- Restores the initial composer placeholder
- Prevents the `Discard` action text from becoming composer content

### Send

After the support agent reviews and edits the draft, the native `SfChat` Send button inserts the approved reply as a support-agent message.

After sending, the selection and composer return to their initial state.

## AI Reply Behavior

Azure OpenAI is instructed to act as an experienced customer-support manager rather than a generic chatbot or response template.

The generated reply should:

- Understand the selected message and conversation context
- Recognize troubleshooting already completed
- Avoid repeating completed steps
- Distinguish confirmed facts from assumptions
- Avoid robotic or generic language
- Provide concise and relevant next steps
- Request only the minimum missing information
- Be suitable for agent review and editing

The model is instructed not to invent:

- Product menus, screen names, buttons, or commands
- URLs or unsupported product capabilities
- Account, license, or subscription status
- Policies or subscription rules
- Investigation or verification results
- Resolution timelines
- Refunds, escalations, or completed account actions

The model is also instructed not to request passwords, API keys, authentication codes, payment-card details, or other sensitive credentials.

Generated responses are returned as customer-facing plain text and validated before being placed in the composer.

## Response Validation

Generated replies are checked for:

- Empty or excessive content
- Unsupported links
- Internal source identifiers
- Requests for sensitive credentials
- Unsupported account or verification claims
- Guarantees or promises not supported by the conversation

## Configuration

The sample supports Azure OpenAI configuration through environment variables or a local configuration file.

### Environment variables

Set the following Azure OpenAI environment variables:

```bash
export AZURE_OPENAI_ENDPOINT="https://YOUR-RESOURCE.openai.azure.com/"
export AZURE_OPENAI_API_KEY="YOUR-AZURE-OPENAI-API-KEY"
export AZURE_OPENAI_DEPLOYMENT_NAME="YOUR-DEPLOYMENT-NAME"
```

The deployment name must exactly match the deployment configured in Azure OpenAI. The deployment name may differ from the underlying model name.

When all three variables are available, they take precedence over the local configuration file.

### Local configuration

If one or more Azure OpenAI environment variables are missing, the sample loads the complete configuration from:

```text
AI Solutions/Chat/SupportReplyAIDraft/azure-openai.local.json
```

Use the following format:

```json
{
  "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
  "ApiKey": "YOUR-AZURE-OPENAI-API-KEY",
  "DeploymentName": "YOUR-DEPLOYMENT-NAME"
}
```

The endpoint must be a plain HTTPS Azure OpenAI URL without HTML formatting.

Keep this file local and never commit it. To exclude it locally:

```bash
cd ~/Desktop/maui-ai-usecase-demos

grep -qxF \
"AI Solutions/Chat/SupportReplyAIDraft/azure-openai.local.json" \
.git/info/exclude || \
echo \
"AI Solutions/Chat/SupportReplyAIDraft/azure-openai.local.json" \
>> .git/info/exclude
```

If valid Azure OpenAI settings are unavailable, the sample displays a controlled configuration message and does not send a reply.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload
- Azure OpenAI resource and deployment
- Syncfusion license configuration required by the repository
- Xcode and Apple platform tools for Mac Catalyst

Verify the installed tools:

```bash
dotnet --version
dotnet workload list
```

## Run on Mac Catalyst

From the repository root:

```bash
cd ~/Desktop/maui-ai-usecase-demos

dotnet run \
--project "AI Solutions/Chat/SupportReplyAIDraft/SupportReplyAIDraft.csproj" \
-f net10.0-maccatalyst \
-c Debug
```

## Key Files

The following tree lists the primary files involved in the sample workflow:

```text
SupportReplyAIDraft/
├── Models/
│   ├── SupportReplyRequest.cs
│   ├── SupportReplyResponse.cs
│   └── SupportReplyValidationResult.cs
├── Services/
│   ├── AzureOpenAISettings.cs
│   ├── ConversationService.cs
│   ├── SupportReplyService.cs
│   └── SupportReplyValidator.cs
├── ViewModels/
│   └── SupportChatViewModel.cs
├── Views/
│   ├── MessageTemplates/
│   ├── SupportChatPage.xaml
│   └── SupportChatPage.xaml.cs
├── azure-openai.local.json        # Local only; never commit
├── README.md
└── SupportReplyAIDraft.csproj
```

- `SfChat` provides the conversation surface, native composer, suggestions, send behavior, and message templates.
- `SupportChatViewModel` manages selection, drafting, regeneration, retry, discard, and send states.
- `SupportReplyService` builds the grounded prompt and communicates with Azure OpenAI.
- `SupportReplyValidator` validates generated replies before review.

## Scope

Included:

- Native .NET MAUI XAML and C#
- Syncfusion `SfChat`
- MVVM workflow
- Azure OpenAI reply generation
- Review-before-send behavior
- Local response validation

Not included:

- Knowledge retrieval
- Source panels or source IDs
- WebView content
- Automatic reply sending
- Production customer data
- Persistent conversation storage

## Platform Targets

The project targets .NET 10 for Mac Catalyst, iOS, Android, and Windows.

The complete workflow described in this README has been validated on Mac Catalyst. Validate the workflow separately on iOS, Android, and Windows before release.

## Safe Usage

- Use fictional demonstration data only.
- Never commit API keys or license keys.
- Review every AI-generated reply before sending.
- Do not treat generated content as verified account information.
- Do not log credentials or sensitive customer data.
- Remove sensitive information from screenshots and logs.