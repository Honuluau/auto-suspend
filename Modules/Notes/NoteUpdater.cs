/*
The purpose of this class is to handle how the program updates notes inside of Alma.
*/

public class NoteUpdater
{
    /*
    Update note by:
    1. Grabbing the current User's Details.
    2. Replacing the note in the User's Details via the note.Id at the end of the string.
    3. OR adding the note to User Details if the note was not found.
    4. Then a PUT request with the modified User Details
    */
    public static async Task<int> UpdateNote(Note note)
    {
        // Get the current UserPrimaryIdentifier for GET request.
        string? userPrimaryIdentifier = SQLInterface.GetUserPrimaryIdentifier(note.PatronId);
        if (userPrimaryIdentifier == null)
        {
            return 28;
        }


        HttpClient httpClient = HttpClientHouse.GetHttpClient();
        string url = $"{SensitiveInfo.GetUserDetailsUrl}{userPrimaryIdentifier.ToString()}?apikey={SensitiveInfo.DevelopmentServerAPIKey}&format=json";
        Logger<NoteUpdater>.Log(url, LogLevel.Debug);

        string jsonData = await httpClient.GetStringAsync(url);
        Logger<NoteUpdater>.Log(jsonData, LogLevel.Debug);
        // works. must get uesrPrimaryIdentifier through SQL Interface.

        return 0;
    }
}