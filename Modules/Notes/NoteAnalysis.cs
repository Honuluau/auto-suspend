using System.Data;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Data.Sqlite;

public class NoteAnalysis
{
    private static readonly string GET_NOTES_SQL_COMMAND = $"""
        SELECT *
        FROM note
        WHERE note.status IS NULL OR note.status <> 'RESOLVED' AND note.status <> 'GRACE' AND NOT EXISTS (
            SELECT 1
            FROM perm_suspend
            WHERE perm_suspend.patron_id = note.patron_id
        )
    """;


    // Check if Note is All Returned.
    public static bool AllReturned(Note note)
    {
        bool returned = true;

        foreach (Loan loan in note.Loans)
        {
            if (loan.ReturnDate == null)
            {
                returned = false;
            }
        }

        return returned;
    }


    public static DateTime? GetMostRecentReturnDate(Note note)
    {
        DateTime? recentDate = note.Loans[0].ReturnDate;

        foreach (Loan loan in note.Loans)
        {
            if (loan.ReturnDate != null) // Loan was returned.
            {
                if (recentDate == null) // The first loan is not returned.
                {
                    recentDate = loan.ReturnDate; // Make this loan the last
                }
                else // First loan is returned.
                {
                    if (recentDate < loan.ReturnDate) // This new return date is earlier than the current most recent.
                    {
                        recentDate = loan.ReturnDate; // The most recent is now the new return date.
                    }
                }
            }
        }

        return recentDate;
    }


    public static DateTime? GetReinstatementDateForNote(Note note)
    {
        DateTime? mostRecentReturnDate = GetMostRecentReturnDate(note);
        int suspendableInstance = MathUtil.Clamp(note.Instance, 1, Config.Current.SuspensionLengthsPerInstance.Length);
        Console.WriteLine($"Most recent return date is: {mostRecentReturnDate.ToString()}");
        int suspensionLengthInDays = Config.Current.SuspensionLengthsPerInstance[suspendableInstance-1]*7; // -1 because C# Arrays start at an index = 0.

        if (mostRecentReturnDate != null)
        {
            return mostRecentReturnDate.Value.AddDays(suspensionLengthInDays);
        }

        return null;
    }


    // Converts SQL Rows into a computable C# class.
    private static Note? ConvertDataRowIntoNote(DataRow row)
    {
        try
        {
            // Variables from datarow.
            int id = Convert.ToInt32(row[0]);
            int patron_id = Convert.ToInt32(row[1]);
            DateTime date = ParseDates.ConvertStringToDateTime(row[2].ToString()!); // Date Field is NN so it is safe to assert.
            string? status = row[3].ToString();
            int updated = Convert.ToInt32(row[4]);

            int instance = SQLInterface.GetInstance(id);
            StatusType statusType = StatusType.NULL;

            // Update the Status Type if status is not null or "".
            if (status != null && status != "")
            {
                statusType = (StatusType)Enum.Parse(typeof(StatusType), status!, true); // Safe assert because this can only run if status is not null.
            }

            // Create Note.
            Note note = new Note(id, patron_id, date, statusType, updated, instance);
            note.InitializeLoans();

            return note;
        }
        catch (Exception e)
        {
            Logger<NoteAnalysis>.Error($"An error occured while converting a datarow ({row.ToString()}) into a note.", e);
            return null;
        }
    }


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

                using (SqliteCommand command = new SqliteCommand(GET_NOTES_SQL_COMMAND, connection))
                {
                    SqliteDataReader reader = command.ExecuteReader();
                    DataTable notesTable = new DataTable();
                    notesTable.Load(reader);

                    foreach (DataRow row in notesTable.Rows)
                    {
                        Note? note = ConvertDataRowIntoNote(row);
                        if (note == null)
                        {
                            throw new Exception($"Unable to convert row ({row[0]}) to Note.");
                        }
                        else
                        {
                            // Decide which method to continue to for further analyzation.
                            switch (note.Status)
                            {
                                case StatusType.SUSPENDED:
                                    int suspended = AnalyzeSuspendedNote(note, connection);
                                    if (suspended != 0)
                                    {
                                        return suspended;
                                    } 
                                    break;
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
        bool allReturned = AllReturned(note);
        int longestOverdue = -1;
        int longestGrace = -1;

        foreach (Loan loan in note.Loans!)
        {
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
                        // Check if past grace (most likely, if not all are before grace for a few days). During the development of ASAS, the excel sheets would contain loans within grace.
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
                    else // All items are returned, but over the grace period. Create a suspension.
                    {
                        command.Parameters.AddWithValue("$status", "REINSTATEMENT");
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


    // Analyze note whose status is suspended i.e. check if patron qualifies for reinstatement.
    public static int AnalyzeSuspendedNote(Note note, SqliteConnection connection)
    {
        try
        {
            // To go from suspended -> reinstated, we must check if all items are returned.
            // To go form suspended -> resolved, we must Analyze for Resolved immediately. Maybe make it so that it goes up the entire chain as it gets analyzed? Yes. Do it. Must be smart enough to check for notes and know that one did not exist prior because of skipping. This is all incase the program is not ran every day (not happening).
        
            bool allReturned = AllReturned(note);

            if (allReturned == true)
            {
                // Update SQL and then continue to AnalyzeReinstatementNote -> Resolved.
            }
        }
        catch (Exception e)
        {
            Logger<NoteAnalysis>.Error($"An error occured while analyzing a suspension note: (noteId:{note.Id})", e);
            return 25;
        }

        return 0;
    }
}