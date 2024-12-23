using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;

namespace PLG_Connect_Presenter;

public static class Logger
{
    private readonly static string LogPath = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      "PLG-Development",
      "PLG-Connect-Presenter",
      "log.txt"
    );

    public static void Log(string toLog, LogType l = LogType.Information){
        DateTime d = DateTime.Now;
        if(l == LogType.Error){
            toLog = d.ToString() + " - ERROR - " + toLog;          
        } else if (l==LogType.Warning){
            toLog = d.ToString() + " - WARNING - " + toLog;
        } else if (l==LogType.Information){
            toLog = d.ToString() + " - INFO - " + toLog;
        } else {
            toLog = d.ToString() + " - [unknown logtype] - " + toLog;
        }

        Console.WriteLine(toLog);
        WriteToLog(toLog);
    }

    public enum LogType{
        Error,
        Warning,
        Information,
        None
    }

    public static void WriteToLog(string input, string? filePath = null)
    {
        filePath ??= LogPath;
        File.AppendAllText(filePath, input);
    }
}