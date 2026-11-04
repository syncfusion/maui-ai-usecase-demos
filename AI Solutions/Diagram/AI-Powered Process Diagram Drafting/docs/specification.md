# Specification Document

## Product name

AI-Powered Process Diagram Drafting

## Purpose

This sample turns a short process description into a structured, editable flowchart in a .NET MAUI app. It uses Syncfusion .NET MAUI Diagram for rendering and editing. It uses AI only to extract workflow structure.

## Problem

Business analysts and process owners often describe workflows in plain language before they draw them. Manual diagramming takes time. It also creates repeated work when the description changes.

The sample removes the first drafting step. It converts a short description into a diagram draft that the user can review and edit.

## Solution

The app accepts a process description, sends it to an AI extraction service, validates the returned workflow, and builds a diagram draft from that data. The diagram uses a top-to-bottom layout with orthogonal connectors. The user can move nodes, change labels, reconnect lines, and then accept, regenerate, or discard the draft.

The AI does not run the workflow. The app does not publish the workflow. The app only helps the user draft it.

## Target users

- Business analysts
- Process owners
- Operations teams

## Experience goals

- Keep the app on one screen.
- Make the AI result easy to inspect.
- Make the flowchart editable immediately after generation.
- Show warnings when the process description is incomplete or ambiguous.
- Prevent invalid AI responses from replacing the current diagram.

## In scope

- Accept a natural-language process description.
- Offer an example process selector.
- Generate a basic flowchart draft.
- Support Start, Process, Decision, and End nodes.
- Generate directional connectors and branch labels.
- Allow the user to edit the generated diagram.
- Allow the user to accept, regenerate, or discard the draft.
- Preserve the current diagram when the AI response is invalid or the service fails.
- Show warnings for missing or ambiguous outcomes.

## Out of scope

- BPMN generation
- UML generation
- Swimlanes
- Process execution
- Workflow automation
- Workflow optimization
- Multi-user collaboration
- Automatic saving
- Automatic publishing
- Integration with external workflow systems

## Supported workflow model

The AI returns a structured workflow model with these parts:

- A title
- A list of nodes
- A list of connectors
- A list of warnings

Supported node types:

- start
- process
- decision
- end

### Example response

```json
{
  "title": "Purchase Request Approval",
  "nodes": [
    { "id": "N1", "type": "start", "label": "Start" },
    { "id": "N2", "type": "process", "label": "Submit purchase request" },
    { "id": "N3", "type": "process", "label": "Manager reviews request" },
    { "id": "N4", "type": "decision", "label": "Amount below ₹50,000?" },
    { "id": "N5", "type": "process", "label": "Approve request" },
    { "id": "N6", "type": "process", "label": "Finance reviews request" },
    { "id": "N7", "type": "process", "label": "Create purchase order" },
    { "id": "N8", "type": "process", "label": "Notify employee" },
    { "id": "N9", "type": "end", "label": "End" }
  ],
  "connectors": [
    { "id": "C1", "sourceId": "N1", "targetId": "N2", "label": "" },
    { "id": "C2", "sourceId": "N2", "targetId": "N3", "label": "" },
    { "id": "C3", "sourceId": "N3", "targetId": "N4", "label": "" },
    { "id": "C4", "sourceId": "N4", "targetId": "N5", "label": "Yes" },
    { "id": "C5", "sourceId": "N4", "targetId": "N6", "label": "No" },
    { "id": "C6", "sourceId": "N5", "targetId": "N7", "label": "" },
    { "id": "C7", "sourceId": "N6", "targetId": "N8", "label": "" },
    { "id": "C8", "sourceId": "N7", "targetId": "N9", "label": "" },
    { "id": "C9", "sourceId": "N8", "targetId": "N9", "label": "" }
  ],
  "warnings": [
    "The description does not define the outcome of the finance review. The branch is rendered as described and flagged for review."
  ]
}
```

This sample response shows the expected shape. The AI must not return coordinates. The app must calculate all node positions itself.

## Screen layout

### Desktop

Use a two-column layout.

- Left column: process description, example selector, and generation controls
- Right column: editable diagram surface

### Mobile

Use a vertical layout.

1. Process description
2. Generate Draft button
3. Diagram surface
4. Review actions

After generation, collapse the input section so the diagram gets more space.

## Main UI elements

- Page title
- Multiline process description input
- Example process selector
- Generate Draft button
- AI warning banner
- Diagram surface
- Accept Draft button
- Regenerate button
- Discard button

## AI responsibilities

The AI must:

- Extract only the process described by the user.
- Identify steps, decisions, branches, and outcomes.
- Generate concise, action-oriented node labels.
- Return structured JSON.
- Report missing or ambiguous outcomes as warnings.

The AI must not:

- Add unsupported steps.
- Make business decisions.
- Generate diagram coordinates.
- Execute the process.
- Replace an edited diagram without confirmation.

## Validation rules

The app must validate the AI response before rendering.

- Node IDs must be unique.
- Connector IDs must be unique.
- Every connector source must exist.
- Every connector target must exist.
- Exactly one Start node must exist.
- At least one End node must exist.
- Node types must be supported.
- Node labels must not be empty.
- Connectors must not point to the same node.
- Invalid responses must not replace the current diagram.

## Limits

- Process description: 1,500 characters
- Nodes: 20
- Decision nodes: 6
- Connectors: 30
- Node label: 60 characters
- Connector label: 30 characters

## Editing rules

The user must be able to:

- Move nodes
- Edit node labels
- Add nodes
- Delete nodes
- Reconnect nodes
- Edit branch labels
- Undo changes
- Redo changes
- Fit the diagram to the view

If the user regenerates after editing, the app must ask for confirmation before replacing the current draft.

## Error handling

- Empty input: disable Generate Draft.
- Unclear description: ask the user to add steps and outcomes.
- Invalid AI response: keep the current diagram and show Retry.
- Ambiguous branch: render valid content and show a warning.
- Service failure: preserve the input and current diagram.
- Process exceeds limits: ask the user to shorten the description.

## Acceptance criteria

1. A process description can generate a readable flowchart.
2. Start, Process, Decision, and End nodes render correctly.
3. Connectors and branch labels represent the extracted workflow.
4. The generated diagram is editable.
5. Ambiguities are shown to the user.
6. Invalid responses do not affect the current diagram.
7. Regeneration requires confirmation after manual edits.
8. The user explicitly accepts the generated draft.
9. No workflow is executed or published automatically.
10. The sample stays a single-screen AI showcase.

## Glossary

- Draft: The generated diagram before the user accepts it.
- Workflow extraction: The AI step that turns prose into structured nodes and connectors.
- Diagram surface: The editable Syncfusion diagram canvas.
- Warning: A message that signals an incomplete or ambiguous AI result.
