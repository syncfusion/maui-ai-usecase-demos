# ADR-0001: Limit the showcase to conflict detection

Status: Proposed
Date: 2026-05-06

## Context

The AI-Powered Requirement Conflict Detector is an AI solution showcase for Business Analysts, Solution Architects, Product Managers, and Enterprise Architects. The brief requires one AI capability so the demo stays focused and does not overlap other showcases. The spec is `docs/specs/requirement-conflict-detector/spec.md`. US-3 is the analysis story. The hero grid only visualizes this capability.

## Options

- **Conflict detection only**: analyze a requirement set for contradictions, opposing rules, and mutually exclusive requirements. Cost: the showcase cannot answer similarity, risk, or summary questions.
- **Several AI capabilities in one app**: add duplicate detection, anomaly detection, categorization, or summarization beside conflict detection. Cost: the demo no longer has a single story and collides with other showcases.

## Decision

Ship conflict detection as the only AI capability. The conflict-detection module must not classify duplicates, anomalies, categories, summaries, risks, dependencies, or impact.

## Consequences

- The hero grid, prompt, and export all speak about conflicts only, which keeps the demo easy to explain.
- Requests for the excluded capabilities are out of scope for this application.
- Revisit this only if the showcase brief is rewritten to cover a second AI capability.
