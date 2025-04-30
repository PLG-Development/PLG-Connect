using System.IO;
using Newtonsoft.Json;
using System;
using PLG_Connect_Network;
using System.Collections.Generic;


namespace PLG_Connect;


class SettingsManager
{
    public Settings Settings = new();
    private readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PLG-Development",
        "PLG-Connect",
        "config.json"
    );

    /// <summary>
    /// Loads the settings from the specified file path. Defaults to the standard settings path.
    /// </summary>
    public void Load(string? filePath = null)
    {
        filePath ??= SettingsPath;

        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(new Settings()));
        }

        string json = File.ReadAllText(filePath);
        Settings = JsonConvert.DeserializeObject<Settings>(json)!;
        Logger.Log("Settings loaded!");
    }

    /// <summary>
    /// Saves the settings to the specified file path. Defaults to the standard settings path.
    /// </summary>
    public void Save(string? filePath = null)
    {
        filePath ??= SettingsPath;
        string json = JsonConvert.SerializeObject(Settings);
        File.WriteAllText(filePath, json);

    }
}


public struct Settings
{
    public List<Display> Displays;
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


public class Display : PLGClient
{
    public string Name;
    public string IPAddress;
    public bool IsChecked = false;


    public Display(string name, string ipAddress, string macAddress, string password = "0") : base(ipAddress, macAddress, password)
    {
        Name = name;
        IPAddress = ipAddress;
        MacAddress = macAddress;
        Password = password;
    }
}
