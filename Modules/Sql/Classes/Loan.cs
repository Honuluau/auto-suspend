public class Loan {
    public string AlmaId { get; set; }
    public int DaysOfGrace { get; set; }
    public DateTime DueDate { get; set; }
    public int Id { get; set; }
    public string InCircDesk { get; set; }
    public Item Item { get; set; }
    public DateTime LoanDate { get; set; }
    public string OutCircDesk { get; set; }
    public int PatronId { get; set; }
    public string Policy { get; set; }
    public string PreferredEmail { get; set; }
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Constructor for Loan class.
    /// </summary>
    /// <param name="id">Database Id.</param>
    /// <param name="almaId">Alma Loan Id.</param>
    /// <param name="outCircDesk">Circulation desk that checked out the item.</param>
    /// <param name="inCircDesk">Circulation desk that checked in the item.</param>
    /// <param name="patronId">Database Id.</param>
    /// <param name="item">Item class.</param>
    /// <param name="policy">Alma policy.</param>
    /// <param name="preferredEmail">Email at the time of loan.</param>
    /// <param name="loanDate">Date the item was loaned.</param>
    /// <param name="dueDate">Date the item is due.</param>
    /// <param name="returnDate">Date the item is returned. Possibly null.</param>
    public Loan(int id, string almaId, string outCircDesk, string inCircDesk, int patronId, Item item,
        string policy, string preferredEmail, DateTime loanDate, DateTime dueDate, DateTime? returnDate) {
        this.Id = id;
        this.AlmaId = almaId;
        this.OutCircDesk = outCircDesk;
        this.InCircDesk = inCircDesk;
        this.PatronId = patronId;
        this.Item = item;
        this.Policy = policy;
        this.PreferredEmail = preferredEmail;
        this.LoanDate = loanDate;
        this.DueDate = dueDate;
        this.ReturnDate = returnDate;

        // Calculate Grace Period
        TimeSpan loanPeriod = DueDate - LoanDate;
        if (loanPeriod.Days <= 1) {
            this.DaysOfGrace = 1; // 24 Hour Loan.
        }
        else {
            this.DaysOfGrace = 3;
        }
    }
}