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
            type TEXT
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

    public static int InitializeSQL(String dbPath)
    {
        try
        {
            Logger<SQLInterface>.Log("Connection opening.", LogLevel.Info);
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS loan (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    alma_id TEXT,
                    out_circ_desk TEXT,
                    in_circ_desk TEXT,
                    patron_id INTEGER,
                    item_id INTEGER,
                    loan_date TEXT,
                    due_date TEXT,
                    return_date TEXT
                )
            """;
            command.ExecuteNonQuery();

            Logger<SQLInterface>.Log("Connection closed", LogLevel.Info);
        }
        catch (Exception e)
        {
            Logger<SQLInterface>.Log($"Failed to Initialize SQL database: {e.Message}", LogLevel.Error);
            return 8;
        }

        return 0;
    }
}