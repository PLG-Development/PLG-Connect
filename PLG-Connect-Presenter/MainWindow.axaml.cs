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
using System.Threading.Tasks;
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
        server.firstRequestHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(BeforeFirstRequest));
        server.openFileHandlers.Add((string path) => Dispatcher.UIThread.InvokeAsync(() => OpenFile(path)));
        server.beforeRequestHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(BeforeRequest));
        server.nextSlideHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(NextSlide));
        server.previousSlideHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(PreviousSlide));
    }


    private string openWindowId = "nothing";
    async private void OpenFile(string path)
    {
        // Open file with default application
        Process program = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = path,
                UseShellExecute = true
            }
        };
        program.Start();

        string windowId = ownWindowId!;
        // wait until the latest window id changes -> app has started
        while (windowId == ownWindowId)
        {
            Console.WriteLine("Waiting for window to open ...");
            await Task.Delay(1000);
            windowId = await WindowManager.getLatestWindowId();
        }
        openWindowId = windowId;
        WindowManager.FocusWindow(windowId);
    }

    private string? ownWindowId;
    private async void BeforeFirstRequest()
    {
        ownWindowId = await WindowManager.getLatestWindowId();

        startInfo.IsVisible = false;
        content.IsVisible = true;
    }

    private async void BeforeRequest()
    {
        // only run this code if an additional window was opened
        if (openWindowId == "") { return; }

        await WindowManager.CloseWindow(openWindowId);
        openWindowId = "";
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
        string theme = Application.Current!.ActualThemeVariant.ToString();
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

    private async void NextSlide() {
        EventSimulator simulator = new EventSimulator();

        simulator.SimulateKeyPress(KeyCode.VcRight);
        await Task.Delay(100);
        simulator.SimulateKeyRelease(KeyCode.VcRight);
    }

    private async void PreviousSlide() {
        EventSimulator simulator = new EventSimulator();

        simulator.SimulateKeyPress(KeyCode.VcLeft);
        await Task.Delay(100);
        simulator.SimulateKeyRelease(KeyCode.VcLeft);
    }


    public async void Start()
    {
        string hostName = Dns.GetHostName();
        string ipAddress = Array.Find(
            Dns.GetHostAddresses(hostName)
            , ip => ip.AddressFamily == AddressFamily.InterNetwork
        )!.ToString();
        string macAddress = GetMacAddress();

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

    private static string GetMacAddress()
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
