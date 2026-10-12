# Repositories

Repository implementations. Each repository wraps a `SQLiteAsyncConnection`
(provided by `sqlite-net-pcl`) through the shared `AppDbContext` and
exposes async CRUD methods that the service layer can call.

- `IRepository.cs` — Generic async CRUD contract.
- `Repository.cs` — Default generic implementation with soft-delete
  filtering and timestamp management.
