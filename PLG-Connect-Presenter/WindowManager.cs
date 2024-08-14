using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace PLG_Connect_Presenter;

public class WindowManager {
    public async static void focusWindow(string windowId) {
        await runTerminalCommand("wmctrl", $"-i -a {windowId}");
        await runTerminalCommand("wmctrl", $"-i -r {windowId} -b add,fullscreen");
    }

    public async static Task<string> getLatestWindowId() {
        // get ids and names of all open windows
        string openWindows = await runTerminalCommand("wmctrl", "-l");
        // get the last outputed window id
        string idPattern = "(0x[a-fA-F0-9]+) .*\n*$";

        string winwdoId = Regex.Match(openWindows, idPattern).Groups[1].Value;
        return winwdoId;
    }

    private async static Task<string> runTerminalCommand(string command, string arguments) {
        Process terminal = new Process { StartInfo = new ProcessStartInfo {
            FileName = command, Arguments = arguments, RedirectStandardOutput = true
        }};
        terminal.Start();
        await terminal.WaitForExitAsync();
        return terminal.StandardOutput.ReadToEnd();
    }
}
