using System.Data;
using System.IO.Pipelines;
using Microsoft.Data.Sqlite;
using SQLitePCL;

public class SQLInterface
{
    public static readonly string CREATE_PATRON_TABLE_COMMAND = """
        CREATE TABLE IF NOT EXISTS patron (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
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
            description TEXT
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
            policy TEXT,
            preferred_email TEXT,
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
            updated INTEGER, 

            FOREIGN KEY(patron_id) REFERENCES patron(id)
        )
    """; // 0 = NOT UPDATED, NOTE NEEDS TO BE PUBLISHED TO ALMA // 1 = UPDATED, NO ACTION NECESSARY.

    public static readonly string CREATE_NOTE_LOAN_TABLE_COMMAND = """
        CREATE TABLE IF NOT EXISTS note_loan (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            note_id INTEGER,
            loan_id INTEGER,

            FOREIGN KEY(note_id) REFERENCES note(id),
            FOREIGN KEY(loan_id) REFERENCES loan(id)
        )
    """;

    public static readonly string CREATE_PERM_SUSPEND_TABLE_COMMAND = """
        CREATE TABLE IF NOT EXISTS perm_suspend (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            patron_id INTEGER,
            note TEXT,

            FOREIGN KEY(patron_id) REFERENCES patron(id)
        )
    """;

    public static readonly string[] CREATE_TABLE_COMMANDS = [CREATE_PATRON_TABLE_COMMAND, CREATE_ITEM_TABLE_COMMAND, CREATE_LOAN_TABLE_COMMAND,
        CREATE_NOTE_TABLE_COMMAND, CREATE_NOTE_LOAN_TABLE_COMMAND, CREATE_PERM_SUSPEND_TABLE_COMMAND];

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
                SqliteCommand command = new SqliteCommand("SELECT loan.id, loan.loan_date, loan.patron_id FROM loan WHERE NOT EXISTS (SELECT 1 FROM note_loan WHERE note_loan.loan_id = loan.id)", connection);
                SqliteDataReader reader = command.ExecuteReader();

                DataTable dataTable = new DataTable();
                dataTable.Load(reader);

                reader.Close();

                // Consolidation
                foreach (DataRow row in dataTable.Rows)
                {
                    int loanId = Convert.ToInt32(row[0]);
                    DateTime loanDate = ParseDates.ConvertStringToDateTime(row[1].ToString()!);
                    int patronId = Convert.ToInt32(row[2]);
                    int noteId = -1;

                    // Ensure note exists in SQL.
                    string query = "SELECT id FROM note WHERE patron_id = $patronId AND date = $loanDate";
                    using (SqliteCommand queryCommand = new SqliteCommand(query, connection))
                    {
                        queryCommand.Parameters.AddWithValue("$patronId", patronId);
                        queryCommand.Parameters.AddWithValue("$loanDate", loanDate.ToString("yyyy-MM-dd"));

                        var result = queryCommand.ExecuteScalar();
                        if (result == null)
                        {
                            string append = "INSERT INTO note (patron_id, date, updated) VALUES ($patronId, $date, 0) RETURNING id";
                            using (SqliteCommand appendCommand = new SqliteCommand(append, connection))
                            {
                                appendCommand.Parameters.AddWithValue("$patronId", patronId);
                                appendCommand.Parameters.AddWithValue("$date", loanDate.ToString("yyyy-MM-dd"));

                                noteId = Convert.ToInt32(appendCommand.ExecuteScalar()!);
                            }
                        }
                        else
                        {
                            noteId = Convert.ToInt32(result);
                        }
                    }

                    // Create note_loan
                    string insert = "INSERT INTO note_loan (note_id, loan_id) VALUES ($noteId, $loanId)";
                    using (SqliteCommand insertCommand = new SqliteCommand(insert, connection))
                    {
                        insertCommand.Parameters.AddWithValue("$noteId", noteId);
                        insertCommand.Parameters.AddWithValue("$loanId", loanId);

                        insertCommand.ExecuteNonQuery();
                    }
                }

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

    // Get instance of a note using the corresponding note ID.
    public static int GetInstance(int noteId)
    {
        try
        {
            int result = 0;
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING))
            {
                connection.Open();
                string query = $"SELECT row_num FROM ( SELECT id, patron_id, ROW_NUMBER() OVER (PARTITION BY patron_id ORDER BY id) AS row_num FROM note ) t WHERE id = {noteId};";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    result = Convert.ToInt32(command.ExecuteScalar())!;
                }

                connection.Close();
            }

            return result;
        }
        catch (Exception e)
        {
            Logger<SQLInterface>.Log($"Failed to get instance number for note id: {noteId}.\t{e.Message}", LogLevel.Error);
            return 0;
        }
    }

    public static Item GetItemFromRow(DataRow row)
    {
        int id = Convert.ToInt32(row[0]);
        string mmsId = row[1].ToString()!;
        string barcode = row[2].ToString()!;
        string title = row[3].ToString()!;
        string description = row[4].ToString()!;
        string policy = row[5].ToString()!;

        return new Item(id, mmsId, barcode, title, description, policy);
    }

    public static Item? GetItemFromId(int itemId)
    {
        try
        {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING))
            {
                connection.Open();

                string query = "SELECT * FROM item WHERE id = $id";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("$id", itemId);
                    SqliteDataReader reader = command.ExecuteReader();
                    DataTable table = new DataTable();
                    table.Load(reader);

                    connection.Close();
                    return GetItemFromRow(table.Rows[0]);
                }
            }
        }
        catch (Exception e)
        {
            Logger<SQLInterface>.Log($"Failed to get item from id: {itemId}\t{e.Message}", LogLevel.Error);
            return null;
        }
    }

    // Processes a DataRow generated by DataTable, specialized for Loans in SQL.
    public static Loan GetLoanFromRow(DataRow row)
    {
        int id = Convert.ToInt32(row[0]);
        string almaId = row[1].ToString()!;
        string outCircDesk = row[2].ToString()!;
        string inCircDesk = row[3].ToString()!;
        int patronId = Convert.ToInt32(row[4]);
        Item item = GetItemFromId(Convert.ToInt32(row[5])!)!;
        DateTime loanDate = ParseDates.ConvertStringToDateTime(row[6].ToString()!);
        DateTime dueDate = ParseDates.ConvertStringToDateTime(row[7].ToString()!);

        // Returns can be Null.
        DateTime? returnDate = null;
        if (row[8].ToString() != "")
        {
            returnDate = ParseDates.ConvertStringToDateTime(row[8].ToString()!);
        }

        return new Loan(id, almaId, outCircDesk, inCircDesk, patronId, item, loanDate, dueDate, returnDate);
    }

    // Get all loans (including item) pertaining to notes
    public static Loan[]? GetLoansForNote(int noteId)
    {
        try
        {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING))
            {
                connection.Open();

                string query = "SELECT * FROM loan JOIN note_loan ON loan.id = note_loan.loan_id WHERE note_loan.note_id = $noteId";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("$noteId", noteId);
                    SqliteDataReader reader = command.ExecuteReader();
                    DataTable loansTable = new DataTable();
                    loansTable.Load(reader);
                    connection.Close();

                    Loan[] loans = new Loan[loansTable.Rows.Count];

                    for (int i = 0; i < loansTable.Rows.Count; i++)
                    {
                        loans[i] = GetLoanFromRow(loansTable.Rows[i]);
                    }

                    return loans;
                }
            }
        }
        catch (Exception e)
        {
            Logger<SQLInterface>.Log($"Failed to get loans for note: {noteId}\t{e.Message}", LogLevel.Error);
            return null;
        }
    }
}