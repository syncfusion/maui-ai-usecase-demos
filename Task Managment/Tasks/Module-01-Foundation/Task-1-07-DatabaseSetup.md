# Task 1-07: Implement Database Layer (AppDbContext, LocalStorageService)

**Module:** 1 - Foundation & Setup  
**Phase:** 1  
**Priority:** Critical  
**Duration:** 1 day  
**Dependencies:** Task 1-02 (NuGet), Task 1-03 (MVVM)  
**Status:** ⬜ Not Started

---

## 📋 TASK DESCRIPTION

Create the SQLite database layer with AppDbContext, table definitions, and LocalStorageService. This establishes data persistence for the entire application.

## 🎯 OBJECTIVES

- [ ] Create AppDbContext class
- [ ] Define all database tables (Task, TaskList, User)
- [ ] Create database initialization logic
- [ ] Implement LocalStorageService interface
- [ ] Test database operations
- [ ] Verify schema creates correctly

---

## 📝 FILES TO CREATE

```
Data/
├── AppDbContext.cs          ← Database context
├── Repositories/
│   ├── IRepository.cs       ← Generic repository interface
│   └── Repository.cs        ← Generic repository implementation
Services/Storage/
├── ILocalStorageService.cs  ← Interface
└── LocalStorageService.cs   ← Implementation
Models/
├── Task.cs                  ← Task model
├── TaskList.cs              ← TaskList model
└── User.cs                  ← User model
```

---

## 💻 DATABASE SCHEMA

### Table: Users
```sql
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,
    Email TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    DisplayName TEXT,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    DeletedAt DATETIME NULL
);
```

