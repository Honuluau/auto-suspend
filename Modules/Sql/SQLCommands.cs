public class SQLCommands {
    /// <summary> Commands that create tables in the database.</summary>
    public class CreateTable {
        /// <summary> Create the table for items if it does not already exist. </summary>
        public static readonly string CREATE_TABLE_ITEM = """
        CREATE TABLE IF NOT EXISTS item (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            mms_id TEXT,
            barcode TEXT,
            title TEXT,
            description TEXT
        )
        """;

        /// <summary> 
        /// Create the table for loans if it does not already exist. 
        /// "preferred_email" is the email of the user in Alma at the time of the loan. 
        /// </summary>
        public static readonly string CREATE_TABLE_LOAN = """
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

        /// <summary>
        /// Create the table for notes if it does not already exist.
        /// For updated integer, 0 = NOT UPDATED, 1 = UPDATED.
        /// Auto-Suspend checks this integer to see if it needs to PUT an API request to update the note.
        /// </summary>
        public static readonly string CREATE_TABLE_NOTE = """ 
        CREATE TABLE IF NOT EXISTS note (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            patron_id INTEGER,
            date TEXT,
            status INTEGER,
            updated INTEGER, 

            FOREIGN KEY(patron_id) REFERENCES patron(id)
        )
        """;

        /// <summary> Create a table that pairs Notes to Loans if it does not already exist. </summary>
        public static readonly string CREATE_TABLE_NOTE_LOAN = """
        CREATE TABLE IF NOT EXISTS note_loan (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            note_id INTEGER,
            loan_id INTEGER,

            FOREIGN KEY(note_id) REFERENCES note(id),
            FOREIGN KEY(loan_id) REFERENCES loan(id)
        )
        """;

        /// <summary> Create a table for patrons if it does not already exist. </summary>
        public static readonly string CREATE_TABLE_PATRON = """
        CREATE TABLE IF NOT EXISTS patron (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_primary_identifier TEXT,
            first_name TEXT,
            last_name TEXT,
            user_group TEXT
        )
        """;

        /// <summary> Create table for permanent suspensions if it does not already exist. </summary>
        public static readonly string CREATE_TABLE_PERM_SUSPEND = """
        CREATE TABLE IF NOT EXISTS perm_suspend (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            patron_id INTEGER,
            note TEXT,

            FOREIGN KEY(patron_id) REFERENCES patron(id)
        )
        """;

        // StyleGuide Note: This must be AFTER the strings because of initialization order of variables.
        /// <summary> A string array of all commands that create a table in the database. </summary>
        public static readonly string[] CREATE_TABLE_COMMANDS = [
            CREATE_TABLE_ITEM,
            CREATE_TABLE_LOAN,
            CREATE_TABLE_NOTE,
            CREATE_TABLE_NOTE_LOAN,
            CREATE_TABLE_PATRON,
            CREATE_TABLE_PERM_SUSPEND
        ];
    }

    /// <summary> Commands that pertain to notes.</summary>
    public class Notes {
        /// <summary>Gets the current instance per note id.</summary>
        /// <remarks>Requires @noteId</remarks> 
        public static string GET_INSTANCE = """
            SELECT row_num 
            FROM ( 
                SELECT 
                    id, 
                    patron_id, 
                    ROW_NUMBER() OVER (PARTITION BY patron_id ORDER BY id) AS row_num
                FROM note 
            )
            WHERE id = @noteId
        """;
    }
}