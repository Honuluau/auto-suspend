using System.Net;

public class SystemCheck
{
    public static bool online { get; set; }
    public static bool availableStorage { get; set; }
    public static bool directories { get; set; }
    public static bool files { get; set; }

    public static bool CheckInternetConnection()
    {
        try
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead("http://www.google.com"))
            {
                Logger<SystemCheck>.Log("Internet connected.", LogLevel.Info);
                return true;
            }
        }
        catch
        {
            Logger<SystemCheck>.Log("No internet.", LogLevel.Error);
            return false;
        }
    }

    public static int CheckSystem()
    {
        online = CheckInternetConnection();
        if (!online)
        {
            return 2;
        }

        return 0;
    }
}