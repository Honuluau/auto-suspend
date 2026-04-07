public class Stopwatch
{
    private static DateTime? start = null;
    private static DateTime? end = null;

    public static void Start()
    {
        start = DateTime.Now;
    }

    public static string End()
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