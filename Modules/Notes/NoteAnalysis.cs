using System.Data;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Data.Sqlite;

public class NoteAnalysis {
    // Grab all notes EXCEPT for notes set as RESOLVED or GRACE.
    private static readonly string GET_NOTES_SQL_COMMAND = $"""
        SELECT *
        FROM note
        WHERE note.status IS NULL OR note.status <> 'RESOLVED' AND note.status <> 'GRACE'
    """;

    private static readonly string SET_COMMAND = "UPDATE note SET status = $status, updated"
        + " = $updated WHERE id = $id";

    /// <summary>
    /// Checks if all of the loans of a note have been returned.
    /// </summary>
    /// <param name="note">Note to check.</param>
    /// <returns>False if a single return date is null.</returns>
    public static bool AllReturned(Note note) {
        bool returned = true;

        foreach (Loan loan in note.Loans) {
            if (loan.ReturnDate == null) {
                returned = false;
            }
        }

        return returned;
    }

    /// <summary>
    /// This method iterates every note that is not either RESOLVED or GRACE. It checks to see it's current
    /// status and then sorts it to a second, more specific, method to analyze it further.
    /// </summary>
    /// <returns>Integer overflow.</returns>
    public static int AnalyzeNotes() {
        Logger<NoteAnalysis>.Log($"Begun Note Analysis @ {DateTime.Now.ToString()}", LogLevel.Info);

        try {
            using (SqliteConnection connection = new SqliteConnection(SQLInterface.CONNECTION_STRING)) {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand(GET_NOTES_SQL_COMMAND, connection)) {
                    SqliteDataReader reader = command.ExecuteReader();
                    DataTable notesTable = new DataTable();
                    notesTable.Load(reader);

                    foreach (DataRow row in notesTable.Rows) {
                        Note? note = ConvertDataRowIntoNote(row);
                        if (note == null) {
                            throw new Exception($"Unable to convert row ({row[0]}) to Note.");
                        }
                        else {
                            // Decide which method to continue to for further analyzation.
                            switch (note.Status) {
                                case StatusType.REINSTATEMENT:
                                    int reinstated = AnalyzeReinstatementNote(note);
                                    if (reinstated != 0) {
                                        return reinstated;
                                    }
                                    break;
                                case StatusType.SUSPENDED:
                                    int suspended = AnalyzeSuspendedNote(note);
                                    if (suspended != 0) {
                                        return suspended;
                                    }
                                    break;
                                default:
                                    int error = AnalyzeNullNote(note);
                                    if (error != 0) {
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
        catch (Exception e) {
            Logger<NoteAnalysis>.Error("Something went wrong analyzing notes.", e);
            return 10;
        }

        Logger<NoteAnalysis>.Log($"Ended Note Analysis @ {DateTime.Now.ToString()}", LogLevel.Info);
        return 0;
    }

    // Analyze Notes whose Status is NULL
    /// <summary>
    /// A null note is a note that was just created in the current cycle or a note that has not past the grace
    /// period yet with unreturned items. To push it along further, it must be analyzed. Auto-Suspend's 
    /// decision is based off if the item's have been returned and how long have they been overdue. More 
    /// information can be found in the method itself.
    /// </summary>
    /// <param name="note">Note to check</param>
    /// <param name="connection">Sqlite connection to database.</param>
    /// <returns>Integer overflow.</returns>
    public static int AnalyzeNullNote(Note note) {
        /*
        A note gets set as GRACE if all items have been returned before the grace period ends.
        A note gets set as SUSPENDED if the items have not been returned and today is past the grace period.
        A note's status stays at NULL if all of the loans have not been returned but it's before each
        grace Deadline.
        */
        bool suspended = false;
        bool allReturned = true;

        foreach (Loan loan in note.Loans) {
            DateTime graceDeadline = loan.DueDate.AddDays(loan.DaysOfGrace);

            if (loan.ReturnDate == null) {
                allReturned = false;
                if (DateTime.Today > graceDeadline) suspended = true;
            }
            else {
                if (loan.ReturnDate > graceDeadline) suspended = true;
            }
        }

        if (suspended) {
            int success = SQLInterface.SetNoteStatus(note.Id, StatusType.SUSPENDED);
            if (success != 0) return success;
            note.Status = StatusType.SUSPENDED;
            int chain = AnalyzeSuspendedNote(note);
            if (chain != 0) return chain;
        }
        else if (allReturned) {
            int success = SQLInterface.SetNoteStatus(note.Id, StatusType.GRACE);
            if (success != 0) return success;
            note.Status = StatusType.GRACE;
        }

        return 0;
    }

    /// <summary>
    /// This method checks to see if a note that is currently in REINSTATEMENT, all loans have been returned,
    /// can be set to RESOLVED. A note is set to RESOLVED if the note's reinstatement date is today or in
    /// the past.
    /// </summary>
    /// <param name="note">Note to check.</param>
    /// <returns>Integer overflow.</returns>
    public static int AnalyzeReinstatementNote(Note note) {
        try {
            if (DateTime.Today >= GetReinstatementDateForNote(note)!.Value.Date) {
                int success = SQLInterface.SetNoteStatus(note.Id, StatusType.RESOLVED);
                if (success != 0) {
                    return success;
                }
            }

            return 0;
        }
        catch (Exception e) {
            Logger<NoteAnalysis>.Error($"Failed to analyze a reinstatement note ({note.Id})", e);
            return 27;
        }
    }

    /// <summary>
    /// This method checks to see if all loans have been returned. If they have, then Auto-Suspend will
    /// set their status to REINSTATEMENT which means that the suspension is no longer indefinite.
    /// </summary>
    /// <param name="note">Note to check.</param>
    /// <returns>Integer overflow.</returns>
    public static int AnalyzeSuspendedNote(Note note) {
        try {
            bool allReturned = AllReturned(note);

            if (allReturned == true) {
                int success = SQLInterface.SetNoteStatus(note.Id, StatusType.REINSTATEMENT);
                if (success != 0) {
                    return success;
                }

                AnalyzeReinstatementNote(note);
            }
        }
        catch (Exception e) {
            Logger<NoteAnalysis>.Error($"An error occured while analyzing"
                + $" a suspension note: (noteId:{note.Id})", e);
            return 25;
        }

        return 0;
    }

    /// <summary>
    /// This method gets the most recent return date for a note. It is singularly used by the method named
    /// GetReinstatementDateForNote
    /// </summary>
    /// <param name="note"></param>
    /// <returns>Most recent return date.</returns>
    public static DateTime? GetMostRecentReturnDate(Note note) {
        DateTime? recentDate = null;

        foreach (Loan loan in note.Loans) {
            if (loan.ReturnDate != null && (recentDate == null || recentDate < loan.ReturnDate)) {
                recentDate = loan.ReturnDate;
            }
        }

        return recentDate;
    }

    /// <summary>
    /// This method gets the reinstatement date for a note by getting the most recent return date on a note.
    /// If a note has no items returned, it will return Null because there is no most recent return date.
    /// </summary>
    /// <param name="note">Note to check.</param>
    /// <returns>Reinstatement DateTime for note.</returns>
    public static DateTime? GetReinstatementDateForNote(Note note) {
        DateTime? mostRecentReturnDate = GetMostRecentReturnDate(note);
        int maxSuspensionIndex = Config.Current.SuspensionLengthsPerInstance.Length;
        int suspendableInstance = MathUtil.Clamp(note.Instance, 1, maxSuspensionIndex);
        int suspensionLengthInDays = Config.Current.SuspensionLengthsPerInstance[suspendableInstance - 1] * 7;

        if (mostRecentReturnDate != null) {
            return mostRecentReturnDate.Value.AddDays(suspensionLengthInDays);
        }

        return null;
    }


    /// <summary>
    /// This method converts a sqlite datarow into a computable C# class that represents a note.
    /// </summary>
    /// <param name="row">DataRow from Sqlite</param>
    /// <returns>Note</returns>
    private static Note? ConvertDataRowIntoNote(DataRow row) {
        try {
            // Variables from datarow.
            DateTime date = ParseDates.ConvertStringToDateTime(row[2].ToString()!); // Safe to assert.
            int id = Convert.ToInt32(row[0]);
            int patron_id = Convert.ToInt32(row[1]);
            string? status = row[3].ToString();
            int updated = Convert.ToInt32(row[4]);

            int instance = SQLInterface.GetInstance(id);
            StatusType statusType = StatusType.NULL;

            // Update the Status Type if status is not null or "".
            if (status != null && status != "") {
                statusType = (StatusType)Enum.Parse(typeof(StatusType), status!, true); // Safe assert.
            }

            // Create Note.
            Note note = new Note(id, patron_id, date, statusType, updated, instance);
            note.InitializeLoans();

            return note;
        }
        catch (Exception e) {
            Logger<NoteAnalysis>.Error($"An error occured while converting"
                + $" a datarow ({row.ToString()}) into a note.", e);
            return null;
        }
    }
}