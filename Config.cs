public class Config
{
    public static int InitializeConfig(String path)
    {
        return 0;
    }

    // Create the Config which gets requested by SystemCheck.cs
    public static int CreateConfig(String path)
    {
        try
        {
            FileStream fs = File.Create(path);
        }
        catch (IOException e)
        {
            Logger<Config>.Log($"Failed to create config file: {e.Message}", LogLevel.Error);
            return 5;
        }
        return 0;
    }
}