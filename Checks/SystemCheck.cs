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
                return true;
            }
            else
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
            return 0;
        }
    }

    public static int CheckDirectories(string path)
    {
        // Main Directory.
        if (!Directory.Exists(path))
        {
            Logger<SystemCheck>.Log("Main directory not found.", LogLevel.Info);
            try
            {
                Directory.CreateDirectory(path);
                Logger<SystemCheck>.Log("Created main directory.", LogLevel.Info);
            }
            catch (IOException e)
            {
                Logger<SystemCheck>.Log($"Main directory unable to be created: {e.Message}", LogLevel.Error);
                return 4;
            }
        }

        return 0;
    }

    public static int CheckFiles(String path)
    {
        int exitCode = 0;

        // Config file.
        string configFilePath = path + "config.json";
        if (!File.Exists(configFilePath))
        {
            Logger<SystemCheck>.Log("Config file not found", LogLevel.Info);
            exitCode = Config.CreateConfig(configFilePath);
        }
        else
        {
            exitCode = Config.InitializeConfig(configFilePath);
        }
        if (exitCode != 0)
        {
            return exitCode;
        }

        return exitCode;
    }

    public static async Task<int> CheckSystem(HttpClient httpClient, string path)
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

        int directories = CheckDirectories(path);
        if (directories != 0)
        {
            return directories;
        }

        int checkFiles = CheckFiles(path);
        if (checkFiles != 0)
        {
            return checkFiles;
        }

        Logger<SystemCheck>.Log("System check complete, no errors found.", LogLevel.Info);

        return 0;
    }
}