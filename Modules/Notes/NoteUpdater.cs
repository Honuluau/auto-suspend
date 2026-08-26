/*
The purpose of this class is to handle how the program updates notes inside of Alma.
*/

using System.Collections;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

/// <summary>
/// Update note by:
/// 1. Grabbing the current User's Details.
/// 2. Replacing the note in the User's Details via the note.Id at the end of the string.
/// 3. OR adding the note to User Details if the note was not found.
/// 4. Then a PUT request with the modified User Details
/// </summary>
public class NoteUpdater
{
    /// <summary>
    /// Uses a regular expression to take the number ouf of a written note such as: "...(29)" = "29"
    /// </summary>
    /// <param name="writtenNote">A note that has been turned into a string.</param>
    /// <returns>The id as a string. If no Id is found, returns empty.</returns>
    public static string GetIdFromWrittenNote(String writtenNote)
    {
        MatchCollection matches = Regex.Matches(writtenNote, @"\((.*?)\)");
        if (matches.Count > 0)
        {
            // Get last number in parenthesis (#). Sometimes notes will be shortened which include a list of items also in parenthesis.
            return matches[matches.Count - 1].Groups[1].Value;
        }

        return "";
    }

    /// <summary>
    /// Quick method that is shared across methods to check if note is visible.
    /// </summary>
    /// <param name="note">Note object.</param>
    /// <returns>If the note is viewable by a patron based on it's status.</returns>
    public static bool IsNoteViewable(Note note)
    {
        return !(note.Status == StatusType.RESOLVED);
    }

    /// <summary>
    /// This method updates a note by pulling User Details, modifying only the note, and then sending a PUT request.
    /// </summary>
    /// <remarks>
    /// Still working on this.
    /// </remarks>
    /// <param name="note">The note object to update.</param>
    /// <returns></returns>
    public static async Task<int> UpdateNote(Note note)
    {
        // Get the current UserPrimaryIdentifier for GET request.
        string? userPrimaryIdentifier = SQLInterface.GetUserPrimaryIdentifier(note.PatronId);
        if (userPrimaryIdentifier == null)
        {
            return 28;
        }

        // Get jsonData from Alma.
        HttpClient httpClient = HttpClientHouse.GetHttpClient();
        string url = $"{SensitiveInfo.GetUserDetailsUrl}{userPrimaryIdentifier}?apikey={SensitiveInfo.DevelopmentServerAPIKey}&format=json";
        string jsonStringData = await httpClient.GetStringAsync(url);
        if (jsonStringData == null)
        {
            return 29;
        }

        // Using Dynamic Nodes in order to preserve any data in the UserDetails that is not modified.
        JsonNode? userDetails = JsonNode.Parse(jsonStringData);
        if (userDetails == null)
        {
            return 30;
        }

        /*
        Identify the note that auto-suspend created (if there is one).
        As well as flag notes that are possible duplicates from Lonnie's manual suspensions.
        */
        JsonNode? targetNode = null; // The node/note the program updates.
        JsonArray userNotes = userDetails["user_note"]!.AsArray(); // user_note is always there.
        foreach (JsonNode userNote in userNotes!)
        {
            String note_text = userNote["note_text"]!.GetValue<String>().ToLower();

            // Check to see if it's a manual suspension. Logged for migrating notes.
            if (note_text.Contains("instance") && !note_text.Contains("auto-suspend"))
            {
                Logger<NoteUpdater>.Log($"A note was flagged as a possible old suspension not created from Auto-Suspend. UserPrimaryIdentifier: {userPrimaryIdentifier}", LogLevel.Info);
                continue;
            }

            // Check for Auto-Suspension tag, find the note's Id and check if it matches with the input note.
            if (note_text.Contains("auto-suspend"))
            {
                String note_id = GetIdFromWrittenNote(note_text);
                if (note_id == note.Id.ToString())
                {
                    targetNode = userNote;
                }
            }
        }

        bool noteChanged = false; // Important value that determines whether or not to send a put request.

        /*
        For there to be a targetNode, the array for userNotes must be valid so it is safe to assume userNotes is not null in this logic.
        */
        // Add note if no targetNode was found
        if (targetNode == null)
        {
            Logger<NoteUpdater>.Log($"No previous note was found for note whose id={note.Id.ToString()}. Adding note.", LogLevel.Info);

            // Hide note if note is RESOLVED.
            bool viewable = IsNoteViewable(note);

            // Adding note.
            JsonObject addedJson = new JsonObject
            {
                ["note_type"] = new JsonObject
                {
                    ["value"] = "ADDRESS",
                    ["desc"] = "Address"
                },
                ["note_text"] = NoteConcatenation.FormatNote(note), // Double check this.
                ["user_viewable"] = viewable,
                ["popup_note"] = viewable,
                ["created_by"] = "AUTO-SUSPEND",
                ["created_date"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), // Absolutely sure it is the correct format instead of plain UtcNow
                ["segment_type"] = "Internal"
            };

            userNotes.Add(addedJson);
            noteChanged = true;
        }
        else // Edit note if targetNode was found.
        {
            // Hide note if note is RESOLVED.
            bool viewable = IsNoteViewable(note);

            // Save old text to check if it updated.
            String oldNoteText = targetNode["note_text"]!.ToString();

            targetNode["note_text"] = NoteConcatenation.FormatNote(note);
            targetNode["user_viewable"] = viewable;
            targetNode["popup_note"] = viewable;

            // Only if the note has changed, send the note change signal.
            if (oldNoteText != targetNode["note_text"]!.ToString())
            {
                noteChanged = true;
            }
        }

        /*
        Check to see if the note changed at all. If yes, send a PUT request for the new json object.
        */
        if (noteChanged)
        {
            // Format UserDetails
            var jsonStringNotes = new StringContent(userDetails.ToJsonString(), System.Text.Encoding.UTF8, "application/json");

            string putURL = $"{SensitiveInfo.GetUserDetailsUrl}{userPrimaryIdentifier}?apikey={SensitiveInfo.DevelopmentServerAPIKey}&format=json";
            var putResponse = await httpClient.PutAsync(putURL, jsonStringNotes);

            if (!putResponse.IsSuccessStatusCode)
            {
                return 32;
            }
            else
            {
                Logger<NoteUpdater>.Log($"Successfully updated note id({note.Id.ToString()}) for user({userPrimaryIdentifier})", LogLevel.Info);
            }
            putResponse.EnsureSuccessStatusCode();
        }

        return 0;
    }
}