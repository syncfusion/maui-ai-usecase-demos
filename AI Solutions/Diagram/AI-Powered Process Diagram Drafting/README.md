# AI-Powered Process Diagram Drafting

This folder defines the sample for a single-screen .NET MAUI app under **AI Solutions** that uses Syncfusion .NET MAUI Diagram to turn a short process description into an editable flowchart.

The sample uses Azure OpenAI to extract workflow nodes and connectors. The app validates the response, calculates diagram positions, renders the draft, and waits for the user to accept, regenerate, or discard it.

## Documentation set

- [Specification Document](docs/specification.md)
- [Requirements](docs/requirements.md)
- [User Flows](docs/user-flows.md)
- [Architecture Guidelines](docs/architecture-guidelines.md)
- [Harness](docs/harness.md)
- [Development Tasks](docs/development-tasks.md)

## Product summary

The sample covers one AI workflow:

1. The user enters a process description.
2. The user selects Generate Draft.
3. The app sends the description to the AI extraction service.
4. The AI returns structured workflow data.
5. The app validates the response.
6. The app renders an editable diagram.
7. The user edits the draft if needed.
8. The user accepts, regenerates, or discards the draft.

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

## Design rule

The AI must not generate diagram coordinates. The app must calculate layout and render the diagram itself.
