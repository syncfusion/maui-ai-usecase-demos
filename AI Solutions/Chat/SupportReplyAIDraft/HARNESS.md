# Support Reply AI Draft Internal Harness

## Source Priority
1. Approved product contract in the task prompt
2. Derived EARS requirements and acceptance criteria in this harness set
3. DesignReference HTML (primary visual source of truth), then PNG screenshots (supporting visual evidence)

## Allowed Path
- `AI Solutions/Chat/SupportReplyAIDraft`

## Frozen Artifacts
- `AI Solutions/Chat/SupportReplyAIDraft/DesignReference`
- Fictional conversation content
- Structured JSON field names
- Approved workflow and state names

## DesignReference Contract
- DesignReference HTML is the primary visual source of truth; PNG screenshots are supporting visual evidence.
- DesignReference must remain unchanged.
- Desktop and mobile must closely match all four approved states: initial, drafting, generated reply, generation error.
- The implementation must follow the approved hierarchy, layout, spacing, alignment, colors, typography, borders, controls, and responsive behavior.
- Static code inspection, build success, and `git diff --check` are not visual validation. Visual validation requires running the app and comparing each state directly with DesignReference.

## SfChat Architecture Contract
- Standard .NET MAUI controls may be used above the Conversation header for the application header, subject, and case metadata.
- Below the Conversation header, SfChat must be the single primary control and must extend through the bottom of the support workspace.
- The conversation and reply editing must appear as one continuous SfChat surface using the native SfChat composer, native multiline Editor, and native Send button.
- Reply workflow elements use supported native SfChat capabilities only: typing indicator (`Drafting reply...`), SfChat suggestions (Regenerate, Discard, Retry), message templates (customer-message selection and Draft Reply), and supported composer styling.
- The native composer stays disabled or read-only until the workflow permits reply editing.
- Prohibited: a custom page-level reply footer, a separate Reply heading or reply panel, an external or second Editor, an external Send button, or a second composer.
- Prohibited without human approval: replacing SfChat with CollectionView, WebView, SfAIAssistView, or another chat control.

## SfChat Feasibility (Gate 2 Result)
Verified for Syncfusion.Maui.Chat 35.1.43: native Editor access, `AllowMultilineInput`, `SendMessage` event and `SendMessageCommand`, `ShowMessageInputView`, typing indicator, suggestions, message templates, attachment-button template, and supported composer styling.
Not provided: a documented public full composer/footer template for arbitrary controls.
Approved adaptation: native Editor for the generated draft, native Send, typing indicator for drafting, suggestions for Regenerate, Discard, and Retry, and message templates for selection and Draft Reply. No custom footer or external Editor is approved.
If any further required element cannot be achieved with a verified public SfChat API, stop, report the limitation and closest supported option, and request human approval. Never silently add an external page-level Editor or alternative control.

## Mandatory Gate Order
Gates are sequential. Do not begin a later gate until the previous gate has human-confirmed evidence.
1. Gate 1: Predefined conversations (local runtime evidence required)
2. Gate 2: SfChat reply-editor feasibility (Complete; SfChat-native adaptation approved)
3. Gate 3: Message selection and Draft Reply (runtime-verified)
4. Gate 4: Azure OpenAI reply workflow (runtime-verified)
5. Gate 5: Desktop visual correction (all four states compared with DesignReference)
6. Gate 6: Mobile correction (all four states compared with DesignReference)
Gate details and acceptance evidence are defined in `SPEC.md`; status is tracked in `TASKS.md`.

Current checkpoint: Gate 4 is still in progress. Do not start Gate 5 or Gate 6 until human runtime evidence confirms the latest Gate 4 implementation.

## Document Governance
- `HARNESS.md` changes only when permanent execution rules change.
- `INTENT.md` changes only when approved product intent changes.
- `SPEC.md` changes only when approved requirements or architecture change.
- `PLAN.md` changes only when the gate sequence or current gate changes.
- `TASKS.md` changes regularly for progress, blockers, issues, and evidence.
- `README.md` changes only when user-facing configuration or finished behavior changes.
- New implementation bugs normally update `TASKS.md` only.
- Stable documents must not contain bug history, failed attempts, compiler errors, or commit history.

## Scope and UI Constraints
- No WebView or copied web code may be used.
- No knowledge retrieval, knowledge UI, source IDs, or source panels may be introduced.
- No automatic sending may be introduced.

## Package and Secret Policy
- Use `Syncfusion.Maui.Chat` 35.1.43.
- Use `Syncfusion.Maui.Core` 35.1.43.
- Use exact version 35.1.43 consistently for all required Syncfusion packages.
- Do not use floating versions.
- Authoritative sources:
  - https://www.nuget.org/packages/Syncfusion.Maui.Chat/35.1.43
  - https://www.nuget.org/packages/Syncfusion.Maui.Core/35.1.43
  - https://help.syncfusion.com/maui/chat/getting-started
  - https://help.syncfusion.com/maui/chat/messages
- Verified compatibility: Syncfusion.Maui.Chat 35.1.43 supports .NET 10 and depends on Syncfusion.Maui.Core 35.1.43 or later.
- Never write real secret values.
- Allowed secret sources only: `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, `AZURE_OPENAI_DEPLOYMENT_NAME`, `SYNCFUSION_LICENSE_KEY`.

## Verification Status
- Syncfusion package verification: **Complete**.
- Required registration: import `Syncfusion.Maui.Core.Hosting` and call `ConfigureSyncfusionCore()` in `MauiProgram.cs`.
- License registration: register the license from `SYNCFUSION_LICENSE_KEY` only when a non-empty value is available.

## Required Notes for Later Agent Mode
- `SfChat` supports custom message templates through `ChatMessageTemplateSelector`.
- Required Syncfusion registration must be recorded.
- Whether `ConfigureSyncfusionCore` is required must be recorded.
- `SfChat` message-template strategy must be recorded.
- Customer-message selection approach must be recorded.
- Selected for reply presentation must be recorded.
- Draft Reply placement must be recorded.
- The exact selected-message and Draft Reply template implementation was the first inspection task in the implementation phase.

## Permissions and Approval Gates
Approval is required before:
- Restore or build
- Tool installation
- Shared-file changes
- Commit or push
- Pull request
- Deletion other than approved harness cleanup

## Completion Evidence
- Internal harness files remain in the allowed path.
- Implementation changes stay within the allowed path.
- No design-reference files modified.
- Package verification recorded with verified versions, sources, and compatibility.

## Stop Rules
Stop when:
- Specification is contradictory
- Package versions cannot be verified
- A required change exceeds the allowed path
- A shared-file change is required
- A real secret is found
- A speculative third-party control-model or API approach fails once in local runtime validation
- Required tooling or SfChat capability cannot be verified
- Package compatibility is unresolved
- Scope expansion or approval is required

## Cleanup and Final Git Review
Before final implementation commit and push:
1. Keep `HARNESS.md`, `INTENT.md`, `SPEC.md`, `PLAN.md`, and `TASKS.md` in place.
2. Verify Git status and staged files contain only the intended implementation changes.
3. Confirm no temporary harness artifacts remain outside the preserved harness files.

## Optional Build Policy
- Build is deferred until explicit approval.
- No restore or build in harness-preparation work.
