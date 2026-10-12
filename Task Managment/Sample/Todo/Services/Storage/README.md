# Storage Services

Local persistence services. The SQLite-backed
`LocalStorageService` is created in Task 1-07
(`Database Setup`) and the database initialization
goes through the `sqlite-net-pcl` / `SQLitePCLRaw.bundle_green` packages
added in Task 1-02 (`Install and Verify NuGet Packages`).

- `ILocalStorageService.cs` — Generic key/value contract that returns
  `Result<T>` so failures surface as structured values.
- `LocalStorageService.cs` — JSON-serialised implementation that stores
  entries in the SQLite `Preferences` table.
