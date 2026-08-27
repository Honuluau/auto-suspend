public class Stopwatch
{
    private static DateTime? start = null;
    private static DateTime? end = null;

    /// <summary>
    /// This method starts the stop watch by saving DateTime.now to a static property.
    /// </summary>
    /// <returns>The string format of DateTime.now.</returns>
    public static string Start()
    {
        start = DateTime.Now;

        return start.ToString()!;
    }

    /// <summary>
    /// This stops the stopwatch.
    /// </summary>
    /// <returns>Returns the length of the time between start and stop.</returns>
    public static string Stop()
    {
        end = DateTime.Now;

        if (start != null && end != null)
        {
            return (end - start).ToString()!;
        }
        else
        {
            return "Stopwatch never started";
        }
    }
}