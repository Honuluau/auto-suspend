public class ParseDates
{
    /// <summary>
    /// Turns datestring into an american format: Aug 27, 2026
    /// </summary>
    /// <param name="dateTime">DateTime</param>
    /// <returns>A string formatted as such: "Aug 27, 2026"</returns>
    public static string AmericanFormat(DateTime dateTime)
    {
        return dateTime.ToString("MMM dd, yyyy");
    }

    /// <summary>
    /// Convertings a string into a datetime.
    /// </summary>
    /// <param name="dateTimeString">A string that matches DateTimeStyle: RoundtripKind</param>
    /// <returns>A DateTime object.</returns>
    public static DateTime ConvertStringToDateTime(string dateTimeString)
    {
        /*
        This method can fail and cause the entire program to shut down. This is intentional because if there
        is any date-value that does not follow the format, then it is a deeper issue that needs to be
        addressed in Alma.
        */
        return DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    /// <summary>
    /// This method gets the current date and sets it to the following format: 2026-08-27. August 27th, 2026.
    /// </summary>
    /// <returns>Returns the current day as the following format: 2026-08-27. August 27th, 2026</returns>
    public static string TodayYearMonthDay()
    {
        return DateTime.Today.ToString("yyyy-MM-dd");
    }
}