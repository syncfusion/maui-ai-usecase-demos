# User Flows

## State model

The page moves through these states:

- Idle: no draft is loaded.
- Ready: the user has entered text and can generate a draft.
- Generating: the app is waiting for the AI response.
- DraftReady: the diagram is rendered and editable.
- Edited: the user has changed the generated draft.
- ConfirmingRegeneration: the user asked to regenerate after edits.
- Error: generation failed, but the input and current draft stay visible.

## Desktop flow

1. The page opens in a two-column layout.
2. The left column shows the description box, the example selector, and the Generate Draft button.
3. The right column shows the diagram surface.
4. The user types a process description or selects an example.
5. The user selects Generate Draft.
6. The app disables editing actions while the request is running.
7. The AI returns structured workflow data.
8. The app validates the response.
9. The app renders nodes and connectors on the diagram surface.
10. The user reviews the draft and edits it if needed.
11. The user chooses Accept Draft, Regenerate, or Discard.

## Mobile flow

1. The page opens in a vertical layout.
2. The description box appears first.
3. The Generate Draft button appears under the input.
4. The diagram surface appears below the button.
5. The review actions appear below the diagram.
6. After generation, the app collapses the input section so the diagram gets more space.
7. The user edits the draft or accepts it.

## Happy path

1. The user enters the process description.
2. The user selects Generate Draft.
3. The app sends the text to the extraction service.
4. The service returns nodes, connectors, and warnings.
5. The validator checks the structure.
6. The builder creates the diagram model.
7. The page displays the diagram.
8. The warning banner appears if warnings exist.
9. The user reviews the draft.
10. The user selects Accept Draft.

## Example process flow

Use this sample in the selector and in the harness:

> An employee submits a purchase request. The manager reviews it. If the amount is below ₹50,000, the manager approves it. Otherwise, Finance reviews it. Approved requests create a purchase order. Rejected requests notify the employee.

Expected behaviour:

- The app extracts Start, Process, Decision, and End nodes.
- The app renders a decision for the amount check.
- The app creates one branch for the approval path and one branch for the finance path.
- The app shows a warning because the finance outcome is not fully specified.
- The app does not invent any extra business step.

## Editing flow

1. The generated diagram is editable as soon as it appears.
2. The user moves a node, changes a label, or reconnects a line.
3. The app keeps the updated draft in memory.
4. Undo and redo remain available.
5. The user can fit the diagram to the view at any time.

## Regeneration after edits

1. The user edits the draft.
2. The user selects Regenerate.
3. The app checks whether the draft has unsaved edits.
4. The app shows this confirmation message:

   > Regenerating will replace your current diagram edits. Do you want to continue?

5. If the user confirms, the app asks the AI for a new draft.
6. If the user cancels, the current diagram stays in place.

## Discard flow

1. The user selects Discard.
2. The app removes the current draft from the diagram surface.
3. The app returns to the idle state.
4. The typed description stays in the input so the user can regenerate later if needed.

## Invalid response flow

1. The AI returns a response that fails validation.
2. The app keeps the current diagram unchanged.
3. The app shows a retry message.
4. The app keeps the user input in the description box.

## Ambiguous branch flow

1. The AI returns valid nodes and connectors.
2. The AI also includes a warning about a missing or unclear outcome.
3. The app renders the draft.
4. The app shows the warning banner.
5. The app does not block the diagram unless the structural validation fails.

## Service failure flow

1. The extraction service fails or times out.
2. The app keeps the current input.
3. The app keeps the current diagram if one already exists.
4. The app shows an error message and a retry action.

## Empty input flow

1. The input box is empty.
2. The Generate Draft button stays disabled.
3. The app does not send a request.

## Over-limit flow

1. The user pastes a description that is longer than the character limit.
2. The app blocks generation.
3. The app asks the user to shorten the description.
