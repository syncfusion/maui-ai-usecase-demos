# Support Reply AI Draft Plan

Gates are sequential. A later gate must not begin until the previous gate has human-confirmed evidence. Detailed gate criteria are in `SPEC.md`; operational status is in `TASKS.md`.

## Ordered Gates
1. Gate 1: Predefined conversations
2. Gate 2: SfChat reply-editor feasibility
3. Gate 3: Message selection and Draft Reply
4. Gate 4: Azure OpenAI reply workflow
5. Gate 5: Desktop visual correction
6. Gate 6: Mobile correction

## Current Gate
Gate 4: Azure OpenAI reply workflow.

Scope: draft generation, editing, Review and edit before sending, Regenerate, Discard, Retry, conditional Send, and sent-message insertion as Jordan Lee outgoing.

Gate 4 requires human-confirmed runtime evidence before Gate 5 can begin.

Stop condition: any need for a custom footer, external Editor, or alternative chat control requires human approval.
