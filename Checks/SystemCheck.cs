using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
    public static void CheckAvailableStorage() {
        string currentDirectory = Directory.GetCurrentDirectory()!;
        DriveInfo drive = new DriveInfo(Path.GetPathRoot(currentDirectory)!);
        long availableFreeSpace = drive.AvailableFreeSpace;

        if (drive.AvailableFreeSpace < 1000000) // 1 MB
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("There is not enough space on the executing drive.");
            builder.Append(drive.Name);
            builder.Append(" has ");
            builder.Append(FileSizeHelper.GetReadableFileSize(availableFreeSpace));
            builder.Append(" of storage which is less than 1MB.");

            throw new InsufficientMemoryException(builder.ToString());
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
    public static async Task CheckInternetConnection(HttpClient httpClient) {
        HttpResponseMessage response = await httpClient.GetAsync("http://www.google.com");

        if (!response.IsSuccessStatusCode) {
            throw new FailedInternetConnectionException("Response had no success code.");
        }
    }

    /// <summary>
    /// This method is the overall check system method that executes more specific methods such as:
    /// Check Internet Connection, Available Storage, Directories, and Files.
    /// </summary>
    /// <param name="path">Main Auto-Suspend path.</param>
    /// <returns>Integer overflow</returns>
    public static async Task CheckSystem(string path) {
        HttpClient httpClient = HttpClientHouse.GetHttpClient();

        try {
            await CheckInternetConnection(httpClient);
            CheckAvailableStorage();
        }
        catch {
            return;
        }
        await CheckInternetConnection(httpClient);
        CheckAvailableStorage();

        int directories = CheckDirectories(path);
        if (directories != 0) {
            throw new Exception();
        }

        int checkFiles = CheckFiles(path);
        if (checkFiles != 0) {
            throw new Exception();
        }

        Logger<SystemCheck>.Log("System check complete, no errors found.", LogLevel.Info);
    }
}