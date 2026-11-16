# AI-Powered Process Diagram Drafting

This folder contains the sample for a single-screen .NET MAUI app under **AI Solutions**. The app turns a short business process description into an editable Syncfusion Diagram draft.

## User flow

1. Enter a process description.
2. Select **Generate Draft**.
3. Review the generated title, warnings, and diagram.
4. Select **Accept**, **Regenerate**, or **Discard**.

## Configuration

The sample uses Azure OpenAI when all of the following environment variables are present:

- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_DEPLOYMENT`
- `AZURE_OPENAI_API_KEY`

If any of those values are missing, the app falls back to a local rule-based extraction service.

### Fallback behavior

The local fallback is deterministic and designed for demo scenarios and offline development. It can generate simple linear flows and a single conditional branch with labeled connectors so the diagram remains demonstrable without Azure OpenAI.

Its limitations are intentional:

- it does not perform semantic reasoning,
- it only infers a small set of branch patterns,
- it should be reviewed before using any output as a real workflow specification.

## Architecture notes

- `ProcessDiagramViewModel` owns the user actions, current draft, status message, title, and warnings.
- `AzureOpenAIWorkflowExtractionService` and `FakeWorkflowExtractionService` both return a `WorkflowResponse`.
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
- Accept, regenerate, and discard actions
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
