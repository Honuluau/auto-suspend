public static class Logger<T>
{
    private const string LOG_PATH = "log.txt";

    public static void Log(string message, LogLevel level)
    {
        string log = $"[{DateTime.Now}]\t[{level}]\t[{typeof(T).Name}]\t{message}";
        File.AppendAllText(LOG_PATH, log + Environment.NewLine);
        Console.WriteLine(log);
    }

    // This ensures that the Log file exists and it is the first method called by Auto-Suspend.
    public static bool InitializeLog()
    {
        try
        {
            if (!File.Exists(LOG_PATH))
            {
                File.Create(LOG_PATH).Close();
            }
            return true;
        }
        catch (IOException e)
        {
            Console.WriteLine($"An error occured initializing log: {e.Message}");
            return false;
        }
    }
}