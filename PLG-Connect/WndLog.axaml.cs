using Avalonia.Controls;


namespace PLG_Connect;

public partial class WndLog : Window
{
    public WndLog(string msgs)
    {
        InitializeComponent();
        LblContents.Content = msgs;
    }
}