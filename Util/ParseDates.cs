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
    /// <returns>A DateTime object or null.</returns>
    public static DateTime? ConvertStringToDateTime(string dateTimeString)
    {
        try {
            return DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);        
        } catch (Exception) {
            return null;
        }
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