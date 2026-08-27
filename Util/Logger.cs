public static class Logger<T> {
    private const string LOG_PATH = "log.txt";

    /// <summary>
    /// This is a handler method for when an error occurs. It's a convenience method for developers to use,
    /// in order to quickly print an error that took place in the program.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="e"></param>
    public static void Error(string message, Exception e) {
        Log($"{message}. ( {e.Message} ).\n{e.StackTrace}", LogLevel.Error);
    }

    /// <summary>
    /// This method ensures that the Log File exists. It is the first method called by Program.cs.
    /// </summary>
    /// <returns>A bool that is true if the log has loaded into the program.</returns>
    public static bool InitializeLog() {
        try {
            if (!File.Exists(LOG_PATH)) {
                File.Create(LOG_PATH).Close();
            }

            Log("Auto-Suspend started.", LogLevel.Info);
            return true;
        }
        catch (IOException e) {
            Console.WriteLine($"An error occured initializing log: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Writes a line in the log.txt file based off of the message and log level.
    /// </summary>
    /// <param name="message">Log message.</param>
    /// <param name="level">Level of the log.</param>
    public static void Log(string message, LogLevel level) {
        string log = $"[{DateTime.UtcNow}]\t[{level}]\t[{typeof(T).Name}]\t{message}";
        File.AppendAllText(LOG_PATH, log + Environment.NewLine);
        Console.WriteLine(log);
    }
}