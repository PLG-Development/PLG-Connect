using Avalonia.Controls;
using Avalonia;
using System.Text.RegularExpressions;
using System;
using Avalonia.Interactivity;
using Avalonia.Media;


namespace PLG_Connect;


public partial class NewMonitorWindow : Window
{
    public NewMonitorWindow()
    {
        InitializeComponent();
    }

    public string Name;
    public string IP;
    public string MAC;
    public DisplayCreationState CreationState;

    private void TextBoxGotFocus(object sender, Avalonia.Input.GotFocusEventArgs e)
    {
        TextBox? tb = (sender as TextBox);
        if (tb == null)
        {
            return;
        }

    }

    bool finished = false;

    private void AddButtonClick(object sender, RoutedEventArgs e)
    {

        CreationState = DisplayCreationState.Ready;
        Close();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        CreationState = DisplayCreationState.Cancelled;
        Close();
    }

    private void Window_Closing(object sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        if (!finished)
        {
            MainWindow.new_mon_canceled = true;
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
            MAC = MacTextBox.Text;

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
            IP = IpTextBox.Text;

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
            CreationState = DisplayCreationState.Ready;
            return;
        }

        AddButton.IsEnabled = false;

    }
    private void NameTextBoxTextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        Name = NameTextBox.Text;
    }
}

public enum DisplayCreationState
{
    Created,
    Cancelled,
    Failed,
    Ready
}
