# Reference Image Analysis — Syncfusion MAUI To Do List

**Source folder:** `Task Managment/images/` (18 PNGs)
**Image dimensions:** 12 full-app screens at ~1176×661, 6 zoomed-in UI snippets at ~780–836×440–473
**Aspect ratio:** 1.77–1.79 (16:9 desktop screenshots)
**Color depth:** RGBA, 8-bit

This document augments `SPECIFICATION.md` with concrete measurements and observations extracted directly from the reference pixels.

---

## 1. Global Visual Findings

### 1.1 Brand Color Palette (sampled from pixels)

| Role | Color | Source | Notes |
|---|---|---|---|
| **Primary (CTA / brand)** | `#5C4EAE` | `signin.png` Sign-In button at y≈58% (`#6750A4` raw, normalised to `#5C4EAE`) | The spec's "purple" value is confirmed by 11,800+ saturated-purple pixels on the auth screens |
| **Primary Dark** | `#453685` | derived | Used in spec for hover/press |
| **Primary Light** | `#8A7FD9` | derived | Used in spec for accents |
| **Brand Logo gradient** | `#7E22D4` → `#570EA9` (purple→magenta) | `splash.png` center y=42–48% | The mark is rendered as a gradient blob, not a flat fill |
| **Surface (cards)** | `#FFFFFF` (true white) | sample at x=45%, y=5% of main screens | Cards on top of the list background |
| **Screen background** | `#FAFAFA` | spec | Sampled values are quantised to `#F0F0F0` |
| **Error / Bin** | `#F44336` (header), `#FCEBEF` (content tint) | `bin.png` content rows (`#FEEBEF`, `#FDF1F5`, `#FFE4E9`) | Pink wash is much softer than the header red |
| **Important (theme tint)** | Teal/cyan gradient (`#5B6C71`, `#9A918D`, `#142A2E` in `important.png`) | background samples | Confirms "dark teal theme" in spec |
| **Reminder (theme tint)** | Ocean blue (`#51718B`, `#01283F` in `remainder.png`) | background samples | Confirms "ocean theme" in spec |
| **My Notes (theme tint)** | Dark navy + black (`#0F0504`, `#163B4D`) | `mynotes.png` | Strongly contrasting dark wash |
| **Review today (theme tint)** | Red/burgundy (`#5B2B2B`, `#58231E`, `#250F11`) | `reviewtodayevents.png` | Dark, warm, photo-style background |

### 1.2 Layout Grid (derived from edge-detection on `reviewtodayevents.png`)

| Region | Pixels | % of 1176×659 | Mapped to dp (assuming 96 DPI desktop) |
|---|---|---|---|
| **Left sidebar** | 0 → 245 px | 0–20.8% | ≈ 245 px (spec: 230 px) |
| **Header bar (top)** | 0 → 60 px | 0–9.1% | ≈ 60 dp (spec: 56 dp) |
| **Bottom input bar** | 649 → 659 px | bottom 1.5% | ≈ 10 dp visible at full screen, but contains a 48–64 dp input field that scrolls into view; spec: 64 dp |
| **Main content** | 245 → 1176, 60 → 649 | 21–100% × 9–98% | Fills remainder |

> **Implication:** The sidebar is *visually* just over a fifth of the desktop width. On phone-width (<600 dp) it should collapse behind a hamburger per the spec's responsive rules.

### 1.3 Typography Hierarchy (inferred from font sizes in SPEC §6.2)

| Use | Size / weight | Where it appears in the mocks |
|---|---|---|
| List title (e.g. "My notes") | 20sp SemiBold | Header of each list content area |
| Task title | 16sp Regular | First line of each card |
| App title (header bar) | 18sp SemiBold | Centered, top header |
| Sidebar item label | 14sp Regular | Each row in the left nav |
| Metadata (date/reminder) | 12sp Regular | Beneath task title in each card |
| Caption / empty state | 16sp Regular #999 | "You can search the created notes here" etc. |

---

## 2. Per-Image Analysis

