using System.Text.Json;

public class PatronInformationAPI
{
    public static int TotalRequests = 0;

    public static async Task<PatronAPI?> LookupPatron(HttpClient httpClient, string eagleId)
    {
        string url = $"{SensitiveInfo.GetUserDetailsUrl}{eagleId}?apikey={SensitiveInfo.DevelopmentServerAPIKey}&format=json";
        try
        {
            string jsonString = await httpClient.GetStringAsync(url);
            PatronAPI patron = JsonSerializer.Deserialize<PatronAPI>(jsonString)!;
            
            return patron;
        }
        catch (Exception e)
        {
            Logger<PatronInformationAPI>.Log($"An error occured while looking up a patron: {e.Message}", LogLevel.Error);
            return null;
        }
    }

    public static async Task<int> RetrievePatronInformation(HttpClient httpClient, List<string> eagleIds)
    {
        PatronAPI? patron = await LookupPatron(httpClient, "901494752");
        if (patron != null)
        {
            SQLInterface.InsertPatron(patron.primary_id, patron.first_name, patron.last_name, patron.user_group.value);
        }

        return 0;
    }
}