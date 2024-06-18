using System.Collections.Generic;
using Avalonia.Controls;
using System.Text.Json;
using System.IO;
using PLG_Connect_Network;
using System;
using System.Linq;


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
        DisplaySettings[] settings = Displays.Select(d => d.Settings).ToArray();
        string json = JsonSerializer.Serialize(settings);
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

    private void BtnAddNewMonitor_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WndNewMonitor w = new WndNewMonitor();
        w.Closing += AddNewMonitor;
        w.Show();
    }

    private void AddNewMonitor(object sender, WindowClosingEventArgs e)
    {
        WndNewMonitor output = sender as WndNewMonitor;
        if (output != null)
        {
            if (output.CreationState == DisplayCreationState.Ready)
            {
                foreach (Display disp in Displays)
                {
                    if (disp.MacAddress == output.MAC)
                    {
                        output.CreationState = DisplayCreationState.Failed;
                        throw new Exception("PLG_Connect.MacAlreadyExistsException");
                    }
                }
                Displays.Add(new Display(new DisplaySettings() { Name = output.Name, IPAddress = output.IP, MacAddress = output.MAC }));
            }
        }

        RefreshGUI();
    }

    ///<summary>
    /// Goes through every single instance of displays and mobile clients and updates their appearance in the window
    ///</summary>
    public void RefreshGUI()
    {
        foreach (Display disp in Displays)
        {

        }
    }
}

class Display : ClientConnection
{
    public DisplaySettings Settings;

    public Display(DisplaySettings settings) : base(settings.IPAddress, settings.MacAddress, settings.Password)
    {
        Settings = settings;
    }
}


struct DisplaySettings
{
    public string Name;
    public string IPAddress;
    public string MacAddress;
    public string Password;
}