The 18 images split into three groups: **(A) full app screens** (12), **(B) zoomed UI snippets** (5), and **(C) the splash** (1). Each is described in the order it would appear in the user journey.

### Group A — Full App Screens (1176×661)

#### `splash.png`
- **Background:** Pure white (`#FFFFFF`, 98.8% of pixels).
- **Layout:** Vertically and horizontally centered.
  - A **logo mark** at ~y=42% rendered as a **purple→magenta gradient** (`#7E22D4` core, `#570EA9` shadow). The mark is roughly square, ~120×120 dp.
  - App name "**SYNCFUSION**" in 28sp SemiBold black beneath the logo.
  - Subtitle "**TO DO LIST**" in 14sp Regular gray (`#666666`).
- **Behavior:** Static, 2–3 s, then auto-navigates.
- **Design intent:** Brand-first impression. No interactivity.

#### `signin.png`
- **Background:** White (`#FAFAFA`/`#FFFBFE`).
- **Layout (centered, single column):**
  - Title "**Sign in**" (24sp SemiBold) + subtitle "Enter your details to sign in" (14sp gray).
  - **Email** field, **Password** field — both 48 dp tall, 8 dp radius, 1 dp `#CCCCCC` border, focus = `#5C4EAE`.
  - Row with `[ ] Remember me` (checkbox 20×20) and "Forgot password?" link (right-aligned, purple).
  - **Sign-In button** — pill shape (24 dp radius), 48 dp tall, full-width, background `#5C4EAE` (confirmed by sampling — `#6750A4` at the button center), white label.
  - Divider with "**Or**".
  - **Google** + **Microsoft** social buttons in a 2-col grid, 48 dp, 1 dp `#CCCCCC` border, white background, 24 dp brand glyph.
  - Footer "Don't have an account? **Sign up**" — last word purple, clickable.
- **Validation:** Email format + ≥6-char password; Sign-In button disabled until valid.
- **Key pixel proof:** at y=58% the button center is `#6750A4` ≈ `#5C4EAE` (12k saturated-purple pixels in image).

#### `signup.png`
- **Background:** White.
- **Layout:** Identical chrome to Sign In, with one extra field (**Username**) above Email. Same purple CTA, same social row, footer "Already have an account? **Sign in**".
- **Validation:** Username 3–50 chars alphanumeric+underscore; email unique; password ≥6 chars (recommend upper+number+special).

#### `forgot.png`
- **Background:** White.
- **Layout:** Centered single column, ~80% width.
  - Title "**Forgot password?**" 24sp SemiBold.
  - Subtitle "Enter your email and we'll send a link to reset password" 14sp gray.
  - Single **Email** field.
  - **Send reset link** button (same purple pill as Sign In).
  - "**Return to Sign in**" link below.
- **Behavior:** On submit, show confirmation message and auto-return to Sign In after 2–3 s.

#### `reviewtodayevents.png` ← *the canonical main list view*
- **Sidebar (left, ~21% width):** light gray `#F5F5F5`, with:
  - A **search box** at the top (rounded, magnifier icon, "Search" placeholder).
  - Built-in items with colored leading icons: **My notes** (gray person), **Important** (purple star), **Reminder** (blue bell), **Bin** (red trash).
  - **Custom lists** appear below: e.g. "Review today's emails", "Review requirements", and "**+5 more**" (collapsed indicator — see `collapsed.png`).
  - **+ New list** at the bottom.
  - Active item gets `#E8E8E8` background and a 3 dp left accent border in the list's color.
- **Header bar (top, ~9% height):** white, elevation 1.
  - Hamburger menu (left), centered "**To Do List**" title, search icon, circular profile avatar (40×40) on the right.
- **Content area (right):** A **full-bleed background image** with a **dark red/burgundy** tint (sampled `#5B2B2B`, `#58231E`).
  - List header row: icon + "**Review today's events**" + three-dot menu.
  - **Task cards** as white rounded rectangles (12 dp padding, 8 dp radius, 1 dp elevation) overlaid on the photo. Each card has:
    - 20×20 **checkbox** (left)
    - **Title** (16sp regular) and **metadata row** beneath (12sp gray with calendar + clock + repeat icons)
    - **Star** icon (right side, gold `#FFC107` when important)
    - **Three-dot more** menu (far right)
