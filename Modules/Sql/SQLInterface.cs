using Microsoft.Data.Sqlite;

public class SQLInterface
{
    public static readonly string CREATE_PATRON_TABLE_COMMAND = """
        CREATE TABLE IF NOT EXISTS patron (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            alma_id TEXT,
            eagle_id TEXT,
            first_name TEXT,
            last_name TEXT,
            user_group TEXT
        )
    """;

    public static readonly string CREATE_ITEM_TABLE_COMMAND = """
        CREATE TABLE IF NOT EXISTS item (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            mms_id TEXT,
            barcode TEXT,
            title TEXT,
            description TEXT,
            policy TEXT
        )
    """;

    public static readonly string CREATE_LOAN_TABLE_COMMAND = """
        CREATE TABLE IF NOT EXISTS loan (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            alma_id TEXT,
            out_circ_desk TEXT,
            in_circ_desk TEXT,
            patron_id INTEGER,
            item_id INTEGER,
            loan_date TEXT,
            due_date TEXT,
            return_date TEXT,

            FOREIGN KEY(patron_id) REFERENCES patron(id),
            FOREIGN KEY(item_id) REFERENCES item(id)
        )
    """;

    public static readonly string CREATE_NOTE_TABLE_COMMAND = """
        CREATE TABLE IF NOT EXISTS note (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            status INTEGER
        )
    """;

    public static readonly string CREATE_NOTE_LOAN_TABLE_COMMAND = """
        CREATE TABLE IF NOT EXISTS note_loan (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            note_id INTEGER,
            loan_id INTEGER,

            FOREIGN KEY(note_id) REFERENCES note(id),
            FOREIGN KEY(loan_id) REFERENCES loan(id)
        )
    """;

    public static readonly string[] CREATE_TABLE_COMMANDS = [CREATE_PATRON_TABLE_COMMAND, CREATE_ITEM_TABLE_COMMAND,
        CREATE_LOAN_TABLE_COMMAND, CREATE_NOTE_TABLE_COMMAND, CREATE_NOTE_LOAN_TABLE_COMMAND];

    public static string CONNECTION_STRING = "";

    public static int InitializeSQL(String dbPath)
    {
        try
        {
            Logger<SQLInterface>.Log("SQL initialization sequence started.", LogLevel.Info);
            CONNECTION_STRING = $"Data Source={dbPath}";

            using SqliteConnection connection = new SqliteConnection(CONNECTION_STRING);
            connection.Open();

            // Create Tables
            for (int i = 0; i < CREATE_TABLE_COMMANDS.Length; i++)
            {
                using var command = connection.CreateCommand();
                command.CommandText = CREATE_TABLE_COMMANDS[i];
                command.ExecuteNonQuery();
            }

            Logger<SQLInterface>.Log("SQL initialized successfully.", LogLevel.Info);
        }
        catch (Exception e)
        {
            Logger<SQLInterface>.Log($"Failed to Initialize SQL database: {e.Message}", LogLevel.Error);
            return 8;
        }

        return 0;
    }

    public static int ConsolidateNotes()
    {
        try
        {
            using SqliteConnection connection = new SqliteConnection(CONNECTION_STRING);
            connection.Open();

            
        }
        catch (Exception e)
        {
            Logger<SQLInterface>.Log($"Failed to consolidate loans into notes: {e.Message}", LogLevel.Error);
            return 9;
        }

        return 0;
    }
}