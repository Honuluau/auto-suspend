public class DataCheck
{

    private static string databaseFileName = "database.db";

    public static int createDatabase(string path)
    {
        try
        {
            File.Create(path).Dispose();
            Logger<DataCheck>.Log($"Created database.db", LogLevel.Info);

            int initializedSQL = SQLInterface.CreateSqliteDB();
            if (initializedSQL != 0)
            {
                return initializedSQL;
            }
        }
        catch (Exception e)
        {
            Logger<DataCheck>.Error("Cannot create database.db", e);
            return 7;
        }

        return 0;
    }

    // Check if database exists, return path.
    public static int assertDatabase(string path)
    {
        string databasePath = Path.Join(path, databaseFileName);
        SQLInterface.Initialize(databasePath); // Initialize SQLInterface; Extremely important.
        if (!File.Exists(databasePath))
        {
            int database = createDatabase(databasePath);
            if (database != 0)
            {
                return database;
            }
        } 

        return 0;
    }

    public static int CheckData(string path)
    {
        int assertedDatabase = assertDatabase(path);
        if (assertedDatabase != 0)
        {
            return assertedDatabase;
        }

        Logger<DataCheck>.Log("Data check complete, no errors found.", LogLevel.Info);

        return 0;
    }
}