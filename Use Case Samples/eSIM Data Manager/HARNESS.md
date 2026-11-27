# eSIM Data Manager — AI Development Harness

This harness guides an AI coding agent implementing the sample defined in [`SPEC.md`](SPEC.md). The harness is for software development only: **the application itself has no AI feature**.

## Implementation status

Implementation is in progress. The current implementation includes the shared MAUI shell, in-memory domain workflows, dashboard and plan/profile content, and domain unit tests. `README.md` records environment checks. A .NET SDK/compiler is unavailable, so restore, compilation, tests, and runtime verification remain unverified. Do not treat structural XML checks as build evidence or mark the associated `TASKS.md` checks complete without running them.

## Agent objective

Implement the eSIM Data Manager as a compact, polished .NET MAUI MVVM showcase using Syncfusion MAUI controls. Satisfy the requirements and acceptance criteria in `SPEC.md`, and complete tasks in [`TASKS.md`](TASKS.md) in dependency order. Do not expand the product scope.

## Source of truth and precedence

1. `SPEC.md` is the product and behavior contract.
2. `TASKS.md` defines the implementation sequence and completion checks.
3. Existing repository and project conventions define code style, target framework, Syncfusion package versions, and build/test commands.
4. If these sources conflict or a required control/API is incompatible with the project version, inspect existing usage and package documentation, choose the smallest compatible implementation, and record the deviation. Do not silently invent requirements.

## Mandatory constraints

- No AI functionality in the app; no AI SDKs, prompts, credentials, endpoint settings, or AI-related screens.
- No backend, database, filesystem persistence, preferences, secure storage, network service, actual eSIM provisioning, or payment processing.
- Keep sample data deterministic, local, fictional, and in memory for the current process only.
- Clearly label simulated transactions, plans/providers, activation code/QR, and provisioning. Never suggest a sample QR installs a real eSIM.
- Keep business transitions and calculations in the app's view-model/store source files; share one in-memory source of truth across the UI. The current UI-focused project intentionally does not split the domain into a separate project.
- Use the existing project’s target framework and package versions unless a required change is justified by the spec.
- Use Syncfusion controls specified in the spec when compatible; verify APIs against installed package versions and follow existing repository patterns. Do not guess control properties.
- Protect user data: collect no payment or identity information and do not introduce credentials or external calls.
- Keep changes focused. Avoid unrelated refactors and dependencies.

## Execution protocol

1. Inspect the repository and sibling MAUI samples before creating project structure. Check the current branch and preserve unrelated work.
2. Read `SPEC.md`, then `TASKS.md`. Identify relevant existing examples and actual installed Syncfusion/MAUI versions.
3. Implement in task order. Keep each task small and update task status in `TASKS.md` only when its completion checks pass.
4. For each feature, verify binding/data flow and state transitions, not just the visual markup. Cover business rules with tests where feasible.
5. Run formatting/build/tests supported by the current host. Never claim an unsupported platform build passed; report exact host/workload limitations.
6. Review the diff for scope, labels that could imply real service, accidental persistence/network dependencies, secrets, and consistency of data arithmetic.
7. At completion, report implemented screens/flows, controls, tests/commands and outcomes, and any platform-specific limitation.

## Definition of done

- Every acceptance criterion in `SPEC.md` is met or an explicit, justified limitation is documented.
- Tasks in `TASKS.md` have completion evidence and are not marked complete on assumption alone.
- Android, iOS, Windows, and Mac Catalyst targets are configured as appropriate to project conventions; host limitations are distinguished from code failures.
- Search/filter, purchase, activation, usage, and top-up state changes work across screens in the same session.
- Business logic remains in the MAUI app project. The user-requested UI-focused project does not include a separate unit-test project; validation claims must reflect checks actually run.
- App behavior remains offline and session-only, with demo/simulation labels visible at relevant decision points.
