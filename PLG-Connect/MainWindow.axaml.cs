using System.Collections.Generic;
using Avalonia.Controls;
using PLG_Connect_Network;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Input;
using System;
using Avalonia.Media;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Threading;


namespace PLG_Connect;


partial class MainWindow : Window
{
    public SettingsManager SettingsManager = new();
    public MainWindow()
    {
        InitializeComponent();
        Logger.Log("Welcome to PLG Connect!");
        Logger.Log("Starting up...");
        this.KeyDown += HandleKeyboardKeyDown;
        Task.Run(async () => await Analytics.SendEvent("connect"));

        SettingsManager.Load();
        RefreshGUI();
        Logger.Log("GUI initialized!");
    }

    // SaveAs
    public void MenuSaveSettingsAsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(OpenSettingsSaveAsDialog);
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

            SettingsManager.Save(settingsSavePath);
            Logger.Log("Settings saved at " + settingsSavePath);
        }
        catch (Exception ex)
        {
            Logger.Log("Saving file not successfull: " + ex.Message, Logger.LogType.Error);
            await MessageBox.Show(this, ex.Message, "Fehler beim Speichern der Datei");
        }
    }

    // Load
    public void MenuLoadSettingsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(OpenSettingsLoadDialog);
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
            Dispatcher.UIThread.InvokeAsync(OpenSettingsLoadDialog);
            return;
        }
    }

    private void MenuAddMonitorClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddMonitor();
    }
    private void AddMonitorClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddMonitor();
    }
    private void AddMonitor()
    {
        NewMonitorWindow w = new NewMonitorWindow(this);
        w.Show();
    }

    private void MenuClearAllMonitorsClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display d in SettingsManager.Settings.Displays)
        {
            d.DisplayText("");
        }

        Logger.Log("Cleared all monitors");
    }

    private void MenuPreferencesClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        new WndPreferences().Show();
    }

    private void MenuHelpOnlineClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.plg-berlin.de/technik/plg_connect",
            UseShellExecute = true
        });
    }

    private void MenuGithubClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/PLG-Development/PLG-Connect",
            UseShellExecute = true
        });
    }

    private void MenuHomepageClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.plg-berlin.de/technik/plg_connect",
            UseShellExecute = true
        });
    }

    private async void BtnPreviousClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            await display.PreviousSlide();
        }
    }

    private async void BtnNextClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            await display.NextSlide();
        }
    }

    private async void BtnRunCommandClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            await display.RunCommand(CommandInput.Text);
        }
    }

    private async void BtnDisplayTextClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            await display.DisplayText(DisplayTextTextInput.Text);
        }
    }

    private async void BtnToggleBlackscreenClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            await display.ToggleBlackScreen();
        }
    }

    private async void BtnDeleteClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var res = await MessageBox.Show(this, "Do you really want to delete this display? Deleting a display by clicking the delete-button will delete\nit permanentely and non-recoverable.", "Delete this display?", MessageBoxButton.YesNo);
        if (res == MessageBoxResult.No) return;

        var displaysToRemove = new List<Display>();
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (display.IsChecked) displaysToRemove.Add(display);
        }
        foreach (Display display in displaysToRemove)
        {
            SettingsManager.Settings.Displays.Remove(display);
        }
        RefreshGUI();
    }

    private async void BtnOpenFileClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Console.WriteLine("Opening file dialog");
    }

    ///<summary>
    /// Goes through every single instance of displays and mobile clients and updates their appearance in the window
    ///</summary>
    public void RefreshGUI()
    {
        SettingsManager.Save();

        UIDisplays.Children.Clear();

        foreach (Display display in SettingsManager.Settings.Displays)
        {
            CheckBox selectedCheckBox = new()
            {
                IsChecked = display.IsChecked,
                Margin = new Thickness(5)
            };
            selectedCheckBox.IsCheckedChanged += (object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                display.IsChecked = selectedCheckBox.IsChecked ?? false;
            };

            StackPanel displayControllElementLeft = new()
            {
                Children = {
                    selectedCheckBox
                }
            };

            StackPanel displayControlElementCenter = new()
            {
                Children = {
                    new Label() { Content = display.IPAddress + " - " + display.Name, Margin= new Thickness(5), FontWeight = FontWeight.Bold },
                }
            };

            Grid displayControllElement = new Grid()
            {
                Margin = new Thickness(5),
                Children = {
                        displayControllElementLeft,
                        displayControlElementCenter,
                    },
                Background = new SolidColorBrush(Color.Parse("#545457"))
            };

            displayControllElement.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            displayControllElement.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            displayControllElement.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            Grid.SetColumn(displayControllElementLeft, 0);
            Grid.SetColumn(displayControlElementCenter, 1);

            UIDisplays.Children.Add(displayControllElement);
        }

        Logger.Log("Successfully refreshed GUI");
    }
}