### Table: TaskLists
```sql
CREATE TABLE TaskLists (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Color TEXT,
    DisplayOrder INTEGER,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    DeletedAt DATETIME NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

### Table: Tasks
```sql
CREATE TABLE Tasks (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL,
    ListId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT,
    IsCompleted BOOLEAN DEFAULT 0,
    IsImportant BOOLEAN DEFAULT 0,
    DueDate DATETIME NULL,
    ReminderTime DATETIME NULL,
    RecurrenceType TEXT,
    DisplayOrder INTEGER,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    DeletedAt DATETIME NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ListId) REFERENCES TaskLists(Id)
);
```

---

## 💻 IMPLEMENTATION COMPONENTS

### 1. Model Classes (Task.cs, TaskList.cs, User.cs)

**Task.cs**
```csharp
[Table("Tasks")]
public class Task
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; }
    public string ListId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsImportant { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderTime { get; set; }
    public string RecurrenceType { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
```

**TaskList.cs**
```csharp
[Table("TaskLists")]
public class TaskList
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; }
    public string Title { get; set; }
    public string Color { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
```

### 2. AppDbContext

```csharp
public class AppDbContext
{
    private SQLiteConnection _connection;
    
    public AppDbContext(string dbPath)
    {
        _connection = new SQLiteConnection(dbPath);
        CreateTables();
    }
    
    private void CreateTables()
    {
        _connection.CreateTable<User>();
        _connection.CreateTable<TaskList>();
        _connection.CreateTable<Task>();
    }
}
```

### 3. Generic Repository Pattern

```csharp
public interface IRepository<T> where T : new()
{
    Task<T> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> InsertAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<int> DeleteAsync(string id);
}
```

### 4. LocalStorageService

```csharp
public interface ILocalStorageService
{
    Task<Result<T>> GetAsync<T>(string key);
    Task<Result> SetAsync<T>(string key, T value);
    Task<Result> DeleteAsync(string key);
}

public class LocalStorageService : ILocalStorageService
{
    private readonly SQLiteConnection _connection;
    
    // Implementations...
}
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] AppDbContext compiles and initializes
- [ ] All tables created in SQLite database
- [ ] Task model has 15+ properties
- [ ] TaskList model has 7+ properties
- [ ] User model has required properties
- [ ] Soft delete pattern implemented (DeletedAt field)
- [ ] Primary keys as TEXT (GUID)
- [ ] Foreign keys setup correctly
- [ ] Repository pattern implemented
- [ ] LocalStorageService works
- [ ] Database file created successfully
- [ ] No SQL errors on table creation

---

## 🧪 TESTING

### Unit Tests

**Test: Database Initialization**
```
Given: New AppDbContext
When: Initialize database
Then: All tables created successfully
```

**Test: Insert Task**
```
Given: Empty Tasks table
When: Insert new Task
Then: Task stored with generated ID
```

**Test: Query Tasks**
```
Given: Tasks in database
When: Query all tasks for user
Then: Returns only user's tasks
```

**Test: Soft Delete**
```
Given: Task in database
When: DeletedAt set
Then: Query excludes soft-deleted tasks
```

### Manual Tests

- [ ] Open database file with SQLite browser
- [ ] Verify all 3 tables exist
- [ ] Verify column names match schema
- [ ] Verify primary/foreign keys exist
- [ ] Insert test data
- [ ] Query data successfully
- [ ] Soft delete test

---

## 📊 DEFINITION OF DONE

**Code Quality:**
- [ ] Models use sqlite-net-pcl attributes correctly
- [ ] Tables use [Table] and [PrimaryKey] attributes
- [ ] Consistent naming (PascalCase)
- [ ] All properties documented
- [ ] No hardcoded connection strings

**Testing:**
- [ ] Database creates without errors
- [ ] All 3 tables exist
- [ ] CRUD operations work
- [ ] Foreign keys enforced
- [ ] Soft delete logic works

**Documentation:**
- [ ] XML comments on public classes
- [ ] Schema documented
- [ ] Task marked complete

---

## 🔗 CROSS-REFERENCES

### Related Tasks
- Task 2-01: Implement AuthenticationService (first service using DB)
- Task 4-01: Implement TaskService (main DB service)

### Reference Materials
- sqlite-net-pcl docs: https://github.com/praeclarum/sqlite-net
- SPECIFICATION.md → Section 11: Data Models
- QUICK_REFERENCE.md → Data Storage

---

## 📝 DATABASE INITIALIZATION

### Determine Database Path

```csharp
// In MauiProgram or startup
var dbPath = Path.Combine(
    FileSystem.AppDataDirectory,
    "todo.db"
);
// Result: ~/.local/share/todo.db (varies by platform)
```

### First Launch Setup

```csharp
public class AppDbContext
{
    public AppDbContext(string dbPath)
    {
        _connection = new SQLiteConnection(dbPath);
        
        // Create tables if don't exist
        _connection.CreateTable<User>();
        _connection.CreateTable<TaskList>();
        _connection.CreateTable<Task>();
    }
}
```

---

## 🛠️ SQLITE-NET ATTRIBUTES

```csharp
// Primary Key
[PrimaryKey]
public string Id { get; set; }

// Table Name
[Table("Tasks")]
public class Task { }

// Column Properties
[Column("task_title")]      // Custom column name
public string Title { get; set; }

[Ignore]                    // Skip column
public string TransientData { get; set; }

[Unique]                    // Unique constraint
public string Email { get; set; }

[NotNull]                   // Not nullable
public string Name { get; set; }
```

---

## ⚠️ COMMON ISSUES

**Issue:** Database file not created
- **Cause:** Invalid path or permissions
- **Solution:** Use FileSystem.AppDataDirectory, verify permissions

**Issue:** Tables not created
- **Cause:** CreateTable() not called or SQLiteConnection not opened
- **Solution:** Call CreateTable<T>() in constructor

**Issue:** Foreign key constraint fails
- **Cause:** Referenced record doesn't exist
- **Solution:** Insert parent record first

---

## 📋 IMPLEMENTATION CHECKLIST

- [ ] Create Models/ folder
- [ ] Create Task.cs with all 15+ properties
- [ ] Create TaskList.cs with 7+ properties
- [ ] Create User.cs
- [ ] Create Data/ folder
- [ ] Create AppDbContext.cs
- [ ] Create Repository interface and implementation
- [ ] Create LocalStorageService
- [ ] Add database initialization logic
- [ ] Test database creation
- [ ] Test table creation
- [ ] Test CRUD operations
- [ ] Commit to version control

---

**Created:** October 2026  
**Version:** 1.0  
**Status:** Ready for Implementation
