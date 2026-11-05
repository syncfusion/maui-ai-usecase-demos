# Support Reply AI Draft Specification

## 1. Approved Source Specification
This specification is the approved product contract in the task prompt. It is the source of truth for implementation and for all requirement derivation.

### Product
AI-Powered Support Reply Drafting

### Target user
Customer-support agent

### Purpose
Generate an editable professional reply for one selected customer message using relevant recent conversation context while keeping Send under explicit agent control.

### Structured Response Contract
Required fields:
- `reply: string`
- `requiresMoreInformation: boolean`
- `requestedInformation: string[]`
- `warnings: string[]`

Constraints:
- Do not include `sourceIds`
- Reply length must not exceed 1,000 characters
- Raw JSON must never be displayed
- Invalid output must never replace valid composer content

### Approved Generated Reply
Thanks for confirming that the renewal was successful. I understand that the application still shows the license as expired after restarting. Please share your order ID so we can review the renewal status and assist you further.

### Fictional Conversation
- MSG-001: Alex Morgan, Customer, 9:12 AM — renewal completed but license still expired
- MSG-002: Jordan Lee, SupportAgent, 9:15 AM — asks whether renewal completed successfully
- MSG-003: Alex Morgan, Customer, 9:17 AM — confirms renewal and email receipt
- MSG-004: Alex Morgan, Customer, 9:18 AM — reports expired-license message after restart

MSG-004 is the selected message in the Generated Reply state.

## 2. EARS Requirements
- REQ-001: The system shall display one fictional support conversation with four predefined messages (three customer messages and one support-agent message).
- REQ-002: The system shall allow exactly one customer message to be selected.
- REQ-003: The system shall not allow support-agent messages to be selected.
- REQ-004: When a customer message is selected, the system shall show `Selected for reply`.
- REQ-005: While no customer message is selected, the system shall disable Draft Reply.
- REQ-006: While no customer message is selected, the system shall leave the Reply composer empty.
- REQ-007: While no customer message is selected, the system shall hide Regenerate.
- REQ-008: While no customer message is selected, the system shall disable Discard.
- REQ-009: While no customer message is selected, the system shall disable Send.
- REQ-010: When the user activates Draft Reply for a selected customer message, the system shall send the selected message and relevant recent conversation context to Azure OpenAI.
- REQ-011: While reply generation is in progress, the system shall display `Drafting reply...`.
- REQ-012: While reply generation is in progress, the system shall display a progress indicator.
- REQ-013: While reply generation is in progress, the system shall display `Drafting reply...` in the selected customer message card.
- REQ-014: While reply generation is in progress, the system shall display `Preparing a grounded reply...` in the Reply composer placeholder.
- REQ-015: While reply generation is in progress, the system shall keep the Reply composer empty.
- REQ-016: While reply generation is in progress, the system shall disable the Reply composer.
- REQ-017: While reply generation is in progress, the system shall disable Discard.
- REQ-018: While reply generation is in progress, the system shall disable Send.
- REQ-019: While reply generation is in progress, the system shall hide Regenerate.
- REQ-020: The system shall not display partial AI output.
- REQ-021: When the system receives valid structured AI output, the system shall validate it before display.
- REQ-022: The system shall reject malformed JSON.
- REQ-023: The system shall reject unsupported URLs.
- REQ-024: The system shall reject unsupported factual claims.
- REQ-025: The system shall reject unsupported verification claims.
- REQ-026: The system shall reject unsupported account-action claims.
- REQ-027: The system shall reject unsupported promise claims.
- REQ-028: The system shall reject output that exceeds 1,000 characters in `reply`.
- REQ-029: The system shall require `reply` to be present and non-whitespace.
- REQ-030: The system shall require `requiresMoreInformation` to be boolean.
- REQ-031: The system shall require `requestedInformation` to be a string array.
- REQ-032: The system shall require `warnings` to be a string array.
- REQ-033: When a valid reply is received, the system shall display it in the editable Reply composer.
- REQ-034: When a valid reply is received, the system shall display `AI draft`.
- REQ-035: When a valid reply is received, the system shall display `Review before sending`.
- REQ-036: When applicable, the system shall display requested information.
- REQ-037: While a valid reply is present, the system shall enable Discard.
- REQ-038: While a valid reply is present, the system shall enable Regenerate.
- REQ-039: While a valid reply is present, the system shall enable Send.
- REQ-040: When the user sends non-empty composer content, the system shall add that content as a support-agent message.
- REQ-041: When send succeeds, the system shall clear the composer.
- REQ-042: The system shall not label sent content as AI.
- REQ-043: The system shall not send automatically.
- REQ-044: While the composer is empty, the system shall disable Send.
- REQ-045: When generation fails with no valid draft, the system shall preserve the selected message and conversation.
- REQ-046: When generation fails with no valid draft, the system shall display `Unable to draft a reply`.
- REQ-047: When generation fails with no valid draft, the system shall display `The AI service is currently unavailable. Your conversation and selected message have been preserved.`
- REQ-048: When generation fails with no valid draft, the system shall display Retry.
- REQ-049: When regeneration fails, the system shall preserve the current draft.
- REQ-050: When regeneration fails, the system shall display `Unable to regenerate the reply`.
- REQ-051: When regeneration fails, the system shall display `Your current draft has been preserved.`
- REQ-052: When regeneration fails, the system shall display Retry.
- REQ-053: When the user discards a draft, the system shall clear the draft and indicators.
- REQ-054: When the user discards a draft, the system shall preserve the conversation and selection.
- REQ-055: When the user attempts to regenerate an edited draft, the system shall require confirmation before replacement.
- REQ-056: When the user confirms regeneration, the system shall replace the current draft.
- REQ-057: When the user cancels regeneration, the system shall preserve the current draft.
- REQ-058: When a cancellation or stale request occurs, the system shall not update current UI state.
- REQ-059: The system shall keep all sample data fictional.
- REQ-060: The system shall keep implementation inside `AI Solutions/Chat/SupportReplyAIDraft`.
- REQ-061: The system shall not modify `DesignReference` during implementation.
- REQ-062: The system shall not package the design-reference files as application resources.
- REQ-063: The system shall not use WebView to reproduce the design.
- REQ-064: The system shall not implement knowledge retrieval.
- REQ-065: The system shall not implement knowledge UI.
- REQ-066: The system shall not implement source IDs.
- REQ-067: The system shall not implement source cards.
- REQ-068: The system shall not implement automatic ticket resolution.
- REQ-069: The system shall not implement ticket classification.
- REQ-070: The system shall not implement sentiment analysis.
- REQ-071: The system shall not implement assignment or routing.
- REQ-072: The system shall not implement summarization.
- REQ-073: The system shall not implement external support-platform integration.
- REQ-074: The system shall not expose prompts, reasoning, tokens, or model details.
- REQ-075: The system shall not commit secrets.
- REQ-076: The system shall not add floating package versions.
- REQ-077: The system shall not restore or build during harness preparation.

