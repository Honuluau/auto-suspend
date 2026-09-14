using System.Data;
using Microsoft.Data.Sqlite;

public class SQLInterface {
    public static string CONNECTION_STRING { get; set; } = "";

    /// <summary>This method sets the CONNECTION_STRING.</summary>
    /// <param name="dbPath">Path to database file.</param>
    public static void Initialize(String dbPath) {
        CONNECTION_STRING = $"Data Source={dbPath}";
    }

    /// <summary>
    /// This method creates all of the tables that do not already exist inside the database.
    /// </summary>
    /// <returns>Integer overflow.</returns>
    public static int CreateSqliteDB() {
        Logger<SQLInterface>.Log("SQL initialization sequence started.", LogLevel.Info);

        try {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                connection.Open();

                // Create tables.
                foreach (string command in SQLCommands.CreateTable.CREATE_TABLE_COMMANDS) {
                    using SqliteCommand sqliteCommand = new SqliteCommand(command, connection);
                    sqliteCommand.ExecuteNonQuery();
                }
            }

            Logger<SQLInterface>.Log("SQL initialized successfully.", LogLevel.Info);
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error("Failed to Initialize SQL database", e);
            return 8;
        }

        return 0;
    }

    /*
    This method pairs loans to their notes in SQL.
    If a loan's note does not exist, it will create a note for the loan.
    */
    public static int ConsolidateLoans() {
        Logger<SQLInterface>.Log($"Consolidating loans into notes: {CONNECTION_STRING}", LogLevel.Info);
        try {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
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
                foreach (DataRow row in dataTable.Rows) // For each loan that does not have a note connected to it,
                {
                    int loanId = Convert.ToInt32(row[0]);
                    DateTime loanDate = ParseDates.ConvertStringToDateTime(row[1].ToString()!);
                    int patronId = Convert.ToInt32(row[2]);
                    int noteId = -1;

                    // Find the matching note id from patron_id and loandate and create the note if it does not already exist.
                    string query = "SELECT id FROM note WHERE patron_id = $patronId AND date = $loanDate";
                    using (SqliteCommand queryCommand = new SqliteCommand(query, connection)) {
                        queryCommand.Parameters.AddWithValue("$patronId", patronId);
                        queryCommand.Parameters.AddWithValue("$loanDate", loanDate.ToString("yyyy-MM-dd"));

                        var result = queryCommand.ExecuteScalar();
                        if (result == null) {
                            string append = "INSERT INTO note (patron_id, date, updated) VALUES ($patronId, $date, 0) RETURNING id";
                            using (SqliteCommand appendCommand = new SqliteCommand(append, connection)) {
                                appendCommand.Parameters.AddWithValue("$patronId", patronId);
                                appendCommand.Parameters.AddWithValue("$date", loanDate.ToString("yyyy-MM-dd"));

                                noteId = Convert.ToInt32(appendCommand.ExecuteScalar()!);
                            }
                        }
                        else {
                            noteId = Convert.ToInt32(result);
                        }
                    }

                    // Create note_loan
                    string insert = "INSERT INTO note_loan (note_id, loan_id) VALUES ($noteId, $loanId)";
                    using (SqliteCommand insertCommand = new SqliteCommand(insert, connection)) {
                        insertCommand.Parameters.AddWithValue("$noteId", noteId);
                        insertCommand.Parameters.AddWithValue("$loanId", loanId);

                        insertCommand.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error("Failed to consolidate loans into notes", e);
            return 9;
        }

        return 0;
    }

    /// <summary>
    /// Gets the instance of a note using it's corresponding note id. An instance is the ordinal position of 
    /// a note within a patron's note history. The first note recorded for a patron is instance #1, the 
    /// second is instance #2.
    /// </summary>
    /// <param name="noteId">Database id for note.</param>
    /// <returns>Result which will either be a true instance number or 0 if failed.</returns>
    public static int GetInstance(int noteId) {
        int result = 0;

        try {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                connection.Open();

                string command = SQLCommands.Note.GET_INSTANCE;
                using (SqliteCommand sqliteCommand = new SqliteCommand(command, connection)) {

                    sqliteCommand.Parameters.AddWithValue("$noteId", noteId);
                    result = Convert.ToInt32(sqliteCommand.ExecuteScalar())!;
                }
            }
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to get instance number for note id: {noteId}", e);
        }

        return result;
    }

    /// <summary>
    /// Translates a datarow into an Item class.
    /// </summary>
    /// <param name="row">Datarow from sqlite command.</param>
    /// <returns>An <see cref="Item"/> instance populated from row.</returns>
    public static Item GetItemFromRow(DataRow row) {
        int id = Convert.ToInt32(row["id"]);
        string mmsId = row["mms_id"].ToString()!;
        string barcode = row["barcode"].ToString()!;
        string title = row["title"].ToString()!;
        string description = row["description"].ToString()!;

        return new Item(id, mmsId, barcode, title, description);
    }

    /// <summary>
    /// Gets an item by searching for it with its id.
    /// </summary>
    /// <param name="itemId">Database id.</param>
    /// <returns>An <see cref="Item"/> or null if not found.</returns>
    public static Item? GetItemFromId(int itemId) {
        try {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                connection.Open();

                string command = SQLCommands.Item.GET_ITEM_FROM_ID;
                using (SqliteCommand sqliteCommand = new SqliteCommand(command, connection)) {
                    sqliteCommand.Parameters.AddWithValue("$id", itemId);
                    SqliteDataReader reader = sqliteCommand.ExecuteReader();
                    DataTable table = new DataTable();
                    table.Load(reader);

                    if (table.Rows.Count == 0) {
                        Logger<SQLInterface>.Error($"No rows found for id ({itemId}).", null);
                    }

                    return GetItemFromRow(table.Rows[0]);
                }
            }
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to get item with id({itemId}).", e);
            return null;
        }
    }



    /// <summary>
    /// Translates a datarow into a <see cref="Loan"/>.
    /// </summary>
    /// <param name="row">DataRow from sqlite command.</param>
    /// <returns>A <see cref="Loan"/>.</returns>
    public static Loan GetLoanFromRow(DataRow row) {
        // Get variables from DataRow
        int id = Convert.ToInt32(row["id"]);
        string almaId = row["alma_id"].ToString()!;
        string outCircDesk = row["out_circ_desk"].ToString()!;
        string inCircDesk = row["in_circ_desk"].ToString()!;
        int patronId = Convert.ToInt32(row["patron_id"]);
        Item item = GetItemFromId(Convert.ToInt32(row["item_id"])!)!;
        string policy = row["policy"].ToString()!;
        string preferredEmail = row["preferred_email"].ToString()!;
        DateTime loanDate = ParseDates.ConvertStringToDateTime(row["loan_date"].ToString()!);
        DateTime dueDate = ParseDates.ConvertStringToDateTime(row["due_date"].ToString()!);

        // Returns may be null so this checks for one.
        DateTime? returnDate = null;
        string? returnDateString = row["return_date"].ToString();
        if (returnDateString != null && returnDateString != "") {
            returnDate = ParseDates.ConvertStringToDateTime(returnDateString);
        }

        return new Loan(id, almaId, outCircDesk, inCircDesk, patronId, item, policy, preferredEmail,
            loanDate, dueDate, returnDate);
    }

    /// <summary>
    /// Get loans using an sqlite query with its parameters to add.
    /// </summary>
    /// <param name="query">Sqlite command/query.</param>
    /// <param name="parameters">List of tuplse that has a parameter name and then the parameter.</param>
    /// <returns>List of <see cref="Loan"/></returns>
    public static Loan[]? GetLoans(string query, Tuple<string, object>[]? parameters) {
        try {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(query, connection)) {
                    if (parameters != null) {
                        foreach (Tuple<string, object> parameter in parameters) {
                            command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
                        }
                    }

                    SqliteDataReader reader = command.ExecuteReader();
                    DataTable loansTable = new DataTable();
                    loansTable.Load(reader);
                    connection.Close();

                    Loan[] loans = new Loan[loansTable.Rows.Count];

                    for (int i = 0; i < loansTable.Rows.Count; i++) {
                        loans[i] = GetLoanFromRow(loansTable.Rows[i]);
                    }

                    return loans;
                }
            }
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"An error occurred while getting notes.", e);
            return null;
        }
    }

    // Get all loans (including item) pertaining to notes
    /// <summary>
    /// Get all of the loans for a note.
    /// </summary>
    /// <param name="noteId">Database Id.</param>
    /// <returns>List of <see cref="Loan"/>.</returns>
    public static Loan[]? GetLoansForNote(int noteId) {
        try {
            // Set up arguments to get loans.
            string query = SQLCommands.Loan.GET_LOANS_FOR_NOTE;

            // Lengthy declaration because of atypical data type.
            Tuple<string, object>[] parameters = [
                new Tuple<string, object>("$noteId", noteId)
            ];

            // Get loans.
            Loan[]? loans = GetLoans(query, parameters);
            return loans;
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to get loans for note: {noteId}", e);
            return null;
        }
    }

    /// <summary>
    /// This method gets all loans in the SQL database that do not have a return_date.
    /// </summary>
    /// <returns>List of unreturned loans.</returns>
    public static Loan[]? GetAllNonReturnedLoans() {
        try {
            string query = SQLCommands.Loan.GET_ALL_NON_RETURNED_LOANS;
            Loan[]? loans = GetLoans(query, null);
            return loans;
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error("An error occurred retrieving all loans.", e);
            return null;
        }
    }

    /// <summary>
    /// This method gets the id of anything in the SQL database pertaining to one table, column, and value.
    /// </summary>
    /// <remarks>This method is used for checking to see if rows of data already exist or not 
    /// in the database.</remarks>
    /// <param name="tableName">Name of table.</param>
    /// <param name="columnName">Name of column.</param>
    /// <param name="variable">Value</param>
    /// <returns></returns>
    public static int GetIdFromTable(string tableName, string columnName, object variable) {
        try {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                connection.Open();

                string query = SQLCommands.Generic.GET_ID;
                using (SqliteCommand command = new SqliteCommand(query, connection)) {
                    command.Parameters.AddWithValue("$var", variable);

                    object? result = command.ExecuteScalar();
                    return (result != null) ? Convert.ToInt32(result) : 0;
                }
            }
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to get {tableName} id from {columnName} = {variable}", e);
            return -21;
        }
    }

    /*
    This method gets the UserPrimaryIdentifier from a patronId.
    Check for NULL value for failure.
    */
    public static string? GetUserPrimaryIdentifier(int patronId) {
        try {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand("SELECT user_primary_identifier FROM patron WHERE id = $patron_id", connection)) {
                    command.Parameters.AddWithValue("$patron_id", patronId);
                    SqliteDataReader reader = command.ExecuteReader();
                    DataTable table = new DataTable();
                    table.Load(reader);

                    connection.Close();

                    if (table.Rows.Count > 0) {
                        return table.Rows[0][0].ToString();
                    }

                    return null;
                }
            }
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to get UserPrimaryIdentifier for {patronId.ToString()}.", e);
            return null;
        }
    }

    // [a, b] = (a, b)
    public static string ConvertStringListIntoSQLTuple(string[] stringList) {
        string result = "(";

        foreach (string str in stringList) {
            result = $"{result}{str}, ";
        }

        return $"{result.Substring(0, result.Length - 2)})";
    }

    // Turn number of variables into an SQL Tuple
    public static string GetPlaceholdersForSQLTuple(int count) {
        string result = "(";

        for (int i = 0; i < count; i++) {
            result = $"{result}$var{i}, ";
        }

        return $"{result.Substring(0, result.Length - 2)})";
    }

    /*
    Insert one row of information into any table. Columns and Variables should be the same length with matching variables.
    checkIndex is the variable at x in columns and variables that the method will use to retrieve the id.
    Automatically checks for duplicates.
    */
    public static int InsertData(string tableName, string[] columns, object[] variables, int checkIndex) {
        try {
            int id = GetIdFromTable(tableName, columns[checkIndex], variables[checkIndex]);
            if (id == 0) // 0 means that there is no id found meaming the data has not been already created.
            {
                using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                    connection.Open();

                    string insertText = $"INSERT INTO {tableName} {ConvertStringListIntoSQLTuple(columns)} VALUES {GetPlaceholdersForSQLTuple(variables.Length)}";
                    using (SqliteCommand insertCommand = new SqliteCommand(insertText, connection)) {
                        for (int i = 0; i < variables.Length; i++) {
                            insertCommand.Parameters.AddWithValue($"$var{i}", variables[i]);
                        }

                        insertCommand.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            else if (id < 0) // Error.
            {
                return id;
            }
            return 0;
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to write ({columns}) to {tableName} with ({variables})", e);
            return 22;
        }
    }



    public static int SetNoteStatus(int noteId, StatusType status) {
        try {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                connection.Open();

                string setCommand = "UPDATE note SET status = $status, updated = $updated WHERE id = $id";

                using (SqliteCommand command = new SqliteCommand(setCommand, connection)) {
                    command.Parameters.AddWithValue("$status", status.ToString());
                    command.Parameters.AddWithValue("$updated", 0);
                    command.Parameters.AddWithValue("$id", noteId);

                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to set note status {status.ToString()}"
                + $" to note ({noteId})", e);
            return 26;
        }

        return 0;
    }
}