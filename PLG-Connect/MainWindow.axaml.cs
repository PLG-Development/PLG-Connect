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
using Avalonia.Platform.Storage;


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

    public void HandleKeyboardKeyDown(object? sender, KeyEventArgs e)
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
            try { await display.PreviousSlide(); }
            catch (Exception ex)
            {
                Logger.Log($"Could not go to previous slide on {display.Name}: {ex.Message}", Logger.LogType.Error);
                await MessageBox.Show(this, $"Could not go to previous slide on {display.Name}", "Error");
            }
        }

        RefreshGUI();
    }

    private async void BtnNextClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            try { await display.NextSlide(); }
            catch (Exception ex)
            {
                Logger.Log($"Could not go to next slide on {display.Name}: {ex.Message}", Logger.LogType.Error);
                await MessageBox.Show(this, $"Could not go to next slide on {display.Name}", "Error");
            }
        }

        RefreshGUI();
    }

    private async void BtnRunCommandClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            try { await display.RunCommand(CommandInput.Text); }
            catch (Exception ex)
            {
                Logger.Log($"Could not run command on {display.Name}: {ex.Message}", Logger.LogType.Error);
                await MessageBox.Show(this, $"Could not run command on {display.Name}", "Error");
            }
        }

        RefreshGUI();
    }

    private async void BtnDisplayTextClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            try { await display.DisplayText(DisplayTextTextInput.Text); }
            catch (Exception ex)
            {
                Logger.Log($"Could not display text on {display.Name}: {ex.Message}", Logger.LogType.Error);
                await MessageBox.Show(this, $"Could not display text on {display.Name}", "Error");
            }
        }

        RefreshGUI();
    }

    private async void BtnToggleBlackscreenClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            try { await display.ToggleBlackScreen(); }
            catch (Exception ex)
            {
                Logger.Log($"Could not toggle blackscreen on {display.Name}: {ex.Message}", Logger.LogType.Error);
                await MessageBox.Show(this, $"Could not toggle blackscreen on {display.Name}", "Error");
            }
        }

        RefreshGUI();
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
        var topLevel = TopLevel.GetTopLevel(this);
        var file = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select File To Upload To Displays",
            AllowMultiple = false,
            FileTypeFilter = new[] {new FilePickerFileType("Images & Presentations") {
                // Patterns = new[] {"*.png", "*.jpg", "*.pptx"},
                Patterns = new[] {"*"},
            }}
        });

        if (file == null) return;

        string filePath = file[0].Path.LocalPath;

        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            try { await display.OpenFile(filePath); }
            catch (Exception ex)
            {
                Logger.Log($"Could not open file {filePath} on {display.Name}: {ex.Message}", Logger.LogType.Error);
                await MessageBox.Show(this, $"Could not open file on {display.Name}", "Error");
            }
        }

        RefreshGUI();
    }

    private void BtnPowerOn(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            try { display.SendWakeOnLAN(); }
            catch (Exception ex)
            {
                Logger.Log($"Could not power on {display.Name}: {ex.Message}", Logger.LogType.Error);
                MessageBox.Show(this, $"Could not power on {display.Name}", "Error");
            }
        }

        RefreshGUI();
    }

    private async void BtnPowerOff(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        foreach (Display display in SettingsManager.Settings.Displays)
        {
            if (!display.IsChecked) continue;
            try { await display.Shutdown(); }
            catch (Exception ex)
            {
                Logger.Log($"Could not power of {display.Name}: {ex.Message}", Logger.LogType.Error);
                await MessageBox.Show(this, $"Could not power of {display.Name}", "Error");
            }
        }

        RefreshGUI();
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
            CheckBox checkbox = new() { IsChecked = display.IsChecked };
            DockPanel.SetDock(checkbox, Dock.Left);
            checkbox.IsCheckedChanged += (object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                display.IsChecked = checkbox.IsChecked.Value;
                SettingsManager.Save();
            };

            StackPanel title = new()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children = {
                    new TextBlock() { Text = display.Name, FontWeight = FontWeight.Bold },
                    new TextBlock() { Text = $" ({display.IPAddress})"}
                }
            };
            DockPanel.SetDock(title, Dock.Left);

            StackPanel status = new()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 4,
                Children = {
                    new TextBlock() { Text = display.LastSuccessfulAction.ToString(), FontStyle = FontStyle.Italic },
                    new TextBlock() { Text = display.ShowsBlackScreen ? "⬛" : "🟦"},
                }
            };
            DockPanel.SetDock(status, Dock.Right);

            Border uiDisplay = new()
            {
                BorderBrush = new SolidColorBrush(Color.Parse("#545457")),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4, 4, 4, 4),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                Child = new DockPanel()
                {
                    LastChildFill = false,
                    Children = { checkbox, title, status }
                }
            };

            UIDisplays.Children.Add(uiDisplay);
        }

        Logger.Log("Successfully refreshed GUI");
    }
}