- REQ-078: The system shall load the four predefined conversation messages in SfChat.
- REQ-079: The system shall render the three Alex Morgan messages as incoming and the one Jordan Lee message as outgoing.
- REQ-080: The system shall set Jordan Lee as `SfChat.CurrentUser`.
- REQ-081: The system shall start with no message selected.
- REQ-082: The system shall visibly mark the selected customer bubble.
- REQ-083: The system shall show Draft Reply only for the selected customer request.
- REQ-084: The system shall display generated text automatically in the single approved reply editor.
- REQ-085: The system shall show Regenerate and Discard only when draft text exists.
- REQ-086: The system shall disable Send for empty or whitespace-only text.
- REQ-087: When Send is activated, the system shall add the reviewed response to SfChat as a Jordan Lee outgoing message, then clear the draft and the selected request.
- REQ-088: The AI shall never send automatically.
- REQ-089: The system shall use standard .NET MAUI controls only above the Conversation header (application header, subject, case metadata).
- REQ-090: Below the Conversation header, SfChat shall be the single primary control extending through the bottom of the support workspace.
- REQ-091: The conversation and reply editing shall appear as one continuous SfChat surface, with the normal arbitrary customer-message composer hidden.
- REQ-092: The system shall not provide a separate Reply card, a second composer, a second Editor, or a visually disconnected page-level reply section.
- REQ-093: The system shall not replace SfChat with CollectionView, WebView, SfAIAssistView, or another chat control without human approval.
- REQ-094: Before implementing the reply area, the implementer shall verify the public APIs of Syncfusion.Maui.Chat 35.1.43 for multiline reply editing, Reply label, AI draft badge, Review before sending, drafting state, generation error state, Retry, Regenerate, Discard, Send, conditional action visibility, and Send disabled for empty text.
- REQ-095: If the approved reply UI cannot be achieved with a verified public SfChat API, the implementer shall stop, report the exact unsupported elements and the closest supported SfChat option, and obtain human approval before any fallback.
- REQ-096: The system shall not silently add an external page-level Editor or alternative control.
- REQ-097: Desktop and mobile shall closely match all four approved DesignReference states (initial, drafting, generated reply, generation error), following the approved hierarchy, layout, spacing, alignment, colors, typography, borders, controls, and responsive behavior.
- REQ-098: DesignReference HTML is the primary visual source of truth and PNG screenshots are supporting evidence.
- REQ-099: Static code inspection, build success, and `git diff --check` shall not count as visual validation.

## 3. Architecture and Visual Contracts

### 3.1 Desktop
- No unintended outer workspace padding.
- No unintended thin gray inset around the conversation.
- SUBJECT and `License still shows as expired after renewal` display horizontally.
- Case ID, Customer, and Status align according to the approved desktop reference.
- Polished, professional customer-support workspace appearance.
- Header, metadata, dividers, avatars, messages, bubbles, reply area, controls, proportions, and state-specific content match the approved reference.
- The approved hierarchy is preserved when the Mac Catalyst window is resized.

### 3.2 Mobile
- Same SfChat architecture and workflow as desktop.
- Matches all four approved mobile references.
- Supports touch message selection.
- Respects safe areas.
- Keeps the editor accessible above the software keyboard.
- Metadata and actions adapt without clipping or horizontal overflow.
- State-specific actions keep their meaning and visibility rules.

### 3.3 Evidence Gates
Gates are sequential; a later gate must not begin until the previous gate has human-confirmed evidence.
- Gate 1, Predefined conversations: four messages visibly render in SfChat; incoming/outgoing alignment is correct; no message is selected initially; local runtime evidence required.
- Gate 2, SfChat reply-editor feasibility: exact supported public API is identified; if unsupported, implementation stops for human approval.
- Gate 3, Message selection and Draft Reply: customer selection, selected state, and Draft Reply are runtime-verified.
- Gate 4, Azure OpenAI reply workflow: draft generation, editing, Review before sending, Regenerate, Discard, Retry, conditional Send, and sent-message insertion are runtime-verified.
- Gate 5, Desktop visual correction: all four desktop states are directly compared with DesignReference.
- Gate 6, Mobile correction: all four mobile states are directly compared with DesignReference.

## 4. Acceptance Criteria
- AC-001 through AC-099 map directly to REQ-001 through REQ-099.
