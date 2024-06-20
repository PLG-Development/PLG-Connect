using System.Collections.Generic;
using Avalonia.Controls;
using System.Text.Json;
using System.IO;
using PLG_Connect_Network;
using Avalonia.Layout;
using Avalonia.Markup;
using System;
using System.Linq;
using Avalonia;
using Avalonia.Media;


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
        var bc = new BrushConverter();
        StpScreens.Children.Clear();
        foreach (Display disp in Displays)
        {
            TextBox TbContent = new TextBox();
            TbContent.Margin = new Thickness(5);
            Button b = new Button();
            b.Margin = new Thickness(5);
            b.Content = "Display Text";
            b.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) => {
                await disp.DisplayText(TbContent.Text);
                disp.Messages = DateTime.Now + " - Displayed text on screen: " + TbContent.Text + "\n\n" + disp.Messages;
                TbContent.Text = "";
            };
            Button b2 = new Button();
            b2.Margin = new Thickness(5);
            b2.Content = "Next Image";
            b2.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) => {
                await disp.NextSlide();
                disp.Messages = DateTime.Now + " - Image: next" + "\n\n" + disp.Messages;
            };
            
            Button b3 = new Button();
            b3.Margin = new Thickness(5);
            b3.Content = "Prevoius Image";
            b3.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) => {
                await disp.PreviousSlide();
                disp.Messages = DateTime.Now + " - Image: previous" + "\n\n" + disp.Messages;
            };
            
            Button b4 = new Button();
            b4.Margin = new Thickness(5);
            b4.Content = "Blackout";
            b4.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) => {
                await disp.ToggleBlackScreen();
                disp.Messages = DateTime.Now + " - Toggled Blackout" + "\n\n" + disp.Messages;
            };
            StackPanel buttons = new StackPanel(){
                Orientation = Orientation.Horizontal,
                Children = {
                    b,
                    b2,
                    b3,
                    b4,
                }
            }
            StackPanel p = new StackPanel()
            {

                Margin = new Thickness(5),
                Children = {
                    new Label() { Content = "Name: " + disp.Settings.Name },
                    new Label() { Content = disp.Current_Mode },
                    new Label() { Content = disp.Settings.IPAddress },
                    TbContent,
                    buttons,
                    new Label() { Content = disp.Messages },
                },
                Background = new SolidColorBrush(Color.Parse("#545457"))
            };
            

            StpScreens.Children.Add(p);
        }
    }

}

class Display : ClientConnection
{
    public DisplaySettings Settings;
    public string Messages;
    public DisplayMode Current_Mode;

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

enum DisplayMode{
    None,
    Text,
    Image,
    Combined,
    External,
    Slideshow,
    Animation
}
