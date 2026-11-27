# AI-Powered Requirement Conflict Detector

Status: Draft
Version: 1.0
Application type: AI Solution Showcase
Platform: .NET MAUI
Primary control: Syncfusion DataGrid
AI capability: Conflict Detection only
Target audience: Business Analysts, Solution Architects, Product Managers, Enterprise Architects
Related: none (self-contained project spec)

## Positioning statement

AI-Powered Requirement Conflict Detector is a focused AI solution that helps teams discover contradictory requirements before they become development issues. By combining AI-powered conflict reasoning with the structured review capabilities of Syncfusion DataGrid, the solution provides a simple, practical, and highly demonstrable enterprise workflow where AI analyzes relationships between requirements and surfaces conflicts directly within the grid for rapid review and resolution.

## Problem

Large projects collect requirements from multiple stakeholders, departments, and documents. Requirements are usually reviewed one at a time, so contradictions are found late.

Late conflicts cause:

- Rework
- Scope changes
- Development delays
- Testing failures
- Stakeholder disputes

Example that must be detectable:

- R001: Guest users can access reports.
- R015: Authentication is required for all reports.

Both cannot be enforced at the same time. The product must detect that conflict and explain why.

## Solution

Users import a requirement set (Excel or CSV), review it in a Syncfusion DataGrid, run a single AI analysis that looks for contradictions across the set, review conflict status, the conflicting requirement, and the AI explanation in that same grid, open a pair for full detail and a suggested resolution, and export an Excel or PDF report.

The DataGrid is the hero workspace. Users spend most of their time reviewing conflicts, filtering, searching, and reading explanations. No dashboards or extra visualizations.

Storage is in-memory only for the current session. There is no SQLite database and no other database.

## In scope

- Import requirement data (XLSX and CSV)
- Hold the imported set in memory
- Run AI-based conflict analysis (cross-requirement contradiction detection only)
- Show contradictory pairs, explanations, and suggested resolutions
- Filter, sort, and search findings in the DataGrid
- Export conflict reports (Excel and PDF)

## Out of scope

These capabilities are not included, so the showcase stays on one AI capability:

- Duplicate detection
- Anomaly detection
- Predictive data entry
- Requirement categorization
- Requirement summarization
- Risk prediction
- Dependency analysis
- Impact analysis
- SQLite or any other database, file-backed repository, or persistence across process restarts
- Dashboards and advanced visualizations beyond optional KPI counts

## Users

- Business Analysts
- Solution Architects
- Product Managers
- Enterprise Architects

## User workflow

1. **Import requirements** from Excel or CSV.
2. **Review imported data** in the DataGrid (Requirement ID, Requirement Text).
3. **Detect conflicts.** The user clicks Detect Conflicts. AI analyzes the full in-memory set and identifies contradictions.
4. **Review findings** in the DataGrid: conflict status, conflicting requirement, AI explanation. Filter, search, and open a pair for detail.
5. **Export results** as Excel or PDF.

## User stories

### US-1: Import requirements

As a business analyst, I want to import requirements from Excel or CSV so that I can analyze a stakeholder set without retyping it.

Acceptance checks:

- XLSX and CSV are accepted.
- The file must contain columns Requirement ID and Requirement Text (header match is case-insensitive; extra columns are ignored).
- A file missing either required column is rejected with a clear validation message and does not replace the current in-memory set.
- Empty rows and rows with a blank ID or blank text are skipped and counted in the import summary.
- Duplicate IDs in one file: the first row wins; later rows are skipped and counted.
- A successful import replaces the previous in-memory requirement set and clears previous conflict results.
- After import, the grid shows Requirement ID and Requirement Text for every accepted row.
- Closing the app or restarting the process clears all imported data (nothing is written to a database or requirement store on disk).

### US-2: Review the imported set before analysis

As a product manager, I want to see the imported requirements in the DataGrid so that I can confirm the set before running AI.

Acceptance checks:

- Before analysis, Conflict Status, Conflicting Requirement, and AI Explanation are empty (or show a not-analyzed state, consistently).
- Optional KPI cards, if shown, display Total Requirements equal to the in-memory count, Conflicts Found as 0 or "—", and Requirements Reviewed as 0 until analysis completes.
- The user can sort by Requirement ID before analysis.

### US-3: Detect conflicts with AI

As a solution architect, I want to run conflict detection on the full requirement set so that contradictory rules are found before development starts.

Acceptance checks:

