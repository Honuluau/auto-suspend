public class Note {
    public DateTime Date { get; set; }
    public int Id { get; set; }
    public int Instance { get; set; }
    public Loan[] Loans { get; set; }
    public int PatronId { get; set; }
    public StatusType Status { get; set; }
    public int Updated { get; set; }
 
    /// <summary>
    /// This is a constructor method for a Note class in cs.
    /// </summary>
    /// <param name="id">Database Id.</param>
    /// <param name="patronId">Database Id.</param>
    /// <param name="date">Loan date for loans connected to note.</param>
    /// <param name="status">Status of the Note.</param>
    /// <param name="updated">1 = updated in Alma.</param>
    /// <param name="instance">The incident #; for example: second-offense.</param>
    public Note(int id, int patronId, DateTime date, StatusType status, int updated, int instance) {
        this.Id = id;
        this.PatronId = patronId;
        this.Date = date;
        this.Status = status;
        this.Updated = updated;
        this.Instance = instance;
        this.Loans = [];

        InitializeLoans();
    }

    /// <summary>
    /// This method is called upon construction and fills in it's own loans.
    /// </summary>
    public void InitializeLoans() {
        this.Loans = SQLInterface.GetLoansForNote(this.Id) ?? [];
    }
}