# Development Tasks

## Task 1: Create the solution shape

Goal:
Set up the page, view model, model classes, and service interfaces for the sample.

Work items:

- Add the ProcessDiagramPage view.
- Add the ProcessDiagramViewModel class.
- Add workflow model types for nodes, connectors, warnings, and response metadata.
- Add the IWorkflowExtractionService interface.
- Add the WorkflowResponseValidator class.
- Add the DiagramDraftBuilder class.

Done when:

- the page can bind to the view model
- the project compiles with the new contracts in place
- no AI provider code is tied directly to the page

## Task 2: Define the workflow data contract

Goal:
Create the JSON model that the AI must return.

Work items:

- Define the supported node types.
- Define the workflow response model.
- Define the node and connector models.
- Define the warning model.
- Define the input and output limits.

Done when:

- the response schema matches the specification
- the model supports the sample workflow
- the model can be serialized and deserialized cleanly

## Task 3: Implement response validation

Goal:
Reject invalid workflow responses before they reach the diagram.

Work items:

- Check for unique node IDs.
- Check for unique connector IDs.
- Check for valid source and target references.
- Check for exactly one Start node.
- Check for at least one End node.
- Check for supported node types.
- Check for non-empty labels.
- Check for self-referencing connectors.
- Check for limit violations.

Done when:

- invalid responses are blocked
- the current diagram stays unchanged after a failed validation
- validation errors are easy to show in the UI

## Task 4: Build the diagram draft from the validated workflow

Goal:
Translate the validated workflow into Syncfusion diagram nodes and connectors.

Work items:

- Map start, process, decision, and end nodes to diagram shapes.
- Map connectors and branch labels.
- Calculate positions for a top-to-bottom layout.
- Apply orthogonal routing.
- Keep the rendering deterministic.

Done when:

- the sample workflow renders as a readable flowchart
- no coordinate data comes from the AI
- connector labels appear on the rendered branches

## Task 5: Implement the Azure OpenAI extraction service

Goal:
Hide Azure OpenAI behind the service interface.

Work items:

- Build the prompt that asks for JSON only.
- Send the user description to Azure OpenAI.
- Parse the Azure OpenAI response into the workflow model.
- Surface warnings from the AI response.
- Keep the implementation swappable through dependency injection.

Done when:

- the view model can call the service without knowing the provider
- a fake service can replace the real provider in tests
- the service can return both valid and warning-only responses

## Task 6: Implement the view model workflow

Goal:
Manage the screen state and button actions.

Work items:

- Bind the description text.
- Bind the example selector.
- Run Generate Draft.
- Show loading, warning, and error states.
- Accept the draft.
- Regenerate the draft.
- Discard the draft.
- Track whether the current draft has unsaved edits.

Done when:

- the page updates correctly for each action
- invalid responses do not replace the current draft
- regeneration asks for confirmation after edits

## Task 7: Build the responsive page layout

Goal:
Create the desktop and mobile presentation.

Work items:

- Build the two-column desktop layout.
- Build the vertical mobile layout.
- Collapse the input section after generation on mobile.
- Add the page title.
- Add the warning banner.
- Add the main action buttons.

Done when:

- the page is usable on both desktop and mobile
- the diagram gets more space after generation on mobile
- the input controls remain clear and reachable

## Task 8: Enable editing on the diagram surface

Goal:
Let the user adjust the generated draft.

Work items:

- Allow node movement.
- Allow node label edits.
- Allow node add and delete actions.
- Allow reconnecting nodes.
- Allow branch label edits.
- Enable undo and redo.
- Add a fit-to-view action.

Done when:

- edits stay in the current draft
- undo and redo work
- the diagram can be fit to the viewport

## Task 9: Add user feedback and error handling

Goal:
Keep the screen safe and predictable when the AI fails.

Work items:

- Disable Generate Draft for empty input.
- Block input that exceeds the limit.
- Preserve the description after service failure.
- Preserve the current diagram after invalid responses.
- Show a retry action for validation failures and service failures.

Done when:

- the user never loses the current draft because of an invalid response
- the user always sees why generation stopped

## Task 10: Add harness cases and verification steps

Goal:
Make the sample easy to test without a live AI provider.

Work items:

- Add fake service responses for valid, warning, invalid, and failure cases.
- Add the canonical purchase request example.
- Verify the validator and builder through the harness.
- Verify the confirmation flow for regeneration.
- Verify the single-screen experience.

Done when:

- the harness exercises the full workflow
- each acceptance criterion has a matching check

## Implementation order

Build the sample in this order:

1. Define the models and interfaces.
2. Implement validation.
3. Implement diagram building and layout.
4. Implement the AI service wrapper.
5. Implement the view model.
6. Build the UI.
7. Enable editing actions.
8. Add error handling and confirmations.
9. Add harness cases.
10. Verify the acceptance criteria.
