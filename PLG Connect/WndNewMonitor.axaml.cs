using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;
using PLG_Connect_Network;
using System;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;



namespace PLG_Connect;

public partial class WndNewMonitor : Window
{
    public WndNewMonitor()
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

    private void TbMAC_TextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {
        string macAddressPattern = @"^([0-9A-Fa-f]{2}:){5}([0-9A-Fa-f]{2})$";
        if (!Regex.IsMatch(TbMAC.Text, macAddressPattern))
        {
            LblMac.Foreground = new SolidColorBrush(Color.Parse("#FF3333"));
            LblMac.Content = "MAC - Invalid";
        } else {
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
            
        }
    }
    private void TbIP_TextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {

    }
    private void TbName_TextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {

    }
}

public enum DisplayCreationState{
    Created,
    Cancelled,
    Failed
}