- Detect Conflicts is available only when at least two requirements are in memory. With fewer than two, the action is disabled or shows a message and does not call the model.
- The analyzer receives the full in-memory set (ID + text), not a single row.
- The model is instructed only to find contradictions, opposing business rules, and mutually exclusive requirements. It must not categorize, summarize, score risk, find duplicates, or predict missing text.
- Analysis of up to 100 requirements completes in under 30 seconds under normal connectivity, or the UI shows a busy state and a timeout/failure message without corrupting the requirement set.
- Each returned conflict names Requirement A, Requirement B, a business-language explanation, and a suggested resolution.
- Pairs are unordered for identity: (A, B) and (B, A) are the same conflict and are stored once.
- A requirement may participate in more than one conflict.
- Requirements with no conflict are marked No Conflict.
- If the model returns an ID that is not in the set, that pair is dropped.
- If the model call fails, existing requirements stay loaded and the user can retry.
- Results live only in memory.

### US-4: Review findings in the DataGrid

As an enterprise architect, I want AI findings in the DataGrid so that I can filter, search, and read explanations without leaving the grid.

Acceptance checks:

- Hero grid columns, in order: Requirement ID, Requirement, Conflict Status, Conflicting Requirement, AI Explanation.
- A requirement in one conflict shows that conflicting requirement's ID (and text is available in the details screen).
- A requirement in multiple conflicts shows a count or the first conflicting ID plus an indicator that more exist; the details screen lists every pair. The grid must not drop extra pairs.
- Conflict Detected rows use a red row highlight. No Conflict rows use normal appearance. Not-yet-analyzed rows are not highlighted as conflicts.
- Filter: Show Conflicts Only; Show No Conflicts; show all.
- Sort: Requirement ID; Conflict Status.
- Search matches Requirement Text and AI Explanation and returns in under 1 second for 100 rows.
- Optional KPI cards after a successful run: Total Requirements, Conflicts Found (count of unique conflict pairs), Requirements Reviewed (count of requirements included in the last run).

### US-5: Open conflict details

As a business analyst, I want the full pair, explanation, and suggested resolution so that stakeholders can decide how to resolve the contradiction.

Acceptance checks:

- Selecting a conflict opens Conflict Details (or an equivalent detail surface) with:
  - Requirement A: ID and original text
  - Requirement B: ID and original text
  - AI Explanation
  - Suggested Resolution
- If the row has multiple conflicts, the user can move between pairs without leaving the detail surface.
- No Conflict rows do not present a fabricated explanation.

### US-6: Export a conflict report

As a product manager, I want to export findings to Excel and PDF so that I can share them with stakeholders.

Acceptance checks:

- Export is available after at least one successful analysis. Before that, export is disabled or explains that there is nothing to export.
- Excel and PDF both include, for each conflict pair: Requirement A ID and text, Requirement B ID and text, explanation, suggested resolution.
- Non-conflict requirements may be listed in a second section or omitted; the conflict section must be complete.
- Export of up to 100 requirements finishes in under 10 seconds.
- Export reads from memory only. It does not create a database.

## Screens

### Screen 1: Requirement Import

Purpose: upload requirements.

Features:

- Import Excel
- Import CSV
- Preview data (can be the same grid as the hero screen before analysis)

Actions (buttons):

- Import Requirements
- Detect Conflicts
- Export Report

### Screen 2: Conflict Detection Grid (hero screen)

Purpose: primary analysis workspace. This is where users spend most of their time.

DataGrid columns:

| Column | Notes |
| --- | --- |
| Requirement ID | From import |
| Requirement | Requirement text |
| Conflict Status | Not analyzed / No Conflict / Conflict Detected |
| Conflicting Requirement | Other requirement ID, or multiple indicator |
| AI Explanation | Model explanation for the displayed pair; empty if none |

DataGrid features:

- Filtering: Show Conflicts Only; Show No Conflicts
- Sorting: Requirement ID; Conflict Status
- Search: Requirement Text; Conflict Explanation
- Conditional formatting: Conflict Detected = red row highlight; No Conflict = normal appearance

Optional KPI cards (not a dashboard):

- Total Requirements
- Conflicts Found
- Requirements Reviewed

### Screen 3: Conflict Details

Purpose: review one conflict pair in full.

Sections:

- Requirement A — original text
- Requirement B — conflicting text
- AI Explanation — reason for the conflict
- Suggested Resolution — proposed action for stakeholders

## Functional requirements

### FR-01 Requirement import

Import requirement datasets.

Supported formats: XLSX, CSV.

Required columns:

| Column |
| --- |
| Requirement ID |
| Requirement Text |

