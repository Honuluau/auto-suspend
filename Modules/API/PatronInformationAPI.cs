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
            TotalRequests++;
            Logger<PatronInformationAPI>.Log($"Recieved information for {eagleId}", LogLevel.Info);

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
        Logger<PatronInformationAPI>.Log($"Retrieving Patron Information through API.. Expecting {eagleIds.Count} requests...", LogLevel.Info);
        Stopwatch.Start();

        int alreadyExists = 0;

        foreach (string eagleId in eagleIds)
        {
            // Check if exists in the database first.
            int id = SQLPatronInterface.GetPatronId(eagleId);
            if (id == 0) // An id of 0 means that the patron does not exist
            {
                // Patron does not exist in database, look them up and insert.

                PatronAPI? patron = await LookupPatron(httpClient, eagleId);
                if (patron != null)
                {
                    SQLPatronInterface.InsertPatron(patron.primary_id, patron.first_name, patron.last_name, patron.user_group.value);
                }
                else
                {
                    return 16;
                }
            }
            else if (id < 0) // An error occured during the SQL check.
            {
                return id;
            }
            else
            {
                alreadyExists++;
            }
        }

        Logger<PatronInformationAPI>.Log($"Finished retrieving patron information in {Stopwatch.Stop()} with {TotalRequests} API requests. {alreadyExists} patrons were already listed.", LogLevel.Info);
        return 0;
    }
}