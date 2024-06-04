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
        nh.displayTextHandlers.Add((string s) => { Foo(); });
    }

    public void Foo()
    {

    }

    public void LoadImage()
    {
        if (Application.Current != null)
        {
            string theme = Application.Current.ActualThemeVariant.ToString();
            if (theme == "Light")
            {
                ImgLoading.Source = new Bitmap("Schullogo_PNG_dark.png");
            }
            else if (theme == "Dark")
            {
                ImgLoading.Source = new Bitmap("Schullogo_PNG_white.png");
            }
            else
            {
                ImgLoading.Source = new Bitmap("Schullogo_PNG_grey.png");
            }
        }
    }

    // DispatcherTimer starter = new DispatcherTimer();
    // Random r = new Random();
    int count = 0;
    public async void Start()
    {
        string IP = "10.16.10.18"; // HIER IP-ADRESSE ERHALTEN
        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(2000));

        while (await timer.WaitForNextTickAsync())
        {
            switch (count)
            {
                case 0:
                    TbStartupInformation.Text = "Loading IP Address...";
                    count = 1;
                    //(sender as DispatcherTimer).Interval = new TimeSpan(0, 0, 0, 0, r.Next(2000, 4000));
                    break;
                case 1:
                    TbStartupInformation.Text = "Start listening...";
                    count = 2;
                    ///(sender as DispatcherTimer).Interval = new TimeSpan(0, 0, 0, 0, r.Next(2000, 4000));
                    break;
                case 2:
                    count = 3;
                    TbStartupInformation.Text = "Welcome to PLG Connect Presenter!\n" + IP.ToString();
                    //(sender as DispatcherTimer).Interval = new TimeSpan(0, 0, 0, 0, 2500);
                    break;
                case 3:
                    count = 2;
                    TbStartupInformation.Text = "Ready to Connect\n" + IP.ToString();
                    //(sender as DispatcherTimer).Interval = new TimeSpan(0, 0, 0, 0, 2500);
                    break;
            }
        }
    }
}
