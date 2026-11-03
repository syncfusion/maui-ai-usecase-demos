# Architecture Guidelines

## Solution shape

Keep the sample small and single-purpose. The app should have one page, one view model, one AI workflow, and one diagram surface.

The user should not navigate to a second page for the main flow.

## Main components

### ProcessDiagramPage

This page hosts the UI. It should contain:

- the title
- the process description input
- the example selector
- the generation controls
- the AI warning banner
- the Syncfusion diagram surface
- the review actions

The page should switch between a two-column desktop layout and a vertical mobile layout.

### ProcessDiagramViewModel

This view model owns the page state and user actions.

Responsibilities:

- store the description text
- expose the example list
- expose the current diagram model
- track loading, error, warning, and dirty states
- run the generate, accept, regenerate, and discard actions
- ask for confirmation before regeneration replaces edited content
- keep the current input and draft when the service fails

Use commands for the page actions.

### IWorkflowExtractionService

This interface isolates the AI provider.

Responsibilities:

- send the prompt and the user description to the model
- receive the JSON response
- parse the JSON into a workflow response object
- surface warnings and failures to the view model

Keep the provider behind this interface so the sample can swap models without changing the page.

### WorkflowResponseValidator

This service checks the returned workflow before it is rendered.

Responsibilities:

- check node and connector IDs for uniqueness
- check that every connector references valid nodes
- enforce the supported node types
- enforce required Start and End nodes
- enforce label limits and connector rules
- reject self-referencing connectors
- reject malformed graphs

If validation fails, the service must not change the current diagram.

### DiagramDraftBuilder

This service converts validated workflow data into Syncfusion diagram items.

Responsibilities:

- map workflow node types to diagram node shapes
- create diagram connectors
- assign branch labels to connectors
- calculate node positions
- apply a top-to-bottom layout
- keep connector routing orthogonal

The builder must calculate coordinates itself. The AI must not supply coordinates.

## Data contracts

Define the workflow response with these fields:

- title
- nodes
- connectors
- warnings

Each node should have:

- id
- type
- label

Each connector should have:

- id
- sourceId
- targetId
- label

Use a small enum or string-backed model for the supported node types.

## Rendering rules

Map each workflow node type to the diagram as follows:

| Workflow element | Diagram element |
| --- | --- |
| Start | Terminator node |
| Process | Rectangle node |
| Decision | Diamond node |
| End | Terminator node |
| Normal flow | Directional connector |
| Decision branch | Directional connector with a label |

Use a top-to-bottom flow.
Use orthogonal connectors.
Place the start node at the top level.
Place decision branches below the decision node with clear horizontal separation.

## Layout rules

Use deterministic layout rules so the same workflow renders the same way every time.

Recommended approach:

1. Identify the Start node.
2. Build a level order from the Start node through the outgoing connectors.
3. Place each level below the previous one.
4. Spread sibling nodes horizontally within the same level.
5. Keep branch labels near the connector path.
6. Keep enough spacing for labels and editing handles.

The layout engine should avoid overlapping nodes. If the workflow is too wide or too deep, the Fit to View action should make the whole draft visible.

## Editing rules

The diagram must stay editable after generation.

- Node moves should update the current draft state.
- Label edits should update the current draft state.
- New nodes and connectors should update the current draft state.
- Undo and redo should operate on the current draft state.
- Fit to View should not change the workflow data.

If the user edits the diagram and then generates again, the app must ask before replacing the draft.

## Prompt rules

The prompt must instruct the AI to return JSON only.
The prompt must not ask for coordinates.
The prompt must not ask the model to execute, save, publish, or optimize the workflow.
The prompt must ask for warnings when the description is incomplete or ambiguous.

The prompt should also restate the supported node types and limits so the model can stay inside the contract.

## Validation pipeline

Use this order:

1. Read the description.
2. Check input length.
3. Send the prompt.
4. Parse the response.
5. Validate the workflow response.
6. Build the diagram draft.
7. Replace the current diagram only after the response passes validation.

If any step fails, keep the current diagram in place.

## Error handling rules

- Empty input: do not send a request.
- Over-limit input: block generation and tell the user to shorten the text.
- Invalid JSON: keep the current draft and show Retry.
- Invalid graph structure: keep the current draft and show Retry.
- Warning-only response: render the draft and show the warning banner.
- Service failure: preserve the current input and draft.

## Suggested page state fields

The view model should track at least these values:

- description text
- selected example
- loading state
- current workflow response
- current diagram model
- warning list
- error message
- dirty state
- can generate state
- can accept state
- can regenerate state
- can discard state

## Testing seams

Design the code so tests can run at these seams:

- the AI extraction service interface
- the validator
- the diagram draft builder
- the view model commands
- the confirmation flow for regeneration
- the page layout state for desktop and mobile

These seams let the harness test the full feature without a live AI provider.
