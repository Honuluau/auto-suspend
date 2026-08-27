using System.Text;

public class NoteConcatenation {
    /// <summary>
    /// Quick record type for readibilty when passing information to concatenate the information of a note.
    /// </summary>
    record ConcatenateDetail {
        public string EndStatement { get; set; } = "";
        public int Instance { get; set; } = -1;
        public string ItemsList { get; set; } = "";
        public int LoansCount { get; set; } = -1;
        public int NoteId { get; set; } = -1;
        public string PluralItemsMarker { get; set; } = ""; // String for empty char.
        public string Status { get; set; } = "";
        public string TodaysDate { get; set; } = "";
    }

    /// <summary>
    /// This method formats a given note into a suspension note that is visible in Alma.
    /// </summary>
    /// <param name="note"></param>
    /// <returns>A suspension note.</returns>
    public static string FormatNote(Note note) {
        ConcatenateDetail detail = new ConcatenateDetail();
        detail.EndStatement = GetEndStatement(note);
        detail.Instance = note.Instance;
        detail.ItemsList = GetItemsList(note);
        detail.LoansCount = note.Loans.Count();
        detail.NoteId = note.Id;
        detail.PluralItemsMarker = note.Loans.Count() > 1 ? "s" : "";
        detail.Status = "SUSPENDED"; // Always suspended unless RESOLVED.
        detail.TodaysDate = ParseDates.AmericanFormat(DateTime.UtcNow);

        if (note.Status.ToString() == "RESOLVED") {
            detail.Status = "SUSPENDED";
        }

        return ConcatenateNote(detail);
    }

    /// <summary>
    /// Lengthy builder for easy readability. Concatenates a note into a suspension note.
    /// </summary>
    /// <param name="detail">Required information about the note.</param>
    /// <returns>A suspension note.</returns>
    static string ConcatenateNote(ConcatenateDetail detail) {
        StringBuilder builder = new StringBuilder("Acct. Status: ");

        builder.Append(detail.Status);
        builder.Append(" @ Instance #");
        builder.Append(detail.Instance);
        builder.Append(" >> Item");
        builder.Append(detail.PluralItemsMarker);
        builder.Append(" Overdue: ");
        builder.Append(detail.ItemsList);
        builder.Append(" >> ");
        builder.Append(detail.EndStatement);
        builder.Append(" AS OF (");
        builder.Append(detail.TodaysDate);
        builder.Append(") --AUTO-SUSPEND (");
        builder.Append(detail.NoteId);
        builder.Append(")");

        // Check if longer than Auto-Suspend note character limit.
        if (builder.Length >= 256) {
            return ConcatenateNoteShort(detail);
        }

        return builder.ToString();
    }
    
    /// <summary>
    /// Short form replaces the items list with the amount of loans.
    /// </summary>
    /// <param name="detail">Required information about the note.</param>
    /// <returns>A shortened suspension note.</returns>
    static string ConcatenateNoteShort(ConcatenateDetail detail) {
        StringBuilder builder = new StringBuilder("Acct. Status: ");

        builder.Append(detail.Status);
        builder.Append(" @ Instance #");
        builder.Append(detail.Instance);
        builder.Append(" >> Items Overdue: (");
        builder.Append(detail.LoansCount);
        builder.Append(") >> ");
        builder.Append(detail.EndStatement);
        builder.Append(" AS OF (");
        builder.Append(detail.TodaysDate);
        builder.Append(") --AUTO-SUSPEND (");
        builder.Append(detail.NoteId);
        builder.Append(")");

        return builder.ToString();
    }

    /// <summary>
    /// Formats the end of a string for a note. The result is determined on if the items are returned or not.
    /// This essentially either marks a note as UNRESOLVED or gives a REINSTATEMENT DATE.
    /// </summary>
    /// <remarks>
    /// NoteAnalysis.GetReinstatementDate is asserted because to format a note, it must not be null.
    /// </remarks>
    /// <param name="note"></param>
    /// <returns>A string that is either UNRESOLVED, or a REINSTATEMENT date.</returns>
    static string GetEndStatement(Note note) {
        StringBuilder statement = new System.Text.StringBuilder("");

        if (NoteAnalysis.AllReturned(note)) {
            DateTime reinstatementDate = NoteAnalysis.GetReinstatementDateForNote(note)!.Value;
            statement.Append($"REINSTATEMENT ON ({ParseDates.AmericanFormat(reinstatementDate)})");
        }
        else {
            statement.Append($"UNRESOLVED");
        }

        return statement.ToString();
    }

    /// <summary>
    /// Formats the items specified by the loans of a note.
    /// </summary>
    /// <param name="note"></param>
    /// <returns>A string that looks like this: [(Item1,02000),(Item2,03000)]</returns>
    static string GetItemsList(Note note) {
        StringBuilder list = new System.Text.StringBuilder("[");

        foreach (Loan loan in note.Loans) {
            list.Append($"({loan.Item.Title},{loan.Item.Barcode}),");
        }

        // Chop off last comma and close the list.
        list.Length--;
        list.Append("]");

        return list.ToString();
    }
}