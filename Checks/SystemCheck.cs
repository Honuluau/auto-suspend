using System.Net;
using System.Threading.Tasks;

public class SystemCheck
{
    public static bool online { get; set; }
    public static bool availableStorage { get; set; }
    public static bool directories { get; set; }
    public static bool files { get; set; }

    public static async Task<bool> CheckInternetConnection(HttpClient httpClient)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync("http://www.google.com");

            if (response.IsSuccessStatusCode)
            {
                Logger<SystemCheck>.Log("Internet connected.", LogLevel.Info);
                return true;
            } else
            {
                Logger<SystemCheck>.Log("No internet.", LogLevel.Error);
                return false;
            }
        }
        catch
        {
            Logger<SystemCheck>.Log("No internet.", LogLevel.Error);
            return false;
        }
    }

    public static async Task<int> CheckSystem(HttpClient httpClient)
    {
        bool online = await CheckInternetConnection(httpClient);
        if (!online)
        {
            return 2;
        }

        return 0;
    }
}