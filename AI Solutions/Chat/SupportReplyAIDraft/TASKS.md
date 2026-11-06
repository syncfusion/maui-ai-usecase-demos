# Support Reply AI Draft Tasks

| ID | Task | Requirement IDs | Status | Dependencies | Allowed paths | Completion evidence | Blocker |
|---|---|---|---|---|---|---|---|
| T-001 | Verify authoritative Syncfusion package versions and record the result | REQ-074, REQ-076 | Complete | Authoritative package sources | `AI Solutions/Chat/SupportReplyAIDraft` | Verified versions 35.1.43 recorded with source URLs and .NET 10 compatibility | None |
| T-002 | Record the approved product contract and harness rules | REQ-001 through REQ-099 | Complete | Approved prompt contract | `AI Solutions/Chat/SupportReplyAIDraft` | Harness files record the full permanent contract (REQ-001 through REQ-099) | None |
| T-003 | Map requirements to acceptance criteria | REQ-001 through REQ-099 | Complete | SPEC.md | `AI Solutions/Chat/SupportReplyAIDraft` | Every requirement through REQ-099 has a corresponding acceptance criterion documented | None |
| T-004 | Capture package, secret, and cleanup policy | REQ-075, REQ-076, REQ-077 | Complete | Approved prompt contract | `AI Solutions/Chat/SupportReplyAIDraft` | Policy sections recorded in HARNESS.md and SPEC.md | None |
| G-1 | Gate 1: Predefined conversations | REQ-001, REQ-078 through REQ-081 | Complete | Package verification | `AI Solutions/Chat/SupportReplyAIDraft` | Human-confirmed local evidence for commit 9d1b748f: Mac Catalyst Debug arm64 build succeeded; app launched and remained open; four predefined SfChat messages rendered; three Alex Morgan incoming left-aligned and one Jordan Lee outgoing right-aligned; names and timestamps rendered; no message selected initially; Draft Reply not visible initially; no startup crash | None |
| G-2 | Gate 2: SfChat reply-editor feasibility | REQ-090 through REQ-096 | Complete | G-1 | `AI Solutions/Chat/SupportReplyAIDraft` | Verified Syncfusion.Maui.Chat 35.1.43 (NuGet XML docs and help.syncfusion.com) supports native Editor, AllowMultilineInput, SendMessage/SendMessageCommand, ShowMessageInputView, typing indicator, suggestions, message templates, attachment-button template, and composer styling; no documented public full composer/footer template. Human-approved SfChat-native adaptation recorded against DesignReference commit 90d0518: native Editor and Send, typing indicator for drafting, suggestions for Regenerate/Discard/Retry, message templates for selection and Draft Reply; no custom footer or external Editor | None |
| G-3 | Gate 3: Message selection and Draft Reply | REQ-002 through REQ-009, REQ-082, REQ-083 | In Progress | G-2 | `AI Solutions/Chat/SupportReplyAIDraft` | Runtime-verified customer selection, selected-bubble marking, non-selectable support messages, Draft Reply only on selected request | None |
| G-4 | Gate 4: Azure OpenAI reply workflow | REQ-010 through REQ-058, REQ-084 through REQ-088 | Blocked by Gate 3 | G-3 | `AI Solutions/Chat/SupportReplyAIDraft` | Runtime-verified draft generation, editing, Review and edit before sending, Regenerate, Discard, Retry, conditional Send, and sent-message insertion as Jordan Lee outgoing | Gate 3 |
| G-5 | Gate 5: Desktop visual correction | REQ-089 through REQ-093, REQ-097 through REQ-099 | Blocked by Gate 4 | G-4 | `AI Solutions/Chat/SupportReplyAIDraft` | All four desktop states directly compared with DesignReference with human confirmation | Gate 4 |
| G-6 | Gate 6: Mobile correction | REQ-097 through REQ-099 | Blocked by Gate 5 | G-5 | `AI Solutions/Chat/SupportReplyAIDraft` | All four mobile states directly compared with DesignReference with human confirmation | Gate 5 |

## Status Rules
- Only one gate should be In Progress at a time; gates run in order.
- Complete requires evidence.
- Record blockers instead of guessing.
- Update `TASKS.md` for progress, blockers, issues, and evidence; update `PLAN.md` only when the current gate changes.
