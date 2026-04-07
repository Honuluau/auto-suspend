public class Stopwatch
{
    private static DateTime? start = null;
    private static DateTime? end = null;

    public static string Start()
    {
        start = DateTime.Now;

        return start.ToString()!;
    }

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