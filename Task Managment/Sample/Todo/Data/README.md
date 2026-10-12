# Data

The data access layer.

- `AppDbContext.cs` — Wraps the shared `SQLiteAsyncConnection`, creates
  the schema on first run and enables foreign keys (Task 1-07 —
  `Database Setup`).
- `Repositories/`
  - `IRepository.cs` — Generic async CRUD contract (Task 1-07).
  - `Repository.cs` — Generic repository implementation with
    soft-delete filtering (Task 1-07).
