using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace PLG_Connect_Presenter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadImage();
        }

        public void LoadImage()
        {
            ImgLoading.Source = new Bitmap("Schullogo_PNG_white.png");
        }
    }
}