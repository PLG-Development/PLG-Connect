using System.Collections.Generic;
using Avalonia.Controls;
using System.Text.Json;
using System.IO;
using PLG_Connect_Network;
using System;


namespace PLG_Connect;


partial class MainWindow : Window
{
    private List<Display> Displays = new();

    public MainWindow()
    {
        InitializeComponent();

        ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PLG-Connect",
            "config.json"
        );
        LoadConfig();
    }

    // New Monitor static variables just let them here
    public static string new_mon_temp_name = "";
    public static string new_mon_temp_ip = "";
    public static string new_mon_temp_mac = "";
    public static bool new_mon_canceled = false;

    private string ConfigPath;

    private void SaveConfig()
    {
        string json = JsonSerializer.Serialize(Displays);
        File.WriteAllText(ConfigPath, json);
    }

    private void LoadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            File.WriteAllText(ConfigPath, "[]");
        }

        string json = File.ReadAllText(ConfigPath);
        Displays = JsonSerializer.Deserialize<List<Display>>(json)!;
    }
}

class Display : ClientConnection
{
    public required DisplaySettings Settings;

    public Display(DisplaySettings settings) : base(settings.IPAddress, settings.MacAddress) { }
}


struct DisplaySettings
{
    public string Name;
    public string IPAddress;
    public string MacAddress;
}
