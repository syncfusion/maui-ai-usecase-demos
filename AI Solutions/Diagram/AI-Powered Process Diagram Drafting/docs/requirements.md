# Requirements

## Functional requirements

| ID | Requirement | Verification |
| --- | --- | --- |
| FR-01 | The page must show a title, a multiline process description box, an example selector, a Generate Draft button, a diagram surface, an AI warning banner, and Accept, Regenerate, and Discard actions. | Open the page and confirm all controls are present. |
| FR-02 | The Generate Draft button must stay disabled while the description is empty. | Clear the input and confirm the button is disabled. |
| FR-03 | The Generate Draft button must stay disabled while the description exceeds 1,500 characters. | Paste a long description and confirm the button is disabled or blocked with a limit message. |
| FR-04 | The example selector must populate the description box with a predefined sample. | Select an example and confirm the text changes. |
| FR-05 | Generate Draft must send the current description to the AI extraction service. | Use a fake service and verify the request text matches the input. |
| FR-06 | The app must validate the AI response before it updates the diagram. | Return an invalid response and confirm the current diagram does not change. |
| FR-07 | The app must accept only these node types: start, process, decision, and end. | Return an unsupported node type and confirm validation fails. |
| FR-08 | The app must reject duplicate node IDs, duplicate connector IDs, missing node references, self-referencing connectors, and empty labels. | Run validation cases for each failure mode. |
| FR-09 | A valid response must render as a diagram using Syncfusion .NET MAUI Diagram. | Use the sample workflow and confirm the diagram appears. |
| FR-10 | Node positions must be calculated by the app, not returned by the AI. | Inspect the AI payload and confirm no coordinate fields are required. |
| FR-11 | The diagram must use a top-to-bottom layout with orthogonal connectors. | Check the rendered flow direction and connector shape. |
| FR-12 | The app must support branch labels such as Yes, No, or a condition label. | Render a decision node with labeled branches. |
| FR-13 | The user must be able to move nodes, edit node labels, add nodes, delete nodes, reconnect nodes, and edit branch labels. | Perform each edit in the diagram and confirm the change persists in the current draft. |
| FR-14 | The app must support undo and redo for diagram edits. | Make an edit, undo it, and redo it. |
| FR-15 | The app must provide a Fit to View action for the diagram. | Zoom and pan the diagram, then fit it to the visible area. |
| FR-16 | The app must show an AI warning banner when the AI response includes warnings. | Return warnings and confirm the banner appears. |
| FR-17 | The app must keep the current diagram when the AI service fails or returns invalid content. | Force a failure and confirm the current draft stays visible. |
| FR-18 | The app must ask for confirmation before regeneration replaces manual edits. | Edit the draft, select Regenerate, and confirm the warning dialog appears. |
| FR-19 | The app must not accept, publish, or execute the workflow automatically. | Verify no action runs without an explicit user choice. |
| FR-20 | The app must remain a single-screen experience. | Confirm the main flow stays on one page. |

## Non-functional requirements

| ID | Requirement | Verification |
| --- | --- | --- |
| NFR-01 | The app must feel responsive while the AI request is running. | Trigger generation and confirm the UI shows a busy state instead of freezing. |
| NFR-02 | The app must handle short and medium workflow descriptions without delay from diagram layout. | Generate the sample workflow and confirm the diagram renders promptly. |
| NFR-03 | The page must remain usable on desktop and mobile form factors. | Test both layouts and confirm controls remain reachable. |
| NFR-04 | The UI must be accessible with readable labels and clear button names. | Review with a screen reader or accessibility checker. |
| NFR-05 | The AI contract must remain behind an interface so the provider can be swapped without changing the page. | Replace the service implementation in dependency injection and confirm the page still works. |
| NFR-06 | The app must not persist data automatically. | Restart the app and confirm no draft is restored unless the user recreates it. |
| NFR-07 | The AI prompt must not ask the model to generate coordinates, execute actions, or publish a workflow. | Review the prompt and confirm those instructions are absent. |
| NFR-08 | The app must preserve the typed description after service failure. | Fail the AI service and confirm the input text remains visible. |

## Data limits

| Item | Limit |
| --- | --- |
| Process description | 1,500 characters |
| Nodes | 20 |
| Decision nodes | 6 |
| Connectors | 30 |
| Node label | 60 characters |
| Connector label | 30 characters |

## Validation rules

The validator must reject any AI response that breaks these rules:

- Node IDs must be unique.
- Connector IDs must be unique.
- Every connector source must match an existing node.
- Every connector target must match an existing node.
- Exactly one Start node must exist.
- At least one End node must exist.
- Node labels must not be empty.
- Connector labels must not be longer than the limit.
- A connector must not point from a node to itself.
- The response must not exceed the limit for nodes or connectors.

## Acceptance checks

The sample is complete when all of these checks pass:

1. A valid process description generates a readable flowchart.
2. Invalid responses do not replace the current diagram.
3. Warnings show when the AI cannot fully resolve the workflow.
4. The generated diagram is editable.
5. Regeneration requires confirmation after the user edits the draft.
6. The user explicitly accepts the draft before it is treated as final.
7. The app does not auto-run or auto-publish the workflow.
