# Support Reply AI Draft Plan

## Current status
- The static implementation has been laid out in the allowed folder.
- The solution uses the repository's `.slnx` convention.
- Completion evidence is still static only; build and runtime verification remain deferred.

## Phase 1: Package verification
- Expected files: `SupportReplyAIDraft.csproj` and package reference sources
- Dependencies: authoritative Syncfusion and NuGet package information
- Completion condition: exact compatible package versions are verified and recorded, with source URLs and .NET 10 compatibility noted
- Evidence: version list with source references
- Approval: required before changing package references
- Recovery action: stop and mark blocked if verification is unavailable

## Phase 2: Syncfusion registration and templates
- Expected files: implementation files under the allowed path
- Dependencies: verified package versions
- Completion condition: registration approach and template strategy recorded, including `ConfigureSyncfusionCore()` and `ChatMessageTemplateSelector`
- Evidence: documented registration and template plan
- Approval: required before implementation changes
- Recovery action: revisit package notes if APIs differ

## Phase 3: Repository-pattern inspection
- Expected files: existing sample project files under the repo
- Dependencies: repository search results
- Completion condition: a suitable .NET MAUI pattern is identified, and the exact selected-message plus Draft Reply template implementation is confirmed as the first implementation-phase inspection task
- Evidence: notes on project structure and naming conventions
- Approval: not required
- Recovery action: inspect a closer sample if the first pattern is insufficient

## Phase 4: Requirement and acceptance mapping
- Expected files: `SPEC.md`, `TASKS.md`
- Dependencies: approved product contract
- Completion condition: every requirement maps to at least one acceptance criterion
- Evidence: complete requirement-to-acceptance mapping
- Approval: not required
- Recovery action: add missing mappings

## Phase 5: Design-reference review
- Expected files: `DesignReference/README.md`, HTML, PNG references
- Dependencies: read-only design references
- Completion condition: visual and behavioral constraints are captured
- Evidence: reference notes and state inventory
- Approval: not required
- Recovery action: re-check the paired HTML and PNG states

## Phase 6: Project creation
- Expected files: project and solution files inside the allowed path
- Dependencies: approved scope and verified packages
- Completion condition: project skeleton exists without implementation overreach
- Evidence: created project structure
- Approval: required before creation
- Recovery action: stop if a shared-file or scope issue appears

## Phase 7: Static responsive UI
- Expected files: XAML layout files
- Dependencies: project skeleton and visual reference review
- Completion condition: desktop and mobile layouts render as planned
- Evidence: static layout review
- Approval: required before implementation changes
- Recovery action: adjust layout to match the approved references

## Phase 8: SfChat integration
- Expected files: chat page and view-model files
- Dependencies: package verification and layout skeleton
- Completion condition: Syncfusion chat is integrated for the conversation area
- Evidence: control wiring notes
- Approval: required before implementation changes
- Recovery action: revise control selection if APIs differ

## Phase 9: Selection and states
- Expected files: view model, selection logic, UI bindings
- Dependencies: SfChat integration
- Completion condition: one selectable customer message and all states are represented
- Evidence: state matrix against requirements
- Approval: required before implementation changes
- Recovery action: correct selection or state transitions

## Phase 10: Azure OpenAI integration
- Expected files: service and request/response models
- Dependencies: package verification and secret-policy compliance
- Completion condition: structured AI request/response flow is defined
- Evidence: contract and service notes
- Approval: required before implementation changes
- Recovery action: stop if secret handling is unclear

## Phase 11: Validation
- Expected files: validator and result handling
- Dependencies: structured response contract
- Completion condition: all validation rules are mapped to observable behavior
- Evidence: validation checklist
- Approval: required before implementation changes
- Recovery action: tighten rejection rules

## Phase 12: User actions
- Expected files: commands for Draft Reply, Regenerate, Discard, Retry, Send
- Dependencies: state logic and validation
- Completion condition: all actions align with the contract
- Evidence: command matrix
- Approval: required before implementation changes
- Recovery action: adjust command enablement

## Phase 13: Desktop review
- Expected files: desktop layout assets
- Dependencies: UI implementation
- Completion condition: desktop states match reference intent
- Evidence: desktop checklist
- Approval: required before finalizing UI
- Recovery action: revise spacing, alignment, or widths

## Phase 14: Mobile review
- Expected files: mobile layout assets
- Dependencies: UI implementation
- Completion condition: mobile states remain usable without horizontal scrolling
- Evidence: mobile checklist
- Approval: required before finalizing UI
- Recovery action: revise safe-area, spacing, or stacking

## Phase 15: Edge and error review
- Expected files: validator, error states, stale-request handling
- Dependencies: service and command wiring
- Completion condition: all failure modes preserve the correct state
- Evidence: edge-case checklist
- Approval: required before finalizing behavior
- Recovery action: refine error handling and recovery paths

## Phase 16: Final README
- Expected files: repository-specific README if later approved
- Dependencies: completed implementation
- Completion condition: user-facing notes are ready if requested
- Evidence: draft documentation outline
- Approval: required before documentation changes
- Recovery action: defer if documentation is out of scope

## Phase 17: Scope, secret, accessibility, contract, and file review
- Expected files: all implementation files in the allowed path
- Dependencies: all feature work
- Completion condition: no scope drift, no secrets, and all contract constraints are satisfied
- Evidence: final review checklist
- Approval: not required for review, required for any fixes that change scope
- Recovery action: remove or correct any out-of-scope items

## Phase 18: Documentation handoff
- Expected files: harness files and any approved docs
- Dependencies: final review
- Completion condition: handoff notes are complete
- Evidence: summarized status and blockers
- Approval: required for publication beyond the allowed path
- Recovery action: defer publication if approval is missing

## Phase 19: Temporary-file cleanup
- Expected files: delete `HARNESS.md`, `INTENT.md`, `SPEC.md`, `PLAN.md`, `TASKS.md`
- Dependencies: completion of implementation and review
- Completion condition: all five internal files are removed
- Evidence: clean Git status for those files
- Approval: required before deletion in final cleanup
- Recovery action: verify no stale harness files remain

## Phase 20: Final Git inspection
- Expected files: Git status and staged changes
- Dependencies: cleanup complete
- Completion condition: only approved changes remain staged or committed
- Evidence: final status review
- Approval: required before commit or push
- Recovery action: unstage or remove unexpected files

## Phase 21: Commit and push after approval
- Expected files: committed implementation only
- Dependencies: explicit approval and clean final review
- Completion condition: approved commit pushed to the sanctioned branch
- Evidence: commit hash and push result
- Approval: required
