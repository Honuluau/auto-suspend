using System.Data.Common;

public class Note
{
    public int Id { get; set; }
    public int PatronId { get; set; }
    DateTime Date { get; set; }
    public StatusType Status { get; set; }
    int Updated { get; set; }
    int Instance { get; set; }

    public Note(int id, int patronId, DateTime date, StatusType status, int updated, int instance)
    {
        this.Id = id;
        this.PatronId = patronId;
        this.Date = date;
        this.Status = status;
        this.Updated = updated;
        this.Instance = instance;
    }
}