using System.Text.Json;

public class ConfigJSON
{
    public int[] SuspensionLengthsPerInstance { get; set; }
}

public class Config
{
    private static readonly int[] SUSPENSION_LENGTHS_PER_INSTANCE = [1, 2, 4, 8];


    public static ConfigJSON Current { get; set; } = new ConfigJSON();

    public static int InitializeConfig(String path)
    {
        try
        {
            Current = JsonSerializer.Deserialize<ConfigJSON>(File.ReadAllText(path))!;   
        }
        catch (Exception e)
        {
            Logger<Config>.Error("An error occured while initializing config.", e);
            return 6;
        }
        return 0;
    }

    // Create the Config which gets requested by SystemCheck.cs
    public static int CreateConfig(String path)
    {
        try
        {
            Dictionary<string, object> config = new Dictionary<string, object>()
            {
                {"SuspensionLengthsPerInstance", SUSPENSION_LENGTHS_PER_INSTANCE}
            };

            string configJsonString = JsonSerializer.Serialize(config);
            File.WriteAllText(path, configJsonString);

            Logger<Config>.Log("Created new Config File with default parameters.", LogLevel.Info);
        }
        catch (Exception e)
        {
            Logger<Config>.Error("Failed to create config file", e);
            return 5;
        }
        return 0;
    }
}