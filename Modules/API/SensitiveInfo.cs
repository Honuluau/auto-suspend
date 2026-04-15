using System.Text.Json;

public class SensitiveInfoJson
{
    public required string Analytics { get; set; }
    public required string DevelopmentServer { get; set; }
    public required string OverdueReportUrl { get; set; }
    public required string GetUserDetailsUrl { get; set; }
    public required string CustomOverdueReportUrl { get; set; }
}

public class SensitiveInfo
{
    public static string AnalyticsAPIKey = "";
    public static string DevelopmentServerAPIKey = "";
    public static string OverdueReportUrl = "";
    public static string GetUserDetailsUrl = "";
    public static string CustomOverdueReportUrl = "";

    public static int Init()
    {
        try
        {
            string jsonString = File.ReadAllText("/Users/dyl/autosuspend-sensitiveinfo.json");
            SensitiveInfoJson json = JsonSerializer.Deserialize<SensitiveInfoJson>(jsonString)!;

            AnalyticsAPIKey = json.Analytics;
            DevelopmentServerAPIKey = json.DevelopmentServer;
            OverdueReportUrl = json.OverdueReportUrl;
            GetUserDetailsUrl = json.GetUserDetailsUrl;
            CustomOverdueReportUrl = json.CustomOverdueReportUrl;
        }
        catch (Exception e)
        {
            Logger<SensitiveInfo>.Error("An error occured while initializing API keys", e);
            return 12;
        }

        Logger<SensitiveInfo>.Log("Successfully initialized Sensitive Info.", LogLevel.Info);
        return 0;
    }
}