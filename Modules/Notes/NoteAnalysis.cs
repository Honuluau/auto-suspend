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
                        // Converting SQL Note to C# Values.
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
                                int error = AnalyzeNullNote(note, connection);
                                if (error != 0)
                                {
                                    return error;
                                }
                                break;
                        }
                    }
                }

                connection.Close();
            }
        }
        catch (Exception e)
        {
            Logger<NoteAnalysis>.Error("Something went wrong analyzing notes.", e);
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
        int longestGrace = -1;

        foreach(Loan loan in note.Loans!)
        {
            // Check if Returned
            if (loan.ReturnDate == null)
            {
                allReturned = false;
            }

            // Find Longest Grace
            if (loan.DaysOfGrace > longestGrace)
            {
                longestGrace = loan.DaysOfGrace;
            }

            // Find Longest Overdue
            TimeSpan overdue = loan.GetOverdueTimespan();
            if (overdue.Days > longestOverdue)
            {
                longestOverdue = overdue.Days;
            }
        }

        Console.WriteLine($"{longestOverdue}\t{longestGrace}\t{allReturned}");

        // Update SQL.
        try
        {
            using (connection)
            {
                connection.Open();
                
                string setCommand = "UPDATE note SET status = $status, updated = $updated WHERE id = $id";
                using (SqliteCommand command = new SqliteCommand(setCommand, connection))
                {
                    command.Parameters.AddWithValue("$id", note.Id);

                    if (allReturned == false)
                    {
                        // Check if past grace (most likely if not all). During the development of ASAS, the excel sheets would contain loans within grace.
                        if (longestOverdue > longestGrace)
                        {
                            command.Parameters.AddWithValue("$status", "SUSPENDED");
                            command.Parameters.AddWithValue("$updated", "0"); // Value is unaligned with Alma (Needs Update).
                        } 
                        else // Items are not returned but the user is still within the grace period before being overdue.
                        {
                            command.Parameters.AddWithValue("$status", DBNull.Value);
                            command.Parameters.AddWithValue("$updated", "1");
                        }
                    }
                    else if (longestOverdue <= longestGrace) // If all items are already returned and within grace period, then there should not be a suspension.
                    {
                        command.Parameters.AddWithValue("$status", "GRACE");
                        command.Parameters.AddWithValue("$updated", "1");
                    }
                    else // All items are returned, but over the grace period. Create a suspension that has already been resolved.
                    {
                        command.Parameters.AddWithValue("$status", "RESOLVED");
                        command.Parameters.AddWithValue("$updated", "0");
                    }

                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception e)
        {
            Logger<NoteAnalysis>.Error("Error updating Null Note", e);
            return 11;
        }

        return 0;
    }
}