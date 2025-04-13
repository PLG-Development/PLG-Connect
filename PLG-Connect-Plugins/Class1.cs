using PLG_Connect_Network;
using Avalonia.Controls;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace PLG_Connect_Plugins;


public class PluginExecutor
{
    
}

public static class PluginContainer{
    public static List<IConnectPlugin> Plugins { get; } = new List<IConnectPlugin>();

    public static void RegisterPlugin(IConnectPlugin plugin)
    {
        Plugins.Add(plugin);
    }
    public static void UnregisterPlugin(IConnectPlugin plugin)
    {
        Plugins.Remove(plugin);
    }
    public static void Initialize(){
        string pluginFolder = "Plugins";
        foreach (var file in Directory.GetFiles(pluginFolder, "*.dll"))
        {
            var asm = Assembly.LoadFrom(file);
            var types = asm.GetTypes().Where(t => typeof(IConnectPlugin).IsAssignableFrom(t) && !t.IsInterface);
            foreach (var type in types)
            {
                var plugin = (IConnectPlugin)Activator.CreateInstance(type);
                Plugins.Add(plugin);
            }
        }
    }
}

public interface IConnectPlugin
{
    string PluginName { get; }
    
    // Client-Logic
    string GetClientInstruction();

    Control GetServerUI(); // UI-Parts (if necessary) for the server
    
    // Optional: Server-Logic 
    void ExecuteOnServerDisplay();
}
