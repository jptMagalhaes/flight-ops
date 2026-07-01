using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FlightOps.Infrastructure;

public static class DbUniqueViolationDetector
{
    private const int SqliteConstraintUnique = 2067; // SQLITE_CONSTRAINT_UNIQUE

    public static bool IsUniqueViolation(DbUpdateException exception, params string[] columnHints)
    {
        if (exception.InnerException is not SqliteException sqliteException
            || sqliteException.SqliteExtendedErrorCode != SqliteConstraintUnique)
            return false;

        return columnHints.Length == 0
            || columnHints.Any(hint => sqliteException.Message.Contains(hint, StringComparison.OrdinalIgnoreCase));
    }
}
