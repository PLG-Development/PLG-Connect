using System.Net;
using WatsonHttpMethod = WatsonWebserver.Core.HttpMethod;
using WatsonWebserver;
using WatsonWebserver.Core;
using System.Text.Json;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;


namespace PLG_Connect_Network;


public class PLGServer
{
    public List<Action<string>> displayTextHandlers = new List<Action<string>>();
    public List<Action> toggleBlackScreenHandlers = new List<Action>();
    public List<Action<string>> runCommandHandlers = new List<Action<string>>();
    public List<Action> nextSlideHandlers = new List<Action>();
    public List<Action> previousSlideHandlers = new List<Action>();
    public List<Action> firstRequestHandlers = new List<Action>();
    public List<Action> beforeRequestHandlers = new List<Action>();
    public List<Action<string>> openFileHandlers = new List<Action<string>>();
    public List<Action> shutdownHandlers = new List<Action>();
    public string Password;

    private Webserver server;
    public PLGServer(string password = "0", int port = 8080)
    {
        Logger.Log("Welcome to PLG Connect Network Server!");
        Logger.Log("Starting up...");
        Password = password;

        WebserverSettings settings = new()
        {
            Hostname = "*",
            Port = port,
        };
        server = new Webserver(settings, DefaultRoute);

        server.Routes.PreAuthentication.Static.Add(WatsonHttpMethod.GET, "/ping", PingRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/changePassword", ChangePasswordRoute);

        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/displayText", DisplayTextRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/toggleBlackScreen", ToggleBlackScreenRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/runCommand", RunCommandRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/nextSlide", NextSlideRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/previousSlide", PreviousSlideRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/openFile", OpenFileRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.GET, "/shutdown", ShutdownRoute);

        server.Routes.PreRouting = BeforeRequest;
        server.Routes.AuthenticateRequest = AuthenticateRequest;

        server.StartAsync();

        Logger.Log("Successfully initialized server network");
    }

    public void Stop()
    {
        server.Stop();
        Logger.Log("Server stopped");
    }

    bool firstRequestHappend = false;
    Task BeforeRequest(HttpContextBase ctx)
    {
        foreach (var handler in beforeRequestHandlers)
        {
            handler();
        }

        if (firstRequestHappend) { return Task.CompletedTask; }

        foreach (var handler in firstRequestHandlers)
        {
            handler();
        }
        firstRequestHappend = true;
        return Task.CompletedTask;
    }

    async Task<bool> AuthenticateRequest(HttpContextBase ctx)
    {
        string? authHeader = ctx.Request.Headers["Authorization"];
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.Send("Unauthorized");
            return false;
        }

