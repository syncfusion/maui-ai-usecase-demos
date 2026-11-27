# ADR-0002: Store the requirement set in SQLite

Status: Proposed
Date: 2026-05-06

## Context

FR-02 requires a requirement repository so conflict detection analyzes the imported requirement set rather than a transient grid binding. The showcase is a single-user .NET MAUI application with a set of at most 100 requirements. Import, the hero grid, conflict details, and export all read the same set. The spec is `docs/specs/requirement-conflict-detector/spec.md`.

## Options

- **SQLite**: a local file database already named in the brief. Cost: a small persistence dependency and a replace-on-import write path.
- **In-memory only**: keep the requirement set in the screen model. Cost: the set disappears on restart, so the repository requirement is unmet.

## Decision

Store each requirement set in a local SQLite database. A valid import replaces the previous set. Conflict detection, review, and export read from that store.

## Consequences

- Import, analysis, and both review screens share one requirement set across launches.
- A server or multi-user store is out of scope. Moving off SQLite means replacing the repository interface and this record.
- Revisit this if the showcase must share one requirement set across devices or users.
