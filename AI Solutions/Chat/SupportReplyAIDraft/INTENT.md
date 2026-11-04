# Support Reply AI Draft Intent

## Target User
Customer-support agent

## User Problem
The agent needs a controlled way to draft a professional reply to one selected customer message using recent conversation context, without automatic sending.

## Intended Outcome
Produce an editable reply draft that the agent can review, edit, regenerate, discard, and explicitly send.

## Goals
- Display one fictional support conversation
- Allow exactly one customer message to be selected
- Draft a context-aware reply from Azure OpenAI
- Validate structured JSON before display
- Preserve the conversation and selection during failures
- Keep Send under explicit agent control
- Support responsive desktop and mobile layouts

## Non-goals
- Knowledge retrieval or knowledge UI
- Source IDs, source panels, or source actions
- Automatic sending
- Ticket resolution, routing, classification, summarization, or sentiment analysis
- External support-platform integration
- Account or subscription modification
- WebView or copied web implementation

## Success Definition
The harness is ready when the temporary planning files capture the full contract, constraints, acceptance coverage, and cleanup rules needed for later implementation without changing the design references.

## Approval Owner
Human reviewer / product owner

## Completion Evidence
- Five internal harness files created under `AI Solutions/Chat/SupportReplyAIDraft`
- No implementation files created
- No design-reference files modified
- Package verification recorded as blocked rather than guessed
