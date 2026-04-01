using System.Data.Common;

public class Note
{
    public int id { get; set; }
    public int patron_id { get; set; }
    DateTime date { get; set; }
    public StatusType statusType { get; set; }
    int updated { get; set; }
    int instance { get; set; }

    public Note(int id, int patron_id, DateTime date, StatusType statusType, int updated, int instance)
    {
        this.id = id;
        this.patron_id = patron_id;
        this.date = date;
        this.statusType = statusType;
        this.updated = updated;
        this.instance = instance;
    }
}