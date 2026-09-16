public class DataCheck {

    private static readonly string DATABASE_FILE_NAME = "database.db";

    /// <summary>
    /// This method checks to see if the database is found in the given directorry and creates one if not.
    /// </summary>
    /// <param name="path">Directory that houses database.db.</param>
    /// <returns>Integer overflow.</returns>
    public static void AssertDatabase(string path) {
        string databasePath = Path.Join(path, DATABASE_FILE_NAME);
        SQLInterface.Initialize(databasePath); // Initialize SQLInterface; Extremely important.
        if (!File.Exists(databasePath)) {
            int database = CreateDatabase(databasePath);
        }
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