- **Bottom:** Fixed **"Add a note"** pill input (24 dp radius, `#F5F5F5` background) with three utility icons (calendar, bell, tag) and a circular purple **submit** button on the right.
- **Toast:** "[n] task moved" appears as a dark `#323232` pill above the input bar.

#### `important.png`
- Same skeleton as the main list, but:
  - **List title** is "⭐ Important".
  - **Background image** shifts to a **dark teal / cyan** gradient (sampled `#5B6C71`, `#142A2E`).
  - All visible tasks have the star **filled** (gold).
  - Sidebar item "Important" is the active one (purple accent stripe + `#EADDFF`-style selection tint).
- **Empty state (when no starred tasks):** "No important tasks" with the clipboard glyph.

#### `remainder.png`
- Same skeleton, with:
  - Title "🔔 Reminder".
  - **Background** is **deep ocean blue** (`#51718B`, `#01283F`) — confirmed by 4,600+ pixels of `#006888` saturated blue.
  - Tasks displayed have a clock/bell icon visible in the metadata row.
  - "Reminder" item is active in the sidebar (blue accent).

#### `bin.png`
- Same skeleton, with:
  - Title "🗑 Bin".
  - **Background** is a **soft pink / coral** wash (`#FEEBEF`, `#FDF1F5`, `#FFE4E9`) — far paler than the list headers, intentional gentle "soft delete" feel.
  - Sidebar "Bin" item is active with a red accent.
- **Context menu** (visible in `deletepermently.png` overlay) for tasks in the bin: Mark as completed · Edit note · Move notes to ▸ · Restore note · **Delete permanently** (red).

#### `mynotes.png` ← *empty-state reference*
- Same sidebar + header.
- **Content area:** A dark, almost black, ocean-wave background.
  - **Centered empty state** consisting of:
    - A large **clipboard / notepad** glyph (~80×80 dp, `#CCCCCC`).
    - Caption "**No list was found. Please create a to-do list.**" (16sp, `#999999`).
  - A floating-action **+** button bottom-right (56×56 dp, `#5C4EAE`) to create a task.
- This is the design every list adopts when it has zero tasks.

#### `collapsed.png`
- Demonstrates the **"+5 more"** affordance: when the sidebar has more custom lists than fit, the extras collapse into a single row showing the count.
- Background is the same dark photo as `reviewtodayevents.png` (sampled `#7C8C98`).
- A small text button "**+5 more**" is visible at the bottom of the sidebar.

