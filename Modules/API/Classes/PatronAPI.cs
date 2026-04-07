public class UserGroupAPI
{
    public required string value { get; set; }
    public required string desc { get; set; }
}

public class PatronAPI
{
    // Variable names must match JSON return from API.
    public required string primary_id { get; set; }
    public required string first_name { get; set; }
    public required string last_name { get; set; }
    public required UserGroupAPI user_group { get; set; }
}