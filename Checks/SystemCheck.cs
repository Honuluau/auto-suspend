using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

public class SystemCheck {
    public static bool online { get; set; }
    public static bool availableStorage { get; set; }
    public static bool directories { get; set; }
    public static bool files { get; set; }

    /// <summary>
    /// This method checks to see if there is at least 1 MB of storage of the current Auto-Suspend directory.
    /// </summary>
    /// <returns>Integer overflow.</returns>
    public static int CheckAvailableStorage() {
        string currentDirectory = Directory.GetCurrentDirectory()!;
        DriveInfo drive = new DriveInfo(Path.GetPathRoot(currentDirectory)!);
        long availableFreeSpace = drive.AvailableFreeSpace;

        if (drive.AvailableFreeSpace < 1000000) // 1 MB
        {
            Logger<SystemCheck>.Log($"{drive.Name} has {FileSizeHelper.GetReadableFileSize(availableFreeSpace)} of storage which is less than 1MB.", LogLevel.Error);
            return 2;
        }
        else {
            return 0;
        }
    }

    /// <summary>
    /// This method ensures that the main Auto-Suspend directory exists by creating if not found.
    /// </summary>
    /// <param name="path">Main Auto-Suspend path.</param>
    /// <returns>Integer overflow.</returns>
    public static int CheckDirectories(string path) {
        // Main Directory.
        if (!Directory.Exists(path)) {
            Logger<SystemCheck>.Log("Main directory not found.", LogLevel.Info);
            try {
                Directory.CreateDirectory(path);
                Logger<SystemCheck>.Log("Created main directory.", LogLevel.Info);
            }
            catch (Exception e) {
                Logger<SystemCheck>.Error("Main directory unable to be created", e);
                return 4;
            }
        }

        return 0;
    }

    /// <summary>
    /// This method checks to see if important files exist and create them if they are not found.
    /// </summary>
    /// <param name="path">Main Auto-Suspend path.</param>
    /// <returns>Integer overflow.</returns>
    public static int CheckFiles(String path) {
        int exitCode = 0;

        // Config file.
        string configFilePath = path + "config.json";
        if (!File.Exists(configFilePath)) {
            Logger<SystemCheck>.Log("Config file not found", LogLevel.Info);
            exitCode = Config.CreateConfig(configFilePath);
        }
        else {
            exitCode = Config.InitializeConfig(configFilePath);
        }
        if (exitCode != 0) {
            return exitCode;
        }

        return exitCode;
    }

    /// <summary>
    /// This method checks the internet connection by requesting google.com. Google.com should be reliable
    /// enough to check.
    /// </summary>
    /// <param name="httpClient">Standard httpClient that Auto-Suspend holds in httpClientHouse.</param>
    /// <returns>Integer overflow.</returns>
    public static async Task<bool> CheckInternetConnection(HttpClient httpClient) {
        try {
            HttpResponseMessage response = await httpClient.GetAsync("http://www.google.com");

            if (response.IsSuccessStatusCode) {
                return true;
            }
            else {
                Logger<SystemCheck>.Log("No internet.", LogLevel.Error);
                return false;
            }
        }
        catch (Exception e) {
            Logger<SystemCheck>.Error("No internet", e);
            return false;
        }
    }

    /// <summary>
    /// This method is the overall check system method that executes more specific methods such as:
    /// Check Internet Connection, Available Storage, Directories, and Files.
    /// </summary>
    /// <param name="path">Main Auto-Suspend path.</param>
    /// <returns>Integer overflow</returns>
    public static async Task<int> CheckSystem(string path) {
        HttpClient httpClient = HttpClientHouse.GetHttpClient();

        bool online = await CheckInternetConnection(httpClient);
        if (!online) {
            return 2;
        }

        int availableStorage = CheckAvailableStorage();
        if (availableStorage != 0) {
            return availableStorage;
        }

        int directories = CheckDirectories(path);
        if (directories != 0) {
            return directories;
        }

        int checkFiles = CheckFiles(path);
        if (checkFiles != 0) {
            return checkFiles;
        }

        Logger<SystemCheck>.Log("System check complete, no errors found.", LogLevel.Info);

        return 0;
    }
}