# Harness

## Purpose

The harness defines how to exercise the sample without depending on a live AI model during development. It gives the coding agent a deterministic way to verify validation, rendering, editing, and error handling.

## Harness strategy

Use a fake workflow extraction service during development and automated testing. The fake service should return canned JSON payloads that represent both valid and invalid AI responses.

The harness should also let a tester switch to the real provider through dependency injection without changing the page or the view model.

## Core fixtures

### Fixture 1: Happy path workflow

Input:

> An employee submits a purchase request. The manager reviews it. If the amount is below ₹50,000, the manager approves it. Otherwise, Finance reviews it. Approved requests create a purchase order. Rejected requests notify the employee.

Expected result:

- The page renders a flowchart.
- The workflow contains Start, Process, Decision, and End nodes.
- The decision node uses a short action label.
- The connectors show Yes and No labels where needed.
- The app shows a warning because the finance branch is incomplete.
- The diagram stays editable after render.

### Fixture 2: Complete and unambiguous workflow

Input:

> A request is submitted. The manager reviews it. If the request is valid, the manager approves it. Otherwise, the manager rejects it. Approved requests create a purchase order. Rejected requests notify the employee.

Expected result:

- The validator passes.
- The app renders the draft without warnings.
- The connector labels use Yes and No or equivalent condition labels.
- The user can accept the draft.

### Fixture 3: Invalid response

The fake service returns one of these failures:

- missing node IDs
- duplicate connector IDs
- unsupported node type
- self-referencing connector
- connector target that does not exist
- empty label

Expected result:

- Validation fails.
- The current diagram stays unchanged.
- The app shows a retry message.

### Fixture 4: Service failure

The fake service throws an exception or returns a timeout result.

Expected result:

- The current input stays visible.
- The current diagram stays visible if one already exists.
- The app shows an error message.

### Fixture 5: Over-limit input

The input exceeds 1,500 characters.

Expected result:

- The app blocks generation.
- The app asks the user to shorten the description.
- No service call runs.

## Fake service modes

The fake service should support these modes:

| Mode | Purpose |
| --- | --- |
| Valid | Return a valid workflow response. |
| Warning | Return a valid response with one or more warnings. |
| InvalidJson | Return malformed JSON. |
| InvalidGraph | Return a structurally invalid workflow. |
| Failure | Throw an exception or simulate a timeout. |

## Harness assertions

The harness should check these points:

- The description text reaches the extraction service unchanged.
- The validator blocks invalid graphs.
- The builder creates the right number of diagram nodes and connectors.
- The layout places the start node above downstream steps.
- Branch labels appear on decision connectors.
- Accept Draft is only available after a valid render.
- Regenerate prompts for confirmation when edits exist.
- Discard returns the page to the idle state.
- Warnings appear in the banner and do not block rendering by themselves.

## Diagram expectations

For a valid workflow, the harness should check:

- one Start node exists
- at least one End node exists
- every connector references valid nodes
- no node label is empty
- the diagram is editable after generation
- the diagram can be fit to view

## Manual verification checklist

1. Open the page.
2. Select the sample process.
3. Generate the draft.
4. Confirm the diagram renders.
5. Confirm the warning banner appears when the sample is ambiguous.
6. Move a node and edit a label.
7. Trigger Regenerate and confirm the replacement warning appears.
8. Accept the draft.
9. Discard the draft and confirm the page returns to the idle state.

## Expected sample output shape

The harness should expect JSON with these top-level keys:

- title
- nodes
- connectors
- warnings

The harness should not expect node coordinates from the AI.

## Pass criteria

The harness passes when:

- a valid workflow renders
- invalid workflows do not replace the current diagram
- warnings display without blocking valid content
- edits can be made after generation
- regeneration requires confirmation after edits
- the app never runs the workflow automatically
