# Quick Reference - Syncfusion MAUI To Do List Sample

**For:** Syncfusion MAUI To Do List sample application
**Updated:** October 2026
**Scope:** In-scope screens only (auth, profile, splash, settings are out of scope)

---

## Specification Files

| Document | Purpose | Audience |
|----------|---------|----------|
| **README.md** | Project overview, scope, quick start | Everyone |
| **StartHere.md** | Orientation and reading paths | Newcomers |
| **Specification.md** | UI + behavior contract for in-scope screens | Developers, designers |
| **Harness.md** | Gate-Based development plan, task independence rules | Implementers, reviewers |
| **QuickReference.md** | This document – lookups while coding | Developers |
| **ImplementationSummary.md** | Executive overview, key decisions | Decision makers |
| **Deliverables.md** | What is delivered, coverage matrix | Reviewers, leads |
| **Tasks/** | Per-task instructions | Implementers |

---

## Module / Task Checklist

The work is organized into **8 modules**, each broken into small, gate-based, independent tasks. See `Tasks/Module-XX-YY/Task-X-XX-*.md` for the exact instructions.

### Module 01: Foundation
- [ ] 1-01 Project structure (lightweight feature folders)
- [ ] 1-02 NuGet packages (Syncfusion + CommunityToolkit.MVVM)
- [ ] 1-03 MVVM Toolkit baseline
- [ ] 1-04 Resource dictionaries (Colors, Sizes, Fonts, Styles)
- [ ] 1-05 MauiProgram bootstrap (no DI container)
- [ ] 1-06 Seed data (in-memory `ObservableCollection<Note>`)
- [ ] 1-07 Notes list page shell (matches empty `mynotes.png`)

### Module 02: MyNotes
- [ ] 2-1 Notes list ViewModel
- [ ] 2-2 Sidebar navigation (expanded and collapsed)
- [ ] 2-3 Header bar
- [ ] 2-4 Note item template (matches `mynotes.png` card)
- [ ] 2-5 SfListView integration
- [ ] 2-6 Bottom input bar
- [ ] 2-7 Empty state (matches `mynotes.png` empty state)

### Module 03: TaskCRUD
- [ ] 3-1 In-memory note service
- [ ] 3-2 Edit note page
- [ ] 3-3 Edit note ViewModel
- [ ] 3-4 Create from bottom bar
- [ ] 3-5 Edit existing note
- [ ] 3-6 Complete toggle
- [ ] 3-7 Soft delete
- [ ] 3-8 More menu (context menu)

### Module 04: Filtering
- [ ] 4-1 Important view (matches `important.png`)
- [ ] 4-2 Star toggle
- [ ] 4-3 Reminder view (matches `remainder.png`)
- [ ] 4-4 Reminder date picker
- [ ] 4-5 Bin view (matches `bin.png`)
- [ ] 4-6 Restore
- [ ] 4-7 Permanent delete (matches `deletepermently.png`)

### Module 05: CustomLists
- [ ] 5-1 In-memory list service
- [ ] 5-2 Create list dialog (matches `createnewlist.png`)
- [ ] 5-3 Rename list dialog (matches `renamelist.png`)
- [ ] 5-4 Sidebar loads custom lists
- [ ] 5-5 Delete custom list

### Module 06: Search
- [ ] 6-1 In-memory search service
- [ ] 6-2 Search results page (matches `search.png`)
- [ ] 6-3 Search integration in header

### Module 07: DragAndDrop
- [ ] 7-1 Drag reorder in list (matches `draganddrop.png`)
- [ ] 7-2 Drag to sidebar (move between lists)

### Module 08: Polish
- [ ] 8-1 Responsive layout pass (matches `collapsed.png` on narrow widths)
- [ ] 8-2 Accessibility pass
- [ ] 8-3 Empty / loading states pass
- [ ] 8-4 Customer walkthrough
- [ ] 8-5 Final cleanup

> **No module** includes sign-in, sign-up, forgot-password, splash, profile, settings, or account code.

---

## Design Tokens

All values live in `Resources/Styles/`. **No hardcoded values in XAML or code-behind.**

### Colors (matched to the design images)
- **Primary** – primary action / brand accent
- **Important** – Important category accent
- **Reminder** – Reminder category accent
- **Bin** – Bin category accent and destructive action
- **My Notes** – default list, neutral
- **Text Primary / Secondary / Tertiary**
- **Surface / Background / Divider / Border**

Concrete hex values are stored in `Resources/Styles/Colors.xaml` and matched against the reference images. Update the dictionary, not the XAML, when adjusting colors.

### Spacing
```
4, 8, 12, 16, 24, 32 dp
```

### Typography
```
Caption:   11 sp
Small:     12 sp
Label:     14 sp
Body:      16 sp
Title:     18 sp
Heading:   20 sp
Large:     24 sp
```

### Border Radius
```
Small:   4 dp
Medium:  8 dp
Large:   16 dp
Pill:    24 dp
Full:    50%  (avatars, round buttons)
```

### Icon Sizes
```
Small:  20 dp
Medium: 24 dp
Large:  32 dp
```

---

## Responsive Breakpoints

| Width | Sidebar | Notes |
|-------|---------|-------|
| < 600 px | Collapsed (`collapsed.png`) | Single column, sidebar expands on tap |
| 600-1024 px | Visible, condensed | Two-column layout |
| >= 1025 px | Visible, full | Wide layout, content centered with max width |

---

## Project Structure (lightweight)

```
src/
├── App.xaml(.cs)
├── MauiProgram.cs
├── Common/
│   ├── ViewModelBase.cs        # INotifyPropertyChanged only
│   └── SampleData.cs           # Seeded in-memory notes
├── Features/
│   ├── Notes/                  # Default list
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── Models/
│   │   └── Services/           # In-memory store
│   ├── Important/              # Filter view
│   ├── Reminder/               # Filter view
│   ├── Bin/                    # Soft delete + restore + permanent delete
│   ├── CustomLists/            # Create / rename / delete lists
│   ├── Search/                 # In-memory title match
│   └── EditNote/               # Create / edit dialog page
└── Resources/
    ├── Styles/
    │   ├── Colors.xaml
    │   ├── Sizes.xaml
    │   ├── Fonts.xaml
    │   └── Styles.xaml
    └── Images/
```

> There is **no** Authentication/, Profile/, Settings/, or Splash/ folder. If a task needs one, it is the wrong task.

---

## NuGet Packages

### Syncfusion
- `Syncfusion.Maui.ListView` 24.1.x – main notes list
- `Syncfusion.Maui.Calendar` 24.1.x – date pickers (only if a task needs it)
- `Syncfusion.Maui.Core` 24.1.x – base framework

### MVVM
- `CommunityToolkit.MVVM` 8.4.x – `[ObservableProperty]`, `[RelayCommand]`

### Persistence (only if a task needs it)
- Local JSON file in the app data directory is allowed and preferred
- Local SQLite is allowed **only** when a single task truly requires it (none of the current tasks do)

> No HTTP client, no auth library, no push notification package, no cloud SDK.

---

## Development Patterns

### ViewModel
```csharp
public partial class NotesViewModel : ViewModelBase
{
    private readonly INoteService _noteService;

    [ObservableProperty]
    private ObservableCollection<Note> notes = new();

    [ObservableProperty]
    private string newNoteTitle;

    public NotesViewModel(INoteService noteService)
    {
        _noteService = noteService;
    }

    [RelayCommand]
    private async Task AddNoteAsync()
    {
        if (string.IsNullOrWhiteSpace(NewNoteTitle)) return;
        var note = new Note { Title = NewNoteTitle.Trim() };
        await _noteService.CreateAsync(note);
        Notes.Add(note);
        NewNoteTitle = string.Empty;
    }
}
```

### Service Interface
```csharp
public interface INoteService
{
    Task<Note> CreateAsync(Note note);
    Task UpdateAsync(Note note);
    Task DeleteAsync(string id);          // soft delete
    Task RestoreAsync(string id);
    Task ToggleCompleteAsync(string id);
    Task ToggleImportantAsync(string id);
    IReadOnlyList<Note> GetAll();
}
```

### Service Implementation (in-memory)
```csharp
public class InMemoryNoteService : INoteService
{
    private readonly ObservableCollection<Note> _notes = new(SampleData.SeedNotes());

    public Task<Note> CreateAsync(Note note)
    {
        note.Id = Guid.NewGuid().ToString();
        note.CreatedAt = DateTime.UtcNow;
        _notes.Add(note);
        return Task.FromResult(note);
    }

    // ... other methods follow the same pattern
}
```

### Resource Usage in XAML
```xml
<Button
    Text="Add note"
    Style="{StaticResource PrimaryButtonStyle}"
    BackgroundColor="{StaticResource PrimaryColor}" />
```

### No Code-Behind Logic
```csharp
// NotesPage.xaml.cs — keep this file empty of business logic
public partial class NotesPage : ContentPage
{
    public NotesPage(NotesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

---

## Syncfusion Usage Checklist

When you use a Syncfusion control in a task, document in the task file:

- **Control used** – exact name and namespace (e.g. `Syncfusion.Maui.ListView.SfListView`)
- **Why chosen** – the reason this control is preferred over a custom or standard alternative
- **Customer scenario** – the realistic customer use case this control demonstrates

Do not introduce custom controls where Syncfusion already provides a working control. Do not add Syncfusion controls that the design does not require.

---

## Manual Verification Steps (per task)

Each task must include a short checklist the reviewer can run:

```
- [ ] dotnet build succeeds
- [ ] Sample launches on chosen target
- [ ] The new screen / behavior appears
- [ ] Design image is matched
- [ ] No other screens regressed
- [ ] No code-behind logic added
- [ ] No hardcoded values added
- [ ] No out-of-scope code added
```

---

## Debugging Tips

### Common Issues

**Issue:** `SfListView` does not render items
- [ ] Check `ItemsSource` is bound to an `ObservableCollection`
- [ ] Check `ItemTemplate` is set
- [ ] Check virtualization is not over-aggressive for the test data size

**Issue:** MVVM binding does not update
- [ ] Verify `[ObservableProperty]` is used
- [ ] Verify the binding `Mode` is correct (`TwoWay` for edits)
- [ ] Verify the source property is on a public partial class

**Issue:** `dotnet build` fails after adding Syncfusion
- [ ] Confirm `Syncfusion.Maui.Core` is referenced from `MauiProgram.CreateMauiApp()`
- [ ] Confirm all Syncfusion packages share the same major version
- [ ] Clean and restore: `dotnet clean && dotnet restore`

**Issue:** Design tokens do not apply
- [ ] Confirm the resource dictionary is merged in `App.xaml`
- [ ] Confirm the key exists in the dictionary
- [ ] Confirm the spelling in `{StaticResource TokenName}` matches

**Issue:** Sample does not open to the right page
- [ ] Confirm `App.xaml` startup page is set to `NotesPage` (or a navigation root that opens to it)
- [ ] Confirm `AppShell.xaml` (if used) routes to `NotesPage`

---

## Out of Scope (do not implement)

If a task seems to need any of the following, it is the wrong task — split it or remove it.

- Sign-in, sign-up, forgot-password, sign-out
- Splash screens, onboarding flows, settings pages
- Profile pages, account management
- Backend APIs, cloud sync, remote databases
- Push notifications, scheduled reminders, OS notifications
- User accounts, sessions, tokens, SecureStorage
- Global application services, DI containers, shared application state
- Production-scale infrastructure, enterprise patterns

The design images reflect this scope: `images/` contains only the in-scope task-management screens.

---

## Reference Links

### Syncfusion
- [SfListView](https://help.syncfusion.com/maui/listview/listview)
- [SfDatePicker / SfCalendar](https://help.syncfusion.com/maui/calendar/date-picker)
- [MAUI Getting Started](https://help.syncfusion.com/maui/introduction/getting-started)

### MAUI
- [MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [MVVM Toolkit](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction)
- [Data Binding](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/)

### Standards
- [WCAG 2.1](https://www.w3.org/WAI/WCAG21/quickref/)
- [Semantic Versioning](https://semver.org/)

---

**Version:** 2.0
**Updated:** October 2026
**Status:** Aligned to in-scope screens and Gate-Based Harness Approach
