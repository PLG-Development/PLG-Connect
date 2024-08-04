using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;
using PLG_Connect_Network;
using Avalonia;
using Avalonia.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace PLG_Connect_Presenter;


public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
        LoadImage();

        Start();
        Server server = new Server();
        server.displayTextHandlers.Add(
            (string m) => Dispatcher.UIThread.InvokeAsync(() => DisplayText(m))
        );
        server.toggleBlackScreenHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(ToggleBlackScreen));
        server.firstRequestHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(firstRequest));
        server.openFileHandlers.Add((string path) => Dispatcher.UIThread.InvokeAsync(() => OpenFile(path)));
    }


    private void OpenFile(string path)
    {
        // Open file with default application
        Process program = new Process { StartInfo = new ProcessStartInfo {
                FileName = "xdg-open",
                Arguments = path,
                UseShellExecute = true
        }};
        program.Start();
        Thread.Sleep(1000);

        // Find the window ID of the opened program
        string openWindows = runTerminalCommand("wmctrl", "-l");
        string idPattern = "(0x[a-fA-F0-9]+) .*\n*$";
        // string id = Regex.Match(openWindows, idPattern).Groups[0].Value;
        string winwdoId = Regex.Match(openWindows, idPattern).Groups[1].Value;

        // Bring the app into focus
        runTerminalCommand("wmctrl", $"-i -a {winwdoId}");
        runTerminalCommand("wmctrl", $"-i -r {winwdoId} -b add,fullscreen");
    }

    private static string runTerminalCommand(string command, string arguments) {
        Process terminal = new Process { StartInfo = new ProcessStartInfo {
            FileName = command, Arguments = arguments, RedirectStandardOutput = true
        }};
        terminal.Start();
        terminal.WaitForExit();
        return terminal.StandardOutput.ReadToEnd();
    }

    private void firstRequest()
    {
        startInfo.IsVisible = false;
        content.IsVisible = true;
    }

    bool showBlackScreen = false;
    private void ToggleBlackScreen()
    {
        showBlackScreen = !showBlackScreen;
        main.IsVisible = !showBlackScreen;
    }

    private void DisplayText(string content)
    {
        TextContent.Content = content;
    }

    public void LoadImage()
    {
        string theme = Application.Current.ActualThemeVariant.ToString();
        if (theme == "Light")
        {
            ImgLoading.Source = new Bitmap("LOGO_white.png");
        }
        else if (theme == "Dark")
        {
            ImgLoading.Source = new Bitmap("LOGO_white.png");
        }
        else
        {
            ImgLoading.Source = new Bitmap("LOGO_white.png");
        }
    }


    public async void Start()
    {
        string hostName = Dns.GetHostName();
        string ipAddress = Array.Find(
            Dns.GetHostAddresses(hostName)
            , ip => ip.AddressFamily == AddressFamily.InterNetwork
        )!.ToString();
        string macAddress = getMacAddress();

        TbStartupInformation.Text = "Starting Up ...\n\n";
        int count = 0;
        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(2000));
        while (await timer.WaitForNextTickAsync())
        {
            switch (count)
            {
                case 0:
                    TbStartupInformation.Text = "Loading IP Address...\n\n";
                    count = 1;
                    break;
                case 1:
                    TbStartupInformation.Text = "Start Listening...\n\n";
                    count = 2;
                    break;
                case 2:
                    count = 3;
                    TbStartupInformation.Text = $"Welcome to PLG Connect Presenter!\n{ipAddress}\n{macAddress}";
                    break;
                case 3:
                    count = 2;
                    TbStartupInformation.Text = $"Ready to Connect\n{ipAddress}\n{macAddress}";
                    break;
            }
        }
    }

    private static string getMacAddress()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                string rawMacAddress = ni.GetPhysicalAddress().ToString();
                List<string> macAddressParts = new();
                for (int i = 0; i < rawMacAddress.Length; i += 2)
                {
                    macAddressParts.Add(rawMacAddress[i].ToString() + rawMacAddress[i + 1].ToString());
                }
                return String.Join(":", macAddressParts);
            }
        }

        throw new Exception("No MAC Address found");
    }
}
