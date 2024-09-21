using Avalonia.Controls;
using Avalonia;
using System.Text.RegularExpressions;
using System;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Linq;
using PLG_Connect_Network;


namespace PLG_Connect;


public partial class NewMonitorWindow : Window
{
    internal NewMonitorWindow(MainWindow mv)
    {
        InitializeComponent();

        mainWindow = mv;
    }

    public string DisplayName;
    public string DisplayIp;
    public string DisplayMac;
    private MainWindow mainWindow;

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void PingButtonClick(object sender, RoutedEventArgs e)
    {
        PLGClient client = new PLGClient(IpTextBox.Text, MacTextBox.Text, ""); // error
        bool success = await client.Ping();
    }

    private void TextBoxGotFocus(object sender, Avalonia.Input.GotFocusEventArgs e)
    {
        TextBox? tb = (sender as TextBox);
        if (tb == null)
        {
            return;
        }
    }

    private bool macValid = false;
    private void MacTextBoxTextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        string macAddressPattern = @"^([0-9A-Fa-f]{2}:){5}([0-9A-Fa-f]{2})$";
        if (!Regex.IsMatch(MacTextBox.Text, macAddressPattern))
        {
            MacLabel.Foreground = new SolidColorBrush(Color.Parse("#FF3333"));
            MacLabel.Content = "MAC - Invalid";

            macValid = false;
        }
        else
        {
            string theme = Application.Current.ActualThemeVariant.ToString();
            if (theme == "Light")
            {
                MacLabel.Foreground = new SolidColorBrush(Color.Parse("#000000"));
            }
            else if (theme == "Dark")
            {
                MacLabel.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
            }
            MacLabel.Content = "MAC";
            macValid = true;
            DisplayMac = MacTextBox.Text;

        }
        ButtonCheck();
    }

    private bool ipValid = false;
    private void IpTextBoxTextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        try
        {
            string[] parts = IpTextBox.Text.Split('.');
            if (parts.Length == 4)
            {
                foreach (string part in parts)
                {
                    if (0 >= Convert.ToInt32(part) && Convert.ToInt32(part) >= 255)
                    {
                        throw new Exception("PLG_Connect.ContainsInvalidNumberException");
                    }
                }
            }
            else
            {
                throw new Exception("PLG_Connect.WrongPartLengthException");
            }


            string theme = Application.Current.ActualThemeVariant.ToString();
            if (theme == "Light")
            {
                IpLabel.Foreground = new SolidColorBrush(Color.Parse("#000000"));
            }
            else if (theme == "Dark")
            {
                IpLabel.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
            }
            IpLabel.Content = "IP";
            ipValid = true;
            DisplayIp = IpTextBox.Text;

        }
        catch
        {
            IpLabel.Foreground = new SolidColorBrush(Color.Parse("#FF3333"));
            IpLabel.Content = "IP - Invalid";
            ipValid = false;

        }
        ButtonCheck();
    }

    public void ButtonCheck()
    {

        if (macValid && ipValid)
        {
            AddButton.IsEnabled = true;
            PingButton.IsEnabled = true;
            return;
        }

        AddButton.IsEnabled = false;
        PingButton.IsEnabled = false;

    }
    private void NameTextBoxTextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        DisplayName = NameTextBox.Text;
    }

    private void AddButtonClick(object sender, RoutedEventArgs e)
    {
        // Check if mac already exists
        if (mainWindow.Displays.Where(d => d.Settings.MacAddress == DisplayMac).Count() > 0)
        {
            PLG_Connect.MessageBox.Show(this, "This MAC-Address is already configured", "MAC-Adress already found");
            return;
        }

        mainWindow.Displays.Add(new Display(new DisplaySettings { IPAddress = DisplayIp, MacAddress = DisplayMac, Name = DisplayName }));
        mainWindow.RefreshGUI();

        Close();
    }
}
