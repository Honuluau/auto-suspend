/*
This file holds two programs that dictate JSON Objects for user_note in GET UserDetails.
*/

using System.Dynamic;
using System.Text.Json.Serialization;

public class UserNote
{
    [JsonPropertyName("note_type")]
    public required NoteType NoteType { get; set; }

    [JsonPropertyName("note_text")]
    public required string NoteText { get; set; }

    [JsonPropertyName("user_viewable")]
    public required bool UserViewable { get; set; }

    [JsonPropertyName("popup_note")]
    public required bool PopupNote { get; set; }

    [JsonPropertyName("created_by")]
    public required string CreatedBy { get; set; }

    [JsonPropertyName("created_date")]
    public required DateTime CreatedDate { get; set; }

    [JsonPropertyName("segment_tyoe")]
    public required string SegmentType { get; set; }
}

// Sub class for the Table inside of "note_type"
public class NoteType
{
    [JsonPropertyName("value")]
    public required string Value { get; set; }

    [JsonPropertyName("desc")]
    public required string Desc { get; set; }
}