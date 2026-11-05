# Support Reply AI Draft

This sample demonstrates an AI-assisted support reply workflow for one fictional customer conversation that is preloaded with four messages, using native .NET MAUI, a single Syncfusion SfChat conversation surface, MVVM, and Azure OpenAI.

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
- draft a reply from the selected message plus recent context
- review and edit the generated reply draft before sending
- edit, regenerate, discard, retry, or send explicitly
- generated text appears in the reply editor with a `Review before sending` prompt
- send only when you choose; the sent reply appears in the conversation as a support-agent message, and the AI never sends automatically

## Scope
- only native MAUI XAML/C# is used
- knowledge retrieval, source IDs, source panels, and WebView content are intentionally out of scope

## Platform support
Targets .NET 10 on Mac Catalyst, iOS, Android, and Windows, with responsive desktop and mobile layouts.

## Safe usage
- Use fictional data only.
- Keep API keys and license keys in environment variables; never commit them.
- Review every generated draft before sending.