Validation rules are the acceptance checks on US-1.

### FR-02 Requirement repository

Store imported requirements for the current session so analysis and the grid can read them.

Storage: **in-memory only**.

- No SQLite.
- No other database engine.
- No requirement or conflict tables on disk.
- Process exit clears the repository.
- A new successful import replaces the previous set and clears prior conflict results.
- Repository operations are synchronous from the UI's point of view except the AI call and file pickers.

Repository operations (logical interface `IRequirementRepository`):

- `ReplaceAll(requirements)` — replace the set; clear conflicts
- `GetAll()` — current requirements
- `GetById(id)` — one requirement or none
- `Count()` — current count

### FR-03 AI conflict detection

Analyze the requirement set and identify contradictions.

Inputs: the full in-memory requirement set (ID + text).

Outputs per conflict pair:

- Conflict status (conflict detected)
- Conflicting requirement ID
- AI explanation
- Suggested resolution

The AI decides that two requirements conflict when they:

- Cannot coexist
- Enforce opposing rules
- Violate one another
- Create a business inconsistency

Examples the showcase dataset must include and the detector should be able to explain (wording may vary; the contradiction must be recognized):

1. Guest users can access reports **vs** Authentication is required for all reports.  
   Explanation theme: anonymous access contradicts mandatory authentication.
2. Users can permanently delete records **vs** Records must be retained for seven years.  
   Explanation theme: permanent deletion violates retention.
3. Passwordless authentication is mandatory **vs** Username and password authentication is required.  
   Explanation theme: the authentication methods are mutually exclusive mandates.

Non-conflicting sample (must not be forced into a conflict with each other):

- Users can export reports to PDF.
- Reports must be watermarked before export.

These two constrain export; they do not contradict each other.

Logical interface `IConflictAnalyzer`:

- `AnalyzeAsync(requirements, cancellationToken)` → conflict pairs (requirement A ID, requirement B ID, explanation, suggested resolution)

The UI maps pairs onto per-requirement grid rows after analysis.

### FR-04 Conflict details

Display pair details: Requirement A, Requirement B, conflict reason, suggested resolution. See US-5.

Logical read model `ConflictResult`:

- RequirementId
- ConflictingRequirementId
- HasConflict
- Explanation
- SuggestedResolution

Grid projection may aggregate multiple `ConflictResult` rows per requirement. The detail screen shows one pair at a time.

### FR-05 Export report

Export conflict findings to Excel and PDF. See US-6.

Logical interface `IConflictReportExporter`:

- `ExportExcelAsync(requirements, conflicts, destination)`
- `ExportPdfAsync(requirements, conflicts, destination)`

## Data model

In-memory types only. No ORM, no migrations.

```csharp
public class Requirement
{
    public string Id { get; set; }
    public string RequirementText { get; set; }
}

public class ConflictResult
{
    public string RequirementId { get; set; }
    public string ConflictingRequirementId { get; set; }
    public bool HasConflict { get; set; }
    public string Explanation { get; set; }
    public string SuggestedResolution { get; set; }
}
```

Session state (also in memory):

- Requirement list
- Conflict result list from the last successful run
- Analysis state: Idle, Running, Succeeded, Failed
- Last error message (if Failed)

## AI processing flow

```text
Import Requirements
        │
        ▼
In-memory requirement repository
        │
        ▼
AI conflict analysis
        │
        ▼
Conflict pair detection
        │
        ▼
Explanation and suggested resolution
        │
        ▼
DataGrid visualization
        │
        ▼
Export report (Excel / PDF)
```

## AI prompt design

Conflict detection prompt (the only analysis prompt):

```text
You are a Senior Business Analyst.

Review the supplied requirements.

Identify:
- Contradictions
- Opposing business rules
- Mutually exclusive requirements

Do not identify duplicates, anomalies, categories, summaries, risks, dependencies, or impacts.
Do not invent requirement IDs. Use only IDs from the supplied set.
Treat a pair once (do not return both A-vs-B and B-vs-A).

For each conflict provide:
- Requirement A (ID)
- Requirement B (ID)
- Conflict Explanation (business language, no implementation design)
- Suggested Resolution (a stakeholder action, not code)

Requirements:
{Requirements}
```

`{Requirements}` is the in-memory set rendered as ID plus text, one requirement per line.

The implementation must parse the model output into `ConflictResult` records. Unparseable output is a failed analysis, not a silent empty success, unless the model explicitly returns an empty conflict list.

## Mock dataset

Ship a built-in sample the user can load without a file, so the showcase runs offline from the model only (the sample itself needs no database).

