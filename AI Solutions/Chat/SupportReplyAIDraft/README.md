# Support Reply AI Draft

This sample demonstrates an AI-assisted support reply workflow for one fictional customer conversation using native .NET MAUI, a single Syncfusion SfChat conversation surface, MVVM, and Azure OpenAI.

## Configuration
Set these environment variables before using the sample:
- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_API_KEY`
- `AZURE_OPENAI_DEPLOYMENT_NAME`
- `SYNCFUSION_LICENSE_KEY`

If the Azure OpenAI settings are missing or invalid, the sample shows a controlled guidance message and preserves the conversation state.
If the Syncfusion license key is empty, the app starts without registering a license.

## Workflow
- select one customer message from the conversation
- draft a reply from the selected message plus recent context in the integrated composer footer
- review the structured JSON result before sending
- edit, regenerate, discard, retry, or send explicitly
- send only when you choose; the AI never sends automatically

## Scope
- only native MAUI XAML/C# is used
- knowledge retrieval, source IDs, source panels, and WebView content are intentionally out of scope
- temporary harness notes are preserved for this task
