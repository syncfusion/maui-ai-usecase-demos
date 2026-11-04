# AI-Powered Support Reply Drafting Design References

This directory contains the approved visual references for the AI-Powered Support Reply Drafting .NET MAUI solution.

## Purpose

The PNG screenshots and HTML files define the approved desktop and mobile visual states for implementation with:

- .NET MAUI XAML
- Syncfusion .NET MAUI Chat
- MVVM
- Native .NET MAUI layouts and controls
- Azure OpenAI

The application generates a reply using the selected customer message and recent conversation context.

The solution does not use knowledge-base retrieval or display knowledge references.

## Reference Inventory

### Desktop

| State | Screenshot | HTML |
|---|---|---|
| Initial | `Screenshots/desktop-initial.png` | `Html/desktop-initial.html` |
| Drafting | `Screenshots/desktop-drafting.png` | `Html/desktop-drafting.html` |
| Generated Reply | `Screenshots/desktop-generated-reply.png` | `Html/desktop-generated-reply.html` |
| Generation Error | `Screenshots/desktop-generation-error.png` | `Html/desktop-generation-error.html` |

### Mobile

| State | Screenshot | HTML |
|---|---|---|
| Initial | `Screenshots/mobile-initial.png` | `Html/mobile-initial.html` |
| Drafting | `Screenshots/mobile-drafting.png` | `Html/mobile-drafting.html` |
| Generated Reply | `Screenshots/mobile-generated-reply.png` | `Html/mobile-generated-reply.html` |
| Generation Error | `Screenshots/mobile-generation-error.png` | `Html/mobile-generation-error.html` |

## Implementation Rules

1. Implement the application using native .NET MAUI XAML and C#.
2. Use Syncfusion .NET MAUI Chat as the primary conversation control.
3. Use MVVM for application state and commands.
4. Do not embed the HTML files in the application.
5. Do not use a WebView to reproduce the design.
6. Do not copy generated web JavaScript into the .NET MAUI project.
7. Do not package the reference PNG or HTML files as application resources.
8. Use PNG files for visual comparison.
9. Use HTML and CSS only to understand layout, spacing, typography, colors, borders, and component sizing.
10. Preserve the approved state flow and functionality.
11. Do not add features outside the approved specification.
12. Do not implement knowledge-base retrieval, source IDs, source cards, or knowledge-reference UI.

## Source-of-Truth Priority

### Functional Behavior

1. Approved AI-Powered Support Reply Drafting specification
2. Approved state and interaction requirements
3. Design-reference screenshots and HTML

### Visual Appearance

1. Approved PNG screenshot for the corresponding state
2. Matching HTML and CSS reference
3. Written UI guidance in the approved specification

If HTML behavior conflicts with the specification, follow the specification.

If HTML styling conflicts with the corresponding PNG screenshot, follow the PNG screenshot.

## Approved Workflow

1. The support agent selects one customer message.
2. The selected message displays `Selected for reply`.
3. The support agent selects `Draft Reply`.
4. AI reviews the selected message and relevant recent conversation context.
5. The application displays `Drafting reply...`.
6. AI returns a structured response.
7. The application validates the response.
8. The validated draft appears in the editable Reply composer.
9. The support agent can edit, regenerate, discard, or explicitly send the reply.
10. Send adds the current composer content to the conversation as a support-agent message.

AI must never send a message automatically.

## Approved UI States

### Initial

- No customer message is selected.
- Reply composer is empty.
- Draft Reply is disabled.
- Discard and Send are disabled.
- Regenerate is hidden.

### Drafting

- The fourth customer message is selected.
- `Selected for reply` is visible.
- `Drafting reply...` and a progress indicator are visible.
- The Reply composer is empty and disabled.
- Partial AI text is not displayed.
- Discard and Send are disabled.
- Regenerate is hidden.

### Generated Reply

- The fourth customer message remains selected.
- The validated reply appears in the editable composer.
- `AI draft` is visible.
- `Review before sending` is visible.
- Requested information is displayed when applicable.
- Discard, Regenerate, and Send are enabled.

### Generation Error

- The fourth customer message remains selected.
- No invalid reply is inserted into the composer.
- The application displays `Unable to draft a reply`.
- The selected message and conversation are preserved.
- Retry is available.
- Send remains disabled while the composer is empty.

## Responsive Requirements

### Desktop

- Conversation is the primary content area.
- Message bubbles use a comfortable maximum width.
- Reply composer remains near the bottom.
- No knowledge-reference panel is displayed.

### Mobile

- Conversation remains vertically scrollable.
- The selected message remains reachable above the Reply composer.
- The Reply surface extends through the bottom safe area.
- Required actions remain visible and touch friendly.
- No knowledge-reference action or bottom sheet is displayed.

## Prohibited Features

Do not add:

- Knowledge-base retrieval
- Knowledge-reference panels
- Source IDs
- Public web search
- External documentation retrieval
- Automatic sending
- Automatic ticket resolution
- Sentiment analysis
- Ticket classification
- Ticket assignment
- Conversation summarization
- Multiple customer conversations
- External support-platform integration
- Customer-account modification
- AI confidence scoring
- Prompt or model configuration UI
- Raw AI JSON
- Developer diagnostics

## Notes for Coding Agents

Before implementing:

1. Read the approved specification completely.
2. Inspect all eight PNG references.
3. Inspect the matching HTML files for visual measurements.
4. Inspect existing repository samples for framework, package, and coding conventions.
5. Produce a file-level implementation plan before editing source files.
6. Keep all implementation changes within the approved solution directory unless a shared-file change is explicitly approved.