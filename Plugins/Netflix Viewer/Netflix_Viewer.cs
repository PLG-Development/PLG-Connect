using PLG_Connect_Plugins;
using Avalonia.Controls;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Netflix_Viewer;

public class Netflix_ViewerPlugin : IConnectPlugin
{
    public string PluginName => "Netflix Viewer";
    public string GetClientInstruction()
    {
        return "Netflix Viewer, requires Netflix account and a valid session. ";
    }

    public Control GetServerUI()
    {
        //return new NetflixViewerControl();
        return null;
    }

    public void ExecuteOnServerDisplay(string[] args)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "bash", // Für Windows: "cmd.exe"
                Arguments = $"-c chromium-browser --kiosk --start-fullscreen " + args[0], // Für Windows: "/c {command}"
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            //Logger.Log($"Fehler beim Ausführen des Befehls: {ex.Message}");
        }

    }
}
