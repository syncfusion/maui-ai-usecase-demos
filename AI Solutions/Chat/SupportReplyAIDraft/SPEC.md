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
- REQ-001: The system shall display one fictional support conversation with three customer messages and one support-agent message.
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

## 3. Acceptance Criteria
- AC-001 through AC-077 map directly to REQ-001 through REQ-077.
