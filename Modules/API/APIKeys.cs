using System.Text.Json;

public class APIKeysJson
{
    public required string Analytics { get; set; }
    public required string DevelopmentServer { get; set; }
}

public class APIKeys
{
    public static string AnalyticsAPIKey = "";
    public static string DevelopmentServerAPIKey = "";

    public static int Init()
    {
        try
        {
            string jsonString = File.ReadAllText("/Users/dyl/autosuspend-apikeys.json");
            APIKeysJson json = JsonSerializer.Deserialize<APIKeysJson>(jsonString)!;

            AnalyticsAPIKey = json.Analytics;
            DevelopmentServerAPIKey = json.DevelopmentServer;
        }
        catch (Exception e)
        {
            Logger<APIKeys>.Log($"An error occured while initializing API Keys: {e.Message}", LogLevel.Error);
            return 12;
        }

        Logger<APIKeys>.Log("Successfully initialized API keys.", LogLevel.Info);
        return 0;
    }
}