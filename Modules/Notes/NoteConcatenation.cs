using System.Text;

public class NoteConcatenation
{
    // Format the items into the list: [(Item1,02000),(Item2,1829381932)]
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
    private static string GetEndStatement(Note note)
    {
        StringBuilder statement = new System.Text.StringBuilder("");

        if (NoteAnalysis.AllReturned(note))
        {
            int suspendableInstance = MathUtil.Clamp(note.Instance, 1, Config.Current.SuspensionLengthsPerInstance.Length);
            int suspensionLengthInDays = Config.Current.SuspensionLengthsPerInstance[suspendableInstance]*7;
            DateTime lastDateOfSuspension = note.Date.AddDays(suspensionLengthInDays);

            if (DateTime.Now > lastDateOfSuspension)
            {
                statement.Append($"REINSTATEMENT ON ({ParseDates.AmericanFormat(DateTime.Now.AddDays(suspensionLengthInDays))})");
            }
            else
            {
                statement.Append($"REINSTATEMENT ON ({ParseDates.AmericanFormat(DateTime.Now.AddDays(suspensionLengthInDays))})");
            }
        }
        else
        {
            statement.Append($"UNRESOLVED");
        }

        return statement.ToString();
    }

    // Returns Suspension Note.
    public static string FormatNote(Note note)
    {   
        string itemsList = GetItemsList(note);
        string endStatement = GetEndStatement(note);

        string formatted_note = $"Acct. Status: {note.Status.ToString()} @ Instance #{note.Instance} >> Item{(note.Loans.Count() > 1 ? "s": "")} Overdue: {itemsList} >> {endStatement} AS OF ({ParseDates.AmericanFormat(DateTime.Now)}) --AUTO-SUSPEND ({note.Id})";

        // If message length is about 1999 character limit, the note will be formatted to "X Items".
        if (formatted_note.Length >= 1999)
        {
            formatted_note = $"Acct. Status: {note.Status.ToString()} @ Instance #{note.Instance} >> Items Overdue: ({note.Loans.Count().ToString()}) >> {endStatement} AS OF {ParseDates.AmericanFormat(DateTime.Now)}) --AUTO-SUSPEND ({note.Id})";
        }
        
        return formatted_note; 
    }
}