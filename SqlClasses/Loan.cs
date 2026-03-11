public class Loan
{
    public int id { get; set; }
    public string alma_id { get; set; }
    public string out_circ_desk { get; set; }
    public string in_circ_desk { get; set; }
    public int patron_id { get; set; }
    public int item_id { get; set; }
    public DateTime loan_date { get; set; }
    public DateTime return_date { get; set; }
}