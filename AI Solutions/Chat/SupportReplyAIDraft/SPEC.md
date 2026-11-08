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
- Invalid output must never replace valid Editor content

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
- REQ-006: While no customer message is selected, the system shall leave the native SfChat Editor empty and disabled or read-only with the placeholder `Select a customer message to begin.`
- REQ-007: While no customer message is selected, the system shall not show Regenerate, Discard, or Retry suggestions.
- REQ-008: While no draft exists, the system shall not show Discard.
- REQ-009: While no customer message is selected, the system shall disable the native Send button.
- REQ-010: When the user activates Draft Reply for a selected customer message, the system shall send the selected message and relevant recent conversation context to Azure OpenAI.
- REQ-011: While reply generation is in progress, the system shall display `Drafting reply...` using the SfChat typing indicator.
- REQ-012: While reply generation is in progress, the system shall display a progress indicator in the Draft Reply area of the selected customer message and the SfChat typing indicator.
- REQ-013: While reply generation is in progress, the system shall keep the selected customer message visibly selected and show Draft Reply in a processing or disabled state (`Drafting reply...`).
- REQ-014: While reply generation is in progress, the system shall display the placeholder `Drafting reply...` in the native Editor.
- REQ-015: While reply generation is in progress, the system shall keep the native Editor empty.
- REQ-016: While reply generation is in progress, the system shall disable the native Editor.
- REQ-017: While reply generation is in progress, the system shall not show Discard.
- REQ-018: While reply generation is in progress, the system shall disable the native Send button.
- REQ-019: While reply generation is in progress, the system shall not show Regenerate, Discard, or Retry suggestions.
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
- REQ-033: When a valid reply is received, the system shall insert it automatically into the editable native multiline SfChat Editor.
- REQ-034: When a valid reply is received, the system shall hide the typing indicator and Draft Reply.
- REQ-035: When a valid reply is received, the system shall display `Review and edit before sending.`
- REQ-036: When applicable, the system shall display requested information.
- REQ-037: While a valid reply is present, the system shall show the Discard suggestion above the composer.
- REQ-038: While a valid reply is present, the system shall show the Regenerate suggestion above the composer.
- REQ-039: While the Editor text is non-empty and not whitespace, the system shall enable the native Send button as the primary action.
- REQ-040: When the user activates native Send with non-empty content, the system shall add that content as a Jordan Lee outgoing TextMessage in SfChat without calling Azure OpenAI.
- REQ-041: When send succeeds, the system shall clear the native Editor, clear suggestions, clear the selected request, return the composer to the initial disabled state, scroll to the new outgoing message, and prevent duplicate messages.
- REQ-042: The system shall not label sent content as AI.
- REQ-043: The system shall not send automatically.
- REQ-044: While the Editor text is empty or whitespace, the system shall disable the native Send button.
- REQ-045: When generation fails with no valid draft, the system shall preserve the selected customer message and conversation, hide the typing indicator, and keep the native Editor empty and disabled with the placeholder `Unable to generate a reply.`
- REQ-046: When generation fails with no valid draft, the system shall not show Regenerate or Discard.
- REQ-047: When generation fails with no valid draft, the system shall display the inline error `We couldn’t generate a reply. Please try again.`
- REQ-048: When generation fails with no valid draft, the system shall show an SfChat Retry suggestion; Retry returns to the drafting state with the same selected request and conversation context, and native Send remains disabled.
- REQ-049: When regeneration fails, the system shall preserve the current draft.
- REQ-050: When regeneration fails, the system shall display the inline error `We couldn’t generate a reply. Please try again.`
- REQ-051: When regeneration fails, the system shall preserve the current Editor text.
- REQ-052: When regeneration fails, the system shall display Retry.
- REQ-053: When the user discards a draft, the system shall clear the native Editor and suggestions and return to the selected state.
- REQ-054: When the user discards a draft, the system shall preserve the conversation and selection.
- REQ-055: When the user activates Regenerate on an edited draft, the system shall require confirmation before replacement.
- REQ-056: When the user activates Regenerate (and confirms when required), the system shall call Azure OpenAI again and replace the current Editor text.
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
- REQ-083: The system shall show Draft Reply only with the selected customer request, through an SfChat message template.
- REQ-084: The system shall display generated text automatically in the native multiline SfChat Editor.
- REQ-085: The system shall show Regenerate and Discard SfChat suggestions only when draft text exists.
- REQ-086: The system shall disable native Send for empty or whitespace-only text.
- REQ-087: When native Send is activated, the system shall add the reviewed response to SfChat as a Jordan Lee outgoing TextMessage, then clear the Editor, suggestions, and selected request.
- REQ-088: The AI shall never send automatically.
- REQ-089: The system shall use standard .NET MAUI controls only above the Conversation header (application header, subject, case metadata).
- REQ-090: Below the Conversation header, SfChat shall be the single primary control extending through the bottom of the support workspace.
- REQ-091: The conversation and reply editing shall appear as one continuous SfChat surface using the native SfChat composer, which is disabled until the workflow permits reply editing.
- REQ-092: The system shall not provide a custom page-level reply footer, a separate Reply heading or panel, a second composer, a second Editor, an external Send button, or a visually disconnected reply section.
- REQ-093: The system shall not replace SfChat with CollectionView, WebView, SfAIAssistView, or another chat control without human approval.
- REQ-094: Gate 2 verified that Syncfusion.Maui.Chat 35.1.43 supports native Editor access, AllowMultilineInput, SendMessage/SendMessageCommand, ShowMessageInputView, typing indicator, suggestions, message templates, attachment-button template, and composer styling, but no documented public full composer/footer template for arbitrary controls.
- REQ-095: The implementation shall use the approved SfChat-native adaptation: native Editor and Send, typing indicator for drafting, suggestions for Regenerate, Discard, and Retry, and message templates for selection and Draft Reply; any further unsupported element requires stopping and human approval.
- REQ-096: The system shall not add an external page-level Editor, custom footer, or alternative control without human approval.
- REQ-097: Desktop and mobile shall closely match all four approved DesignReference states (initial, drafting, generated reply, generation error), following the approved hierarchy, layout, spacing, alignment, colors, typography, borders, controls, and responsive behavior.
- REQ-098: DesignReference HTML is the primary visual source of truth and PNG screenshots are supporting evidence.
- REQ-099: Static code inspection, build success, and `git diff --check` shall not count as visual validation.

