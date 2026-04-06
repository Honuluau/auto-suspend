using System.Text.Json;

public class SensitiveInfoJson
{
    public required string Analytics { get; set; }
    public required string DevelopmentServer { get; set; }
    public required string OverdueReportUrl { get; set; }
}

public class SensitiveInfo
{
    public static string AnalyticsAPIKey = "";
    public static string DevelopmentServerAPIKey = "";
    public static string OverdueReportUrl = "";

    public static int Init()
    {
        try
        {
            string jsonString = File.ReadAllText("/Users/dyl/autosuspend-sensitiveinfo.json");
            SensitiveInfoJson json = JsonSerializer.Deserialize<SensitiveInfoJson>(jsonString)!;

            AnalyticsAPIKey = json.Analytics;
            DevelopmentServerAPIKey = json.DevelopmentServer;
            OverdueReportUrl = json.OverdueReportUrl;
        }
        catch (Exception e)
        {
            Logger<SensitiveInfo>.Log($"An error occured while initializing API Keys: {e.Message}", LogLevel.Error);
            return 12;
        }

        Logger<SensitiveInfo>.Log("Successfully initialized Sensitive Info.", LogLevel.Info);
        return 0;
    }
}