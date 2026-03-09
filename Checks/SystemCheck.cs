using System.Net;
using System.Security.Cryptography.X509Certificates;
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

    public static int CheckAvailableStorage()
    {
        string currentDirectory = Directory.GetCurrentDirectory()!;
        DriveInfo drive = new DriveInfo(Path.GetPathRoot(currentDirectory)!);
        long availableFreeSpace = drive.AvailableFreeSpace;

        if (drive.AvailableFreeSpace < 1000000) // 1 MB
        {
            Logger<SystemCheck>.Log($"{drive.Name} has {FileSizeHelper.GetReadableFileSize(availableFreeSpace)} of storage which is less than 1MB.", LogLevel.Error);
            return 2;
        }
        else
        {
            Logger<SystemCheck>.Log($"Drive ({drive.Name}) has sufficient storage: {FileSizeHelper.GetReadableFileSize(availableFreeSpace)}", LogLevel.Info);
            return 0;   
        }
    }

    public static async Task<int> CheckSystem(HttpClient httpClient)
    {
        bool online = await CheckInternetConnection(httpClient);
        if (!online)
        {
            return 2;
        }

        int availableStorage = CheckAvailableStorage();
        if (availableStorage != 0)
        {
            return availableStorage;
        }

        return 0;
    }
}