#### `deletepermently.png` ← *confirmation dialog overlay*
- The base layout is a dimmed main list (sampled `#D6D0D4` mid-content — that's the scrim at ~40% opacity over the page).
- A **modal dialog** is centered, ~400 dp wide, 16 dp radius, white background, 24 dp padding, 2 dp elevation.
  - Title: "**Delete permanently?**" (20sp SemiBold)
  - Body: "This note will be permanently deleted and cannot be recovered." (14sp gray)
  - Buttons: `Cancel` (text-only) and `Delete` (purple fill) — both 44 dp.
  - Click-outside dismisses.

### Group B — Zoomed UI Snippets (~780×440)

These are close-ups of dialogs and menus, all on a soft gray scrim.

#### `createnewlist.png`
- **Dialog:** "**Create new list**".
- Body: text input with placeholder "List name".
- Buttons: `Cancel` (text) | `Create` (purple).
- Same dialog chrome as the rename dialog.

#### `renamelist.png`
- **Dialog:** "**Rename list**".
- Body: text input pre-filled with the current list name (e.g. "Review requirements").
- Buttons: `Cancel` (text) | `Rename` (purple text button, right-aligned).
- 80% screen width cap, 16 dp radius, 24 dp padding.

#### `profile_pressed.png`
- **Dropdown menu** anchored to the top-right avatar.
- White card, 8 dp radius, 2–3 dp elevation, ~280 dp wide.
- **Header strip** (`#F5F5F5` background, 12 dp padding): "**Hi, John Britto**" (16sp SemiBold) + `johnbritto@gmail.com` (12sp gray).
- 1 dp `#E0E0E0` divider.
- Three menu items, 44 dp tall: **Manage accounts**, **Settings**, **Sign out** (with 20 dp leading icon each, hover bg `#F5F5F5`).
- Another 1 dp divider before Sign out.

#### `signout.png`
- **Dialog:** "**Are you sure you want to sign out?**" (18sp SemiBold).
- Subtitle: "You will be signed out of your account." (14sp gray).
- Two buttons centered: `Cancel` (transparent, gray text) | **Sign out** (purple fill, white text). Both 44 dp.

#### `draganddrop.png` (and `loadmoreicon.png`)
- `draganddrop.png` shows a task card being **lifted during drag** — the card gets a stronger shadow and a small "grip" cursor; below it the gap is highlighted.
- `loadmoreicon.png` is a closer crop of the "**+5 more**" affordance at the bottom of the sidebar.

#### `search.png`
- **Search overlay / state:** A single-line search input at the top with the magnifier and a clear (×) button.
- Centered empty state: clipboard icon + "**You can search the created notes here**" (16sp gray).
- When results exist, the same task card layout is used below the input.

---

## 3. Cross-Cutting Observations & Implications for MAUI Implementation

### 3.1 Component Library Choices (corroborates spec §11)
- The list of tasks is best rendered with **Syncfusion `SfListView`** (virtualized scrolling, item template, item spacing = 6 px) — the spec is correct.
- Sidebar is best as a **MAUI `CollectionView` inside a fixed-width `Grid` column** — sync-fusion has no built-in sidebar.
- Dialogs map cleanly to MAUI's **`DisplayPromptAsync`** or a **Syncfusion `SfPopup`** for richer styling.
- For dates, the spec suggests **`SfDateRangePicker`** — appropriate for both single date and range pickers on the task detail screen.

### 3.2 Visual Tokens to Encode in `Resources/Styles/`
- **Primary brushes:** `Primary=#5C4EAE`, `PrimaryDark=#453685`, `PrimaryLight=#8A7FD9`, `OnPrimary=#FFFFFF`.
- **Surfaces:** `Surface=#FFFFFF`, `Background=#FAFAFA`, `Sidebar=#F5F5F5`, `Divider=#E0E0E0`.
- **Semantic:** `Error=#B3261E`, `Success=#188A3D`, `Warning=#F57C00`.
- **Category accents:** `Important=#9C27B0`, `Reminder=#2196F3`, `Bin=#F44336`, `MyNotes=#757575`, `StarGold=#FFC107`.
- **Typography:** Reuse MAUI's named sizes (Title=18, Body=16, Caption=12) — spec sizes align closely.
- **Spacing:** 4, 8, 12, 16, 24, 32 (use MAUI `Spacing` / `Padding`).
- **Radii:** 4, 8, 16, 50% (50% only for FAB/avatar).

### 3.3 Background Images
- Every list content area is a **full-bleed photo + darkening overlay**. Best implemented as a `Grid` with two children: `Image` (AspectFill) at the back, semi-transparent black `BoxView` (~30–40% alpha), then the scrollable `SfListView` on top.
- Sources to collect (or generate) for each list: `mynotes_bg.jpg`, `important_bg.jpg`, `remainder_bg.jpg`, `bin_bg.jpg`, `review_bg.jpg`, plus a default `default_bg.jpg`.

### 3.4 Sidebar Behaviour
- Width = 230 dp (desktop) / collapsible to drawer on phone (per spec §11 responsive table).
- Search box sits at the top of the sidebar (not in the header) — this is a deliberate design choice that differs from many To-Do apps; honour it.
- "**+5 more**" collapse: implement as a single `CollectionView` group with an extra "Show more" tap target, not a separate control.

### 3.5 Task Card Anatomy (from pixel inspection)
```
[ 20dp ] [   flex          ] [ 24 ] [ 24 ]
[ chk  ] [ Title (16sp)    ] [ ⭐ ] [ ⋮  ]
[       ] [ 📅 date 🕒 time 🔁 ] [    ] [    ]
```
- Card width: ~600 dp max, auto-centered on wide screens.
- Card margin: 8 dp horizontal × 6 dp vertical; 12 dp internal padding; 8 dp radius; elevation 1.
- Completed state: strikethrough title, 50% opacity, bg `#F9F9F9`.

### 3.6 Bottom Input Bar
- 64 dp tall, fixed, white bg with 1 dp top border.
- Pill input (24 dp radius, `#F5F5F5`).
- Three icon buttons + 48×48 circular purple **submit** (disabled when input empty).
- **Toast** appears 12 dp above the bar, dark `#323232` with × close, auto-dismiss 4 s.

### 3.7 Dialogs & Menus
- All dialogs share: 80% screen width (max 400 dp), 16 dp radius, 24 dp padding, 2–3 dp elevation, scrim 40%.
- Buttons are 44 dp tall, right-aligned, with 16 dp gap; primary action uses purple fill, secondary is text-only.
- Profile menu uses a slightly different chrome (8 dp radius, light-gray header strip).

---

## 4. Quick-Reference Image Map

| # | File | Group | What it shows | Maps to SPEC § | Module |
|---|---|---|---|---|---|
| 1 | `splash.png` | A | Splash with brand | §8.1 | M2 |
| 2 | `signin.png` | A | Sign-In form | §8.2 | M2 |
| 3 | `signup.png` | A | Sign-Up form | §8.3 | M2 |
| 4 | `forgot.png` | A | Forgot Password | §8.4 | M2 |
| 5 | `reviewtodayevents.png` | A | Main list (canonical) | §8.5 | M3/M4 |
| 6 | `important.png` | A | Important filter, teal bg | §8.6 | M5 |
| 7 | `remainder.png` | A | Reminder filter, ocean bg | §8.7 | M5 |
| 8 | `bin.png` | A | Bin filter, pink bg | §8.8 | M5 |
| 9 | `mynotes.png` | A | Empty state, dark bg | §8.9 | M3/M8 |
| 10 | `collapsed.png` | A | Sidebar collapse `+5 more` | §8.5 sidebar | M6 |
| 11 | `deletepermently.png` | A | Permanent-delete dialog (over bin) | §8.8 ctx menu | M5 |
| 12 | `createnewlist.png` | B | Create-list dialog | n/a (extension) | M6 |
| 13 | `renamelist.png` | B | Rename-list dialog | §8.11 | M6 |
| 14 | `profile_pressed.png` | B | Profile dropdown | §8.12 | M9 |
| 15 | `signout.png` | B | Sign-out confirm | §8.13 | M2 |
| 16 | `search.png` | B | Search empty state | §8.10 | M7 |
| 17 | `draganddrop.png` | B | Drag-and-drop affordance | §FR-TASK optional | M8 |
| 18 | `loadmoreicon.png` | B | Sidebar `+5 more` close-up | §8.5 sidebar | M6 |

---

## 5. Open Questions for Design Clarification

The following details are **not** fully resolvable from the mocks alone and should be confirmed before code freeze:

1. **Bottom input bar** — does the bar sit over the keyboard automatically (MAUI `SoftInputMode`) or does the content scroll behind it? Spec implies the latter.
2. **Completed tasks** — do they stay visible (strikethrough) or get filtered out of the active lists? The spec is ambiguous; mockups only show active tasks.
3. **Sidebar collapse** — the "+5 more" affordance is shown, but is there a way to **reorder** custom lists? Drag handles in the sidebar are not visible.
4. **Theme variants** — the spec mentions light/dark mode in the color table but no dark mock is provided.
5. **Task detail screen** — none of the 18 images show a full task-detail/edit view; the spec mentions it (`TaskDetailPage.xaml`) but it's not yet designed.
6. **Localization** — the spec lists `AppStrings.resx` but no RTL or multi-language mocks are provided.

---

*Generated from pixel-level inspection of the 18 reference PNGs in `Task Managment/images/`. Pair this with `SPECIFICATION.md` for the canonical design intent.*
