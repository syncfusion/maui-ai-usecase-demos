# AI-Powered Requirement Conflict Detector

Status: Draft
Related: ADR-0001, ADR-0002
Version: 1.0
Application type: AI Solution Showcase
Platform: .NET MAUI
Primary control: Syncfusion DataGrid
AI capability: Conflict Detection
Audience: Business Analysts, Solution Architects, Product Managers, Enterprise Architects

Any of the four audience roles may perform any story. The named actor shows who values the story. Roles are not separate permissions. There is no sign-in and no role switcher.

## Problem

Business Analysts, Solution Architects, Product Managers, and Enterprise Architects collect requirements from many stakeholders, departments, and documents. They review those requirements one by one, so contradictions stay hidden until development, test, or a stakeholder dispute.

That late discovery causes rework, scope changes, development delays, testing failures, and stakeholder disputes. A typical contradiction is a requirement that guest users can access reports sitting next to a requirement that authentication is required for all reports. Both cannot be enforced at once, but a person reading them separately does not see the pair.

## Solution

The AI-Powered Requirement Conflict Detector is a focused .NET MAUI showcase. The user imports a requirement set, stores it, and runs one AI capability: conflict detection. The AI looks across the set for requirements that cannot coexist, enforce opposing rules, violate one another, or create a business inconsistency.

Each requirement gets one conflict result. If it conflicts with more than one other requirement, that result names a single primary conflicting requirement: the strongest contradiction. The result also carries a conflict flag, a business-language explanation, and a suggested resolution. The explanation may mention other identifiers. The hero grid, the details view, the conflict count, and the export all use the one primary partner. A suggested resolution does not change the stored requirement.

The Syncfusion DataGrid is the hero workspace. Users spend most of their time reviewing conflicts, filtering them, searching requirement text and explanations, and reading the AI explanation in the grid. Counts for total requirements, conflicts found, and requirements reviewed are always visible, either as optional KPI cards or as labeled text. They are not a dashboard. Buttons cover import, detect conflicts, and export.

The local user may read, replace, and export the requirement set and the conflict results. No other user has access. Analysis sends requirement identifiers and requirement text to the configured AI service and nothing else. Before the first send, the user sees a one-time warning and can proceed or cancel.

Positioning: the solution helps teams discover contradictory requirements before they become development issues. AI reasons about relationships between requirements and surfaces conflicts in the hero grid. It does not duplicate other showcase capabilities.

## User stories

### US-1: Import a requirement set

As a Business Analyst, I want to import requirements from Excel or CSV so that a collected set can be analyzed as one connected set.

Acceptance checks:
- The import screen offers Excel (XLSX) and CSV import and a preview of the parsed rows. Preview columns are Requirement ID and Requirement Text only. Preview does not show conflict columns and does not write to the store.
- Confirming a valid import replaces the stored requirement set. Canceling the file picker does not change the set.
- The first row must contain Requirement ID and Requirement Text. Extra columns and, for XLSX, sheets after the first are ignored. CSV uses the first row as the header and commas as separators. Quoted commas belong to the field.
- The file is rejected, nothing is stored, and the previous set is kept when a required column is missing, the file has no data rows, any Requirement ID or Requirement Text is empty or whitespace, any Requirement ID repeats in the file, the file has more than 100 data rows, or the file cannot be read as XLSX or CSV. The validation message identifies which of these rules failed.
- Identifiers are stored and compared as trimmed exact strings. "R001" and "r001" are different identifiers.
- After a confirmed valid import, the preview and the hero grid show Requirement ID and Requirement Text for each stored requirement, with empty conflict columns until the next completed analysis.

### US-2: Keep the requirement set for analysis

As a Business Analyst, I want imported requirements kept in a local repository so that conflict analysis uses the same set I just reviewed.

Acceptance checks:
- A valid import persists every requirement (identifier and text) in SQLite. A failed import or a failed write leaves the previously stored set and findings unchanged.
- Relaunching the application loads the last stored requirement set and, if a completed analysis was stored, its conflict results, without asking the user to import again.
- Replacing the set by a confirmed import or by loading the sample clears stored conflict results.
- The repository does not store failed payloads, the prompt, or the AI service key.

