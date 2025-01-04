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

    ///<summary>
    /// Goes through every single instance of displays and mobile clients and updates their appearance in the window
    ///</summary>
    public void RefreshGUI()
    {
        SettingsManager.Save();

        UIDisplays.Children.Clear();

        foreach (Display display in SettingsManager.Settings.Displays)
        {
            Button openFileButton = new Button()
            {
                Margin = new Thickness(5),
                Content = "Open File",
            };
            openFileButton.Click += (object? sender, Avalonia.Interactivity.RoutedEventArgs e) => { new WndSelectFileType(display).Show(); };

            Button toggleBlackscreenButton = new Button()
            {
                Margin = new Thickness(5),
                Content = "Toggle Blackscreen",
            };
            toggleBlackscreenButton.Click += async (object? sender, Avalonia.Interactivity.RoutedEventArgs e) => { await display.ToggleBlackScreen(); };

            //
            // Blackscreen Toggled Information
            //
            //if(Blackscreen toggled on){
                toggleBlackscreenButton.Background = new SolidColorBrush(Color.FromRgb(86,35,39));
            //} 




            Button previousButton = new Button()
            {
                Margin = new Thickness(5),
                Content = "Previous",
            };
            previousButton.Click += async (object? sender, Avalonia.Interactivity.RoutedEventArgs e) => { await display.PreviousSlide(); };

            Button nextButton = new Button()
            {
                Margin = new Thickness(5),
                Content = "Next",
            };
            nextButton.Click += async (object? sender, Avalonia.Interactivity.RoutedEventArgs e) => { await display.NextSlide(); };


            // Please move this Button to the context menu (via [...]-Button)
            Button deleteDisplayButton = new Button()
            {
                Margin = new Thickness(5),
                Content = "Delete This Display",
                // make button red
                Background = new SolidColorBrush(Color.Parse("#FF3333"))
            };
            deleteDisplayButton.Click += async (object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
            {
                var res = await MessageBox.Show(this, "Do you really want to delete this display? Deleting a display by clicking the delete-button will delete\nit permanentely and non-recoverable.", "Delete this display?", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    SettingsManager.Settings.Displays.Remove(display);
                    RefreshGUI();
                }
            };

            TextBox displayTextTextInput = new TextBox(){
                Margin=new Thickness(5,0,5,5),
                Watermark="Visible Text"
            };
            Button displayTextButton = new Button()
            {
                Margin = new Thickness(5),
                Content = "Display Text",
            };
            displayTextButton.Click += async (object? sender, Avalonia.Interactivity.RoutedEventArgs e) => { await display.DisplayText(displayTextTextInput.Text ?? ""); };

            StackPanel buttons = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    toggleBlackscreenButton,
                    displayTextButton,
                    openFileButton,
                    previousButton,
                    nextButton,
                }
            };

            CheckBox cbSelected = new CheckBox(){
                IsChecked = display.IsChecked,
                Margin = new Thickness(5)
            };

            cbSelected.Checked += async (object? sender, Avalonia.Interactivity.RoutedEventArgs e) => {
                display.IsChecked = true;
            };

            cbSelected.Unchecked += async (object? sender, Avalonia.Interactivity.RoutedEventArgs e) => {
                display.IsChecked = false;
            };


            StackPanel displayControllElementLeft = new StackPanel(){
                Children = {
                    cbSelected
                }
            };


            Button moreOptionsButton = new Button()
            {
                Margin = new Thickness(5),
                Content = "...",
            };
            moreOptionsButton.Click += async (object? sender, Avalonia.Interactivity.RoutedEventArgs e) => { 
                // Open Menu (not implemented)
            };


            StackPanel displayControllElementCenter = new StackPanel(){
                Children = {
                    new Label() { Content = display.IPAddress + " - " + display.Name, Margin= new Thickness(5), FontWeight = FontWeight.Bold },
                        buttons,
                        displayTextTextInput,
                }
            };

            StackPanel displayControllElementRight = new StackPanel(){
                Children={
                    moreOptionsButton
                }
            };


            Grid displayControllElement = new Grid()
            {
                Margin = new Thickness(5),
                Children = {
                        displayControllElementLeft,
                        displayControllElementCenter,
                        displayControllElementRight,
                    },
                Background = new SolidColorBrush(Color.Parse("#545457"))
            };

            displayControllElement.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            displayControllElement.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            displayControllElement.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            Grid.SetColumn(displayControllElementLeft, 0);
            Grid.SetColumn(displayControllElementCenter, 1);
            Grid.SetColumn(displayControllElementRight, 2);

            UIDisplays.Children.Add(displayControllElement);
        }

        Logger.Log("Successfully refreshed GUI");
    }
}
