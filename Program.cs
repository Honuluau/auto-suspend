using System.Collections.Concurrent;

public class AutoSuspend
{
    static int Routine(String[] args)
    {
        // System Check
        int systemWorks = SystemCheck.CheckSystem();
        if (systemWorks != 0)
        {
            return systemWorks;
        }

        return 0;
    }

    static int Main(String[] args)
    {
        // Logger Set-up.
        bool initializedLog = Logger<AutoSuspend>.InitializeLog();
        if (!initializedLog)
        {
            return 1; // No error log necessary because it is handled through Logger itself.
        }

        // A way to end the program with logger.
        int successfulRoutine = Routine(args);
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