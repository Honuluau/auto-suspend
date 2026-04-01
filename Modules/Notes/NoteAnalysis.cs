using System.Data;
using Microsoft.Data.Sqlite;

public class NoteAnalysis
{
    // Iterate over every note via the database and check if they need updating.
    public static int AnalyzeNotes()
    {
        Logger<NoteAnalysis>.Log($"Begun Note Analysis @ {DateTime.Now.ToString()}", LogLevel.Info);

        // SQL Interface is getting crowded and this is going to need it's own section for easy readability and development.
        try
        {
            using (SqliteConnection connection = new SqliteConnection(SQLInterface.CONNECTION_STRING))
            {
                connection.Open();

                string commandString = $"""
                    SELECT *
                    FROM note
                    WHERE note.status IS NULL OR note.status <> 'REINSTATED' AND note.status <> 'GRACE' AND NOT EXISTS (
                        SELECT 1
                        FROM perm_suspend
                        WHERE perm_suspend.patron_id = note.patron_id
                    )
                """;

                using (SqliteCommand command = new SqliteCommand(commandString, connection))
                {
                    SqliteDataReader reader = command.ExecuteReader();
                    DataTable notesTable = new DataTable();
                    notesTable.Load(reader);

                    foreach (DataRow row in notesTable.Rows)
                    {
                        // Converting SQL Values to C# Values.
                        int id = Convert.ToInt32(row[0]);
                        int patron_id = Convert.ToInt32(row[1]);
                        DateTime date = ParseDates.ConvertStringToDateTime(row[2].ToString()!);
                        string status = row[3].ToString()!;
                        int updated = Convert.ToInt32(row[4]);
                        int instance = SQLInterface.GetInstance(id);
                        StatusType statusType = StatusType.NULL;

                        // Update status if not null.
                        if (status != "")
                        {
                            statusType = (StatusType) Enum.Parse(typeof(StatusType), status, true);
                        }


                       Note note = new Note(id, patron_id, date, statusType, updated, instance);
                       note.InitializeLoans();

                       switch(note.Status)
                        {
                            default:
                                AnalyzeNullNote(note, connection);
                                break;
                        }
                    }
                }

                connection.Close();
            }
        }
        catch (Exception e)
        {
            Logger<NoteAnalysis>.Log($"Something went wrong analyzing notes. {e.Message}", LogLevel.Error);
            return 10;
        }

        Logger<NoteAnalysis>.Log($"Ended Note Analysis @ {DateTime.Now.ToString()}", LogLevel.Info);
        return 0;
    }

    // Analyze Notes whose Status is NULL
    public static int AnalyzeNullNote(Note note, SqliteConnection connection)
    {
        bool allReturned = true;
        int longestOverdue = -1;

        return 0;
    }
}