## 3. Architecture and Visual Contracts

### 3.0 SfChat-Native State Contract
DesignReference at commit 90d0518 is the visual source of truth. Syncfusion.Maui.Chat 35.1.43 has no documented public full composer/footer template, so the design uses native SfChat capabilities only.
- Initial: four messages, none selected; native Editor disabled or read-only, placeholder `Select a customer message to begin.`; Send disabled; no Draft Reply, typing indicator, suggestions, Regenerate, Discard, or Retry.
- Selected: only a customer request selectable; one bubble marked `Selected for reply` with Draft Reply; Editor and Send disabled.
- Drafting: selected request stays marked; Draft Reply processing/disabled (`Drafting reply...`); SfChat typing indicator `Drafting reply...`; Editor and Send disabled; no Regenerate, Discard, or Retry.
- Generated reply: typing indicator and Draft Reply hidden; generated text inserted in the editable multiline native Editor (scrollable when long); `Review and edit before sending.`; Regenerate and Discard suggestions above the composer; Send enabled only for non-whitespace text and is the primary action.
- Generation error: selected message preserved; typing indicator hidden; Editor empty and disabled, placeholder `Unable to generate a reply.`; inline error `We couldn’t generate a reply. Please try again.`; Retry suggestion only; Send disabled.
- Send: adds Jordan Lee outgoing TextMessage, never calls Azure OpenAI, never automatic; then clears Editor, suggestions, and selection, returns to the initial disabled state, and scrolls to the new message.

### 3.1 Desktop
- No unintended outer workspace padding.
- No unintended thin gray inset around the conversation.
- SUBJECT and `License still shows as expired after renewal` display horizontally.
- Case ID, Customer, and Status align according to the approved desktop reference.
- Polished, professional customer-support workspace appearance.
- Header, metadata, dividers, avatars, messages, bubbles, reply area, controls, proportions, and state-specific content match the approved reference.
- The approved hierarchy is preserved when the Mac Catalyst window is resized.
- SfChat fills the remaining height below the Conversation header; the composer may expand upward while the message area remains scrollable.
- No separate Reply panel or disconnected controls.

### 3.2 Mobile
- Same SfChat architecture and workflow as desktop.
- Matches all four approved mobile references.
- Supports touch message selection.
- Respects safe areas.
- Keeps the native composer above the software keyboard and safe area.
- Composer expansion reduces the visible conversation viewport.
- Keeps suggestions accessible without clipping; one Editor and one native Send button; no overlap or horizontal overflow.
- Metadata and actions adapt without clipping or horizontal overflow.
- State-specific actions keep their meaning and visibility rules.

### 3.3 Evidence Gates
Gates are sequential; a later gate must not begin until the previous gate has human-confirmed evidence.
- Gate 1, Predefined conversations: four messages visibly render in SfChat; incoming/outgoing alignment is correct; no message is selected initially; local runtime evidence required.
- Gate 2, SfChat reply-editor feasibility (Complete): public API limitation verified and the SfChat-native adaptation approved; no custom footer or external Editor.
- Gate 3, Message selection and Draft Reply (In Progress): customer selection, selected state, and Draft Reply are runtime-verified; scope excludes AI generation.
- Gate 4, Azure OpenAI reply workflow: draft generation, editing, Review and edit before sending, Regenerate, Discard, Retry, conditional Send, and sent-message insertion are runtime-verified.
  - Current status: implementation updates are in progress; final Gate 4 completion still needs fresh human runtime evidence for the latest commit.
- Gate 5, Desktop visual correction: all four desktop states are directly compared with DesignReference.
- Gate 6, Mobile correction: all four mobile states are directly compared with DesignReference.

## 4. Acceptance Criteria
- Each REQ item is accepted only when its observable behavior is verified through the applicable evidence gate in Section 3.3. Static inspection, build success, and git diff --check alone are insufficient for runtime or visual requirements.
