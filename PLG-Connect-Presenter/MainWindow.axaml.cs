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
using SharpHook;
using SharpHook.Native;


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
        string osName = Environment.OSVersion.Platform.ToString().ToLower();

        try
        {
            if (osName.Contains("win"))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(startInfo);
            }
            else if (osName.Contains("linux") || osName.Contains("unix"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = path,
                    UseShellExecute = true
                });

                Thread.Sleep(5000);

                var simulator = new EventSimulator();
                simulator.SimulateKeyPress(KeyCode.VcF11);
                simulator.SimulateKeyRelease(KeyCode.VcF11);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
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