### US-3: Detect conflicts across the set

As a Solution Architect, I want one action that analyzes the whole requirement set so that contradictory requirements are found before development starts.

Acceptance checks:
- The Detect Conflicts control stays visible. It is disabled when fewer than two requirements are stored and while an analysis is running. A second activation during a run is ignored. Import is disabled during a run so the set cannot change mid-analysis.
- Once per install, before the first send, the user is warned that identifiers and requirement text will be sent to the AI service. Cancel does not call the service and does not change findings. Proceed continues the run. Later runs do not show the warning again.
- The action sends the stored requirement set through conflict detection only. It does not run duplicate detection, anomaly detection, categorization, summarization, risk prediction, dependency analysis, or impact analysis.
- One call analyzes the whole set. The module returns either a complete valid result list or a failure. It does not return a partial list. All hero-grid rows update together on success. A busy state is shown until then.
- A valid list has one entry per stored identifier. A true conflict flag includes a primary conflicting identifier that exists in the set and is not the same identifier, a non-empty explanation, and a non-empty suggested resolution. A false flag has an empty conflicting identifier, an empty explanation or none, and an empty resolution. An identifier that was not supplied, a missing supplied identifier, or a duplicate entry makes the run invalid.
- For a set containing "Guest users can access reports." and "Authentication is required for all reports.", each of those two requirements names the other as its primary partner. The explanation states that one allows anonymous access while the other requires authentication.
- For a set containing "Users can permanently delete records." and "Records must be retained for seven years.", each names the other as its primary partner. The explanation states that permanent deletion violates the retention rule.
- For a set containing "Passwordless authentication is mandatory." and "Username and password authentication is required.", each names the other as its primary partner.
- Requirements with no detected contradiction are returned with no conflict. The analyzer must not flag a pair that does not contradict. Extra valid conflicts besides the three pairs are allowed.
- On timeout, refused call, network failure, missing configuration, or invalid output, previous findings stay, a failure message is shown, and the run is not a completed analysis. A late response for a set that has since been replaced is ignored.
- Analysis time is measured from the action until results or the failure message. For up to 100 requirements it completes in under 30 seconds or the user is told that analysis did not finish in time. An unconfigured service fails immediately and does not start that timer.
- The user may run the action again after a failure. The application does not retry by itself and does not ask to confirm replacing previous findings. A new completed analysis replaces them.

### US-4: Review findings in the DataGrid

As a Product Manager, I want conflict findings in the DataGrid so that I can review, filter, and search them without leaving the grid.

Acceptance checks:
- The hero screen's primary content is the Syncfusion DataGrid. Actions, counts, and navigation are secondary. There is no chart and no second analysis workspace.
- After a completed analysis, the columns are Requirement ID, Requirement, Conflict Status, Conflicting Requirement, and AI Explanation, in that order. One row per requirement. Conflict status text is "Conflict" or "No conflict". The conflicting-requirement column shows the primary partner's Requirement ID, or empty when there is no conflict.
- A Conflict row uses a red row highlight that stays visible when the row is selected. A No conflict row keeps the normal appearance.
- The user can filter to conflicts only or to no conflicts, and can clear the filter. The user can sort by Requirement ID (text order, not numeric) and by Conflict Status (Conflict before No conflict when ascending).
- The user can search by case-insensitive substring on requirement text and on the AI explanation. Search does not use Requirement ID. Filter and search compose. Sort applies to the visible rows. For up to 100 requirements, the visible rows update in under 1 second after the user stops typing.
- Counts stay on the latest completed analysis, not the filtered view. Total requirements is the stored set count. Conflicts found is the number of requirements whose conflict flag is true, not the number of unordered pairs. Requirements reviewed is the number of requirements in the latest completed analysis. The last two stay zero until the first completed analysis and do not reset when analysis fails.
- Before the first completed analysis, conflict columns are empty. That is correct, not a defect. Up to 100 rows appear in one grid. The grid does not page.

### US-5: Read a conflict pair in full

As an Enterprise Architect, I want the full pair, explanation, and suggested resolution so that stakeholders can decide how to resolve the contradiction.

