using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System.IO;
using PLG_Connect_Network;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup;
using System;
using System.Linq;
using Avalonia.Media;
using System.Diagnostics;
using PLG_Connect;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;


namespace PLG_Connect;


partial class MainWindow : Window
{
    public List<Display> Displays = new();

    public MainWindow()
    {

        // Debuggers.Launch();
        InitializeComponent();
        this.KeyDown += Handle_Keyboard_KeyDown;

        // ConfigPath is just for Settings
        ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PLG Development",
            "PLG Connect",
            "config.json"
        );
        LoadConfig();
    }

    public void Handle_Keyboard_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.KeyModifiers == KeyModifiers.Shift && e.Key == Key.S)
        {
            SaveAs();
            return;
        }

        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S)
        {
            Save();
            return;
        }

        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.O)
        {
            Open();
            return;
        }

        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.N)
        {
            New();
            return;
        }
    }

    private string ConfigPath;

    private void SaveConfig(string path)
    {
        DisplaySettings[] settings = Displays.Select(d => d.Settings).ToArray();
        string json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(path, json);
    }

    private void LoadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            File.WriteAllText(ConfigPath, "[]");
        }

        string json = File.ReadAllText(ConfigPath);
        //Displays = JsonSerializer.Deserialize<List<Display>>(json)!;
        //Settings-Class needed
    }
    // global internal properties
    private bool isSaved = true; // updates the saved-status of the current project, e.g. adding a new monitor
    private string filepath = null; // filepath of the currently loaded project, e.g. /home/tag/connect-projects/aula-main.pcnt
    private static string projectFileExtension = "pcnt"; // file extension for project-files (currently: pcnt - Plg CoNnecT)
    // Menu Structure variables
    private bool delete = false;

    private void Mnu_File_Exit_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    private void Mnu_File_New_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        New();
    }

    private void Mnu_File_Open_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Open();
    }

    private void Mnu_File_Save_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!Save())
        {
            MessageBox.Show(this, "Error while saving file", "Error", MessageBoxButton.Ok);
        }
    }

    private void Mnu_File_SaveAs_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SaveAs();
    }

    private void Mnu_Edit_AddMonitor_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddMonitor();
    }

    private async void Mnu_Edit_DeleteMonitor_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (delete == true)
        {
            delete = false;
            Mnu_Edit_DeleteMonitor.Header = "Activate Deletion Mode";
            BrdMonitors.Background = new SolidColorBrush(Color.Parse("#232327"));
        }
        else
        {
            var res = await MessageBox.Show(this, "Do you really want to activate deletion mode? Deleting a monitor by clicking the delete-button will delete\nit permanentely and non-recoverable.", "Turn on deletion-mode?", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                delete = true;
                Mnu_Edit_DeleteMonitor.Header = "Deactivate Deletion Mode";
                BrdMonitors.Background = new SolidColorBrush(Color.Parse("#552327"));

            }
        }
        RefreshGUI();
    }

    private void Mnu_Edit_ClearAllMonitors_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display d in Displays)
        {
            d.DisplayText("");
        }
    }

    private void Mnu_Edit_ClearAllDevices_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // remove all mobile camera-devices
    }

    private void Mnu_Edit_Preferences_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

    }

    private void Mnu_Help_Online_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.plg-berlin.de/technik/plg_connect",
            UseShellExecute = true
        });
    }

    private void Mnu_Help_Github_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/PLG-Development/PLG-Connect",
            UseShellExecute = true
        });
    }

    private void Mnu_Help_Homepage_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.plg-berlin.de/technik/plg_connect",
            UseShellExecute = true
        });
    }

    private void Mnu_Help_Updates_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

    }

    private void Mnu_Help_Info_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

    }

    private void BtnAddNewMonitor_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddMonitor();
    }

    private void New()
    {
        if (isSaved)
        {
            // create new
        }
        else
        {
            // Request answer: Save?
        }
    }

    private void Load(string path)
    {
        try
        {
            //string result = File.ReadAllText(path);
            //Displays = JsonSerializer.Deserialize<List<Display>>(result);



            string json = File.ReadAllText(ConfigPath);
            Displays = JsonConvert.DeserializeObject<List<Display>>(json)!;
            RefreshGUI();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error while reading");
        }
    }

    private async void Open()
    {
        try
        {

            var filePicker = new OpenFileDialog
            {
                Title = "Open file...",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "PLG-Connect-Projects", Extensions = { "pcnt" } }
                }
            };

            var result = await filePicker.ShowAsync(this);

            if (result != null)
            {
                filepath = result[0];
                Load(filepath);
            }
        }
        catch
        {

        }
    }
    private bool Save()
    {
        if (filepath != null)
        {
            try
            {
                SaveConfig(filepath);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error while writing");
            }
        }
        else
        {
            SaveAs(true);
            return true;
        }
        return false;
    }
    public async void SaveAs(bool fromsave = false)
    {
        try
        {
            string path, name;
            path = Path.GetDirectoryName(filepath);
            if (fromsave)
            {
                name = "New PLG-Connect Project";
            }
            else
            {
                name = Path.GetFileNameWithoutExtension(filepath) + "-copy";
            }



            var filePicker = new SaveFileDialog
            {
                Title = "Save file...",
                InitialFileName = name + ".pcnt",
                DefaultExtension = ".pcnt",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "PLG-Connect-Projects", Extensions = { "pcnt" } }
                }
            };

            var result = await filePicker.ShowAsync(this);

            if (result != null)
            {
                filepath = result;
                Save();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern der Datei: {ex.Message}");
            //return false;
        }
    }

    private void AddMonitor()
    {
        NewMonitorWindow w = new NewMonitorWindow(this);
        w.Show();
    }

    ///<summary>
    /// Goes through every single instance of displays and mobile clients and updates their appearance in the window
    ///</summary>
    public void RefreshGUI()
    {
        if (delete)
        {
            RefreshDisplaysDeletion();
        }
        else
        {
            RefreshDisplaysDefault();
        }

    }

    public void RefreshDisplaysDefault()
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
            b.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                await disp.DisplayText(TbContent.Text);
                disp.Messages = DateTime.Now + " - Displayed text on screen: " + TbContent.Text + "\n\n" + disp.Messages;
                TbContent.Text = "";
            };
            Button b2 = new Button();
            b2.Margin = new Thickness(5);
            b2.Content = "Next Image";
            b2.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                await disp.NextSlide();
                disp.Messages = DateTime.Now + " - Image: next" + "\n\n" + disp.Messages;
            };

            Button b3 = new Button();
            b3.Margin = new Thickness(5);
            b3.Content = "Prevoius Image";
            b3.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                await disp.PreviousSlide();
                disp.Messages = DateTime.Now + " - Image: previous" + "\n\n" + disp.Messages;
            };

            Button b4 = new Button();
            b4.Margin = new Thickness(5);
            b4.Content = "Blackout";
            b4.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                await disp.ToggleBlackScreen();
                disp.Messages = DateTime.Now + " - Toggled Blackout" + "\n\n" + disp.Messages;
            };
            StackPanel buttons = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    b,
                    b2,
                    b3,
                    b4,
                }
            };
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

    public void RefreshDisplaysDeletion()
    {
        var bc = new BrushConverter();
        StpScreens.Children.Clear();
        foreach (Display disp in Displays)
        {

            Button b2 = new Button();
            b2.Margin = new Thickness(5);
            b2.Content = "Delete";
            b2.Background = new SolidColorBrush(Color.Parse("#772327"));
            b2.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                // Deletion logic
            };

            StackPanel buttons = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    b2,

                }
            };
            StackPanel p = new StackPanel()
            {

                Margin = new Thickness(5),
                Children = {
                    new Label() { Content = "Name: " + disp.Settings.Name },
                    new Label() { Content = disp.Current_Mode },
                    new Label() { Content = disp.Settings.IPAddress },
                    buttons,
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

enum DisplayMode
{
    None,
    Text,
    Image,
    Combined,
    External,
    Slideshow,
    Animation
}
