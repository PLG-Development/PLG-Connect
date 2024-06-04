using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PLG_Connect;

public partial class WndNewMonitor : Window
{
    public WndNewMonitor()
    {
        InitializeComponent();
    }


    public bool start_n = true;
    public bool start_ip = true;
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
    bool finished = false;

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (!start_n && !start_ip)
        {
            MainWindow.new_mon_temp_ip = TbIP.Text;
            MainWindow.new_mon_temp_name = TbName.Text;
            MainWindow.new_mon_canceled = false;
            finished = true;
        }


        Close();
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

    private void TextBox_TextChanged(object sender, Avalonia.Controls.TextChangedEventArgs e)
    {

    }
}