Acceptance checks:
- Choosing a Conflict row opens the details view. Requirement A is the selected requirement's original text. Requirement B is the primary partner's original text. The view also shows the AI explanation and the suggested resolution, in that order.
- The details view is read-only. Suggested resolution does not appear as a hero-grid column.
- A No conflict row does not open details. Restoring a details route whose conflict is no longer stored returns to the hero grid.
- Back navigation returns to the hero grid without discarding the latest findings.

### US-6: Export a conflict report

As a Business Analyst, I want to export the findings so that stakeholders can review conflicts outside the application.

Acceptance checks:
- Excel Report and PDF Report are both available from the hero screen. Export stays visible and is disabled until a completed analysis exists for the current set, and is disabled again when the set is replaced.
- Each report has one row per requirement from the latest completed analysis: Requirement ID, requirement text, conflict status, primary conflicting requirement, AI explanation, and suggested resolution. Pairs are not collapsed. If A names B and B names A, the report has two rows.
- Excel is one worksheet named Conflicts. PDF is a table with the same columns. Neither file includes the prompt or the AI service address. The application does not upload the report.
- Export does not call conflict detection. A failed analysis does not become exportable.
- Export time is measured from the export action until the file is ready to save or share. For up to 100 requirements it completes in under 10 seconds.

### US-7: Demonstrate the capability on a realistic set

As a Product Manager evaluating the showcase, I want a bundled sample of 50 to 100 requirements so that filtering, search, and cross-row conflict reasoning are visible without preparing a file.

Acceptance checks:
- The application can load a bundled sample of between 50 and 100 requirements. The sample is embedded, not downloaded. Identifiers are unique.
- The sample includes these statements verbatim: "Guest users can access reports.", "Authentication is required for all reports.", "Users can permanently delete records.", "Records must be retained for seven years.", "Passwordless authentication is mandatory.", "Username and password authentication is required.", "Users can export reports to PDF.", and "Reports must be watermarked before export."
- In the sample data, each side of the first three pairs can be each other's primary partner without a different requirement being a stronger contradiction. Tests locate those pairs by text.
- Other sample rows may contradict each other. They must not force a different primary partner for either side of those three pairs. The PDF-export and watermark statements are data under analysis, not rules for this application's own report.
- Loading the sample follows the same replace rule as import. If a set is already stored, the user confirms first. First launch with an empty store may load it without confirmation. First launch does not call the AI service.
- The sample is the only dataset shipped with the showcase. The user can still replace it by importing XLSX or CSV.
- Showcase walkthrough, in order: load sample, detect conflicts, filter to conflicts only, open one required pair, export Excel, export PDF. The user may skip a step. The narrative should not skip Detect Conflicts.

## Implementation decisions

The showcase is one .NET MAUI application. Syncfusion DataGrid is the control on the hero screen. The import preview may be a list or a grid. It is not the hero workspace and is not required to be Syncfusion. No chart, dashboard, or second AI control is part of this solution.

There are three routes: import, hero grid, and conflict details. Dialogs for the send warning, replace confirmation, and validation are modal on those routes. They are not extra routes.

Modules and dependency direction:

- **Import**: reads an XLSX or CSV file, applies the US-1 rules, and returns a requirement set or a validation failure. It does not analyze text and does not call the AI service.
- **Requirement repository**: stores and loads the requirement set and the latest valid conflict-result list in SQLite. A new valid import replaces the previous set and clears findings. A failed write leaves both unchanged. It does not call the AI service. See ADR-0002.
- **Conflict detection**: accepts the requirement set and returns one complete conflict-result list or a failure. This is the only AI capability. See ADR-0001. The prompt role is a Senior Business Analyst. The prompt asks only for contradictions, opposing business rules, and mutually exclusive requirements, and for each conflict asks for Requirement A, Requirement B, a conflict explanation, and a suggested resolution. The module may add only the output shape in US-3. It must not add an out-of-scope task. The vendor SDK sits behind one service interface used only by this module. If several contradictions exist for one requirement, this module picks the primary partner.
- **Conflict review**: owns the Detect Conflicts action, the one-time send warning, the busy state, and mapping results onto the hero grid and the details view. It calls conflict detection. It does not invent a pair the module did not return. It owns filter, sort, search, and the three counts.
- **Report export**: turns the latest completed conflict results plus requirement text into Excel and PDF. It does not receive or call the AI service.
- **Sample set**: provides the bundled requirements described in US-7. It does not call the AI service.

