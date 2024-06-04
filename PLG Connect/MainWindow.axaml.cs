using Avalonia.Controls;


namespace PLG_Connect;


public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // New Monitor static variables just let them here
    public static string new_mon_temp_name = "";
    public static string new_mon_temp_ip = "";
    public static string new_mon_temp_mac = "";
    public static bool new_mon_canceled = false;
}
