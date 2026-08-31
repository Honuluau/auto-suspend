public class Stopwatch
{
    private static DateTime? _start = null;
    private static DateTime? _end = null;

    /// <summary>
    /// This method starts the stop watch by saving DateTime.now to a static property.
    /// </summary>
    /// <returns>The string format of DateTime.now.</returns>
    public static string Start()
    {
        _start = DateTime.Now;

        return _start.ToString()!;
    }

    /// <summary>
    /// This stops the stopwatch.
    /// </summary>
    /// <returns>Returns the length of the time between start and stop.</returns>
    public static string Stop()
    {
        _end = DateTime.Now;

        if (_start != null && _end != null)
        {
            return (_end - _start).ToString()!;
        }
        else
        {
            return "Stopwatch never started";
        }
    }
}