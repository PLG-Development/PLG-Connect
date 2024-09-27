using System.Collections.Generic;
using Avalonia.Controls;
using System.IO;
using PLG_Connect_Network;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Input;
using System;
using Avalonia.Media;
using System.Threading.Tasks;
using System.Diagnostics;
using PLG_Connect.Config;


namespace PLG_Connect;


partial class MainWindow : Window
{
    public List<Display> Displays = new();

    public Config.Settings Settings;
    public string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PLG-Development",
        "PLG-Connect",
        "config.json"
    );
    public MainWindow()
    {
        InitializeComponent();
        this.KeyDown += HandleKeyboardKeyDown;
        Settings = Config.Config.Load(SettingsPath);
        Task.Run(async () => await Analytics.SendEvent("connect"));
    }

    // SaveAs
    public void MenuOpenSettingsSaveAsDialog(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Task.Run(async () => await OpenSettingsSaveAsDialog());
    }
    private async Task OpenSettingsSaveAsDialog()
    {
        var settingsFileName = "PLG-Connect-Project";

        try
        {

            var filePicker = new SaveFileDialog
            {
                Title = "Save file...",
                InitialFileName = $"{settingsFileName}.pcnt",
                DefaultExtension = ".pcnt",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "PLG-Connect-Projects", Extensions = { "pcnt" } }
                }
            };

            var settingsSavePath = await filePicker.ShowAsync(this);
            if (settingsSavePath == null) { return; }

            Config.Config.Save(Settings, settingsSavePath);
        }
        catch (Exception ex)
        {
            await MessageBox.Show(this, ex.Message, "Fehler beim Speichern der Datei");
        }
    }

    // Load
    public void MenuOpenSettingsLoadDialog(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Task.Run(async () => await OpenSettingsLoadDialog());
    }
    public async Task OpenSettingsLoadDialog()
    {
        await MessageBox.Show(this, "Macht Nichts", "Macht Nichts");
    }

    public void HandleKeyboardKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.KeyModifiers == KeyModifiers.Shift && e.Key == Key.S)
        {
            Task.Run(async () => await OpenSettingsSaveAsDialog());
            return;
        }

        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.O)
        {
            OpenSettingsLoadDialog();
            return;
        }
    }

    // global internal properties
    private bool isSaved = true; // updates the saved-status of the current project, e.g. adding a new monitor
    private string filepath = null; // filepath of the currently loaded project, e.g. /home/tag/connect-projects/aula-main.pcnt
    private static string projectFileExtension = "pcnt"; // file extension for project-files (currently: pcnt - Plg CoNnecT)
    // Menu Structure variables
    private bool delete = false;

    private void Mnu_File_Open_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OpenSettingsLoadDialog();
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
        new WndPreferences().Show();
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
        Config.Config.Save(Settings, SettingsPath);

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

            if (disp.isLocked)
            {
                TextBox TbContent2 = new TextBox();
                TbContent2.Margin = new Thickness(5);
                Button ba = new Button();
                ba.Margin = new Thickness(5);
                ba.Content = "Unlock";
                ba.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
                {
                    if (new WndEnterPassword(disp.Password).ShowDialogWithResult(this).Result == true)
                    {
                        disp.isLocked = false;
                        RefreshGUI();
                    }

                };

                Button ba6 = new Button();
                ba6.Margin = new Thickness(5);
                ba6.Content = "View log";
                ba6.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
                {
                    new WndLog(disp.Messages).Show();
                };

                StackPanel buttons2 = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Children = {
                        ba,
                        ba6,

                    }
                };
                StackPanel p2 = new StackPanel()
                {

                    Margin = new Thickness(5),
                    Children = {
                        new Label() { Content = "Name: " + disp.Settings.Name },
                        new Label() { Content = disp.Settings.IPAddress },
                        TbContent2,
                        buttons2,
                        new Label() { Content = disp.Messages },
                    },
                    Background = new SolidColorBrush(Color.Parse("#545457"))
                };


                StpScreens.Children.Add(p2);

                continue;
            }

            TextBox TbContent = new TextBox();
            TbContent.Margin = new Thickness(5);
            Button b = new Button();
            b.Margin = new Thickness(5);
            b.Content = "Display Text";
            b.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                try
                {
                    await disp.DisplayText(TbContent.Text);
                    disp.Messages += "\n\n" + DateTime.Now + " - Displayed text on screen: " + TbContent.Text;
                    TbContent.Text = "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            };
            Button b2 = new Button();
            b2.Margin = new Thickness(5);
            b2.Content = "Next Image";
            b2.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {

                try
                {
                    await disp.NextSlide();
                    disp.Messages += "\n\n" + DateTime.Now + " - Image: next";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };

            Button b3 = new Button();
            b3.Margin = new Thickness(5);
            b3.Content = "Prevoius Image";
            b3.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                try
                {
                    await disp.PreviousSlide();
                    disp.Messages += "\n\n" + DateTime.Now + " - Image: previous";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            };

            Button b4 = new Button();
            b4.Margin = new Thickness(5);
            b4.Content = "Blackout";
            bool blackout = true; // Check if blackout is active or not
            if (blackout)
            {
                b4.Background = new SolidColorBrush(Color.Parse("#772327"));
            }
            else
            {
                b4.Background = new SolidColorBrush(Color.Parse("#545458"));
            }
            b4.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                try
                {
                    await disp.ToggleBlackScreen();
                    disp.Messages += "\n\n" + DateTime.Now + " - Toggled Blackout";
                    bool blackout = true; // = true ?? "black" || false; (Keine Ahnung, wie die Syntax exakt funktioniert)

                    if (blackout)
                    {
                        (sender as Button).Background = new SolidColorBrush(Color.Parse("#772327"));
                    }
                    else
                    {
                        (sender as Button).Background = new SolidColorBrush(Color.Parse("#545458"));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            };

            Button b5 = new Button();
            b5.Margin = new Thickness(5);
            b5.Content = "Load Content";
            b5.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                new WndSelectFileType(disp).Show();
            };

            Button b6 = new Button();
            b6.Margin = new Thickness(5);
            b6.Content = "View log";
            b6.Click += async (object sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                new WndLog(disp.Messages).Show();
            };

            StackPanel buttons = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    b,
                    b2,
                    b3,
                    b4,
                    b5,
                    b6,
                }
            };
            StackPanel p = new StackPanel()
            {

                Margin = new Thickness(5),
                Children = {
                    new Label() { Content = "Name: " + disp.Settings.Name },
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
                foreach (Display d in Displays)
                {
                    if (d.Address == disp.Address)
                    {
                        Displays.Remove(d);
                        RefreshGUI();
                        break;
                    }
                }
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
                    new Label() { Content = disp.Settings.IPAddress },
                    buttons,
                },
                Background = new SolidColorBrush(Color.Parse("#545457"))
            };


            StpScreens.Children.Add(p);
        }
    }

}

public class Display : PLGClient
{
    public Config.DisplaySettings Settings;
    public string Messages;
    public bool isLocked = false;

    public Display(Config.DisplaySettings settings) : base(settings.IPAddress, settings.MacAddress, settings.Password)
    {
        Settings = settings;
    }
}
