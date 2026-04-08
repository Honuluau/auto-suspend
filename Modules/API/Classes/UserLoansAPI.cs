public class CircDeskAPI
{
    public string? value { get; set; }
    public string? desc { get; set; }
}

public class ItemPolicyAPI
{
    public string? value { get; set; }
    public string? desc { get; set; }
}

public class LoanAPI
{
    public required string loan_id { get; set; }
    public required CircDeskAPI circ_desk { get; set; }
    public required string user_id { get; set; }
    public required string due_date { get; set; }
    public required string loan_date { get; set; }

    // item related
    public required string item_barcode { get; set; }
    public required string mms_id { get; set; }
    public required string title { get; set; }
    public required string description { get; set; }
    public required ItemPolicyAPI item_policy { get; set; }

    public CircDeskAPI? return_circ_desk { get; set; }
}

public class UserLoansAPI
{
    public required List<LoanAPI> item_loan { get; set; }
    public required int total_record_count { get; set; }
}