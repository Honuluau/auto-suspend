using System.Collections.Concurrent;
using System.Threading.Tasks;

public class AutoSuspend
{
    public static readonly string AUTO_SUSPEND_PATH = "/Users/dyl/.auto-suspend/";
    public static readonly int GRACE_DAYS = 3;

    static async Task<int> Routine(String[] args)
    {
        // System Check
        int systemWorks = await SystemCheck.CheckSystem(AUTO_SUSPEND_PATH);
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

        // NOTE CONCATENATION TESTING
        Note note = new Note(9, 6, ParseDates.ConvertStringToDateTime("2025-04-28"), StatusType.SUSPENDED, 0, SQLInterface.GetInstance(9));
        Logger<NoteConcatenation>.Log(NoteConcatenation.FormatNote(note), LogLevel.Debug);

        /*
        THIS IS FOR THE RETURN SYSETM.
        AlmaLoan? loan = await UserFulfillment.SearchLoan("11012047580002950", "901458044");
        if (loan != null)
        {
            Console.WriteLine(loan.IsReturned());
            Console.WriteLine(loan.ReturnDate);
            Console.WriteLine(ParseDates.ConvertStringToDateTime(loan.ReturnDate).ToString());
        }

        AlmaLoan? loan2 = await UserFulfillment.SearchLoan("11012048070002950", "901458044");
        if (loan2 != null)
        {
            Console.WriteLine(loan2.IsReturned());
            Console.WriteLine(loan2.ReturnDate);
        }
        */

        /*
        int overdueAnalyticsAPI = await OverdueAnalytics.GatherOverdueAnalytics();
        if (overdueAnalyticsAPI != 0)
        {
            return overdueAnalyticsAPI;
        }


        int consolidateLoans = SQLInterface.ConsolidateLoans();
        if (consolidateLoans != 0)
        {
            return consolidateLoans;
        }

        int returnSystem = ReturnSystem.ProcessMissingOverdues();
        if (returnSystem != 0)
        {
            return returnSystem;
        }

        int noteAnalysis = NoteAnalysis.AnalyzeNotes();
        if (noteAnalysis != 0)
        {
            return noteAnalysis;
        }
        */
        
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