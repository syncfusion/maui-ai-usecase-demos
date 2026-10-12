// Stubs/SQLite.cs
// Minimal stub of the sqlite-net-pcl types referenced by Constants.cs so
// that file can be compiled unchanged inside the plain net10.0 test
// project.  The real SQLite assembly is only available through the MAUI
// workload.

using System;

namespace SQLite
{
    /// <summary>
    /// Mirror of <c>SQLite.SQLiteOpenFlags</c>.  Only the flag values used
    /// by <c>Constants.Database.OpenFlags</c> are declared.
    /// </summary>
    [Flags]
    public enum SQLiteOpenFlags
    {
        ReadOnly = 1,
        ReadWrite = 2,
        Create = 4,
        SharedCache = 0x01000000,
    }
}
