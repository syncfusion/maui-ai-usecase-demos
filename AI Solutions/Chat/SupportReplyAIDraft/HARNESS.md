# Support Reply AI Draft Internal Harness

## Source Priority
1. Approved product contract in the task prompt
2. Derived EARS requirements and acceptance criteria in this harness set
3. DesignReference HTML, then PNG as backup visual reference

## Allowed Path
- `AI Solutions/Chat/SupportReplyAIDraft`

## Frozen Artifacts
- `AI Solutions/Chat/SupportReplyAIDraft/DesignReference`
- Fictional conversation content
- Structured JSON field names
- Approved workflow and state names

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
- The same approach fails three times
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
