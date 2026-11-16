# AI-Powered Process Diagram Drafting

This folder contains the sample for a single-screen .NET MAUI app under **AI Solutions**. The app turns a short business process description into an editable Syncfusion Diagram draft.

## User flow

1. Enter a process description.
2. Select **Generate Draft**.
3. Review the generated title, warnings, and diagram.
4. Select **Discard** if you want to clear the draft.

## Configuration

This sample requires Azure OpenAI to generate a draft.

Set the following environment variables before running the app:

- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_DEPLOYMENT`
- `AZURE_OPENAI_API_KEY`

If any of those values are missing, the app will not generate a workflow draft.

## Architecture notes

- `ProcessDiagramViewModel` owns the user actions, current draft, status message, title, and warnings.
- `AzureOpenAIWorkflowExtractionService` returns a `WorkflowResponse` from Azure OpenAI.
- `WorkflowResponseValidator` checks the response before the diagram is built.
- `DiagramDraftBuilder` converts the validated response into Syncfusion diagram nodes and connectors.
- `ProcessDiagramPage` projects the current draft into `SfDiagram` and keeps the control instance alive across updates.

## Scope summary

Included:

- Natural-language process entry
- Structured workflow extraction
- Start, Process, Decision, and End nodes
- Directional connectors with branch labels
- Editable diagram output
- Generate and discard actions
- Warning display for ambiguous or incomplete outcomes

Not included:

- BPMN or UML output
- Swimlanes
- Workflow execution
- Workflow publishing
- Multi-user collaboration
- External workflow system integration

## Security note

Client-side API keys in a demo are for local experimentation only. Production apps should broker tokens from a backend service.

## Design rule

The AI must not generate diagram coordinates. The app calculates layout and renders the diagram itself.
