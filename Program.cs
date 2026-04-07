using System.Collections.Concurrent;
using System.Threading.Tasks;

public class AutoSuspend
{
    public static readonly HttpClient httpClient = new HttpClient();
    public static readonly string AUTO_SUSPEND_PATH = "/Users/dyl/.auto-suspend/";
    public static readonly int GRACE_DAYS = 3;

    static async Task<int> Routine(String[] args)
    {
        // System Check
        int systemWorks = await SystemCheck.CheckSystem(httpClient, AUTO_SUSPEND_PATH);
        if (systemWorks != 0)
        {
            return systemWorks;
        }

        // Data Check
        int dataWorks = DataCheck.CheckData(AUTO_SUSPEND_PATH);
        if (dataWorks != 0)
        {
            return dataWorks;
        }

        // Sensitive Info Check
        int sensitiveInfo = SensitiveInfo.Init();
        if (sensitiveInfo != 0)
        {
            return sensitiveInfo;
        }

        // Development Stuff -- Subject to Change
        /*
        int overdueAnalyticsAPI = await OverdueAnalyticsAPI.GatherOverdueEagleIds(httpClient);
        if (overdueAnalyticsAPI != 0)
        {
            return overdueAnalyticsAPI;
        }
        List<string> EagleIds = OverdueAnalyticsAPI.GetEagleIds();
        */
        // Supplmentary way of getting eagle ids without stress testing the API.
        List<string> EagleIds = File.ReadAllLines("eagleids.txt").ToList();

        int patronInformationAPI = await PatronInformationAPI.RetrievePatronInformation(httpClient, EagleIds);
        if (patronInformationAPI != 0)
        {
            return patronInformationAPI;
        }

        SQLInterface.ConsolidateLoans();
        NoteAnalysis.AnalyzeNotes();
        return 0;
    }

    static async Task<int> Main(String[] args)
    {
        // Logger Set-up.
        bool initializedLog = Logger<AutoSuspend>.InitializeLog();
        if (!initializedLog)
        {
            return 1; // No error log necessary because it is handled through Logger itself.
        }

        // A way to end the program with logger.
        int successfulRoutine = await Routine(args);
        if (successfulRoutine != 0)
        {
            Logger<AutoSuspend>.Log($"Auto-Suspend ended with error code: {successfulRoutine}", LogLevel.Error);
        } else
        {
            Logger<AutoSuspend>.Log($"Auto-Suspend ended without errors.", LogLevel.Info);
            return 0;
        }

        return 0;
    }
}