Recommended size: 50–100 requirements. That volume is enough to demonstrate AI reasoning, DataGrid filtering, sorting, search, and review of a large set.

The sample must include at least:

- Guest users can access reports.
- Authentication is required for all reports.
- Users can permanently delete records.
- Records must be retained for seven years.
- Passwordless authentication is mandatory.
- Username and password authentication is required.
- Users can export reports to PDF.
- Reports must be watermarked before export.

Remaining rows are additional plausible business requirements, some of which may conflict and some of which must not, so the grid shows both Conflict Detected and No Conflict after analysis.

Loading the sample uses the same import path as a file: it replaces memory and clears previous conflicts.

## Non-functional requirements

| Requirement | Target |
| --- | --- |
| Dataset size | Up to 100 requirements |
| Analysis time | Under 30 seconds |
| Export time | Under 10 seconds |
| Search response | Under 1 second |
| Platform | .NET MAUI |
| Persistence | In-memory session only; no database |
| Hero control | Syncfusion DataGrid |
| AI scope | Conflict detection only |

## Supporting controls

Buttons:

- Import Requirements
- Detect Conflicts
- Export Report

Optional KPI cards only (no charts):

- Total Requirements
- Conflicts Found
- Requirements Reviewed

## Implementation decisions

Modules (names, not file paths). All of them belong to this project.

- **Import module** — file picker, XLSX/CSV parse, column validation, row cleanup, hands a requirement list to the repository. Also loads the built-in sample through the same replace path.
- **In-memory repository** — holds requirements and the last conflict list for the process lifetime. Implements `IRequirementRepository` plus conflict get/replace/clear. No database adapter.
- **Conflict analyzer** — builds the prompt, calls the configured AI provider, validates IDs, deduplicates pairs, returns `ConflictResult` records. Implements `IConflictAnalyzer`.
- **Grid workspace** — Syncfusion DataGrid binding, filter (conflicts only / no conflicts / all), sort, search, conditional row formatting, KPI counts, and navigation into details.
- **Conflict details** — reads one or more pairs for a selected requirement from memory and shows texts, explanation, and suggested resolution.
- **Report exporter** — builds Excel and PDF from the in-memory conflict list. Implements `IConflictReportExporter`.
- **Session coordinator** — wires button commands to import, analyze, and export; owns analysis state (Idle / Running / Succeeded / Failed) so the grid does not mix old conflicts with a new set.

Interfaces between modules:

- Import → repository: `ReplaceAll`
- Analyzer ← repository: `GetAll`
- Analyzer → repository: replace conflict results
- Grid and details ← repository: requirements and conflicts
- Exporter ← repository: requirements and conflicts

AI provider credentials are configuration, not stored requirements. The spec does not require a particular vendor beyond a chat completion that can follow the conflict prompt. If no credential is configured, Detect Conflicts fails with a configuration message and leaves the imported set intact.

## Testing decisions

| Seam | What the tests prove |
| --- | --- |
| Import parser (XLSX and CSV fixtures, no UI) | Required columns enforced; extras ignored; blank and duplicate IDs skipped; valid rows become requirements |
| `IRequirementRepository` | Replace clears prior conflicts; get/count match the last replace; nothing is written to a database |
| Conflict result normalizer (model text or structured fixture in, pairs out) | Unknown IDs dropped; A/B duplicates collapsed; multiple conflicts per ID preserved; empty explicit list is success; garbage text is failure |
| Analyzer prompt builder | Prompt includes every requirement ID and text and forbids out-of-scope AI tasks |
| Grid projection | Conflict rows flagged for red highlight; non-conflicts not flagged; search matches text and explanation; conflict-only and no-conflict filters |
| Exporter | Excel and PDF contain every stored pair's IDs, texts, explanation, and resolution |
| Sample loader | Sample size is between 50 and 100 and includes the eight required sentences |

UI tests are not required for this spec. The seams above are enough to prove import, memory, conflict mapping, and export.

## Success criteria

Business:

- Requirement conflicts are automatically identified
- Stakeholders can review conflicts before development begins
- Conflicts are explained in business-friendly language

Technical:

- AI findings are displayed in Syncfusion DataGrid
- Filtering and searching work within the targets above
- Reports can be exported to Excel and PDF
- Storage is in-memory only

Showcase:

- DataGrid is the hero control
- AI performs meaningful cross-row reasoning
- Workflow stays import → detect → review → export
- The solution does not cover duplicate detection, anomaly detection, or predictive entry
