# Support Reply AI Draft Intent

## Target User
Customer-support agent

## User Problem
The agent needs a controlled way to draft a professional reply to one selected customer message using recent conversation context, without automatic sending.

## Intended Outcome
Produce an editable reply draft that the agent can review, edit, regenerate, discard, and explicitly send.

## Goals
- Display one fictional support conversation in a single SfChat surface
- Allow exactly one customer message to be selected
- Draft a context-aware reply from Azure OpenAI
- Validate structured JSON before display
- Preserve the conversation and selection during failures
- Keep Send under explicit agent control
- Support responsive desktop and mobile layouts
- Keep the reply editor integrated with the same conversation surface while the built-in SfChat composer remains hidden

## Non-goals
- Knowledge retrieval or knowledge UI
- Source IDs, source panels, or source actions
- Automatic sending
- Ticket resolution, routing, classification, summarization, or sentiment analysis
- External support-platform integration
- Account or subscription modification
- WebView or copied web implementation

## Success Definition
The sample is ready when the approved support-reply workflow is implemented in the allowed folder, the design references remain unchanged, and the documented states match the approved workflow.

## Approval Owner
Human reviewer / product owner

## Completion Evidence
- Five internal harness files are preserved under `AI Solutions/Chat/SupportReplyAIDraft`
- Implementation files remain within the allowed folder
- No design-reference files modified
- Package verification recorded as 35.1.43 and build validation is unavailable in the sandbox