        string token = authHeader.Substring("Bearer ".Length);
        if (token != Password)
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.Send("Unauthorized");
            return false;
        }
        Logger.Log("Successfully Authenticated!");
        return true;
    }

    static T ExtractObject<T>(HttpContextBase ctx)
    {
        var result = JsonConvert.DeserializeObject<T>(ctx.Request.DataAsString);
        if (result == null)
        {
            throw new Exception("Invalid JSON");
        }
        Logger.Log("JSON Object from HttpContextBase extracted");
        return result;
    }

    async Task ChangePasswordRoute(HttpContextBase ctx)
    {
        ChangePasswordMessage result;

        try
        {
            result = ExtractObject<ChangePasswordMessage>(ctx);
        }
        catch (Exception e)
        {
            Logger.Log("Error while changing password: " + e.Message, Logger.LogType.Error);
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await ctx.Response.Send($"ERROR: {e.Message}");
            return;
        }

        Password = result.NewPassword;
        Logger.Log("Successfully changed password");
        await ctx.Response.Send("");

    }

    static async Task DefaultRoute(HttpContextBase ctx)
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.Send("This should never be called");
    }

    static async Task PingRoute(HttpContextBase ctx)
    {
        await ctx.Response.Send("pong");
        Logger.Log("Ping answered: pong! |   o|");
    }

    // General purpose routes
    async Task DisplayTextRoute(HttpContextBase ctx)
    {
        DisplayTextMessage result;

        try
        {
            result = ExtractObject<DisplayTextMessage>(ctx);
        }
        catch (Exception e)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Logger.Log("Error while displaying text: " + e.Message, Logger.LogType.Error);
            await ctx.Response.Send($"ERROR: {e.Message}");
            return;
        }

        foreach (var handler in displayTextHandlers)
        {
            handler(result.Text);
        }
        await ctx.Response.Send("");
        Logger.Log("Displayed text: " + result);
    }

    // note: this function handels the black screen state paralel to the actual presenter logic which duplicats the code and is not ideal
    // however this makes it much simpler to handler because the presenter need to work on different thread and therefore cant easily return values
    private bool blackScreenEnabled = false;
    async Task ToggleBlackScreenRoute(HttpContextBase ctx)
    {
        foreach (var handler in toggleBlackScreenHandlers)
        {
            handler();
        }
        blackScreenEnabled = !blackScreenEnabled;
        await ctx.Response.Send(JsonConvert.SerializeObject(new ToggleBlackScreenReturnMessage { BlackScreenEnabled = blackScreenEnabled }));
        Logger.Log("Toggled blackscreen");
    }
    async Task RunCommandRoute(HttpContextBase ctx)
    {
        RunCommandMessage result;

        try
        {
            result = ExtractObject<RunCommandMessage>(ctx);
        }
        catch (Exception e)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Logger.Log("Error while running command: " + e.Message, Logger.LogType.Error);
            await ctx.Response.Send($"ERROR: {e.Message}");
            return;
        }

        foreach (var handler in runCommandHandlers)
        {
            handler(result.Command);
            Logger.Log("Extracted something from anything: " + result.Command);
        }
        await ctx.Response.Send("");
        Logger.Log("Ran commands");
    }

    async Task ShutdownRoute(HttpContextBase ctx)
    {

        foreach (var handler in shutdownHandlers)
        {
            handler();
        }
        await ctx.Response.Send("");
        Logger.Log("Bye bye display: powered off display");
    }



    async Task OpenFileRoute(HttpContextBase ctx)
    {
        string? fileEnding = ctx.Request.Query.Elements.Get("fileEnding");
        string? type = ctx.Request.Query.Elements.Get("type");
        if (fileEnding == null)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Logger.Log("Error while opening file: No file ending found", Logger.LogType.Error);
            await ctx.Response.Send("Missing fileEnding");
            return;
        }

        if (type == null){
            // try getTypeByExtension()
            // else: error "Missing type"
        }


        string fileHash = BitConverter.ToString(SHA1.Create().ComputeHash(ctx.Request.DataAsBytes)).Replace("-", "").ToLower();

        string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PLG-Development",
            "PLG-Connect-Presenter",
            "data"
        );
        // Create folder if it doesn't exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, fileHash) + $".{fileEnding}";

        // Only donwload file if it doesn't exist - NONSENS, hier muss noch was hin, irgendne Rückmeldung. Könnte ja ne neue Version sein
        if (!File.Exists(filePath))
        {
            await File.WriteAllBytesAsync(filePath, ctx.Request.DataAsBytes);
        } else {
            // ????
        }

        foreach (var handler in openFileHandlers)
        {
            handler(filePath);
        }

        await ctx.Response.Send("");
        Logger.Log("Opened file");
    }

    async Task NextSlideRoute(HttpContextBase ctx)
    {
        foreach (var handler in nextSlideHandlers)
        {
            handler();
        }
        await ctx.Response.Send("");
        Logger.Log("Invoked next slide");
    }

    async Task PreviousSlideRoute(HttpContextBase ctx)
    {
        foreach (var handler in previousSlideHandlers)
        {
            handler();
        }
        await ctx.Response.Send("");
        Logger.Log("Invoked previous slide");
    }
}
