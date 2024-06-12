using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.NetworkInformation;
using System.IO;
using PLG_Connect_Network;
using Avalonia;
using Avalonia.Styling;


namespace PLG_Connect_Presenter;


public partial class MainWindow : Window
{
        public MainWindow()
        {
            InitializeComponent();
            LoadImage();

            Start();
            Server nh = new Server();
            nh.displayTextHandlers.Add((string s) => {DisplayText(s);});
        }

        public void Foo(){

        }

        public void DisplayText(string content)
        {
            TbContents.Content = content;
        }

        public void LoadImage()
        {
            string theme = Application.Current.ActualThemeVariant.ToString();
            if (theme == "Light")
            {
                ImgLoading.Source = new Bitmap("LOGO_black.png");
            }
            else if (theme == "Dark")
            {
                ImgLoading.Source = new Bitmap("LOGO_white.png");
            }
            else
            {
                ImgLoading.Source = new Bitmap("LOGO_black.png");
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
