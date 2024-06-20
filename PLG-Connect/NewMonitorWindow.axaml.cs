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


    private bool start_n = true;
    private bool start_ip = true;
    private bool start_mac = true;
    private void TextBox_GotFocus(object sender, Avalonia.Input.GotFocusEventArgs e)
    {
        TextBox? tb = (sender as TextBox);
        if (tb == null)
        {
            return;
        }
        if (start_n && tb.Text == "Enter name...")
        {
            tb.Text = "";
            start_n = false;
        }
    }

    public string Name;
    public string IP;
    public string MAC;
    public DisplayCreationState CreationState;

    private void TextBox_GotFocus_1(object sender, Avalonia.Input.GotFocusEventArgs e)
    {
        TextBox? tb = (sender as TextBox);
        if (tb == null)
        {
            return;
        }
        if (start_ip && tb.Text == "Enter IP...")
        {
            tb.Text = "";
            start_ip = false;
        }
    }
    private void TextBox_GotFocus_2(object sender, Avalonia.Input.GotFocusEventArgs e)
    {
        TextBox? tb = sender as TextBox;
        if (tb == null)
        {
            return;
        }
        if (start_mac && tb.Text == "Enter MAC...")
        {
            tb.Text = "";
            start_mac = false;
        }
    }
    bool finished = false;

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (!start_n && !start_ip && !start_mac)
        {
            //end
            Close();
        }
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closing(object sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        if (!finished)
        {
            MainWindow.new_mon_canceled = true;
        }
    }

    private bool mac_valid = false;
    private void TbMAC_TextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        string macAddressPattern = @"^([0-9A-Fa-f]{2}:){5}([0-9A-Fa-f]{2})$";
        if (!Regex.IsMatch(TbMAC.Text, macAddressPattern))
        {
            LblMac.Foreground = new SolidColorBrush(Color.Parse("#FF3333"));
            LblMac.Content = "MAC - Invalid";

            mac_valid = false;
        }
        else
        {
            string theme = Application.Current.ActualThemeVariant.ToString();
            if (theme == "Light")
            {
                LblMac.Foreground = new SolidColorBrush(Color.Parse("#000000"));
            }
            else if (theme == "Dark")
            {
                LblMac.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
            }
            LblMac.Content = "MAC";
            mac_valid = true;
            MAC = TbMAC.Text;

        }
        ButtonCheck();
    }

    private bool ip_valid = false;
    private void TbIP_TextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        try
        {
            string[] parts = TbIP.Text.Split('.');
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
                LblIP.Foreground = new SolidColorBrush(Color.Parse("#000000"));
            }
            else if (theme == "Dark")
            {
                LblIP.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
            }
            LblIP.Content = "IP";
            ip_valid = true;
            IP = TbIP.Text;

        }
        catch
        {
            LblIP.Foreground = new SolidColorBrush(Color.Parse("#FF3333"));
            LblIP.Content = "IP - Invalid";
            ip_valid = false;

        }
        ButtonCheck();
    }

    public void ButtonCheck()
    {

        if (mac_valid && ip_valid)
        {
            BtnAdd.IsEnabled = true;
            CreationState = DisplayCreationState.Ready;
            return;
        }

        BtnAdd.IsEnabled = false;

    }
    private void TbName_TextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        Name = TbName.Text;
    }
}

public enum DisplayCreationState
{
    Created,
    Cancelled,
    Failed,
    Ready
}
