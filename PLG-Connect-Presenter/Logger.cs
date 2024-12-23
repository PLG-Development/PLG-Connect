using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;

namespace PLG_Connect_Presenter;

public static class Logger
{
     private readonly static string LogDirectory = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
      "PLG-Development",
      "PLG-Connect-Presenter"
    );
    private readonly static string LogPath = Path.Combine(
      LogDirectory,
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
        try{
            WriteToLog(toLog);
        } catch (Exception ex) {
            Console.WriteLine(d.ToString() + " - ERROR - unable to Log to file " + LogPath + ": " + ex.Message);
        }
        
    }

    public enum LogType{
        Error,
        Warning,
        Information,
        None
    }

    private static readonly object _lock = new();

public static void WriteToLog(string input, string? filePath = null)
{
    lock (_lock)
    {
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }

        filePath ??= LogPath;

        // File.AppendAllText erstellt die Datei automatisch, wenn sie nicht existiert
        File.AppendAllText(filePath, "\n" + input);
    }
}

}