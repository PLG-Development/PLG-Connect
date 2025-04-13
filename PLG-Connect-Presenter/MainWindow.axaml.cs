using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Timers;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PLG_Connect_Network;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using System.Threading.Tasks;
using SharpHook.Native;
using WatsonWebserver.Core;


namespace PLG_Connect_Presenter;


public partial class MainWindow : Window
{

    private System.Timers.Timer _mouseHideTimer;   // Timer, um den Mauszeiger auszublenden
    private bool _mouseMoved;
    Process fileProgram;
    private bool fileWindowOpen = false;
    private string? fileWindowId;

    public MainWindow()
    {
        InitializeComponent();
        Logger.Log("Welcome to PLG Connect Presenter!");
        Logger.Log("Starting up...");
        LoadImage();
        TempInitialize();

        _mouseHideTimer = new System.Timers.Timer(1000);
        _mouseHideTimer.Elapsed += OnMouseHideTimerElapsed;
        _mouseHideTimer.AutoReset = false;

        // PointerMoved-Event registrieren
        this.AddHandler(InputElement.PointerMovedEvent, OnMouseMoved, handledEventsToo: true);
        _mouseHideTimer.Start();

        Task.Run(async () => await Analytics.SendEvent("presenter"));

        Start();

        this.AddHandler(PointerPressedEvent, (sender, e) =>
        {
            ToggleFullscreen();
        }, handledEventsToo: true);

        PLGServer server = new PLGServer();

        server.displayTextHandlers.Add(
            (string m) => Dispatcher.UIThread.InvokeAsync(() => DisplayText(m))
        );
        server.runCommandHandlers.Add(
            (string m) => Dispatcher.UIThread.InvokeAsync(() => ExecuteCommand(m))
        );
        server.toggleBlackScreenHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(ToggleBlackScreen));
        server.firstRequestHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(BeforeFirstRequest));
        server.openFileHandlers.Add((string path, string fileExtension) => Dispatcher.UIThread.InvokeAsync(() => OpenFile(path, fileExtension)));
        server.nextSlideHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(NextSlide));
        server.previousSlideHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(PreviousSlide));
        server.shutdownHandlers.Add(() => Dispatcher.UIThread.InvokeAsync(Shutdown));
        server.pluginHandlers.Add((string message) => Dispatcher.UIThread.InvokeAsync(() => HandlePlugin(message)));
        server.beforeRequestHandlers.Add((HttpContextBase ctx) => Dispatcher.UIThread.InvokeAsync(() => BeforeRequest(ctx)));
        Logger.Log("Successfully initialized GUI!");
    }

    private void Shutdown()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows: shutdown-Befehl
            ExecuteCommand("shutdown /s /t 0");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux: shutdown-Befehl
            ExecuteCommand("shutdown -h now");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: shutdown-Befehl
            ExecuteCommand("shutdown -h now");
        }
        else
        {
            throw new PlatformNotSupportedException("Das Betriebssystem wird nicht unterstützt.");
        }
    }

    private static void ExecuteCommand(string command)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "bash", // Für Windows: "cmd.exe"
                Arguments = $"-c \"{command}\"", // Für Windows: "/c {command}"
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            Logger.Log($"Fehler beim Ausführen des Befehls: {ex.Message}");
        }
    }

    private static void HandlePlugin(string command)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "bash", // Für Windows: "cmd.exe"
                Arguments = $"-c \"{command}\"", // Für Windows: "/c {command}"
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            Logger.Log($"Fehler beim Ausführen des Befehls: {ex.Message}");
        }
    }


    private void OnMouseMoved(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Cursor = new Cursor(StandardCursorType.Arrow);

        _mouseHideTimer.Stop();
        _mouseHideTimer.Start();

        _mouseMoved = true;
    }

    private void OnMouseHideTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_mouseMoved)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Cursor = new Cursor(StandardCursorType.None);
            });
        }
        _mouseMoved = false;
    }

    private void TempInitialize()
    {
        string folderPath = "Assets/";
        ImageList = new List<string>(Directory.GetFiles(folderPath));
    }

    public void ToggleFullscreen()
    {
        if (this.WindowState == WindowState.FullScreen)
        {
            this.WindowState = WindowState.Maximized;
        }
        else
        {
            this.WindowState = WindowState.FullScreen;
        }
        currentWindowState = this.WindowState;
    }

    private async void BeforeRequest(HttpContextBase ctx)
    {
        if (fileWindowOpen && !ctx.Request.Url.Full.Contains("toggleBlackScreen"))
        {
            if (fileWindowId != null)
            {
                await WindowManager.CloseWindow(fileWindowId);
                Logger.Log("Closed file window normaly");
                fileWindowId = null;
            }
            if (!fileProgram.HasExited)
            {
                fileProgram.Kill(entireProcessTree: true);
                Logger.Log("Closed file window process");
            }

            fileWindowOpen = false;
        }
    }

    async private void OpenFile(string path, string fileExtension)
    {
        if (fileExtension == "pptx")
        {
            OpenPresentationFile(path);
        }
        else
        {
            await OpenSimpleFile(path);
        }

        fileWindowOpen = true;
    }

    private void OpenPresentationFile(string path)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "soffice",
            Arguments = "--show " + path,
            UseShellExecute = true
        };
        fileProgram = Process.Start(startInfo)!;
    }

    private async Task OpenSimpleFile(string path)
    {
        // Open file with its default application
        ProcessStartInfo startInfo = new()
        {
            FileName = "xdg-open",
            Arguments = path,
            UseShellExecute = true
        };
        fileProgram = Process.Start(startInfo)!;

        // wait until the latest window id changes -> app has started
        string windowId = ownWindowId!;
        while (windowId == ownWindowId)
        {
            await Task.Delay(1000);
            windowId = await WindowManager.getLatestWindowId();
        }
        fileWindowId = windowId;

        // focus the file window only when the black screen is not shown
        // if the black screen is shown focus the main window so that the user
        // wont see file window
        if (showBlackScreen)
        {
            WindowManager.FocusWindow(ownWindowId!);
        }
        else
        {
            WindowManager.FocusWindow(windowId);
        }
    }

    private string? ownWindowId;
    private async void BeforeFirstRequest()
    {
        ownWindowId = await WindowManager.getLatestWindowId();

        startInfo.IsVisible = false;
    }

    bool showBlackScreen = false;
    private void ToggleBlackScreen()
    {
        showBlackScreen = !showBlackScreen;

        main.IsVisible = !showBlackScreen;

        // also hide the file window if it was opened
        if (showBlackScreen && fileWindowOpen)
        {
            WindowManager.FocusWindow(ownWindowId!);
            Logger.Log("Toggled blackscreen to state: on");
        }
        // focus the opend file window when exiting the black screen
        if (!showBlackScreen && fileWindowOpen)
        {
            WindowManager.FocusWindow(fileWindowId!);
            Logger.Log("Toggled blackscreen to state: off");
        }
    }

    private void DisplayText(string content)
    {
        SetWorkingMode("text");

        TextContent.IsVisible = true;
        TextContent.Content = content.Replace("\\n", "\n");
        Logger.Log("Displayed text: " + content);
    }

    public void LoadImage()
    {
        try
        {
            string theme = Application.Current!.ActualThemeVariant.ToString();
            if (theme == "Light")
            {
                ImgLoading.Source = new Bitmap("Assets/LOGO_white.png");
            }
            else if (theme == "Dark")
            {
                ImgLoading.Source = new Bitmap("Assets/LOGO_white.png");
            }
            else
            {
                ImgLoading.Source = new Bitmap("Assets/LOGO_white.png");
            }

            Logger.Log("Loaded image");

        }
        catch (Exception ex)
        {
            Logger.Log("Error while loading image: " + ex.Message, Logger.LogType.Error);
        }
    }


    private readonly SlideControlType slideControlType = SlideControlType.Presentation;

    private async void NextSlide()
    {

        if (slideControlType == SlideControlType.ImageList)
        {
            SetWorkingMode("switcher");
            LoadNextImage();
        }
        else if (slideControlType == SlideControlType.Presentation)
        {
            SetWorkingMode("external");
            await WindowManager.KeyControl(KeyCode.VcRight);
        }
    }

    private async void PreviousSlide()
    {

        if (slideControlType == SlideControlType.ImageList)
        {
            SetWorkingMode("switcher");
            LoadPreviousImage();
        }
        else if (slideControlType == SlideControlType.Presentation)
        {
            SetWorkingMode("external");
            await WindowManager.KeyControl(KeyCode.VcLeft);
        }
    }

    public WindowState currentWindowState = WindowState.FullScreen;

    private async void SetWorkingMode(string mode)
    { // enum is coming soon :joy:
        switch (mode)
        {
            case "switcher":
                this.Show();
                this.WindowState = currentWindowState;
                ImageViewer.IsVisible = true;
                break;
            case "text":
                this.Show();
                this.WindowState = currentWindowState;
                ImageViewer.IsVisible = false;
                break;
            case "external":
                this.Hide();
                break;
            default:
                break;
        }
    }

    private List<string> ImageList = new List<string>();
    private int imageListPosition = 0;

    private async void LoadNextImage()
    {
        if (imageListPosition < ImageList.Count - 1)
        {
            imageListPosition++;
            LoadImageFromList();
        }
    }

    private async void LoadPreviousImage()
    {
        if (imageListPosition > 0)
        {
            imageListPosition--;
            LoadImageFromList();
        }
    }

    private async void LoadImageFromList()
    {
        if (ImageList.Count >= imageListPosition + 1)
        {
            //DisplayText(ImageList[imageListPosition]);
            ImageViewer.Source = new Bitmap(ImageList[imageListPosition]);
        }
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
                    count++;
                    break;
                case 1:
                    TbStartupInformation.Text = "Start Listening...\n\n";
                    count++;
                    break;
                case 2:
                    count++;
                    TbStartupInformation.Text = $"Welcome to PLG Connect Presenter!\n{ipAddress}\n{macAddress}";
                    break;
                case 3:
                    count--;
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

        Logger.Log("No MAC Address found - maybe there is no network interface available", Logger.LogType.Error);

        throw new Exception("No MAC Address found");
    }
}


public enum SlideControlType
{
    ImageList,
    Presentation,
    Textfile,
    PDF
}
