public class DataCheck {

    private static readonly string DATABASE_FILE_NAME = "database.db";

    /// <summary>
    /// This method ensures that a database exists for Auto-Suspend to use by creating one if not found.
    /// </summary>
    /// <param name="path">Directory of where the database.db file should be.</param>
    /// <returns>Integer overflow.</returns>
    public static int CheckData(string path) {
        int assertedDatabase = AssertDatabase(path);
        if (assertedDatabase != 0) {
            return assertedDatabase;
        }

        Logger<DataCheck>.Log("Data check complete, no errors found.", LogLevel.Info);
        return 0;
    }

    /// <summary>
    /// This method checks to see if the database is found in the given directorry and creates one if not.
    /// </summary>
    /// <param name="path">Directory that houses database.db.</param>
    /// <returns>Integer overflow.</returns>
    private static int AssertDatabase(string path) {
        string databasePath = Path.Join(path, DATABASE_FILE_NAME);
        SQLInterface.Initialize(databasePath); // Initialize SQLInterface; Extremely important.
        if (!File.Exists(databasePath)) {
            int database = CreateDatabase(databasePath);
            if (database != 0) {
                return database;
            }
        }

        return 0;
    }

    /// <summary>
    /// This method creates a database.db file in the given path and executes SQLInterface.CreateSqliteDB().
    /// </summary>
    /// <param name="path">Directory that houses database.db.</param>
    /// <returns>Integer overflow.</returns>
    private static int CreateDatabase(string path) {
        try {
            File.Create(path).Dispose();
            Logger<DataCheck>.Log($"Created database.db", LogLevel.Info);

            int initializedSQL = SQLInterface.CreateSqliteDB();
            if (initializedSQL != 0) {
                return initializedSQL;
            }
        }
        catch (Exception e) {
            Logger<DataCheck>.Error("Cannot create database.db", e);
            return 7;
        }

        return 0;
    }
}