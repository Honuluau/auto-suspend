public class ParseDates
{
    public static DateTime ConvertStringToDateTime(string dateTimeString)
    {
        return DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    public static string TodayYearMonthDay()
    {
        return DateTime.Today.ToString("yyyy-MM-dd");
    }

    public static string AmericanFormat(DateTime dateTime)
    {
        return dateTime.ToString("MMM dd, yyyy");
    }
}