public static class Logger<T>
{
    public static void Log(string message, LogLevel level)
    {
        string log = $"[{DateTime.Now}]\t[{level}]\t[{typeof(T).Name}]\t{message}";
        Console.WriteLine(log);
    }
}