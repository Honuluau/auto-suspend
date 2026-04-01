using System.Data.Common;

public class Note
{
    public int Id { get; set; }
    public int PatronId { get; set; }
    public DateTime Date { get; set; }
    public StatusType Status { get; set; }
    public int Updated { get; set; }
    public int Instance { get; set; }
    public Loan[] Loans { get; set; }

    public Note(int id, int patronId, DateTime date, StatusType status, int updated, int instance)
    {
        this.Id = id;
        this.PatronId = patronId;
        this.Date = date;
        this.Status = status;
        this.Updated = updated;
        this.Instance = instance;
    }

    public void InitializeLoans()
    {
        this.Loans = SQLInterface.GetLoansForNote(this.Id)!;
    }
}