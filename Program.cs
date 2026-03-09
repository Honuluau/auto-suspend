public class AutoSuspend
{
    static int Main(String[] args)
    {
        bool initializedLog = Logger<AutoSuspend>.InitializeLog();
        if (!initializedLog)
        {
            return 1;
        }

        Console.WriteLine("Hello, World!");
        Logger<AutoSuspend>.Log("Program completed line 5 of program.cs", LogLevel.Info);
        Logger<AutoSuspend>.Log("Program completed line 6 of program.cs", LogLevel.Debug);
        return 0;
    }
}