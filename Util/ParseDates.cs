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
}