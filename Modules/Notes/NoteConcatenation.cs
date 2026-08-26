using System.Text;

public class NoteConcatenation
{
    /// <summary>
    /// Formats the items specified by the loans of a note.
    /// </summary>
    /// <param name="note"></param>
    /// <returns>A string that looks like this: [(Item1,02000),(Item2,03000)]</returns>
    private static string GetItemsList(Note note)
    {
        StringBuilder list = new System.Text.StringBuilder("[");

        foreach (Loan loan in note.Loans)
        {
            list.Append($"({loan.Item.Title},{loan.Item.Barcode}),");
        }

        // Chop off last comma and close the list.
        list.Length--;
        list.Append("]");

        return list.ToString();
    }

    // Formats the end of the string based on if the items are returned or not.
    /// <summary>
    /// Formats the end of a string for a note. The result is determined on if the items are returned or not.
    /// This essentially either marks a note as UNRESOLVED or gives a REINSTATEMENT DATE.
    /// </summary>
    /// <param name="note"></param>
    /// <returns>A string that is either UNRESOLVED, or a REINSTATEMENT date.</returns>
    private static string GetEndStatement(Note note)
    {
        StringBuilder statement = new System.Text.StringBuilder("");

        if (NoteAnalysis.AllReturned(note))
        {
            int suspendableInstance = MathUtil.Clamp(note.Instance, 1, Config.Current.SuspensionLengthsPerInstance.Length);
            int suspensionLengthInDays = Config.Current.SuspensionLengthsPerInstance[suspendableInstance - 1] * 7;

            statement.Append($"REINSTATEMENT ON ({ParseDates.AmericanFormat(DateTime.Now.AddDays(suspensionLengthInDays))})");
        }
        else
        {
            statement.Append($"UNRESOLVED");
        }

        return statement.ToString();
    }

    /// <summary>
    /// This method formats a given note into a suspension note that is visible in Alma.
    /// </summary>
    /// <param name="note"></param>
    /// <returns>A suspension note.</returns>
    public static string FormatNote(Note note)
    {
        // Assign values to variables that require logic to form the string.
        string endStatement = GetEndStatement(note);
        string itemsList = GetItemsList(note);
        string pluralItems = note.Loans.Count() > 1 ? "s" : "";
        string status = "SUSPENDED";
        string todaysDate = ParseDates.AmericanFormat(DateTime.UtcNow);
        if (note.Status.ToString() == "RESOLVED")
        {
            status = "RESOLVED";
        }


        /*
        Through testing, I was able to discern that Alma notes have a 1,999 character limit. Anything past that gets cut off.
        If the note gets cut off prematurely, the ID required for Auto-Suspend to find to update the note is not present which will cause the program to shut down.
        To circumvent this, I set a maximum suspension length to 256 characters. Anything more than 256 characters is a ridiculously long note.
        If necessary, an end-user can just click the "Okay" button and view all their overdues anyway.
        */
        string formatted_note = $"Acct. Status: {status} @ Instance #{note.Instance} >> Item{pluralItems} Overdue: {itemsList} >> {endStatement} AS OF ({todaysDate}) --AUTO-SUSPEND ({note.Id})";
        if (formatted_note.Length >= 256)
        {
            formatted_note = $"Acct. Status: {status} @ Instance #{note.Instance} >> Items Overdue: ({note.Loans.Count().ToString()}) >> {endStatement} AS OF {todaysDate}) --AUTO-SUSPEND ({note.Id})";
        }

        return formatted_note;
    }
}