Processing order for the user is: import, store, analyze, review, export. Pair detection and explanation are not extra stages. They come back in the one conflict-detection call.

Shared data:

- A requirement has an identifier and a requirement text.
- A conflict result has a requirement identifier, a conflicting requirement identifier, a conflict flag, an explanation, and a suggested resolution. The conflicting identifier is the primary partner when the flag is true, and empty when the flag is false.

Non-functional targets: up to 100 requirements, analysis under 30 seconds, export under 10 seconds, search under 1 second, platform .NET MAUI. Phone, tablet, and desktop layouts must all complete the walkthrough. On a narrow layout the actions may sit in a menu. The hero grid may not.

Open decision: which Azure OpenAI (or equivalent) deployment the showcase calls is not named in the brief. The conflict-detection module must talk to it through one service interface so the rest of the application does not depend on a vendor SDK. The service key must not be hard-coded in source. Where that key is stored waits on this decision. Until it is closed, tests use the interface and do not need a live deployment.

## Testing decisions

Automated tests do not call a live model. Live-model quality is checked by the manual walkthrough, using the three required pairs as the demo script. A valid response that misses a required pair is still rendered. That miss is a prompt or service failure, not a grid defect.

| Seam | What the tests prove |
|---|---|
| Import | XLSX and CSV with the two required columns are accepted only after confirm. Missing columns, empty fields, repeated identifiers, more than 100 data rows, and unreadable files are rejected and do not change the store. |
| Requirement repository | A valid import replaces the previous set and clears findings. A failed write leaves the previous set and findings. Load returns stored requirements and the latest valid conflict results only. Failed payloads, the prompt, and the service key are not stored. |
| Conflict detection | Given the three contradiction pairs, a fake analyzer result maps to mutual primary partners. The public module returns one complete valid list or a failure, never a partial list. A capturing fake shows the prompt contains the Senior Business Analyst instructions and only the supplied identifiers and texts, and does not ask for an out-of-scope capability. |
| Detect Conflicts action | Fewer than two requirements disables the action. A second click during a run is ignored. Canceling the send warning does not call the service. Proceeding calls it once. Timeout, invalid output, and a late response after replace leave previous findings in place. |
| Conflict review | A conflict result becomes a Conflict row with the primary partner id, explanation, and conflict visual state. A no-conflict result does not. Filters, sorts, search, and counts follow US-4. Details opens only for a Conflict row and shows both original texts plus the resolution. |
| Report export | Excel and PDF match US-6 and do not call conflict detection. Export stays disabled until a completed analysis exists for the current set. |
| Sample set | Load returns 50 to 100 requirements, the eight verbatim texts, unique identifiers, and the three mutual primary pairs identifiable by text. |
| Hero control | Inspection confirms the hero screen displays the review contract in a Syncfusion DataGrid. Automated tests cover the review model, not XAML. |

Tests use the module interfaces. They may substitute an in-memory repository. The application build uses SQLite. Private helpers and storage tables are not seams.

## Out of scope

- AI duplicate detection. Rejecting a repeated Requirement ID in a file is still required. That rejection is validation, not duplicate detection.
- Anomaly detection. Import and analysis failures are validation or call failures, not anomalies.
- Predictive data entry
- Requirement categorization
- Requirement summarization
- Risk prediction
- Dependency analysis
- Impact analysis
- Dashboards, charts, and advanced visualizations
- Editing, adding, or deleting a single requirement in the grid
- Dismissing or accepting a single conflict result
- Multi-user or server-side requirement stores, accounts, and audit logs
- A fourth route, a settings screen, or a second AI capability
- Localization beyond English, custom themes, report styling beyond readable headers, and watermarking this application's own PDF
- Offline analysis, telemetry, redaction, app-level login, and encryption beyond the platform default
- Any change to other showcases in this repository

Grouping is not required. Search, filter, and sort are the required grid features. If grouping is added later, it must not replace them.
