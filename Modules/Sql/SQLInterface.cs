using System.Data;
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
            patron_id INTEGER,
            date TEXT,
            status INTEGER,

            FOREIGN KEY(patron_id) REFERENCES patron(id)
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

    public static string CONNECTION_STRING { get; set; } = "";

    public static void Initialize(String dbPath)
    {
        CONNECTION_STRING = $"Data Source={dbPath}";
    }

    public static int CreateSqliteDB()
    {
        try
        {
            Logger<SQLInterface>.Log("SQL initialization sequence started.", LogLevel.Info);
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

    public static int ConsolidateLoans()
    {
        Logger<SQLInterface>.Log($"Consolidating loans into notes: {CONNECTION_STRING}", LogLevel.Info);
        try
        {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING))
            {
                /*
                READ DATA

                Pull the id and loan_date from the database of all loans that are not attached to a note via a note_loan instance. 
                This creates a 2x? data table if not empty.
                */

                connection.Open();
                SqliteCommand command = new SqliteCommand("SELECT loan.id, loan.loan_date FROM loan WHERE NOT EXISTS (SELECT 1 FROM note_loan WHERE note_loan.loan_id = loan.id)", connection);
                SqliteDataReader reader = command.ExecuteReader();
    
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);

                // Consolidation
                foreach (DataRow row in dataTable.Rows)
                {
                    int loanId = Convert.ToInt32(row[0]);
                    DateTime loanDate = ParseDates.ConvertStringToDateTime(row[1].ToString()!);

                    // Do some algorithm here to sort the loans into days/notes, possibly a dictionary.
                    // You need to make it so that the note table knows what day and patron to sort to.
                    Console.WriteLine($"{loanId}\t{loanDate.Date}");
                }

                // Write Data


                reader.Close();
                connection.Close();
            }
        }
        catch (Exception e)
        {
            Logger<SQLInterface>.Log($"Failed to consolidate loans into notes: {e.Message}", LogLevel.Error);
            return 9;
        }

        return 0;
    }
}