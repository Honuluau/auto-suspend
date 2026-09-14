using System.Data;
using Microsoft.Data.Sqlite;

/// <summary>This class is the SOLE interface between the modules and the database. </summary>
/// <remarks>All commands it uses can be found in SQLCommands.</remarks>
public class SQLInterface {
    public static string CONNECTION_STRING { get; set; } = "";

    /// <summary>
    /// Helper method that turns a bunch of strings into one tuple.
    /// </summary>
    /// <remarks>[a, b, c] -> (a, b, c)</remarks.>
    /// <param name="stringList"></param>
    /// <returns>SQL Tuple string of strings.</returns>
    public static string ConvertStringListIntoSQLTuple(string[] stringList) {
        string result = "(";

        foreach (string str in stringList) {
            result = $"{result}{str}, ";
        }

        return $"{result.Substring(0, result.Length - 2)})";
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

                    sqliteCommand.Parameters.AddWithValue("$note_id", noteId);
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
                new Tuple<string, object>("$note_id", noteId)
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
    /// Helper method that turns a bunch of variables into an SQL tuple.
    /// </summary>
    /// <param name="count">Amount of variables.</param>
    /// <returns>SQL Tuple String of placeholders.</returns>
    public static string GetPlaceholdersForSQLTuple(int count) {
        string result = "(";

        for (int i = 0; i < count; i++) {
            result = $"{result}$var{i}, ";
        }

        return $"{result.Substring(0, result.Length - 2)})";
    }


    /// <summary>
    /// This method gets the UserPrimaryIdentifier of a patron in the database.
    /// </summary>
    /// <param name="patronId">Database Id for Patron.</param>
    /// <returns>UserPrimaryIdentifier</returns>
    public static string? GetUserPrimaryIdentifier(int patronId) {
        try {
            using (SqliteConnection sqliteConnection = new SqliteConnection(CONNECTION_STRING)) {
                sqliteConnection.Open();

                string query = SQLCommands.Patron.GET_USER_PRIMARY_IDENTIFIER;
                using (SqliteCommand sqliteCommand = new SqliteCommand(query, sqliteConnection)) {
                    sqliteCommand.Parameters.AddWithValue("$patron_id", patronId);

                    object? result = sqliteCommand.ExecuteScalar();
                    return (result != null) ? result.ToString() : null;
                }
            }
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to get UserPrimaryIdentifier for {patronId.ToString()}", e);
            return null;
        }
    }

    /// <summary>
    /// This method links every loan to a note if not already linked. It works by getting a list of all
    /// notes that do not have a corresponding note_loan field. And then creating a note if it does not
    /// already exist for a loan. Then creating a note_loan for the matching loan and note. 
    /// </summary>
    /// <remarks>This an important method that acts as it's own module inside of the program.</remarks>
    /// <returns>Integer overflow.</returns>
    public static int LinkLoansToNote() {
        try {
            using (SqliteConnection sqliteConnection = new SqliteConnection(CONNECTION_STRING)) {
                sqliteConnection.Open();
                
                // Load a datatable that contains all of the loans without a note_loan.    
                string getAllNonLinkedLoans = SQLCommands.Loan.GET_ALL_NON_LINKED_LOANS;
                SqliteCommand sqliteCommand = new SqliteCommand(getAllNonLinkedLoans, sqliteConnection);
                SqliteDataReader dataReader = sqliteCommand.ExecuteReader();

                DataTable nonLinkedLoansTable = new DataTable();
                nonLinkedLoansTable.Load(dataReader);
                dataReader.Close();

                // Link Machine
                foreach (DataRow row in nonLinkedLoansTable.Rows) {
                    // Throw an exception if loandate is missing.
                    string? loanDateString = row["loan_date"].ToString();
                    if (loanDateString == null) {
                        throw new Exception("No loan_date");
                    }

                    DateTime loanDate = ParseDates.ConvertStringToDateTime(loanDateString);
                    int loanId = Convert.ToInt32(row["id"]);
                    int patronId = Convert.ToInt32(row["patron_id"]);
                    int noteId = -1;

                    // Find note OR create if it does not already exist.
                    string noteQuery = SQLCommands.Note.GET_ID;
                    using (SqliteCommand noteCommand = new SqliteCommand(noteQuery, sqliteConnection)) {
                        noteCommand.Parameters.AddWithValue("$patronId", patronId);
                        noteCommand.Parameters.AddWithValue("$loanDate", loanDate);

                        object? result = noteCommand.ExecuteScalar();
                        if (result == null) {
                            // Create a note. We cannot use InsertData because we need the noteId.

                            using (SqliteCommand insertNote = new SqliteCommand(
                                SQLCommands.Note.INSERT_NOTE, sqliteConnection)) {
                                
                                insertNote.Parameters.AddWithValue("$patronId", patronId);
                                insertNote.Parameters.AddWithValue("$loanDate", loanDate);

                                object? insertResult = insertNote.ExecuteScalar();
                                if (insertResult != null) {
                                    noteId = Convert.ToInt32(insertResult);
                                }
                                else {
                                    throw new Exception("Failed to insert note.");
                                }
                            }
                        }
                        else {
                            noteId = Convert.ToInt32(result);
                        }
                    }

                    // Link Note to Loan (cannot use insert data because there is no unique constraint)
                    string linkLoan = SQLCommands.Note.LINK_LOAN;
                    using (SqliteCommand linkCommand = new SqliteCommand(linkLoan, sqliteConnection)) {
                        linkCommand.Parameters.AddWithValue("$noteId", noteId);
                        linkCommand.Parameters.AddWithValue("$loanId", loanId);
                    }
                }
            }
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to consolidate loans into notes.", e);
            return 9;
        }
        return 0;
    }

    /// <summary>This method sets the CONNECTION_STRING.</summary>
    /// <param name="dbPath">Path to database file.</param>
    public static void Initialize(String dbPath) {
        CONNECTION_STRING = $"Data Source={dbPath}";
    }

    /// <summary>
    /// Insert one row of information into any table. Columns and Variables should be the same length with
    /// matching variables. "checkIndex" is the variable at x in columns and variables that the method will
    /// use to retrieve the id. Consider "checkIndex" as the index of any column with a unique constraint. 
    /// </summary>
    /// <remarks>This method automatically checks for duplicates and will not add if already found.</remarks>
    /// <param name="tableName"></param>
    /// <param name="columns"></param>
    /// <param name="variables"></param>
    /// <param name="checkIndex"></param>
    /// <returns>Integer overflow.</returns>
    public static int InsertData(string tableName, string[] columns, object[] variables, int checkIndex) {
        try {
            int id = GetIdFromTable(tableName, columns[checkIndex], variables[checkIndex]);

            // If the id is 0, then no item was already found.
            if (id == 0) {
                using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                    connection.Open();

                    // Creating the Command.
                    string columnsTuple = ConvertStringListIntoSQLTuple(columns);
                    string placeholdersTuple = GetPlaceholdersForSQLTuple(variables.Length);
                    string query = SQLCommands.Generic.INSERT_DATA(tableName,
                        columnsTuple, placeholdersTuple);

                    using (SqliteCommand insertCommand = new SqliteCommand(query, connection)) {
                        for (int i = 0; i < variables.Length; i++) {
                            insertCommand.Parameters.AddWithValue($"$var{i}", variables[i]);
                        }

                        insertCommand.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            else if (id < 0) {
                return id; // Error in getting id.
            }

            return 0;
        }
        catch (Exception e) {
            Logger<SQLInterface>.Error($"Failed to write ({columns}) to {tableName} with ({variables})", e);
            return 22;
        }
    }

    /// <summary>
    /// This method updates a notes status and marks it to be updated in Alma.
    /// </summary>
    /// <param name="noteId">Database Id.</param>
    /// <param name="status">New status.</param>
    /// <returns></returns>
    public static int SetNoteStatus(int noteId, StatusType status) {
        try {
            using (SqliteConnection connection = new SqliteConnection(CONNECTION_STRING)) {
                connection.Open();

                string query = SQLCommands.Note.UPDATE_STATUS;
                using (SqliteCommand command = new SqliteCommand(query, connection)) {
                    command.Parameters.AddWithValue("$status", status.ToString());
                    command.Parameters.AddWithValue("$updated", 0);
                    command.Parameters.AddWithValue("$id", noteId);

                    command.ExecuteNonQuery();
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