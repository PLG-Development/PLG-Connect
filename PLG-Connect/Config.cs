using System.IO;
using Newtonsoft.Json;


namespace PLG_Connect.Config;


static class Config
{
    public static Settings Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(new Settings()));
        }

        string json = File.ReadAllText(filePath);
        Settings settings = JsonConvert.DeserializeObject<Settings>(json)!;
        return settings;
    }

    public static void Save(Settings settings, string filePath)
    {
        string json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(filePath, json);
    }
}

public struct Settings
{
    public DisplaySettings[] Displays;
    public Themes Theme;
    public string Language;

    public Settings()
    {
        Displays = [];
        Theme = Themes.Auto;
        Language = "en";
    }
}

public enum Themes { Auto, Light, Dark };


public struct DisplaySettings
{
    public string Name;
    public string IPAddress;
    public string MacAddress;
    public string Password;

    public DisplaySettings()
    {
        Name = "New Display";
        IPAddress = "127.0.0.1";
        MacAddress = "00:00:00:00:00:00";
        Password = "0";
    }
}
