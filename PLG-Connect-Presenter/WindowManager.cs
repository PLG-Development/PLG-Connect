using System.Diagnostics;
using System.Text.RegularExpressions;


namespace PLG_Connect_Presenter;

public class WindowManager {
    public static void focusWindow(string windowId) {
        runTerminalCommand("wmctrl", $"-i -a {windowId}");
        runTerminalCommand("wmctrl", $"-i -r {windowId} -b add,fullscreen");
    }

    public static string getLatestWindowId() {
        // get ids and names of all open windows
        string openWindows = runTerminalCommand("wmctrl", "-l");
        // get the last outputed window id
        string idPattern = "(0x[a-fA-F0-9]+) .*\n*$";

        string winwdoId = Regex.Match(openWindows, idPattern).Groups[1].Value;
        return winwdoId;
    }

    private static string runTerminalCommand(string command, string arguments) {
        Process terminal = new Process { StartInfo = new ProcessStartInfo {
            FileName = command, Arguments = arguments, RedirectStandardOutput = true
        }};
        terminal.Start();
        terminal.WaitForExit();
        return terminal.StandardOutput.ReadToEnd();
    }
}
