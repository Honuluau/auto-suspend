public class ParseDates
{
    public static DateTime ConvertStringToDateTime(string dateTimeString)
    {
        return DateTime.Parse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }
}