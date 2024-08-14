using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PLG_Connect_Presenter;

public class WindowManager
{
    public async static void FocusWindow(string windowId)
    {
        await RunTerminalCommand("wmctrl", $"-i -a {windowId}");
        await RunTerminalCommand("wmctrl", $"-i -r {windowId} -b add,fullscreen");
    }

    public async static Task<string> getLatestWindowId()
    {
        // get ids and names of all open windows
        string openWindows = await RunTerminalCommand("wmctrl", "-l");
        // get the last outputed window id
        string idPattern = "(0x[a-fA-F0-9]+) .*\n*$";

        string winwdoId = Regex.Match(openWindows, idPattern).Groups[1].Value;
        return winwdoId;
    }

    public async static Task CloseWindow(string windowId)
    {
        await RunTerminalCommand("wmctrl", $"-ic {windowId}");
    }

    private async static Task<string> RunTerminalCommand(string command, string arguments)
    {
        Process terminal = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true
            }
        };
        terminal.Start();
        await terminal.WaitForExitAsync();
        return terminal.StandardOutput.ReadToEnd();
    }
}
