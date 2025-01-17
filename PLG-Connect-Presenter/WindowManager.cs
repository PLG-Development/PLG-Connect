using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpHook;
using SharpHook.Native;


namespace PLG_Connect_Presenter;

public class WindowManager
{
    public async static void FocusWindow(string windowId)
    {
        try
        {
            await RunTerminalCommand("wmctrl", $"-i -a {windowId}");
            await RunTerminalCommand("wmctrl", $"-i -r {windowId} -b add,fullscreen");
        }
        catch (Exception ex)
        {
            Logger.Log("Could not proceed: wmctrl seems to be not installed: " + ex.Message);
        }

    }

    public static async Task KeyControl(KeyCode c)
    {
        EventSimulator simulator = new EventSimulator();

        simulator.SimulateKeyPress(c);
        await Task.Delay(100);
        simulator.SimulateKeyRelease(c);
    }

    public async static Task<string> getLatestWindowId()
    {
        try
        {
            // get ids and names of all open windows
            string openWindows = await RunTerminalCommand("wmctrl", "-l");
            // get the last outputed window id
            string idPattern = "(0x[a-fA-F0-9]+) .*\n*$";

            string winwdoId = Regex.Match(openWindows, idPattern).Groups[1].Value;
            return winwdoId;
        }
        catch (Exception ex)
        {
            Logger.Log("Could not proceed: wmctrl seems to be not installed: " + ex.Message);
            return null;
        }
    }

    public async static Task CloseWindow(string windowId)
    {
        try
        {
            await RunTerminalCommand("wmctrl", $"-ic {windowId}");
        }
        catch (Exception ex)
        {
            Logger.Log("Could not proceed: wmctrl seems to be not installed: " + ex.Message);
        }
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
