public class Loan
{
    public int Id { get; set; }
    public string AlmaId { get; set; }
    public string OutCircDesk { get; set; }
    public string InCircDesk { get; set; }
    public int PatronId { get; set; }
    public Item Item { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int DaysOfGrace { get; set; }

    public Loan(int id, string almaId, string outCircDesk, string inCircDesk, int patronId, Item item, DateTime loanDate, DateTime dueDate, DateTime? returnDate)
    {
        this.Id = id;
        this.AlmaId = almaId;
        this.OutCircDesk = outCircDesk;
        this.InCircDesk = inCircDesk;
        this.PatronId = patronId;
        this.Item = item;
        this.LoanDate = loanDate;
        this.DueDate = dueDate;
        this.ReturnDate = returnDate;

        // Calculate Grace Period
        TimeSpan loanPeriod = DueDate - LoanDate;
        if (loanPeriod.Days <= 1) // 24 Hour Loan
        {
            this.DaysOfGrace = 1;
        } else
        {
            this.DaysOfGrace = 3;
        }
    }

    public override string ToString()
    {
        return $"{this.Id}\t{this.AlmaId}\t{this.OutCircDesk}\t{this.InCircDesk}\t{this.PatronId}\t{this.Item.Barcode}\t{this.LoanDate}\t{this.DueDate}\t{this.ReturnDate}";
    }

    public TimeSpan GetOverdueTimespan()
    {
        // Check for grace period.
        return DateTime.Now - this.DueDate;
    }
}